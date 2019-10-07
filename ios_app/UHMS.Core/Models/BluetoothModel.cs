using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Diagnostics;
using System.Linq;
using System.Collections.Concurrent;
using MvvmCross.ViewModels;
using MvvmCross.Commands;
//using MvvmCross.Commands;

namespace UHMS.Core.Models.Old {
    public enum DataType { None, ecg, scg, red, temp, foot_temp, chest_temp, ppg, ir, accl }
    public enum DeviceType { None, Master, Slave };

    public partial class BluetoothModel : IBluetoothModel {
        public IAdapter _adapter { get; set; }
        public IBluetoothLE _bluetooth { get; set; }
        public IUserDialogs _userDialogs { get; set; }
        public IFileManagementModel _fileManagementModel { get; set; }

        static Thread DataWriter1;
        static Thread DataWriter2;
        private static Queue<DataProfile> queue1 = new Queue<DataProfile>();
        private static Queue<DataProfile> queue2 = new Queue<DataProfile>();

        public ConcurrentDictionary<DeviceType, IDevice> ConnectedDevices { get; set; }
        //public Collection<DeviceProfile> ScannedDevices { get; set; }

        public static Dictionary<DataType, UInt32> DataTime { get; set; }
        public ConcurrentDictionary<string, Collection<DataProfile>> DataCollection { get; set; }

        static Dictionary<DataType, uint> DataPoints = new Dictionary<DataType, uint> { { DataType.ecg, 500 }, { DataType.scg, 100 }, { DataType.foot_temp, 10 }, { DataType.chest_temp, 10 }, { DataType.ir, 100 }, { DataType.red, 100 } };
        static Dictionary<DataType, uint> SamplingRate = new Dictionary<DataType, uint>() { { DataType.ecg, 5 }, { DataType.scg, 1 }, { DataType.foot_temp, 1 }, { DataType.chest_temp, 1 }, { DataType.ir, 1 }, { DataType.red, 1 } };
        bool initialized = false;

        private double MasterTemp;
        private double SlaveTemp;

        public static readonly Collection<string> MasterCharacteristicUuids = new Collection<string> { "2a37", "2a18", "2a1c" };
        public static readonly Collection<string> SlaveCharacteristicUuids = new Collection<string> { "2a49", "2a1c" };

        //public static Dictionary<DeviceType, Collection<DataType>> DataTypesAvailable = new Dictionary<DeviceType, Collection<DataType>> {
            //    { DeviceType.Master, new Collection<DataType> {DataType.ecg, DataType.scg, DataType.chest_temp }},
            //    { DeviceType.Slave, new Collection<DataType> { DataType.ir, DataType.red, DataType.foot_temp }},
            //};

        public static readonly Collection<DataType> DataTypesOnNewSession = new Collection<DataType> {
                DataType.ecg, DataType.accl, DataType.ppg, DataType.chest_temp, DataType.foot_temp
            };

