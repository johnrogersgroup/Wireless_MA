// Based on the BLE.Client DeviceListViewModel in the Bluetooth LE plugin separated into a service context
// https://github.com/xabre/xamarin-bluetooth-le/blob/master/Source/BLE.Client/BLE.Client/ViewModels/DeviceListViewModel.cs

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Logging;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using UHMS.Core.Models.Bluetooth;
using UHMS.Core.Models.Data;

namespace UHMS.Core.Services
{
    /// <summary>
    /// Bluetooth Service. Manages current bluetooth state and connections to peripheral sensor devices.
    /// </summary>
    public partial class BluetoothService : IBluetoothService
    {
        private readonly ISensorDataService _sensorDataService;
        private readonly IDataLoggingService _dataLoggingService;
        private readonly IDeviceSlotService _deviceSlotService;

        private readonly IAdapter _adapter;
        private readonly IBluetoothLE _bluetooth;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxLog _log;

        public IBluetoothLE Bluetooth => _bluetooth;
        public IAdapter Adapter => _adapter;
        public bool IsScanning => _adapter.IsScanning;
        public bool IsOn => _bluetooth.IsOn;
        public string StateText => GetStateText();

        public List<DataType> SubscribedCharacteristicsList { get; set; }

        private volatile bool _isBusy = false;
        public bool IsBusy => _isBusy;

        /// <summary>
        /// Keeps track of events tied to characteristics to properly debind them later.
        /// </summary>
        private readonly ConcurrentDictionary<ICharacteristic, EventHandler<CharacteristicUpdatedEventArgs>> _characteristicEventAggregator;

        //private EventHandler<CharacteristicUpdatedEventArgs> _infoUpdateEventHandler;

        public List<int> OutputData { get; }

        private const string IMU_MEAS_CHARACTERISTIC = "05791301-78c9-481e-b5ee-1866f83eda7b";
        private const string INFO_CHARACTERISTIC = "05791001-78c9-481e-b5ee-1866f83eda7b";
        private const string CMD_CHARACTERISTIC = "05791002-78c9-481e-b5ee-1866f83eda7b";
        private const string DATA_CHARACTERISTIC = "05791003-78c9-481e-b5ee-1866f83eda7b";

        private const string BATTERY_UUID = "2a19";
        /// <summary>
        /// The valid characteristics.
        /// </summary>
        private HashSet<string> _validCharacteristicUuids = new HashSet<string>
        {
            "2a37", // accl
            IMU_MEAS_CHARACTERISTIC,
            BATTERY_UUID,
            INFO_CHARACTERISTIC,
            CMD_CHARACTERISTIC, // Cmd Characteristic
            DATA_CHARACTERISTIC,
        };

        /// <summary>
        /// Caracteristic UUIDs and their corresponding data types.
        /// </summary>
        /// <remarks>
        /// 2a18 = GlucoseMeasurement.
        /// 2a37 = HeartRateMeasurement.
        /// 2a1c = TemperatureMeasurement.
        /// 2a49 = BloodPressureFeature.
        /// </remarks>
        private Dictionary<string, DataType> _dataTypeForCharacteristicUUID = new Dictionary<string, DataType>()
        {
            { "2a37",  DataType.accl },
            { IMU_MEAS_CHARACTERISTIC,  DataType.accl }
        };


