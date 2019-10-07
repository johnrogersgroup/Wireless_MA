using System;
using System.Collections.Generic;

namespace UHMS.Core.Models.Bluetooth
{
    /// <summary>
    /// Model to signify sensor bluetooth slots available for connected devices to occupy.
    /// Used to help keep track of the connected devices and whether more devices can be connected.
    /// </summary>
    public class DeviceSlot
    {
        /// <summary>
        /// Gets the index of the current device slot among all available slots.
        /// </summary>
        /// <value>The index.</value>
        public int Index { get; private set; }

        /// <summary>
        /// The allowed device types for this particular device slot.
        /// </summary>
        public HashSet<DeviceType> AllowedDeviceTypes = new HashSet<DeviceType>();

        /// <summary>
        /// The connected SensorDevice.
        /// </summary>
        public SensorDevice ConnectedDevice;

        public bool IsBusy = false;

        public int SessionStatus = -1;

        public int BatteryLevel = -1;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:UHMS.Core.Models.Bluetooth.DeviceSlot"/> is empty.
        /// </summary>
        /// <value><c>true</c> if is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty => (ConnectedDevice == null);

        public List<int> OutputDataIDs = new List<int>();

        public Guid LastConnectedId;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UHMS.Core.Models.Bluetooth.DeviceSlot"/> class.
        /// </summary>
        /// <param name="allowedDeviceTypes">A collection of allowed device types.</param>
        /// <param name="index">Index of the Slot.</param>
        public DeviceSlot(ICollection<DeviceType> allowedDeviceTypes, int index)
        {
            Index = index;

            foreach (var deviceType in allowedDeviceTypes)
            {
                AllowedDeviceTypes.Add(deviceType);
            }
        }

        /// <summary>
        /// Adds the SensorDevice instance refrence that would occupy the slot.
        /// </summary>
        /// <param name="device">Device.</param>
        public void AddDevice(SensorDevice device)
        {
            ConnectedDevice = device;
            ConnectedDevice.SlotIndex = Index;
            LastConnectedId = ConnectedDevice.Id;
        }

        /// <summary>
        /// Empties the slot by removing the reference to the connected device.
        /// </summary>
        public void EmptySlot()
        {
            OutputDataIDs.Clear();
            ConnectedDevice.SlotIndex = -1;
            ConnectedDevice = null;
        }
    }
}