        public BluetoothModel(IAdapter adapter, IBluetoothLE bluetooth, IUserDialogs userDialogs, IFileManagementModel fileManagementModel) {
            if (!initialized) {
                initialized = true;
                _adapter = adapter;
                _bluetooth = bluetooth;
                _userDialogs = userDialogs;
                _fileManagementModel = fileManagementModel;

                //_adapter.DeviceDiscovered += (s, d) => { AddToDevices(new DeviceProfile(d.Device, DeviceType.None, 0, _adapter.ConnectedDevices.Contains(d.Device), _adapter, _bluetooth, this)); };
                //_adapter.DeviceConnected += (s, d) => {
                //    Debug.WriteLine("asdf");
                //};
                //_adapter.DeviceDisconnected += (s, d) => {
                //    Debug.WriteLine("asdf");
                //    //Debug.WriteLine(d.Device.Id);
                //    //OnDisconnected(ConnectedDevices.FirstOrDefault(devicePair => devicePair.Value.Device.Id == d.Device.Id).Value);
                //    //OnDisconnected(disconnectedDevice.Value);
                //};
                _adapter.DeviceConnectionLost += (s, d) => {
                    OnDisconnected(d.Device, true);
                };
                _adapter.DeviceDisconnected += (s, d) => {
                    OnDisconnected(d.Device, false);
                };
                ConnectedDevices = new ConcurrentDictionary<DeviceType, IDevice>();
                ConnectedDevices.TryAdd(DeviceType.Master, null);
                ConnectedDevices.TryAdd(DeviceType.Slave, null);

                //ScannedDevices = new ObservableCollection<DeviceProfile>();
                DataTime = new Dictionary<DataType, uint>();
                DataCollection = new ConcurrentDictionary<string, Collection<DataProfile>>();
                foreach (DataType type in Enum.GetValues(typeof(DataType))) {
                    try {
                        //TODO[UHMS - 84]: DataList.Add(type, new MvxObservableCollection<DataProfile>());
                        DataCollection.TryAdd("DataType." + type.ToString(), new MvxObservableCollection<DataProfile>());

                        DataTime.Add(type, 0);
                        //DataListString["DataType." + type.ToString()] = DataListMobile[type];
                        for (uint i = 0; i < BluetoothModel.DataPoints[type] * BluetoothModel.SamplingRate[type]; i++) {
                            DataCollection["DataType." + type.ToString()].Add(new DataProfile(type, i, Double.NaN));
                        }

                        Debug.WriteLine("length: " + DataCollection["DataType." + type.ToString()].Count + "," + DataCollection["DataType." + type.ToString()].Count);
                        Debug.WriteLine("DataType." + type.ToString());
                    } catch (Exception ex) {
                        Debug.WriteLine("Exception :" + type.ToString() + ex.Message);
                    }
                }
                foreach (var col in DataCollection.Keys) {
                    Debug.WriteLine(col);
                }
                DataWriter1 = new Thread(() => PopAndWrite(queue1));
                DataWriter2 = new Thread(() => PopAndWrite(queue2));
                DataWriter1.Start(); DataWriter2.Start();
            }
        }

        //private void AddToDevices(DeviceProfile device) { ScannedDevices.Add(device); }

        public async Task TryScanningAsync() {
            string bluetoothStateMessage = null;
            switch (_bluetooth.State) {
                case BluetoothState.Off:
                    bluetoothStateMessage = "Off";
                    break;
                case BluetoothState.TurningOff:
                    bluetoothStateMessage = "TurningOff";
                    break;
                case BluetoothState.Unauthorized:
                    bluetoothStateMessage = "Unauthorized";
                    break;
                case BluetoothState.Unavailable:
                    bluetoothStateMessage = "Unavailable";
                    break;
                case BluetoothState.Unknown:
                    bluetoothStateMessage = "Unkown";
                    break;
            }
            if (bluetoothStateMessage != null) {
                _userDialogs.Alert("Bluetooth is " + bluetoothStateMessage + ". Check bluetooth status.");
                return;
            }
            try {
                await StartScanningDevicesAsync();
            } catch (Exception e) {
                _userDialogs.Alert("Error occurred: " + e.Message);
            }
        }

        public async Task StartScanningDevicesAsync() {
            await _adapter.StopScanningForDevicesAsync();
            //_adapter.DiscoveredDevices.Clear();

            foreach (var device in ConnectedDevices.Values) {
                if (device != null)
                    _adapter.DiscoveredDevices.Add(device);
            }
            await _adapter.StartScanningForDevicesAsync();
        }

