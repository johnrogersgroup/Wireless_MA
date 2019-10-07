using System.Collections.ObjectModel;
using System.Linq;
using Acr.UserDialogs;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using UHMS.Core.Models.Bluetooth;
using UHMS.Core.Services;

namespace UHMS.Core.ViewModels
{
    public class DownloadViewModel : MvxViewModel
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IDeviceSlotService _deviceSlotService;
        private readonly IDataLoggingService _dataLoggingService;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxLog _log;

        /// <summary>
        /// List of scanned and currently conencted devices.
        /// </summary>
        public ObservableCollection<DownloadSlotViewModel> _devices = new ObservableCollection<DownloadSlotViewModel>();

        /// <summary>
        /// Gets or sets the devices.
        /// </summary>
        /// <value>The devices.</value>
        public ObservableCollection<DownloadSlotViewModel> Devices
        {
            get { return _devices; }
            set
            {
                if (value != null)
                {
                    _devices = value;
                }
                RaisePropertyChanged(() => Devices);
            }
        }

        private bool _slotsAreEmpty = false;

        public bool SlotsAreEmpty
        {
            get => _slotsAreEmpty;
            set
            {
                SetProperty(ref _slotsAreEmpty, value, "SlotsAreEmpty");
            }
        }

        public DownloadViewModel(IBluetoothService bluetoothService, IDeviceSlotService deviceSlotService, IDataLoggingService dataLoggingService, IUserDialogs userDialogs, IMvxLog log)
        {
            // Register services
            _bluetoothService = bluetoothService;
            _deviceSlotService = deviceSlotService;
            _dataLoggingService = dataLoggingService;
            _userDialogs = userDialogs;
            _log = log;

            _deviceSlotService.DeviceAdded += OnDeviceAdded;
            _deviceSlotService.DeviceRemoved += OnDeviceRemoved;

            SlotsAreEmpty = true;
        }

        private void OnDeviceAdded(object sender, SlotEventArgs e)
        {
            var slot = e.Slot;

            AddDevice(slot);
        }

        private void OnDeviceRemoved(object sender, SlotEventArgs e)
        {
            var deviceId = e.Slot.LastConnectedId;

            DownloadSlotViewModel deviceViewModel = Devices.FirstOrDefault(d => d.Id == deviceId);

            if (deviceViewModel != null)
            {
                //deviceViewModel.CleanupCharacteristics();
                deviceViewModel.Dispose();
                Devices.Remove(deviceViewModel);
            }
            SlotsAreEmpty |= Devices.Count == 0;
        }

        /// <summary>
        /// Adds the or update the device within the Devices list.
        /// </summary>
        /// <param name="device">Device.</param>
        private void AddDevice(DeviceSlot slot)
        {
            Devices.Add(new DownloadSlotViewModel(slot, this,
                                                _bluetoothService,
                                                _deviceSlotService,
                                                _dataLoggingService,
                                                _userDialogs,
                                                _log));
            SlotsAreEmpty = false;
        }
    }
}
