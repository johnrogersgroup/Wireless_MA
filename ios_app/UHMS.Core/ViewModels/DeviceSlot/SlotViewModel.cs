using System;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions.EventArgs;
using UHMS.Core.Models.Bluetooth;
using UHMS.Core.Services;

namespace UHMS.Core.ViewModels
{
    public class SlotViewModel : MvxViewModel
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IDeviceSlotService _deviceSlotService;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxLog _log;

        private DeviceSlot _slot;
        public SlotViewModel(DeviceSlot slot, IDeviceSlotService deviceSlotService, IMvxLog log)
        {
            _bluetoothService = Mvx.Resolve<IBluetoothService>();
            _deviceSlotService = deviceSlotService;
            _userDialogs = Mvx.Resolve<IUserDialogs>();
            _log = log;

            _slot = slot;

            _deviceSlotService.DeviceAdded += SubscribeToUpdates;
            _deviceSlotService.DeviceWillBeRemoved += UnsubscribeToUpdates;
            _deviceSlotService.SessionInfoChanged += UpdateSessionStatus;
            _deviceSlotService.BatteryInfoChanged += UpdateBatteryStatus;
        }

        private EventHandler<CharacteristicUpdatedEventArgs> _batteryUpdateEventHandler;
        private EventHandler<CharacteristicUpdatedEventArgs> _infoUpdateEventHandler;

        private async void SubscribeToUpdates(object sender, SlotEventArgs args)
        {
            var slot = args.Slot;
            if (slot.Index != _slot.Index) return;

            var device = _slot.ConnectedDevice;
            if (device != null)
            {
                try
                {
                    _batteryUpdateEventHandler = new EventHandler<CharacteristicUpdatedEventArgs>((s, a) => SetBatteryStatus(s, a));
                    device.BatteryCharacteristic.ValueUpdated += _batteryUpdateEventHandler;

                    _infoUpdateEventHandler = new EventHandler<CharacteristicUpdatedEventArgs>((s, a) => SetSessionStatus(s, a));
                    device.InfoCharacteristic.ValueUpdated += _infoUpdateEventHandler;

                    _log.Info("Sending Get Info Command.");
                    var commandSuccessful = await SendCommand(CommandType.GetInfo);
                    if (commandSuccessful)
                    {
                        _log.Info("Command successfully sent.");
                    }

                    //BatteryLevel = -1;
                    //SessionStatus = -1;
                }
                catch (Exception)
                {
                    _log.Error("Characteristic subscription for data logging could not be completed.");
                }
            }
        }

        private void UnsubscribeToUpdates(object sender, SlotEventArgs args)
        {
            var slot = args.Slot;
            if (slot.Index != _slot.Index) return;

            var device = _slot.ConnectedDevice;
            if (device != null)
            {
                try
                {
                    device.BatteryCharacteristic.ValueUpdated -= _batteryUpdateEventHandler;
                    device.InfoCharacteristic.ValueUpdated -= _infoUpdateEventHandler;
                }
                catch (Exception)
                {
                    _log.Error("Characteristic unsubscription for data logging could not be completed.");
                }
                BatteryLevel = -1;
                SessionStatus = -1;
            }
        }

        public async Task<bool> SendCommand(CommandType commandType)
        {
            bool success = false;
            var device = _slot.ConnectedDevice;

            if (device == null)
            {
                _log.Debug($"Tried to send the command <{commandType}> but the device is set to null.");
                return success;
            }
            if (device.CommandCharacteristic == null)
            {
                _log.Debug($"Tried to send the command <{commandType}> but the device does not have a valid characteristic for it.");
                return success;
            }

            try
            {
                byte[] data = { (byte)commandType };
                _log.Debug($"Writing to characteristic command <{commandType}>.");
                success = await device.CommandCharacteristic.WriteAsync(data);
                _log.Debug($"Writing complete for characteristic command <{commandType}>.");
            }
            catch (Exception e)
            {
                _log.Debug($"Error: Characteristic write cannot be completed. {e.Message}");
            }
            return success;
        }

        private int _batteryLevel = -1;

        public int BatteryLevel
        {
            get => _batteryLevel;
            set
            {
                SetProperty(ref _batteryLevel, value, "BatteryLevel");
            }
        }

        private int _sessionStatus = -1;

        public int SessionStatus
        {
            get => _sessionStatus;
            set
            {
                SetProperty(ref _sessionStatus, value, "SessionStatus");
            }
        }

        public async void SetBatteryStatus(object sender, CharacteristicUpdatedEventArgs args)
        {
            byte[] data = args.Characteristic.Value;
            if (data.Length > 0)
            {
                var newBatteryLevel = (int)data[0];

                _slot.BatteryLevel = newBatteryLevel;
                BatteryLevel = newBatteryLevel;

                if (newBatteryLevel < 2)
                {
                    _userDialogs.Alert(new AlertConfig
                    {
                        Message = $"The device's battery level has reached critical levels (2%). Please charge the device. Disconnecting from the device."
                    });
                    await _bluetoothService.DisconnectDevice(_slot.ConnectedDevice);
                }
            }
        }

        private async void SetSessionStatus(object sender, CharacteristicUpdatedEventArgs args)
        {
            byte[] data = args.Characteristic.Value;
            _log.Debug("Parsing Session Info");

            if (data.Length == 6)
            {
                var newSessionStatus = data[4];
                _log.Debug($"Info Characteristic update. Session Status changed for slot {_slot.Index}: {SessionStatus} => {data[4]}");
                _slot.ConnectedDevice.IsRunningSession = (newSessionStatus == 0);

                _slot.SessionStatus = newSessionStatus;
                SessionStatus = newSessionStatus;

                var nand_flash_status = data[5];
                if (nand_flash_status != 0)
                {
                    _userDialogs.Alert(new AlertConfig
                    {
                        Message = $"The device's memory unit communication failed. Please check the device for mechanical problem. Disconnecting from the device."
                    });
                    await _bluetoothService.DisconnectDevice(_slot.ConnectedDevice);
                    
                }
            }
        }

        private void UpdateSessionStatus(object sender, SlotEventArgs args)
        {
            var slot = args.Slot;
            if (slot.Index != _slot.Index) return;

            _log.Debug($"Session status change detected in <DeviceSlotService>. Changing session status to {slot.SessionStatus}");
            SessionStatus = slot.SessionStatus;
        }

        private void UpdateBatteryStatus(object sender, SlotEventArgs args)
        {
            var slot = args.Slot;
            if (slot.Index != _slot.Index) return;

            _log.Debug($"Battery status change detected in <DeviceSlotService>. Changing session status to {slot.BatteryLevel}");
            BatteryLevel = slot.BatteryLevel;

            if (BatteryLevel < 2)
            {
                _userDialogs.Alert(new AlertConfig
                {
                    Message = $"The device's battery level has reached critical levels (2%). Please charge the device. Disconnecting from the device."
                });
                Task.Run(() => { Thread.Sleep(2000); _bluetoothService.DisconnectDevice(_slot.ConnectedDevice); });

            }
        }
    }
}