        public async Task AttemptConnectOrDispose(IDevice device) {
            if (!_bluetooth.IsAvailable)
                _userDialogs.Alert("Check your device. Ble is not available.");
            else if (!_bluetooth.IsOn)
                _userDialogs.Alert("Turn on your bluetooth.");

            if (device == null)
                return;

            var connectionStatus = _adapter.ConnectedDevices.Contains(device);

            Debug.WriteLine("Attempting to " + (connectionStatus ? "disconnect " : "connect to ") + device.Name);
            if (!connectionStatus) {
                //if should connect.
                try {
                    var cancelSrc = new CancellationTokenSource();
                    var config = new ProgressDialogConfig()
                        .SetTitle("Trying to establish connection with device...")
                        .SetIsDeterministic(false)
                        .SetMaskType(MaskType.None)
                        .SetCancel(onCancel: cancelSrc.Cancel);
                    using (_userDialogs.Progress(config)) {
                        bool result = false;
                        await Task.Run(async () => {
                            result = await ConnectToDevice(device);
                        }, cancelSrc.Token);
                        if (!cancelSrc.IsCancellationRequested && result) {
                            _userDialogs.Alert("Connected to Device", "Success", "Ok");
                        } else
                            _userDialogs.Alert("Error occurred", "Fail", "Ok");
                    }
                } catch (Exception e) {
                    var res = await DisconnectDevice(device);
                    _userDialogs.Alert(new AlertConfig {
                        Title = "Error Occurred",
                        Message = "If this event occurred, please notify developers with following message.\n(Connect)" + e.Message + "."
                    });
                }
            } else {
                //cases when it should disconnect
                try {
                    var result = await DisconnectDevice(device);
                    if (result) {
                        _userDialogs.Alert(new AlertConfig {
                            Title = "Successful",
                            Message = "Successfully disconnected."
                        });
                    } else {
                        _userDialogs.Alert(new AlertConfig {
                            Title = "Failed",
                            Message = "Failed to disconnect following device: " + device.Name
                        });
                    }
                } catch (Exception e) {
                    _userDialogs.Alert(new AlertConfig {
                        Title = "Error Occurred",
                        Message = "If this event occurred, please notify developers with following message.\n(Disconnect)" + e.Message + "."
                    });
                }
            }
        }

        public async Task<DeviceType> CheckDeviceType(IDevice device) {
            // Check if device is already connected to device.
            // Then create flag that indicates function to stay connected or disconnect at the end.
            bool disconnectAfterCheck = false;
            if (!_adapter.ConnectedDevices.Contains(device)) {
                await _adapter.ConnectToDeviceAsync(device);
                disconnectAfterCheck = true;
            }

            var discoveredCharacteristics = await GetAvailableCharacteristics(device);

            DeviceType deviceType = DeviceType.None;
            Collection<string> deviceCharacteristicUuids = new Collection<string>();
            foreach (var characteristic in discoveredCharacteristics.Values) {
                deviceCharacteristicUuids.Add(characteristic.Uuid);
            }
            bool isMaster = BluetoothModel.MasterCharacteristicUuids.All(s => deviceCharacteristicUuids.Contains(s));
            bool isSlave = BluetoothModel.SlaveCharacteristicUuids.All(s => deviceCharacteristicUuids.Contains(s));
            if (isMaster || isSlave) {
                if (isMaster)
                    deviceType = DeviceType.Master;
                else if (isSlave)
                    deviceType = DeviceType.Slave;
            }
            if (disconnectAfterCheck)
                await _adapter.DisconnectDeviceAsync(device);
            if (deviceType == DeviceType.None)
                return DeviceType.None;

            return deviceType;
        }

