using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using UHMS.Core.Models.Bluetooth;
using UHMS.Core.Services;
using System.IO;
using Plugin.BLE.Abstractions.EventArgs;

namespace UHMS.Core.ViewModels
{
    public class DownloadSlotViewModel : MvxViewModel, IDisposable

    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IDeviceSlotService _deviceSlotService;
        private readonly IDataLoggingService _dataLoggingService;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxLog _log;
        private readonly MvxNotifyPropertyChanged _parentViewModel;

        private DeviceSlot _slot;

        /// <summary>
        /// The device model.
        /// </summary>
        private SensorDevice _device;

        /// <summary>
        /// Binding to the currently selected device to the connected View.
        /// </summary>
        public SensorDevice SensorDevice
        {
            get
            {
                return _device;
            }
            set
            {
                _device = value;
                RaisePropertyChanged(() => SensorDevice);
                RaiseAllPropertiesChanged();
            }
        }

        /// <summary>
        /// Gets the identifier of the Sensor Device.
        /// </summary>
        /// <value>The Id.</value>
        public Guid Id => SensorDevice.Id;

        /// <summary>
        /// Gets the type of the Sensor Device.
        /// </summary>
        /// <value>The type.</value>
        public string Type => SensorDevice.Type.ToString();

        /// <summary>
        /// Gets the name of the Sensor Device.
        /// </summary>
        /// <value>The name of the device.</value>
        public string DeviceName => SensorDevice.Name;

        private float _downloadProgress = -1;

        public float DownloadProgress
        {
            get => _downloadProgress;
            set
            {
                SetProperty(ref _downloadProgress, value, "DownloadProgress");
            }
        }

