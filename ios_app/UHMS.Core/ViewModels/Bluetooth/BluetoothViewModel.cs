using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using UHMS.Core.Models.Bluetooth;
using UHMS.Core.Services;

namespace UHMS.Core.ViewModels
{
    public class BluetoothViewModel : MvxViewModel
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IDeviceSlotService _deviceSlotService;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxLog _log;

        /// <summary>
        /// Token to prematurely cancel the task.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Gets the refresh command used to connect to a event in the view. 
        /// </summary>
        /// <value>The refresh command.</value>
        public MvxCommand RefreshCommand => new MvxCommand(() => TryStartScanning(true));

        /// <summary>
        /// Keeps track of whether the list is open or not to prevent unnecessary scanning operations.
        /// </summary>
        private Boolean bluetoothListOpened = false;

        /// <summary>
        /// List of scanned and currently conencted devices.
        /// </summary>
        public ObservableCollection<DeviceViewModel> _devices = new ObservableCollection<DeviceViewModel>();

        /// <summary>
        /// List of DeviceViewModels that the View can access to display.
        /// </summary>
        /// <value>The scanned devices.</value>
        public ObservableCollection<DeviceViewModel> ScannedDevices
        {
            get { return _devices; }
            set
            {
                if (value != null)
                {
                    _devices = value;
                }
                _log.Debug("Scanned Devices collection set");
                RaisePropertyChanged(() => ScannedDevices);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the central bluetooth device is refreshing.
        /// </summary>
        /// <value><c>true</c> if is refreshing; otherwise, <c>false</c>.</value>
        public bool IsRefreshing => _bluetoothService.IsScanning;

        public RefreshState StateObject = new RefreshState();
        public bool IsRefreshingList {
            get => StateObject.IsRefreshingList;
            set => StateObject.IsRefreshingList = value;
        }
        /// <summary>
        /// Gets a value indicating whether the central bluetooth device is state on.
        /// </summary>
        /// <value><c>true</c> if is state on; otherwise, <c>false</c>.</value>
        public bool IsStateOn => _bluetoothService.IsOn;

        /// <summary>
        /// Gets the state string of the current bluetooth state.
        /// </summary>
        /// <value>The state text.</value>
        public string StateText => _bluetoothService.StateText;

        /// <summary>
        /// Keeps track of the currently selected device.
        /// </summary>
        private DeviceViewModel _selectedDevice;

        /// <summary>
        /// Binding to the currently selected device in the view.
        /// </summary>
        public DeviceViewModel SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _log.Debug("Device Selected");
                if (value != null)
                {
                    _selectedDevice = value;
                }

                RaisePropertyChanged(() => SelectedDevice);
            }
        }

        /// <summary>
        /// Command to stop scanning for bluetooth devices.
        /// </summary>
        /// <value>The stop scan command.</value>
        public MvxCommand StopScanCommand => new MvxCommand(() =>
        {
            _cancellationTokenSource.Cancel();
            CleanupCancellationToken();
            RaisePropertyChanged(() => IsRefreshing);
        }, () => _cancellationTokenSource != null);

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UHMS.Core.ViewModels.BluetoothViewModel"/> class.
        /// </summary>
        /// <param name="bluetoothService">Bluetooth service.</param>
        /// <param name="userDialogs">User dialogs.</param>
        /// <param name="log">Log.</param>
        public BluetoothViewModel(IBluetoothService bluetoothService, IDeviceSlotService deviceSlotService, IUserDialogs userDialogs, IMvxLog log)
        {
            // Register services
            _bluetoothService = bluetoothService;
            _deviceSlotService = deviceSlotService;
            _userDialogs = userDialogs;
            _log = log;

            // Event Handlers reacting to bluetooth and connected device state changes
            _bluetoothService.Bluetooth.StateChanged += OnStateChanged;
            _bluetoothService.Adapter.DeviceDiscovered += OnDeviceDiscovered;
            _bluetoothService.Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            // _bluetoothService.Adapter.DeviceConnected += OnDeviceConnected;
            _bluetoothService.Adapter.DeviceDisconnected += OnDeviceDisconnected;
            _bluetoothService.Adapter.DeviceConnectionLost += OnDeviceConnectionLost;
        }