        public async Task<Dictionary<DataType, ICharacteristic>> GetAvailableCharacteristics(IDevice device) {
            // Check if device is already connected to device.
            // Then create flag that indicates function to stay connected or disconnect at the end.
            bool disconnectAfterCheck = false;
            if (!_adapter.ConnectedDevices.Contains(device)) {
                await _adapter.ConnectToDeviceAsync(device);
                disconnectAfterCheck = true;
            }

            Dictionary<DataType, ICharacteristic> discoveredCharacteristics = new Dictionary<DataType, ICharacteristic>();
            var services = await device.GetServicesAsync();
            foreach (var service in services) {
                var characteristics = await service.GetCharacteristicsAsync();
                foreach (var characteristic in characteristics) {
                    if (characteristic.CanUpdate) {
                        Debug.WriteLine(characteristic.Name + " " + characteristic.Uuid);
                        try {
                            discoveredCharacteristics.Add(CharacteristicToDataTypeTranslator(characteristic), characteristic);
                        } catch (Exception e) {
                            Debug.WriteLine(e.Message);
                        }
                    }
                }
            }
            if (disconnectAfterCheck)
                await _adapter.DisconnectDeviceAsync(device);
            return discoveredCharacteristics;
        }

        public async Task<bool> ConnectToDevice(IDevice device) {
            if (!_adapter.DiscoveredDevices.Contains(device))
                return false;
            Debug.WriteLine("asdf1");
            await _adapter.ConnectToDeviceAsync(device);
            DeviceType deviceType = await CheckDeviceType(device);
            Debug.WriteLine("\nPrevious Master Address: " + ConnectedDevices[DeviceType.Master]?.Id);
            Debug.WriteLine("Previous Slave Address: " + ConnectedDevices[DeviceType.Slave]?.Id);
            Debug.WriteLine("Device Type: " + deviceType);
            var discoveredCharacteristics = await GetAvailableCharacteristics(device);
            Debug.WriteLine("asdf2");
            string type = deviceType == DeviceType.Master ? "chest" : "foot";
            bool response = true;
            if (ConnectedDevices[deviceType] != null) {
                response = await _userDialogs.ConfirmAsync(new ConfirmConfig {
                    Message = "There is already " + type + "device connected.",
                    Title = "Device conflict",
                    OkText = "Yes, connect this device.",
                    CancelText = "No, use previous device connected."
                });
            }
            if (response) {
                Debug.WriteLine("asdf3");
                if (ConnectedDevices[deviceType] != null)
                    await DisconnectDevice(ConnectedDevices[deviceType]);
                try {
                    bool deviceExists = false;
                    if (ConnectedDevices[DeviceType.Master] != null || ConnectedDevices[DeviceType.Slave] != null)
                        deviceExists = true;
                    await _fileManagementModel.CreateNewLogFile(deviceExists);
                    ConnectedDevices[deviceType] = device;

                    Debug.WriteLine("New Master Address: " + ConnectedDevices[DeviceType.Master]?.Id);
                    Debug.WriteLine("New Slave Address: " + ConnectedDevices[DeviceType.Slave]?.Id + "\n");
                    foreach (var characteristic in discoveredCharacteristics.Values) {
                        if (characteristic.CanUpdate) {
                            await characteristic.StartUpdatesAsync();
                            void func(object o, CharacteristicUpdatedEventArgs args) => DataUpdate(args.Characteristic.Value, CharacteristicToDataTypeTranslator(args.Characteristic), deviceType);
                            characteristic.ValueUpdated += func;
                        }
                    }
                } catch (Exception e) {
                    Debug.WriteLine(e.Message);
                }
            }

            return true;
        }

        public async Task<bool> DisconnectDevice(IDevice device) {
            if (device == null)
                return false;

            DeviceType deviceType = DeviceType.None;
            foreach (var type in ConnectedDevices) {
                if (type.Value != null) {
                    if (device.Id == type.Value.Id) {
                        deviceType = type.Key;
                        break;
                    }
                }
            }

            var services = await device.GetServicesAsync();
            foreach (var service in services) {
                var characteristics = await service.GetCharacteristicsAsync();
                foreach (var characteristic in characteristics) {
                    try {
                        await characteristic.StopUpdatesAsync();
                        if (characteristic.CanUpdate) {
                            void func(object o, CharacteristicUpdatedEventArgs args) => DataUpdate(args.Characteristic.Value, CharacteristicToDataTypeTranslator(args.Characteristic), deviceType);
                            characteristic.ValueUpdated -= func;
                        }
                    } catch (Exception e) {
                        Debug.WriteLine("Characteristic unsubscription error: " + e.Message + characteristic.Name);
                    }
                }
            }

            await _adapter.DisconnectDeviceAsync(device);

            return true;
        }

