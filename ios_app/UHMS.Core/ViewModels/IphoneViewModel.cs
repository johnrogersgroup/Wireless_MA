using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using UHMS.Core.Models;
using UHMS.Core.Models.Data;
using UHMS.Core.Services;

namespace UHMS.Core.ViewModels
{
    public class IphoneViewModel : BaseViewModel
    {
        BluetoothViewModel BluetoothViewModel { get; set; }
        VitalSignsMonitorViewModel VitalSignsMonitorViewModel { get; set; }

        public IphoneViewModel(IBluetoothService bluetoothService, ISensorDataService sensorDataService, IUserDialogs userDialogs, IDataLoggingService dataLoggingService, IDeviceSlotService deviceSlotService, IMvxLog log)
            : base(bluetoothService, sensorDataService, userDialogs, dataLoggingService, deviceSlotService, log)
        {
            BluetoothViewModel = new BluetoothViewModel(bluetoothService, deviceSlotService, userDialogs, log);
            VitalSignsMonitorViewModel = new VitalSignsMonitorViewModel(bluetoothService, sensorDataService, deviceSlotService, userDialogs, log);
            //_dataLoggingService.Open();
        }
    }
}
