using System;
using System.Collections.Generic;
using UHMS.Core.Models.Bluetooth;

namespace UHMS.Core.Services
{
    public interface IDeviceSlotService
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:NICU.Core.Services.IDeviceSlotService"/> slots are full.
        /// </summary>
        /// <value><c>true</c> if slots are full; otherwise, <c>false</c>.</value>
        bool AllSlotsAreFull { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:NICU.Core.Services.IDeviceSlotService"/> all slots are empty.
        /// </summary>
        /// <value><c>true</c> if all slots are empty; otherwise, <c>false</c>.</value>
        bool AllSlotsAreEmpty { get; }

        /// <summary>
        /// Gets the total number of currently connected devices.
        /// </summary>
        /// <value>The number connected devices.</value>
        int NumConnectedDevices { get; }

        /// <summary>
        /// Gets the slot with the specified SensorDevice.
        /// </summary>
        /// <returns>The slot with device.</returns>
        /// <param name="sensorDevice">Sensor device.</param>
        DeviceSlot GetSlotWithDevice(SensorDevice sensorDevice);

        /// <summary>
        /// Gets the slot at a given index.
        /// </summary>
        /// <returns>The slot at index.</returns>
        /// <param name="index">Index.</param>
        DeviceSlot GetSlotAtIndex(int index);

        /// <summary>
        /// Gets the first empty slot that is available for a given device type.
        /// </summary>
        /// <returns>The empty slot.</returns>
        /// <param name="type">DeviceType.</param>
        DeviceSlot GetEmptySlot(DeviceType type);

        /// <summary>
        /// The device slots.
        /// </summary>
        List<DeviceSlot> DeviceSlots { get; }

        /// <summary>
        /// Adds the SensorDevice instance refrence that would occupy the slot.
        /// </summary>
        /// <param name="slotIndex">Slot index.</param>
        /// <param name="device">Device.</param>
        void AddDeviceToSlot(int slotIndex, SensorDevice device);

        /// <summary>
        /// Empties the slot by removing the reference to the connected device.
        /// </summary>
        /// <param name="slotIndex">Slot index.</param>
        void EmptySlot(int slotIndex);

        /// <summary>
        /// Updates the session status for a given slot at
        /// </summary>
        /// <param name="slotIndex">Slot index.</param>
        /// <param name="status">Status.</param>
        void UpdateSessionStatus(int slotIndex, int status);

        /// <summary>
        /// Updates the session status for a given slot at
        /// </summary>
        /// <param name="slotIndex">Slot index.</param>
        /// <param name="status">Status.</param>
        void UpdateBatteryStatus(int slotIndex, int status);

        /// <summary>
        /// The name of the slot.
        /// </summary>
        List<string> SlotName { get; }

        /// <summary>
        /// Gets the available slot indices.
        /// </summary>
        /// <returns>The list of available slots.</returns>
        List<int> GetAvailableSlots();

        event EventHandler<SlotEventArgs> DeviceAdded;

        event EventHandler<SlotEventArgs> DeviceRemoved;

        event EventHandler<SlotEventArgs> DeviceWillBeRemoved;

        event EventHandler<SlotEventArgs> SessionInfoChanged;

        event EventHandler<SlotEventArgs> BatteryInfoChanged;

        bool ASlotIsBusy { get; }
    }
}