        public void OnConnected(IDevice device) {

        }

        public void OnDisconnected(IDevice device, bool showMessage) {
            if (device == null)
                return;

            DeviceType type = DeviceType.None;

            foreach (var typeDevicePair in ConnectedDevices)
                if (device.Id == typeDevicePair.Value?.Id)
                    type = typeDevicePair.Key;

            if (type != DeviceType.None) {
                foreach (var dataType in DataTypesAvailable[type]) {
                    DataCollection["DataType." + dataType.ToString()].Clear();
                    for (uint i = 0; i < DataPoints[dataType] * SamplingRate[dataType]; i++)
                        DataCollection["DataType." + dataType.ToString()].Add(new DataProfile(dataType, i, Double.NaN));
                    DataTime[dataType] = 0;
                }
                ConnectedDevices[type] = null;
            }

            if (showMessage) {
                _userDialogs.Alert(new AlertConfig {
                    Message = "Connection to " + device.Name + " has been lost."
                });
            }
        }

        public void DataUpdate(byte[] data, DataType type, DeviceType senderDeviceType) {
            List<Collection<DataProfile>> dataCollection = new List<Collection<DataProfile>>();
            dataCollection.Add(new Collection<DataProfile>());
            if (senderDeviceType == DeviceType.Master) {
                if (type == DataType.temp) {
                    var raw = data[0] << 8 | data[1];
                    var temp = raw * 0.00390625;
                    temp = (int)(temp * 10);
                    temp = temp * .1;
                    MasterTemp = temp;
                    Debug.WriteLine("Master Temperature" + MasterTemp + "\t Time: " + DateTime.Now);
                    dataCollection[0].Add(new DataProfile(DataType.chest_temp, DataTime[DataType.chest_temp]++, temp));
                    //DataTime[DataType.masterTemp]++; DataTime[DataType.slaveTemp]++;
                } else if (type == DataType.scg) {
                    for (int i = 0; i < 3; i++) {
                        dataCollection[0].Add(new DataProfile(type, DataTime[type]++, (UInt16)(data[6 * i + 1] << 8 | data[6 * i])));
                        dataCollection[0].Add(new DataProfile(type, DataTime[type]++, (UInt16)(data[6 * i + 3] << 8 | data[6 * i + 2])));
                        dataCollection[0].Add(new DataProfile(type, DataTime[type]++, (UInt16)(data[6 * i + 5] << 8 | data[6 * i + 4])));
                    }
                } else if (type == DataType.ecg) {
                    for (int i = 0; i < 10; i++)
                        dataCollection[0].Add(new DataProfile(type, DataTime[type]++, (UInt16)(data[2 * i + 1] | data[2 * i] << 8)));
                } else {
                    Debug.WriteLine("data leak");
                }
            } else if (senderDeviceType == DeviceType.Slave) {
                if (type == DataType.temp) {
                    var raw = data[0] << 8 | data[1];
                    var temp = raw * 0.00390625;
                    temp = (int)(temp * 10);
                    temp = temp * .1;
                    SlaveTemp = temp;
                    Debug.WriteLine("Slave Temperature" + SlaveTemp + "\t Time: " + DateTime.Now);
                    dataCollection[0].Add(new DataProfile(DataType.foot_temp, DataTime[DataType.foot_temp]++, (UInt16)temp));
                    //DataTime[DataType.masterTemp]++; DataTime[DataType.slaveTemp]++;
                } else if (type == DataType.ppg) {
                    dataCollection.Add(new Collection<DataProfile>());
                    for (int i = 0; i < 5; i++) {
                        var reddata = new DataProfile(DataType.red, DataTime[DataType.red]++, (UInt16)((data[4 * i + 1] << 8) | data[4 * i]));
                        var irdata = new DataProfile(DataType.ir, DataTime[DataType.ir]++, (UInt16)((data[4 * i + 3] << 8) | data[4 * i + 2]));
                        dataCollection[0].Add(reddata);
                        dataCollection[1].Add(irdata);
                    }
                } else {
                    Debug.WriteLine("data leak");
                }
            } else {
                Debug.WriteLine("Data Leak: " + type);
            }

            //static int Maximum = 500;
            //Update user interface.
            //MvxObservableCollection<BleDataReceived> newDataCollection = new MvxObservableCollection<BleDataReceived>();
            foreach (var collection in dataCollection) {
                if (collection.Count > 0) {
                    Collection<DataProfile> newDataCollection = null;
                    DataType specificType = DataType.None;
                    foreach (var dat in collection) {
                        string typeToString = "DataType." + dat.Type.ToString();
                        if (newDataCollection == null) {
                            //newDataCollection = new MvxObservableCollection<BleDataReceived>(DataListString[typeToString]);
                            newDataCollection = DataCollection[typeToString];

                            specificType = dat.Type;
                        }
                        if ((dat.Index % BluetoothModel.SamplingRate[dat.Type] == 0)) {
                            if (dat.Index % BluetoothModel.DataPoints[dat.Type] == 0) {
                                newDataCollection.Clear();
                                for (uint i = DataTime[specificType]; i < BluetoothModel.DataPoints[dat.Type]; i++)
                                    newDataCollection.Add(new DataProfile(type, i, double.NaN));
                            }
                            newDataCollection.Add(dat);
                        }
                        DataCollection[typeToString] = newDataCollection;
                    }
                } else {
                    Debug.WriteLine("UI: " + type);
                }
            }
            foreach (var collection in dataCollection)
                EnqueueData(collection);
        }

