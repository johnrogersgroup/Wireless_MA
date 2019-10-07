using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UHMS.Core.Models.Data
{
    /// <summary>
    /// Data profile used by the sensor devices to package and organize streamed data.
    /// </summary>
    public class DataProfile : INotifyPropertyChanged
    {
        private DataType _type { get; }
        private uint _index { get; }
        private double _value;

        /// <summary>
        /// The type of sensor data.
        /// </summary>
        public DataType Type => _type;

        /// <summary>
        /// The index of the data in the data stream sequence.
        /// </summary>
        public uint Index => _index;

        /// <summary>
        /// The raw data value as read by the sensor device.
        /// </summary>
        public double Value
        {
            get => _value;

            set  
            {
                _value = value;
                NotifyPropertyChanged();
            }  
        }

        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This method is called by the Set accessor of each property.  
        /// The CallerMemberName attribute that is applied to the optional propertyName  
        /// parameter causes the property name of the caller to be substituted as an argument.  
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UHMS.Core.Models.DataProfile"/> class.
        /// </summary>
        /// <param name="type">The type of sensor data.</param>
        /// <param name="idx">The index of the data in the data stream sequence.</param>
        /// <param name="data">The raw data value as read by the sensor device.</param>
        public DataProfile(DataType type, uint idx, double data)
        {
            _type = type;
            _index = idx;
            _value = data;

        }
    }


}