        private bool _isDownloading = false;

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                SetProperty(ref _isDownloading, value, "IsDownloading");
            }
        }
        private bool _isGettingInfo = false;
        /// <summary>
        /// The state of the error.
        /// 0 = No Error
        /// 1 = Device disconnection
        /// 2 = Communication timout
        /// 3 = Exception detected
        /// </summary>
        private int _downloadErrorState = 0;
        private readonly List<string> _errorMessages = new List<string>
        {
            $"No error detected.",
            $"Could not complete the download. The target device has disconnected.",
            $"Could not complete the download. The target device timed out",
            $"Could not complete the download. An exception has been detected.",
            $"Could not start the download. No data to download."
        };

        private uint totalPageCount = 0;
        private int currentPageCount = 0;

        public string SlotName => _device.SlotIndex > -1 ? _deviceSlotService.SlotName[_device.SlotIndex] : "";

        private EventHandler<CharacteristicUpdatedEventArgs> _dataUpdateEventHandler;

        private EventHandler<CharacteristicUpdatedEventArgs> _infoUpdateEventHandler;

        public DownloadSlotViewModel(DeviceSlot slot,
                                    MvxNotifyPropertyChanged parentViewModel,
                                    IBluetoothService bluetoothService,
                                    IDeviceSlotService deviceSlotService,
                                    IDataLoggingService dataLoggingService,
                                    IUserDialogs userDialogs,
                                    IMvxLog log)
        {
            _slot = slot;
            _device = slot.ConnectedDevice;
            _parentViewModel = parentViewModel;
            _bluetoothService = bluetoothService;
            _deviceSlotService = deviceSlotService;
            _dataLoggingService = dataLoggingService;
            _userDialogs = userDialogs;
            _log = log;

            Task.Run(() =>
            {
                ProcessBlockingCollection();
            });

            SetupCharacteristics();

            _deviceSlotService.DeviceAdded += AddCharacteristics;
            _deviceSlotService.DeviceWillBeRemoved += CleanupCharacteristics;
        }

        private void SetupCharacteristics()
        {
            try
            {
                _dataUpdateEventHandler = new EventHandler<CharacteristicUpdatedEventArgs>((s, a) => ParseNotification(s, a));
                SensorDevice.DownloadDataCharacteristic.ValueUpdated += _dataUpdateEventHandler;

                _infoUpdateEventHandler = new EventHandler<CharacteristicUpdatedEventArgs>((s, a) => ParseInfo(s, a));
                SensorDevice.InfoCharacteristic.ValueUpdated += _infoUpdateEventHandler;
            }
            catch (Exception)
            {
                _log.Error("Characteristic subscription for data logging could not be completed.");
            }
        }
        public void AddCharacteristics(object sender, SlotEventArgs args)
        {
            var slot = args.Slot;
            if (slot.Index != _slot.Index) return;

            CleanupCharacteristics(sender, args);
            SetupCharacteristics();
        }

        public void CleanupCharacteristics(object sender, SlotEventArgs args)
        {
            var slot = args.Slot;
            if (slot.Index != _slot.Index) return;

            if (SensorDevice != null)
            {
                SensorDevice.DownloadDataCharacteristic.ValueUpdated -= _dataUpdateEventHandler;
                SensorDevice.InfoCharacteristic.ValueUpdated -= _infoUpdateEventHandler;
                IsDownloading = false;
                _downloadErrorState = 1;
            }
        }

        /// <summary>
        /// Gets the command to handle a connect or dispose request.
        /// </summary>
        /// <value>The command to connect or dispose a connection.</value>
        private IMvxCommand _downloadRequested { get; set; }

        /// <summary>
        /// Gets the command to handle a connect or dispose request.
        /// </summary>
        /// <value>The command to connect or dispose a connection.</value>
        public IMvxCommand DownloadRequested
        {
            get
            {
                return _downloadRequested ?? new MvxCommand(AttemptDownload);
            }
        }

        private async Task Wait()
        {
            while (_isGettingInfo && !_cancellationTokenSource.IsCancellationRequested)
            {
                _log.Debug("Waiting for the download info.");
                await Task.Run(() => Thread.Sleep(500));
            }
            CleanupCancellationToken();
        }

        /// <summary>
        /// Token to prematurely cancel the task.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        private void CleanupCancellationToken()
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        /// <summary>
        /// Handles the download action
        /// </summary>
        private async void AttemptDownload()
        {
            if (IsDownloading)
            {
                _log.Debug("Download function already running.");
                return;
            }
            if (_deviceSlotService.ASlotIsBusy)
            {
                _userDialogs.Alert(new AlertConfig
                {
                    Message = $"Please wait until the previous download action is resolved."
                });
                return;
            }
            _log.Info("Attempting to download");
            DownloadProgress = 0;
            totalPageCount = 0;
            currentPageCount = 0;
            _latestParseTime = DateTime.MaxValue;

            IsDownloading = true;
            _slot.IsBusy = true;
            if (SensorDevice.DownloadDataCharacteristic == null || SensorDevice.InfoCharacteristic == null)
            {
                _log.Info("Could not start download. Characteristics not setup.");
                _userDialogs.Alert(new AlertConfig
                {
                    Message = $"Could not complete the download."
                });
            }

            var commandSuccessful = false;
            var downloadSuccessful = true;

            try
            {
                _log.Info("Sending Get Info Command for download data.");
                _isGettingInfo = true;
                commandSuccessful = await Task.Run(() => SendDownloadCommand(CommandType.GetInfo));
                if (commandSuccessful)
                {
                    _log.Info("[DownloadSlotViewModel] Get Info command successfully sent.");

                    _cancellationTokenSource = new CancellationTokenSource();

                    int timeout = 2000; // 2 second timeout
                    var task = Wait();
                    var ts = new CancellationTokenSource();
                   

                    commandSuccessful = false;
                    if (await Task.WhenAny(task, Task.Delay(timeout)) != task)
                    {
                        _cancellationTokenSource.Cancel();

                        _log.Info("Download info timeout");
                        _userDialogs.Alert(new AlertConfig
                        {
                            Message = $"No download info data received. Download could not be completed."
                        });
                        IsDownloading = false;
                        _slot.IsBusy = false;
                        DownloadProgress = -1;
                        return;
                    }
                }

                if (totalPageCount <= 0)
                {
                    _log.Info("Could not start download. No data to download.");
                    _downloadErrorState = 4;
                }
                else
                {
                    // Wait for the session to completely close.
                    await Task.Run(() => Thread.Sleep(500));

                    commandSuccessful = false;
                    commandSuccessful = await Task.Run(() => SendDownloadCommand(CommandType.StartSessionRead));

                    if (commandSuccessful)
                    {
                        while (IsDownloading || _isProcessingFileData)
                        {
                            if (_downloadErrorState != 0) break;

                            bool receivedDataRecently = (DateTime.Now - _latestParseTime).Seconds < 60;
                            if (!receivedDataRecently && IsDownloading)
                            {
                                _downloadErrorState = 2;
                                break;
                            }

                            await Task.Run(() => Thread.Sleep(500));
                            InvokeOnMainThread(() => { DownloadProgress = (float)currentPageCount / totalPageCount; });
                        }

                        _blockingCollection.Add(Tuple.Create(new byte[0], _fileStream, true));
                    }
                }
            }
            catch (Exception e)
            {
                _log.Debug($"Download Fail: {e.Message}");

                _downloadErrorState = 3;
            }

            if (_downloadErrorState != 0)
            {
                var errorMessage = _errorMessages[_downloadErrorState];

                _userDialogs.Alert(new AlertConfig
                {
                    Message = errorMessage
                });
            }
            else if (downloadSuccessful)
            {
                _log.Info("Finished download");

                _userDialogs.Alert(new AlertConfig
                {
                    Message = $"Download Complete.\r\n{_numSession} Sessions Read\r\n{currentPageCount}/{totalPageCount} Pages Read"
                });
            }

            _downloadErrorState = 0;
            IsDownloading = false;
            _slot.IsBusy = false;
            DownloadProgress = -1;
        }

        public async Task<bool> SendDownloadCommand(CommandType commandType)
        {
            if (SensorDevice.CommandCharacteristic == null)
            {
                _userDialogs.Alert(new AlertConfig
                {
                    Message = $"The download command function is not available for this device."
                });
            }
            bool success = false;
            try
            {
                byte[] data = { (byte)commandType };
                success = await SensorDevice.CommandCharacteristic.WriteAsync(data);
            }
            catch (Exception e)
            {
                _log.Debug($"Error: Characteristic write cannot be completed. {e.Message}");
            }
            return success;
        }

        private void ParseInfo(object sender, CharacteristicUpdatedEventArgs args)
        {
            if (!_isGettingInfo) return;

            byte[] data = args.Characteristic.Value;
            _log.Debug("Parsing Download Info");

            if (data.Length == 6)
            {
                totalPageCount = (uint)(((data[3] << 24) + (data[2] << 16) + (data[1] << 8) + data[0]));
            }
            _numPageReadFailed = 0;
            _numSession = 0;
            _isGettingInfo = false;
        }

        private void ParsePage(byte[] data, FileStream fileStream)
        {
            _dataLoggingService.ParsePage(data, fileStream);

            page++;
        }

        int counter = 0;
        int page = 0;
        int _numPageReadFailed;
        int _numSession;

        private DateTime _latestParseTime = DateTime.MaxValue;

        private void ParseNotification(object sender, CharacteristicUpdatedEventArgs args)
        {
            if (!IsDownloading) return;

            byte[] data = args.Characteristic.Value;
            if (data == null || data.Length == 0)
            {
                _log.Debug("No data was detected from the download characteristic update.");
                return;
            }
            _log.Debug($"characteristic header :{data[0]}, counter: {counter}, Length : {data.Length}, Pages[{currentPageCount}/{totalPageCount}]");

            // Set current time to possibly trigger the two second timeout for the download notifications.
            _latestParseTime = DateTime.Now;

            switch (data[0])
            {
                case 0x0f:
                    uint epochTime = (uint)((data[4] << 24) + (data[3] << 16) + (data[2] << 8) + data[1]);
                    //numPage = data[8] << 24 + (data[7] << 16) + (data[6] << 8) + data[5];
                    var deviceType = data[9];

                    _fileStream = _dataLoggingService.GetFileStream(epochTime, deviceType);
                    _dataLoggingService.WriteHeader(_fileStream);
                    break;
                case 0xff:
                    for (int i = 1; i < data.Length; i++)
                    {
                        Dat[counter++] = data[i];
                        if (counter == 2048)
                        {
                            counter = 0;
                            _blockingCollection.Add(Tuple.Create(Dat.Clone() as byte[], _fileStream, false) );
                            Dat = new byte[2048];
                            currentPageCount++;
                        }
                    }
                    break;
                case 0x10:
                    counter = 0;
                    _numPageReadFailed += (int)((data[4] << 24) + (data[3] << 16) + (data[2] << 8) + data[1]);
                    _numSession++;
                    Dat = new byte[2048];
                    _blockingCollection.Add(Tuple.Create(new byte[0], _fileStream, true));
                    if (_numPageReadFailed + currentPageCount >= totalPageCount)
                    {
                        IsDownloading = false;
                        DownloadProgress = 1.0f;
                    }
                    else
                    {
                        _userDialogs.Alert(new AlertConfig
                        {
                            Message = $"Error: File was corrupted or incomplete."
                        });
                        _log.Debug("The total expected and received page numbers do not match.");
                        IsDownloading = false;


                    }
                    break;
                case 0x11:
                    _log.Debug($"END PAGE ========================= page counter value: {counter}");
                    counter = 0;
                    _numPageReadFailed += (int)((data[4] << 24) + (data[3] << 16) + (data[2] << 8) + data[1]);
                    _numSession++;
                    Dat = new byte[2048];
                    _blockingCollection.Add(Tuple.Create(new byte[0], _fileStream, true));

                    _log.Debug("Start reading another session");
                    fileStreamIndex = (fileStreamIndex == 0 ? 1 : 0);
                    break;
                default:
                    _log.Debug("Data is leaking.");
                    break;
            }
        }

        private void CloseFileStream(FileStream fileStreams)
        {
            if (fileStreams != null)
                fileStreams.Close();
        }
        private byte[] Dat = new byte[2048];
        private BlockingCollection<Tuple<byte[], FileStream, bool>> _blockingCollection = new BlockingCollection<Tuple<byte[], FileStream, bool>>();

        private FileStream _fileStream;
        private int fileStreamIndex = 0;

        private volatile bool _isProcessingFileData = false;
        private void ProcessBlockingCollection()
        {
            foreach (var package in _blockingCollection.GetConsumingEnumerable())
            {
                _log.Debug($"Processing FileStream: {package.Item3}");
                try
                {
                    _isProcessingFileData = true;
                    if (package.Item3 == false)
                    {

                        ParsePage(package.Item1, package.Item2);
                    }
                    else
                    {
                        CloseFileStream(package.Item2);
                    }

                }
                catch (Exception ex)
                {
                    _log.Debug("<<< catch : " + ex.ToString());
                }

                finally
                {
                    _isProcessingFileData = false;
                }

            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _blockingCollection.CompleteAdding();
                _deviceSlotService.DeviceAdded -= AddCharacteristics;
                _deviceSlotService.DeviceWillBeRemoved -= CleanupCharacteristics;
                SensorDevice.DownloadDataCharacteristic.ValueUpdated -= _dataUpdateEventHandler;
                SensorDevice.InfoCharacteristic.ValueUpdated -= _infoUpdateEventHandler;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
         ~DownloadSlotViewModel() {
           // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
           Dispose(false);
         }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