        public void EnqueueData(Collection<DataProfile> data) {
            if (!DataWriter1.IsAlive) {
                foreach (var d in data)
                    queue1.Enqueue(d);
                if (!DataWriter2.IsAlive) {
                    DataWriter1 = new Thread(() => PopAndWrite(queue1)) { IsBackground = true };
                    DataWriter1.Start();
                }
            } else if (!DataWriter2.IsAlive) {
                foreach (var d in data)
                    queue2.Enqueue(d);
                if (!DataWriter1.IsAlive) {
                    DataWriter2 = new Thread(() => PopAndWrite(queue2)) { IsBackground = true };
                    DataWriter2.Start();
                }
            } else {
                Debug.WriteLine("DATA IS LEAKING");
            }
        }

        public void PopAndWrite(Queue<DataProfile> q) {
            while (q.Count != 0) {
                DataProfile data = q.Dequeue();
                _fileManagementModel.WriteToFile(data).Wait();
            }
        }

        private DataType CharacteristicToDataTypeTranslator(ICharacteristic characteristic) {
            //2a18 = GlucoseMeasurement
            //2a37 = HeartRateMeasurement
            //2a1c = TemperatureMeasurement
            //2a49 = BloodPressureFeature
            if (characteristic.Uuid == "2a18")
                return DataType.ecg;
            if (characteristic.Uuid == "2a37")
                return DataType.scg;
            if (characteristic.Uuid == "2a1c")
                return DataType.temp;
            if (characteristic.Uuid == "2a49")
                return DataType.ppg;
            else return DataType.None;
        }

    }

    public class DataProfile {
        private DataType _type;
        private UInt32 _index;
        private double _value;

        public double Value { get { return _value; } }
        public DataType Type { get { return _type; } }
        public UInt32 Index { get { return _index; } set { _index = value; } }

        public DataProfile(DataType type, UInt32 idx, double data) {
            _type = type;
            _index = idx;
            _value = data;
        }
    }
}
