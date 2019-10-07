using System.Collections.ObjectModel;

namespace UHMS.Core.Models.Bluetooth
{
    /// <summary>
    /// Bluetooth model. 
    /// Single central base station bluetooth device that sensors will connect to.
    /// </summary>
    public partial class CentralDevice
    {
        /// <summary>
        /// List of devices currently connected to the central bluetooth device
        /// </summary>
        /// <value>A collection of the connected devices.</value>
        public ObservableCollection<SensorDevice> ConnectedDevices { get; set; }

        /// <summary>
        /// List of devices in the vicinity. Should be based on the most recent scan
        /// </summary>
        /// <value>A collection of the scanned devices.</value>
        public ObservableCollection<SensorDevice> ScannedDevices { get; set; }

        /// <summary>
        /// Temperature of the currently connected Master device
        /// </summary>
        public double MasterTemp { get; set; }

        /// <summary>
        /// Temperature of the currently connected Slave device
        /// </summary>
        public double SlaveTemp { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:UHMS.Core.Models.CentralDevice"/> class.
        /// </summary>
        public CentralDevice() {}
    }
}