        private Dictionary<DataType, HashSet<DataType>> _dataTypeToOutputType = new Dictionary<DataType, HashSet<DataType>>()
        {
            { DataType.accl, new HashSet<DataType>{DataType.accl_x, DataType.accl_y, DataType.accl_z } },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UHMS.Core.Services.BluetoothService"/> class.
        /// </summary>
        /// <param name="sensorDataService">Handler for incoming sensor data.</param>
        /// <param name="adapter">Adapter used to manage the peripheral bluetooth connections.</param>
        /// <param name="bluetooth">Interface to the central bluetooth device.</param>
        /// <param name="userDialogs">Dialogs to manage user-oriented output to the UI.</param>
        /// <param name="log">Logger primarily for debugging and record purposes.</param>
        public BluetoothService(ISensorDataService sensorDataService, IDataLoggingService dataLoggingService, IDeviceSlotService deviceSlotService, IAdapter adapter, IBluetoothLE bluetooth, IUserDialogs userDialogs, IMvxLog log)
        {

            // Services
            _sensorDataService = sensorDataService;
            _dataLoggingService = dataLoggingService;
            _deviceSlotService = deviceSlotService;

            _adapter = adapter;
            _bluetooth = bluetooth;
            _userDialogs = userDialogs;
            _log = log;

            _log.Info("Initializing Bluetooth Service.");

            // Event tracker for subscribed characteristics.
            _characteristicEventAggregator = new ConcurrentDictionary<ICharacteristic, EventHandler<CharacteristicUpdatedEventArgs>>();

            OutputData = new List<int>();

            SubscribedCharacteristicsList = new List<DataType>();
        }

        /// <summary>
        /// Gets the user appropriate status message of the current bluetooth state.
        /// </summary>
        /// <returns>String message of the current bluetooth state.</returns>
        private string GetStateText()
        {
            switch (_bluetooth.State)
            {
                case BluetoothState.Unknown:
                    return "Unknown Bluetooth state.";
                case BluetoothState.Unavailable:
                    return "Bluetooth is not available on this device.";
                case BluetoothState.Unauthorized:
                    return "You do not have the permissions to use Bluetooth.";
                case BluetoothState.TurningOn:
                    return "Bluetooth is warming up, please wait.";
                case BluetoothState.On:
                    return "Bluetooth is on.";
                case BluetoothState.TurningOff:
                    return "Bluetooth is turning off.";
                case BluetoothState.Off:
                    return "Bluetooth is off.";
                default:
                    return "Unknown BLE state.";
            }
        }

        public bool CharacteristicIsActive(DataType type)
        {
            return SubscribedCharacteristicsList.Contains(type);
        }

        /// <summary>
        /// Gets the available characteristics.
        /// </summary>
        /// <returns>The available characteristics.</returns>
        /// <param name="sensorDevice">Sensor device.</param>
        public async Task<List<ICharacteristic>> GetAvailableCharacteristics(SensorDevice sensorDevice)
        {
            var discoveredCharacteristics = new List<ICharacteristic>();

            // Attempt to get characteristics populated in the adapter device.
            var services = await sensorDevice.Device.GetServicesAsync();
            foreach (var service in services)
            {
                var characteristics = await service.GetCharacteristicsAsync();
                foreach (var characteristic in characteristics)
                {
                    _log.Debug(characteristic.Name + " " + characteristic.Uuid);
                    try
                    {
                        // Filter out only the ones we are interested in. 
                        if (_validCharacteristicUuids.Contains(characteristic.Uuid))
                        {
                            // Change from characteristic to internal DataType.
                            discoveredCharacteristics.Add(characteristic);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Debug(e.Message);
                    }
                }
            }
            return discoveredCharacteristics;
        }

        /// <summary>
        /// Bindable event handler to get raw streaming data from the sensor devices.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="device">Sensor Device.</param>
        private void CharacteristicOnValueUpdated(object sender, CharacteristicUpdatedEventArgs args, SensorDevice device)
        {
            _sensorDataService.DataUpdate(args.Characteristic.Value, _dataTypeForCharacteristicUUID[args.Characteristic.Uuid], device);
        }

        public async Task<bool> ConnectDevice(SensorDevice sensorDevice, CancellationTokenSource tokenSource, int slotIndex = -1)
        {
            _isBusy = true;
            // Cannot connect to an already connected device or devices that is neither master or slave.
            if (sensorDevice.IsConnected || sensorDevice.Type == DeviceType.None)
                return false;

            _log.Info("Connecting to a device.");

            if (_deviceSlotService.AllSlotsAreFull)
            {
                _userDialogs.Alert(new AlertConfig
                {
                    Message = $"The device list is full. Please disconnect a device before attempting to connect another one."
                });
                _isBusy = false;
                return false;
            }

            DeviceSlot emptySlot;
            if (slotIndex > -1)
            {
                emptySlot = _deviceSlotService.DeviceSlots[slotIndex];
            }
            else
            {
                emptySlot = _deviceSlotService.GetEmptySlot(sensorDevice.Type);
            }

            if (emptySlot == null)
            {
                _userDialogs.Alert(new AlertConfig
                {
                    Message = $"Cannot connect to more devices of this type."
                });
                _isBusy = false;
                return false;
            }

            _log.Debug($"Connecting to {sensorDevice.Type}");
            // Attempt to connect to device.
            int timeout = 4000; // 4 second timeout
            var task = _adapter.ConnectToDeviceAsync(device: sensorDevice.Device, cancellationToken: tokenSource.Token);
            try
            {
                if (await Task.WhenAny(task, Task.Delay(timeout)) != task)
                {
                    tokenSource.Cancel();
                    _userDialogs.Alert(new AlertConfig
                    {
                        Message = $"Connection to {sensorDevice.Name} timed out."
                    });
                    _isBusy = false;
                    return false;
                }
            }
            catch (DeviceConnectionException)
            {
                _userDialogs.Alert(new AlertConfig
                {
                    Message = $"Could not connect to {sensorDevice.Name}."
                });
                _isBusy = false;
                return false;
            }

            // Clear the Last active output data types list.
            _sensorDataService.RecentOutputAdditions.Clear();

            try
            {
                // TODO: Add file service for sensor data logging.
                sensorDevice.Characteristics = await GetAvailableCharacteristics(sensorDevice);


                _log.Debug($"New {sensorDevice.Type} Device Address: " + sensorDevice.Id);

                if (sensorDevice.Characteristics.Count == 0)
                {
                    _userDialogs.Alert(new AlertConfig
                    {
                        Message = $"Error: No valid characteristics were detected for {sensorDevice.Name}. Disconnecting from the device."
                    });

                    await DisconnectDevice(sensorDevice);
                    _isBusy = false;
                    return false;
                }
                // Subscribe to data characteristics
                foreach (var characteristic in sensorDevice.Characteristics)
                {
                    if (characteristic.Uuid == CMD_CHARACTERISTIC)
                    {
                        sensorDevice.CommandCharacteristic = characteristic;
                    }
                    else if (characteristic.Uuid == DATA_CHARACTERISTIC)
                    {
                        sensorDevice.DownloadDataCharacteristic = characteristic;
                        await sensorDevice.DownloadDataCharacteristic.StartUpdatesAsync();
                    }
                    else if (characteristic.Uuid == INFO_CHARACTERISTIC)
                    {
                        sensorDevice.InfoCharacteristic = characteristic;
                        await sensorDevice.InfoCharacteristic.StartUpdatesAsync();
                    }
                    else if (characteristic.Uuid == BATTERY_UUID)
                    {
                        sensorDevice.BatteryCharacteristic = characteristic;


                        byte[] battery_pkg = await sensorDevice.BatteryCharacteristic.ReadAsync();
                        _deviceSlotService.UpdateBatteryStatus(emptySlot.Index, battery_pkg[0]);

                        await sensorDevice.BatteryCharacteristic.StartUpdatesAsync();
                    }
                    else if (characteristic.CanUpdate)
                    {
                        var datatype = _dataTypeForCharacteristicUUID[characteristic.Uuid];

                        DataType tempDataType = datatype;
                        // Necessary temp conversion due to DataType clash in different chest and limb devices.
                        if (datatype == DataType.temp)
                        {
                            if (sensorDevice.Type == DeviceType.Chest) tempDataType = DataType.chest_temp;
                            else if (sensorDevice.Type == DeviceType.Limb) tempDataType = DataType.foot_temp;
                        }
                        if (!_dataTypeToOutputType.ContainsKey(tempDataType)) continue;
                        foreach (var outputType in _dataTypeToOutputType[tempDataType])
                        {
                            var outputTuple = Tuple.Create(emptySlot.Index, outputType);
                            int outputID = outputTuple.GetHashCode();
                            OutputData.Add(outputID);
                            emptySlot.OutputDataIDs.Add(outputID);
                            _sensorDataService.AddToDataBuffer(outputID);

                            SubscribedCharacteristicsList.Add(outputType);
                            _sensorDataService.RecentOutputAdditions.Add(outputTuple);
                        }

                        var func = new EventHandler<CharacteristicUpdatedEventArgs>((sender, args) => CharacteristicOnValueUpdated(sender, args, sensorDevice));
                        _characteristicEventAggregator.TryAdd(characteristic, func);
                        // Cleanup previous event handler.

                        characteristic.ValueUpdated -= func;
                        // Attach event handler to characteristic value update.
                        characteristic.ValueUpdated += func;

                        // Start Getting Sensor Data.
                        await characteristic.StartUpdatesAsync();
                    }

                }
                _deviceSlotService.AddDeviceToSlot(emptySlot.Index, sensorDevice);
            }
            catch (Exception e)
            {
                _log.Debug(e.Message);
            }

            _isBusy = false;
            return true;
        }

        public async Task<bool> DisconnectDevice(SensorDevice sensorDevice)
        {
            // Check if correct device reference was provided.
            if (sensorDevice == null) return false;

            _isBusy = true;

            // Get type of device.
            DeviceType deviceType = sensorDevice.Type;

            // Clear connected device.
            DeviceSlot occupingSlot = _deviceSlotService.GetSlotWithDevice(sensorDevice);
            if (occupingSlot == null)
            {
                _isBusy = false;
                return false;
            }

            foreach (var outputID in occupingSlot.OutputDataIDs)
            {
                OutputData.Remove(outputID);
                _sensorDataService.RemoveFromDataBuffer(outputID);
            }

            _deviceSlotService.EmptySlot(occupingSlot.Index);
            await _adapter.DisconnectDeviceAsync(sensorDevice.Device);

            _isBusy = false;
            return true;
        }

        public void OnConnected(SensorDevice sensorDevice)
        {
            _log.Info($"Device {sensorDevice.Name} has connected.");
        }

        public void OnDisconnected(SensorDevice sensorDevice, bool showMessage = false)
        {
            // Check if a valid sensor device model was referenced.
            if (sensorDevice == null)
                return;


            _isBusy = true;
            // Cleanup refrences for a device.
            if (sensorDevice.Type != DeviceType.None)
            {
                if (sensorDevice.Characteristics == null)
                {
                    _isBusy = false;
                    return;
                }

                // Cleanup data update listeners for characteristics.
                foreach (ICharacteristic characteristic in sensorDevice.Characteristics)
                {
                    try
                    {
                        if (_dataTypeForCharacteristicUUID.ContainsKey(characteristic.Uuid))
                        {
                            DataType dataType = _dataTypeForCharacteristicUUID[characteristic.Uuid];
                            DataType tempDataType = dataType;


                            // Necessary temp conversion due to DataType clash in different chest and limb devices.
                            if (dataType == DataType.temp)
                            {
                                if (sensorDevice.Type == DeviceType.Chest) tempDataType = DataType.chest_temp;
                                else if (sensorDevice.Type == DeviceType.Limb) tempDataType = DataType.foot_temp;
                            }

                            if (!_dataTypeToOutputType.ContainsKey(tempDataType)) continue;
                            foreach (var outputType in _dataTypeToOutputType[tempDataType])
                            {
                                SubscribedCharacteristicsList.Remove(outputType);
                            }
                        }

                        // Attempt to remove event handler.
                        _characteristicEventAggregator.TryGetValue(characteristic, out EventHandler<CharacteristicUpdatedEventArgs> CharacteristicOnValueUpdated);
                        characteristic.ValueUpdated -= CharacteristicOnValueUpdated;

                        // Attempt to remove reference to the event handler.
                        _characteristicEventAggregator.TryRemove(characteristic, out CharacteristicOnValueUpdated);
                    }
                    catch (Exception e)
                    {
                        _log.Debug("Characteristic unsubscription error: " + e.Message + characteristic.Name);
                    }

                }

                //if (sensorDevice.InfoCharacteristic != null)
                //{
                //    sensorDevice.InfoCharacteristic.ValueUpdated -= _infoUpdateEventHandler;
                //}

                // Clear out the corresponding device slot for connected devices.
                DeviceSlot occupingSlot = _deviceSlotService.GetSlotWithDevice(sensorDevice);
                if (occupingSlot == null)
                {
                    _isBusy = false;
                    return;
                }

                foreach (var outputID in occupingSlot.OutputDataIDs)
                {
                    OutputData.Remove(outputID);
                    _sensorDataService.RemoveFromDataBuffer(outputID);
                }

                // StopSession();

                _deviceSlotService.EmptySlot(occupingSlot.Index);
                _isBusy = false;
            }

            if (showMessage)
            {
                // If disconnection was unexpected.
                _log.Info($"Connection to {sensorDevice.Name} has been lost.");
                _userDialogs.Alert(new AlertConfig
                {
                    Message = "Connection to " + sensorDevice.Name + " has been lost."
                });
            }
            else
            {
                // If disconnection was made by user.
                _log.Info($"Device {sensorDevice.Name} has disconnected.");
            }

        }

        public DeviceType DiscoverDeviceType(IDevice sensorDevice)
        {
            // Type of device to return.
            DeviceType deviceType = DeviceType.None;

            // Get advertisement record of manufacturer specific data.
            var rec = sensorDevice
                    .AdvertisementRecords
                    .FirstOrDefault(advRec => advRec.Type == AdvertisementRecordType.ManufacturerSpecificData);

            // If record exists, check for device information.
            if (rec != null)
            {
                // First two byte contains manufacturer data.
                // From third value, it contains specific data which determines type of device.
                if (rec.Data.Length >= 2 && rec.Data[0] == 0xff && rec.Data[1] == 0xff)
                {
                    var deviceTypeIdentifier = rec.Data[2];
                    switch (deviceTypeIdentifier)
                    {
                        case 0:
                            deviceType = DeviceType.Chest;
                            break;
                        case 1:
                            deviceType = DeviceType.Limb;
                            break;
                        case 3:
                            deviceType = DeviceType.CP;
                            break;
                        case 4:
                            deviceType = DeviceType.Stroke;
                            break;
                        default:
                            deviceType = DeviceType.None;
                            break;
                    }
                }
            }
            return deviceType;
        }

        private bool _isSendingCommand = false;
        private DateTime _latestCmdSent = DateTime.MinValue;


        public bool StartSession(long epochTime)
        {
            if (_isSendingCommand) return false;

            bool sentCommandRecently = (DateTime.Now - _latestCmdSent).Seconds < 1;
            if (sentCommandRecently) return false;

            _log.Debug("Starting device session with command.");
            _isSendingCommand = true;
            for (int i = 0; i < _deviceSlotService.DeviceSlots.Count; i++)
            {
                var slot = _deviceSlotService.DeviceSlots[i];

                // Send stop command for good measure.
                byte[] cmdData = { (byte)CommandType.StopSession };

                _latestCmdSent = DateTime.Now;
                //Task.Run(() => SendCommand(slot, cmdData));

                byte[] epochBytes = BitConverter.GetBytes(epochTime);

                if (!slot.IsEmpty && !slot.IsBusy)
                {
                    _log.Debug($"Sending command to device {slot.ConnectedDevice.Name}");
                    _log.Debug($"Epoch Time: {epochTime}, bytes:{ BitConverter.ToString(epochBytes)}");

                    cmdData = new byte[6];
                    cmdData[0] = (byte)CommandType.StartSession;
                    cmdData[1] = epochBytes[0];
                    cmdData[2] = epochBytes[1];
                    cmdData[3] = epochBytes[2];
                    cmdData[4] = epochBytes[3];
                    cmdData[5] = (byte) slot.Index;
                    Task.Run(() => SendCommand(slot, cmdData));
                    _deviceSlotService.UpdateSessionStatus(slot.Index, 1);
                }
            }
            _isSendingCommand = false;
            return true;
        }

        public bool StopSession()
        {
            if (_isSendingCommand) return false;

            bool sentCommandRecently = (DateTime.Now - _latestCmdSent).Seconds < 1;
            if (sentCommandRecently) return false;

            _log.Debug("Stopping device session with command.");
            _isSendingCommand = true;
            for (int i = 0; i < _deviceSlotService.DeviceSlots.Count; i++)
            {
                var slot = _deviceSlotService.DeviceSlots[i];
                if (!slot.IsEmpty)
                {
                    byte[] cmdData = { (byte)CommandType.StopSession };
                    _latestCmdSent = DateTime.Now;
                    Task.Run(() => SendCommand(slot, cmdData));
                    _deviceSlotService.UpdateSessionStatus(slot.Index, 0);
                }
            }
            _isSendingCommand = false;


            return true;
        }

        private async Task<bool> SendCommand(DeviceSlot slot, byte[] data)
        {
            bool success = false;
            if (slot.IsEmpty) return success;

            if (slot.ConnectedDevice.CommandCharacteristic == null)
            {
                _userDialogs.Alert(new AlertConfig
                {
                    Message = $"Error: The session command function is not available for this device."
                });
            }

            try
            {
                success = await slot.ConnectedDevice.CommandCharacteristic.WriteAsync(data);
            }
            catch (Exception e)
            {
                _log.Debug($"Error: Characteristic write cannot be completed. {e.Message}");
            }
            return success;
        }
    }
}
