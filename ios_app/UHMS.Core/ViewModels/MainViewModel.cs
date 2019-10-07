using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using UHMS.Core.Models;
using UHMS.Core.Models.Data;
using UHMS.Core.Services;
using UHMS.Core.Models.Bluetooth;

// TODO: Create a one-to-one mapping of view models to views
namespace UHMS.Core.ViewModels
{
    /// <summary>
    /// <c>MainViewModel</c> class. Currently operates as a parent view model to connect to a single view controller.
    /// </summary>
    public partial class MainViewModel : BaseViewModel {
        BluetoothViewModel BluetoothViewModel { get; set; }
        public DownloadViewModel DownloadViewModel { get; set; }
        VitalSignsMonitorViewModel VitalSignsMonitorViewModel { get; set; }

        public static bool DEBUG_MODE = false;

        // Main View Model that connects the subviews
        public MainViewModel(IBluetoothService bluetoothService, ISensorDataService sensorDataService, IUserDialogs userDialogs, IDataLoggingService dataLoggingService, IDeviceSlotService deviceSlotService, IMvxLog log)
            : base(bluetoothService, sensorDataService, userDialogs, dataLoggingService, deviceSlotService, log)
        {
            BluetoothViewModel = new BluetoothViewModel(bluetoothService, deviceSlotService, userDialogs, log);
            DownloadViewModel = new DownloadViewModel(bluetoothService, deviceSlotService, dataLoggingService, userDialogs, log);
            VitalSignsMonitorViewModel = new VitalSignsMonitorViewModel(bluetoothService, sensorDataService, deviceSlotService, userDialogs, log);
        }

        // Attach view events and other bindable events to event handlers
        public ObservableCollection<DeviceViewModel> ScannedDevices => BluetoothViewModel.ScannedDevices;
        public ObservableCollection<DownloadSlotViewModel> DownloadDevices => DownloadViewModel.Devices;
        public ConcurrentDictionary<string, ObservableCollection<DataProfile>> DataList => VitalSignsMonitorViewModel.GraphData;

        public bool SlotsAreEmpty => DownloadViewModel.SlotsAreEmpty;
        public RefreshState IsRefreshing => BluetoothViewModel.StateObject;
        public DeviceViewModel SelectedDevice => BluetoothViewModel.SelectedDevice;

        // Commands as Event Handlers.
        public IMvxCommand HandleSelectedDevice => BluetoothViewModel.RefreshCommand;
        public IMvxCommand ScanRequested => BluetoothViewModel.RefreshCommand;

        public IMvxCommand LogEvent1 => new MvxCommand(() => LogEvent(1));
        public IMvxCommand LogEvent2 => new MvxCommand(() => LogEvent(2));
        public IMvxCommand LogEvent3 => new MvxCommand(() => LogEvent(3));
        public IMvxCommand LogEvent4 => new MvxCommand(() => LogEvent(4));
        public IMvxCommand LogEvent5 => new MvxCommand(() => LogEvent(5));
        public IMvxCommand LogEvent6 => new MvxCommand(() => LogEvent(6));
        public IMvxCommand LogEvent7 => new MvxCommand(() => LogEvent(7));
        public IMvxCommand LogEvent8 => new MvxCommand(() => LogEvent(8));

        public bool SessionIsRunning => VitalSignsMonitorViewModel.IsRunningSession;
        public IMvxCommand StartSession => new MvxCommand(() =>
        {
            if (_deviceSlotService.AllSlotsAreEmpty)
            {
                _userDialogs.Toast($"\tNo devices are connected");
                return;
            }
            var epochTime = System.DateTimeOffset.Now.ToUnixTimeSeconds();

            bool sessionStarted = _bluetoothService.StartSession(epochTime);
            if (sessionStarted)
            {
                _dataLoggingService.Open(epochTime);
                _userDialogs.Toast($"\tStarted session");
            }
            else
            {
                _userDialogs.Toast($"\tCould not start session");
            }
        });
        public IMvxCommand StopSession => new MvxCommand(() =>
        {
            if (_deviceSlotService.AllSlotsAreEmpty)
            {
                _userDialogs.Toast($"\tNo devices are connected");
                return;
            }

            bool sessionStopped = _bluetoothService.StopSession();
            if (sessionStopped)
            {
                _dataLoggingService.Close();
                _userDialogs.Toast($"\tStopped session");
            }
            else
            {
                _userDialogs.Toast($"\tCould not stop session");
            }
        });

        public IMvxCommand TryStopSession => new MvxCommand(async () =>
        {
            VitalSignsMonitorViewModel.CheckIfASessionIsRunning();
            if (!VitalSignsMonitorViewModel.IsRunningSession)
            {
                return;
            }

            var result = await _userDialogs.ConfirmAsync(new ConfirmConfig
            {
                Message = "Please stop all sessions before proceeding to downloads. You WILL encounter an error if not stopped.",
                OkText = "Stop",
                CancelText = "Cancel"
            });

            if (result)
            {
                if (_deviceSlotService.AllSlotsAreEmpty)
                {
                    _userDialogs.Toast($"\tNo devices are connected");
                    return;
                }

                bool sessionStopped = _bluetoothService.StopSession();
                if (sessionStopped)
                {
                    await _dataLoggingService.Close();
                    _userDialogs.Toast($"\tStopped session");
                }
                else
                {
                    _userDialogs.Toast($"\tCould not stop session");
                }
            }

        });

        public void LogEvent(int eventNumber)
        {
            _dataLoggingService.WriteEvent(eventNumber);
            _userDialogs.Toast($"\tEvent {eventNumber} has been logged.", TimeSpan.FromMilliseconds(500));
        }

        public SlotViewModel Slot1 => VitalSignsMonitorViewModel.DeviceSlots[0];
        public SlotViewModel Slot2 => VitalSignsMonitorViewModel.DeviceSlots[1];
    }
}