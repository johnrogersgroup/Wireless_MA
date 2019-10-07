using System;
using Acr.UserDialogs;
using MvvmCross.ViewModels;
using MvvmCross.Logging;
using Plugin.BLE.Abstractions.Contracts;
using UHMS.Core.Models;
using UHMS.Core.Services;

namespace UHMS.Core.ViewModels {
    public class BaseViewModel : MvxViewModel {
        public IUserDialogs _userDialogs;
        public IBluetoothService _bluetoothService;
        public ISensorDataService _sensorDataService;
        public IDataLoggingService _dataLoggingService;
        public IDeviceSlotService _deviceSlotService;
        public IMvxLog _log;

        public BaseViewModel(IBluetoothService bluetoothService,
                             ISensorDataService sensorDataService,
                             IUserDialogs userDialogs,
                             IDataLoggingService dataLoggingService,
                             IDeviceSlotService deviceSlotService,
                             IMvxLog log) 
        {
            _userDialogs = userDialogs;
            _bluetoothService = bluetoothService;
            _sensorDataService = sensorDataService;
            _deviceSlotService = deviceSlotService;
            _log = log;
            _dataLoggingService = dataLoggingService;
        }
    }
}
