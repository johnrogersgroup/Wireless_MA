using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using UHMS.Core.Models.Bluetooth;
using UHMS.Core.Models.Data;

namespace UHMS.Core.Services
{
    public interface IBluetoothService
    {
        /// <summary>
        /// Gets the bluetooth service for the central device.
        /// </summary>
        /// <value>The bluetooth.</value>
        IBluetoothLE Bluetooth { get; }

        /// <summary>
        /// Gets the adapter that manages connection between the central and peripheral devices.
        /// </summary>
        /// <value>The adapter.</value>
        IAdapter Adapter { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:UHMS.Core.Services.IBluetoothService"/> is scanning.
        /// </summary>
        /// <value><c>true</c> if is scanning; otherwise, <c>false</c>.</value>
        bool IsScanning { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:UHMS.Core.Services.IBluetoothService"/> is on.
        /// </summary>
        /// <value><c>true</c> if is on; otherwise, <c>false</c>.</value>
        bool IsOn { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:UHMS.Core.Services.BluetoothService"/> is connecting to a device.
        /// </summary>
        /// <value><c>true</c> if is busy with a device; otherwise, <c>false</c>.</value>
        bool IsBusy { get; }

        /// <summary>
        /// Gets the user-friendly state message for the main bluetooth device.
        /// </summary>
        /// <value>The state text.</value>
        string StateText { get; }

        /// <summary>
        /// Gets the list of subscribed characteristics list.
        /// </summary>
        /// <value>The subscribed characteristics list.</value>
        List<DataType> SubscribedCharacteristicsList { get; }

        /// <summary>
        /// Gets the output data.
        /// </summary>
        /// <value>The output data.</value>
        List<int> OutputData { get; }

        /// <summary>
        /// Connects the device. While, connecting assign available characteristic to sensor device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="device">The sensor device to connect to.</param>
        /// <param name="tokenSource">Token to stop the connection.</param>
        /// <param name="slotIndex">Slot index.</param>
        Task<bool> ConnectDevice(SensorDevice device, CancellationTokenSource tokenSource, int slotIndex);


        bool StartSession(long epochTime);

        /// <summary>
        /// Disconnects from the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="device">Device.</param>
        Task<bool> DisconnectDevice(SensorDevice device);


        bool StopSession();

        /// <summary>
        /// Event Handler for after when a device has successfuly connected.
        /// </summary>
        /// <param name="device">Device.</param>
        void OnConnected(SensorDevice device);

        /// <summary>
        /// Event Handler for after when a device has disconnected.
        /// </summary>
        /// <param name="sensorDevice">Sensor device.</param>
        /// <param name="showMessage">If set to <c>true</c> show message.</param>
        void OnDisconnected(SensorDevice sensorDevice, bool showMessage = false);

        /// <summary>
        /// Checks the type of the device.
        /// </summary>
        /// <returns>The type of sensor device should be.</returns>
        /// <param name="sensorDevice">A device discovered from scan.<param>
        DeviceType DiscoverDeviceType(IDevice sensorDevice);

        /// <summary>
        /// Characteristic data is actively being updated by the sensor devices.
        /// </summary>
        /// <returns><c>true</c>, if the characteristic updates are being listened to, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        bool CharacteristicIsActive(DataType type);
    }
}
