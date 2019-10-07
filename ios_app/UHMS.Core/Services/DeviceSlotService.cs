using System;
using System.Collections.Generic;
using System.Linq;
using UHMS.Core.Models.Bluetooth;

namespace UHMS.Core.Services
{
    public class DeviceSlotService : IDeviceSlotService
    {
        private readonly List<DeviceSlot> _deviceSlots = new List<DeviceSlot>();
        /// <summary>
        /// Keeps track of the current bluetooth DeviceSlots, either occupied or empty.
        /// </summary>
        public List<DeviceSlot> DeviceSlots => _deviceSlots;

        /// <summary>
        /// Specifies the device types allowed for each device slot.
        /// Also specifies the number of devices to connect.
        /// </summary>
        private static readonly List<HashSet<DeviceType>> SlotDeviceTypes = new List<HashSet<DeviceType>>{
            new HashSet<DeviceType> {DeviceType.Stroke},
            new HashSet<DeviceType> {DeviceType.Stroke}
        };

        /// <summary>
        /// Gets the maximum number of devices that can connect.
        /// </summary>
        /// <value>The connected device limit.</value>
        private int MaxNumConnectedDevices => SlotDeviceTypes.Count();

        public bool AllSlotsAreFull => NumConnectedDevices == MaxNumConnectedDevices;

        public bool AllSlotsAreEmpty => NumConnectedDevices == 0;

        public int NumConnectedDevices => _deviceSlots.Count(slot => !slot.IsEmpty);

        /// <summary>
        /// The slot counter use to number the device slots.
        /// </summary>
        private int slotCounter = 0;

        public event EventHandler<SlotEventArgs> DeviceAdded;

        public event EventHandler<SlotEventArgs> DeviceRemoved;

        public event EventHandler<SlotEventArgs> DeviceWillBeRemoved;

        public event EventHandler<SlotEventArgs> SessionInfoChanged;

        public event EventHandler<SlotEventArgs> BatteryInfoChanged;

        private void RaiseDeviceAddedEvent(SlotEventArgs e)
        {
            DeviceAdded?.Invoke(this, e);
        }

        private void RaiseDeviceRemovedEvent(SlotEventArgs e)
        {
            DeviceRemoved?.Invoke(this, e);
        }

        private void RaiseDeviceWillBeRemovedEvent(SlotEventArgs e)
        {
            DeviceWillBeRemoved?.Invoke(this, e);
        }
        private void RaiseSessionInfoChangedEvent(SlotEventArgs e)
        {
            SessionInfoChanged?.Invoke(this, e);
        }
        private void RaiseBatteryInfoChangedEvent(SlotEventArgs e)
        {
            BatteryInfoChanged?.Invoke(this, e);
        }

        public DeviceSlotService()
        {
            // Initialize list of currently connected devices.
            foreach (var deviceTypeSet in SlotDeviceTypes)
            {
                _deviceSlots.Add(new DeviceSlot(deviceTypeSet, slotCounter));
                slotCounter++;
            }
        }

        /// <summary>
        /// Gets the slot with given SensorDevice.
        /// </summary>
        /// <returns>The slot with device.</returns>
        /// <param name="sensorDevice">Sensor device.</param>
        public DeviceSlot GetSlotWithDevice(SensorDevice sensorDevice)
        {
            foreach (var slot in _deviceSlots)
            {
                if (!slot.IsEmpty && (slot.ConnectedDevice.Id == sensorDevice.Id))
                    return slot;
            }
            return null;
        }

        public DeviceSlot GetSlotAtIndex(int index)
        {
            if (index < _deviceSlots.Count && index >= 0)
            {
                return _deviceSlots[index];
            }
            return null;
        }

        /// <summary>
        /// Gets the first empty slot that is available for a given device type.
        /// </summary>
        /// <returns>The empty slot.</returns>
        /// <param name="type">DeviceType.</param>
        public DeviceSlot GetEmptySlot(DeviceType type)
        {
            foreach (var slot in _deviceSlots)
            {
                if (slot.IsEmpty && slot.AllowedDeviceTypes.Contains(type))
                    return slot;
            }
            return null;
        }

        public void AddDeviceToSlot(int slotIndex, SensorDevice device)
        {
            if (slotIndex < 0 || slotIndex >= MaxNumConnectedDevices) return;

            _deviceSlots[slotIndex].AddDevice(device);
            device.SlotIndex = slotIndex;
            RaiseDeviceAddedEvent(new SlotEventArgs(_deviceSlots[slotIndex]));
        }

        public void EmptySlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxNumConnectedDevices) return;

            var eventArgs = new SlotEventArgs(_deviceSlots[slotIndex]);
            RaiseDeviceWillBeRemovedEvent(eventArgs);
            _deviceSlots[slotIndex].EmptySlot();
            RaiseDeviceRemovedEvent(new SlotEventArgs(_deviceSlots[slotIndex]));
        }

        public void UpdateSessionStatus(int slotIndex, int status)
        {
            _deviceSlots[slotIndex].SessionStatus = status;
            RaiseSessionInfoChangedEvent(new SlotEventArgs(_deviceSlots[slotIndex]));
        }

        public void UpdateBatteryStatus(int slotIndex, int status)
        {
            _deviceSlots[slotIndex].BatteryLevel = status;
            RaiseBatteryInfoChangedEvent(new SlotEventArgs(_deviceSlots[slotIndex]));
        }

        private readonly List<string> _slotName = new List<string>
        {
            "Left Device",
            "Right Device",
        };

        public List<string> SlotName => _slotName;

        public List<int> GetAvailableSlots()
        {
            var availableSlots = new List<int>();
            foreach (var slot in DeviceSlots)
            {
                if (slot.IsEmpty) // Warning: ONLY works with same type of devices.
                    availableSlots.Add(slot.Index);
            }
            return availableSlots;
        }

        public bool ASlotIsBusy
        {
            get
            {
                foreach (var slot in DeviceSlots)
                {
                    if (slot.IsBusy)
                        return true;
                }
                return false;
            }
        }
    }

    /// <summary>
    /// Connection event arguments.
    /// </summary>
    public class SlotEventArgs : EventArgs
    {
        public DeviceSlot Slot { get; set; }

        public SlotEventArgs(DeviceSlot slot) : base()
        {
            Slot = slot;
        }
    }
}