        /// <summary>
        /// Cleanups the cancellation token.
        /// </summary>
        private void CleanupCancellationToken()
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            RaisePropertyChanged(() => StopScanCommand);
        }

        /// <summary>
        /// Event handler for when the central bluetooth state changes.
        /// </summary>
        /// <param name="sender">Event Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            RaisePropertyChanged(nameof(IsStateOn));
            RaisePropertyChanged(nameof(StateText));
        }

        /// <summary>
        /// Event handler for when a new bluetooth device is discovered during scan.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Event arguments.</param>
        private void OnDeviceDiscovered(object sender, DeviceEventArgs args)
        {
            AddOrUpdateDevice(args.Device);
        }
        /// <summary>
        /// Event handler for when the scan reaches the scan timeout mark.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => IsRefreshing);
            CleanupCancellationToken();
        }

        /// <summary>
        /// Event handler for when a device disconnects. 
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnDeviceDisconnected(object sender, DeviceEventArgs e)
        {

            DeviceViewModel deviceViewModel = ScannedDevices.FirstOrDefault(d => d.Id == e.Device.Id);
            if (deviceViewModel != null)
                deviceViewModel.Update();

            _log.Debug($"Device disconnection detected for Device: {deviceViewModel.SensorDevice.Name}");
            _bluetoothService.OnDisconnected(deviceViewModel.SensorDevice);

            _userDialogs.HideLoading();
            _userDialogs.Toast($"\tDisconnected from {e.Device.Name}");
        }

        /// <summary>
        /// Event handler for when a device loses connection to the central bluetooth device. Can be from both accidnetal or planned disconnects.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnDeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            DeviceViewModel deviceViewModel = ScannedDevices.FirstOrDefault(d => d.SensorDevice.Id == e.Device.Id);
            if (deviceViewModel != null)
                deviceViewModel.Update();
            _bluetoothService.OnDisconnected(deviceViewModel.SensorDevice);

            // Messages to the user
            _userDialogs.HideLoading();
            _userDialogs.Toast($"\tConnection lost to {e.Device.Name}. Check the sensor for possible issues.", TimeSpan.FromMilliseconds(6000));
        }

        /// <summary>
        /// Adds the or update the device within the ScannedDevices list.
        /// </summary>
        /// <param name="device">Device.</param>
        private void AddOrUpdateDevice(IDevice device)
        {
            var dvm = ScannedDevices.FirstOrDefault(d => d.Id == device.Id);

            // If the device is already in the ScannedDevices list
            if (dvm != null)
            {
                dvm.Update();
            }
            // Else is a newly discovered device
            else
            {
                var deviceType = _bluetoothService.DiscoverDeviceType(device);
                  
                // If a device is a stroke device, show device on list
                if (deviceType == DeviceType.Stroke || deviceType == DeviceType.CP)
                {
                    // Create a a new model wrapper for the device.
                    SensorDevice sensorDevice = new SensorDevice(device);
                    _log.Debug($"Created a new SensorDevice model for {device.Name}.");
                    sensorDevice.Type = deviceType;
                    ScannedDevices.Add(new DeviceViewModel(sensorDevice, this, _bluetoothService, _deviceSlotService, _userDialogs, _log));
                }

            }
        }

        /// <summary>
        /// Checks the current bluetooth state and fires the async device scanning Task if bluetooth is available
        /// </summary>
        private void TryStartScanning(bool refresh = false)
        {
            // if the list is being shown in view
            if (!bluetoothListOpened)
            {
                if (IsStateOn && (refresh || !ScannedDevices.Any()) && !IsRefreshing)
                {
                    IsRefreshingList = true;
                    Task.Run(async () => await ScanForDevices());

                }
                bluetoothListOpened = true;
            }
            // Else if the list is being closed from view
            else
            {
                _bluetoothService.Adapter.StopScanningForDevicesAsync();
                IsRefreshingList = false;
                bluetoothListOpened = false;
            }
        }

        /// <summary>
        /// Clear unconnected devices from ScannedDevices.
        /// </summary>
        private void ClearUnAdvertisedDevices()
        {
            // If device was removed from list ScannedDevices, look for another 
            // element which is not connected by running ClearUnconnectedDevices.
            InvokeOnMainThread(() =>
            {
                if (ScannedDevices.Remove(ScannedDevices.FirstOrDefault(deviceViewModel => !deviceViewModel.IsConnected)))
                {
                    ClearUnAdvertisedDevices();
                }
            });
        }

        /// <summary>
        /// Scans for devices.
        /// </summary>
        /// <returns>The for devices.</returns>
        private async Task ScanForDevices()
        {
            _log.Info("Starting Bluetooth Scan for ScannedDevices.");

            foreach (var connectedDevice in _bluetoothService.Adapter.ConnectedDevices)
            {
                //update rssi for already connected devices (so that 0 is not shown in the list)
                try
                {
                    await connectedDevice.UpdateRssiAsync();
                }
                catch (Exception ex)
                {
                    _log.Trace(ex.Message);
                    _userDialogs.Toast($"\tFailed to update RSSI for {connectedDevice.Name}");
                }

                // Update the view for the connected device.
                AddOrUpdateDevice(connectedDevice);
            }

            ClearUnAdvertisedDevices();

            _cancellationTokenSource = new CancellationTokenSource();

            RaisePropertyChanged(() => StopScanCommand);
            RaisePropertyChanged(() => IsRefreshing);

            // Function to filter out the empty name bluetooth devices
            Func<IDevice, bool> deviceFilter = delegate (IDevice device) { return !string.IsNullOrEmpty(device?.Name); };

            // Populate the DeviceList within the adapter
            try
            {
                await _bluetoothService.Adapter.StartScanningForDevicesAsync(null, deviceFilter, false, _cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                _userDialogs.Alert("Error occurred: " + e.Message);
            }
            finally
            {
                RaisePropertyChanged(() => ScannedDevices);
                IsRefreshingList = false;
                //InvokeOnMainThread(() => IsRefreshingList = false );
               
            }
        }
    }

    public class RefreshState : MvxNotifyPropertyChanged
    {
        private bool _isRefreshingList = true;

        public bool IsRefreshingList
        {
            get => _isRefreshingList;
            set
            {
                if (SetProperty(ref _isRefreshingList, !value))
                {
                    RaisePropertyChanged(() => IsRefreshingList);
                }
            }
        }
    }
}
