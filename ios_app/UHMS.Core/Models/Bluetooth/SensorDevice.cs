using System;
using System.Collections.Generic;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using UHMS.Core.Models.Data;

namespace UHMS.Core.Models.Bluetooth
{
    public class SensorDevice
    {
        /// <summary>
        /// The peripheral bluetooth device set by the device adapter.
        /// </summary>
        private readonly IDevice _device;
        public IDevice Device => _device;

        /// <summary>
        /// Gets the identifier of the sensor bluetooth device.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id => _device.Id;

        /// <summary>
        /// Gets the RSSI value (signal strength) of the sensor bluetooth device.
        /// </summary>
        public int Rssi => _device.Rssi;

        /// <summary>
        /// Gets the name of the sensor bluetooth device.
        /// </summary>
        public String Name => _device.Name;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public DeviceType Type { get; set; }

        /// <summary>
        /// Gets or sets the remaining battery charge level for the sensor device
        /// </summary>
        /// <value>The battery.</value>
        public int Battery { get; set; }

        /// <summary>
        /// Gets or sets the name of the slot.
        /// </summary>
        /// <value>The name of the slot.</value>
        public string SlotName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:UHMS.Core.Models.Bluetooth.SensorDevice"/> is connected.
        /// </summary>
        /// <value><c>true</c> if is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected => _device.State == DeviceState.Connected;

        /// <summary>
        /// The device has a running session.
        /// </summary>
        public bool IsRunningSession = false;

        /// <summary>
        /// Characteristics that are associated with the sensor device.
        /// </summary>
        public List<ICharacteristic> Characteristics;

        /// <summary>
        /// Channel for sending commands to the device.
        /// </summary>
        /// <value>The command characteristic.</value>
        public ICharacteristic CommandCharacteristic { get; set; }

        /// <summary>
        /// Gets or sets the download data characteristic.
        /// </summary>
        /// <value>The download data characteristic.</value>
        public ICharacteristic DownloadDataCharacteristic { get; set; }

        /// <summary>
        /// Gets or sets the info characteristic.
        /// </summary>
        /// <value>The info characteristic.</value>
        public ICharacteristic InfoCharacteristic { get; set; }

        /// <summary>
        /// Gets or sets the info characteristic.
        /// </summary>
        /// <value>The info characteristic.</value>
        public ICharacteristic BatteryCharacteristic { get; set; }

        /// <summary>
        /// UUIDs of the characteristics associated with the sensor device.
        /// </summary>
        public List<string> CharacteristicUuids;

        /// <summary>
        /// Gets the string value of the 
        /// </summary>
        /// <value>The type string.</value>
        public string TypeString => Enum.GetName(typeof(DeviceType), Type);

        public int SlotIndex = -1;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:UHMS.Core.Models.Bluetooth.SensorDevice"/> class.
        /// </summary>
        /// <param name="device">IAdapter-created Device.</param>
        /// <param name="type">Device Type.</param>
        public SensorDevice(IDevice device, DeviceType type = DeviceType.None)
        {
            _device = device;
            Type = type;
        }


    }
}
