using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UHMS.Core.Models.Bluetooth;
using UHMS.Core.Models.Data;

namespace UHMS.Core.Services
{
    public interface ISensorDataService
    {
        /// <summary>
        /// Gets or sets the data buffer, a queue of the latest 6 packets of data for each data type.
        /// </summary>
        /// <value>The data buffer.</value>
        ConcurrentDictionary<int, ConcurrentQueue<DataProfile>> GraphDataBuffer { get; set; }

        /// <summary>
        /// Gets the recent output additions based on the most recent device connection.
        /// </summary>
        /// <value>The recent output additions.</value>
        List<Tuple<int, DataType>> RecentOutputAdditions { get; }

        List<int> CurrentOutputs { get; }

        Dictionary<int, int> DataErrorCount { get; }
        /// <summary>
        /// Updates the data in memory based on the packet sent by the connected devices through bluetooth. 
        /// Parses the raw data and downsamples as needed. Procs whenever a packet is sent.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="type">Data type.</param>
        /// <param name="senderDevice">Sender device.</param>
        void DataUpdate(byte[] data, DataType type, SensorDevice senderDevice);


        /// <summary>
        /// Clears the data buffer for the type of data. Typically called after a device disconnect occurs.
        /// </summary>
        /// <param name="outputID">Output identifier.</param>
        void ClearDataBufferForDataType(int outputID);

        /// <summary>
        /// Adds to data buffer.
        /// </summary>
        /// <param name="OutputID">Output identifier.</param>
        void AddToDataBuffer(int OutputID);

        /// <summary>
        /// Removes from data buffer.
        /// </summary>
        /// <param name="OutputID">Output identifier.</param>
        void RemoveFromDataBuffer(int OutputID);
    }
}
