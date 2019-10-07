using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using UHMS.Core.Models.Bluetooth;
using UHMS.Core.Services;

namespace UHMS.Core.ViewModels
{
    public class DeviceViewModel : MvxViewModel
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IDeviceSlotService _deviceSlotService;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxLog _log;
        private readonly MvxNotifyPropertyChanged _parentViewModel;

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
            }
        }

        /// <summary>
        /// Gets the identifier of the Sensor Device.
        /// </summary>
        /// <value>The Id.</value>
        public Guid Id => SensorDevice.Id;

        /// <summary>
        /// Gets the rssi of the Sensor Device.
        /// </summary>
        /// <value>The rssi.</value>
        public int Rssi => SensorDevice.Rssi;

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

        /// <summary>
        /// Gets the battery level of the Sensor Device.
        /// </summary>
        /// <value>The battery level.</value>
        public int Battery => SensorDevice.Battery;

        /// <summary>
        /// Gets a value indicating whether the device is connected.
        /// </summary>
        /// <value><c>true</c> if is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected => SensorDevice.IsConnected;

        /// <summary>
        /// Gets the action string based on the current connection state.
        /// </summary>
        /// <value>The connect or dispose action string.</value>
        public string ConnectOrDisposeActionString => SensorDevice.IsConnected ? "Disconnect" : "Connect";

        /// <summary>
        /// Gets the image source for the sensor device.
        /// </summary>
        /// <value>The image source string.</value>
        public string ImageSource => _device.IsConnected ? "-o-device_pink" : "-o-device_black";

        public string SlotName => _device.SlotIndex > -1 ? _deviceSlotService.SlotName[_device.SlotIndex] : "";

        public DeviceViewModel(SensorDevice device, MvxNotifyPropertyChanged parentViewModel, IBluetoothService bluetoothService, IDeviceSlotService deviceSlotService, IUserDialogs userDialogs, IMvxLog log)
        {
            _device = device;
            _parentViewModel = parentViewModel;
            _bluetoothService = bluetoothService;
            _deviceSlotService = deviceSlotService;
            _userDialogs = userDialogs;
            _log = log;
        }

        /// <summary>
        /// Update the specified device.
        /// </summary>
        /// <param name="sensorDevice">New device.</param>
        public void Update(SensorDevice sensorDevice = null)
        {
            if (sensorDevice != null)
            {
                SensorDevice = sensorDevice;
            }
            RaiseAllPropertiesChanged();
        }

        /// <summary>
        /// Gets the command to handle a connect or dispose request.
        /// </summary>
        /// <value>The command to connect or dispose a connection.</value>
        private IMvxCommand _connectOrDisposeRequested { get; set; }

        /// <summary>
        /// Gets the command to handle a connect or dispose request.
        /// </summary>
        /// <value>The command to connect or dispose a connection.</value>
        public IMvxCommand ConnectOrDisposeRequested
        {
            get
            {
                return _connectOrDisposeRequested ?? new MvxCommand(HandleSelectedDevice);
            }
        }

        /// <summary>
        /// Handles the selection of a device by the user
        /// </summary>
        /// <param name="deviceViewModel">Device view model.</param>
        private async void HandleSelectedDevice()
        {
            _log.Info("Attempting to handle selected bluetooth device");
            // Check bluetooth status and if not on, exit
            if (!_bluetoothService.IsOn)
            {
                _log.Info("Bluetooth is not on");
                _userDialogs.Toast($"\tError: {_bluetoothService.StateText}");
                return;
            }

            // Device is already connected
            if (IsConnected)
            {
                _log.Info("Device is already connected. Attempting to disconnect.");
                Task.Run(async () => await TryToDisconnectDevice());
                _log.Debug("Disconnection task <TryToDisconnectDevice> has been spawned.");
                return;
            }

            // Device has not connected yet
            else
            {
                _log.Info("Connecting to device.");
                var slots = _deviceSlotService.GetAvailableSlots();
                var buttons = new List<string>();
                if (slots.Count > 0)
                {
                    foreach (var slotIndex in slots)
                    {
                        try
                        {
                            string name = _deviceSlotService.SlotName[slotIndex];
                            buttons.Add(name);
                        }
                        catch (KeyNotFoundException)
                        {
                            _userDialogs.Alert("Invalid slot index value");
                            return;
                        }

                    }
                    var result = await _userDialogs.ActionSheetAsync("Which device?", "Cancel", null, null, buttons.ToArray());
                    if (result == "Cancel")
                    {
                        return;
                    }
                    var selected = _deviceSlotService.SlotName.IndexOf(result);
                    await Task.Run(async () => await TryToConnectDevice(selected));
                    return;
                }
                else
                {
                    _userDialogs.Alert("Please disconnect from a device before attempt to connect to another one");
                }
            }
        }

        /// <summary>
        /// Tries to connect device.
        /// </summary>
        /// <returns>The to connect device.</returns>
        /// <param name="deviceViewModel">Device view model.</param>
        /// <param name="showPrompt">If set to <c>true</c> show prompt.</param>
        private async Task<bool> TryToConnectDevice(int index = -1, bool showPrompt = true)
        {
            try
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                var config = new ProgressDialogConfig()
                {
                    Title = $"Trying to establish connection with '{DeviceName}'",
                    CancelText = "Cancel",
                    IsDeterministic = false,
                    MaskType = MaskType.None,
                    OnCancel = tokenSource.Cancel
                };

                bool didConnect = false;
                using (var progress = _userDialogs.Progress(config))
                {
                    progress.Show();
                    if (index > -1)
                    {
                        didConnect = await _bluetoothService.ConnectDevice(SensorDevice, tokenSource, index);
                    }
                    else
                    {
                        didConnect = await _bluetoothService.ConnectDevice(SensorDevice, tokenSource, -1);
                    }

                }

                tokenSource.Dispose();

                if (!didConnect) {
                    return false;
                }
                _userDialogs.Toast($"\tConnected to {SensorDevice.Name}");

                RaisePropertyChanged("ConnectOrDisposeActionString");
                RaisePropertyChanged("ImageSource");
                return true;

            }
            catch (Exception ex)
            {
                _userDialogs.Alert(ex.Message, "Connection error");
                _log.Trace(ex.Message);
                return false;
            }
            finally
            {
                _userDialogs.HideLoading();
                Update();
            }
        }

        /// <summary>
        /// Tries to disconnect from a device.
        /// </summary>
        /// <returns>The to disconnect device.</returns>
        /// <param name="sensorDeviceViewModel">Sensor device view model.</param>
        private async Task TryToDisconnectDevice()
        {
            _userDialogs.ShowLoading($"Started <TryToDisconnectDevice> task.");
            try
            {
                // Check if the device is not connected
                if (!IsConnected)
                {
                    _log.Debug($"{DeviceName} has already disconnected. Aborting disconnect attempt.");
                    _userDialogs.ShowLoading($"Disconnecting {DeviceName}...");
                    return;
                }


                _userDialogs.ShowLoading($"Disconnecting {DeviceName}...");

                await _bluetoothService.DisconnectDevice(SensorDevice);

            }
            catch (Exception ex)
            {
                _userDialogs.Alert(ex.Message, "Disconnect error");
            }
            finally
            {
                Update();
                _userDialogs.HideLoading();

                RaisePropertyChanged("ConnectOrDisposeActionString");
                RaisePropertyChanged("ImageSource");
                _parentViewModel.RaisePropertyChanged();
            }
        }

    }
}
