using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using UHMS.Core.Models.Bluetooth;
using UHMS.Core.Models.Data;

namespace UHMS.Core.Services
{
    public class SensorDataService : ISensorDataService
    {
        private readonly IMvxLog _log;
        private readonly IDataLoggingService _dataLoggingService;

        static Thread DataWriter1;
        static Thread DataWriter2;
        private static Queue<DataProfile> queue1 = new Queue<DataProfile>();
        private static Queue<DataProfile> queue2 = new Queue<DataProfile>();

        /// <summary>
        /// The maximum number of packets that will be sent by the ble sensor device.
        /// </summary>
        private readonly int _blePacketLimit = 6;

        public Dictionary<int, int> DataErrorCount { get; }

        /// <summary>
        /// Gets or sets the data time, which is used to keep track of the index of a data stream for a particular data type.
        /// </summary>
        /// <value>The data time.</value>
        private Dictionary<int, UInt32> _dataTime { get; set; }

        public ConcurrentDictionary<int, ConcurrentQueue<DataProfile>> GraphDataBuffer { get; set; }

        /// <summary>
        /// The sensor data types that will be sent from the sensor devices.
        /// </summary>
        private static readonly List<DataType> ValidSensorDataTypes = new List<DataType>{
            DataType.timestamp,
            DataType.accl_x,
            DataType.accl_y,
            DataType.accl_z
        };

        /// <summary>
        /// The number data points per data packet sent by the sensors.
        /// </summary>
        public static readonly Dictionary<DataType, int> NumDataPointsPerPacket = new Dictionary<DataType, int> {
            { DataType.accl_x, 4 },
            { DataType.accl_y, 4 },
            { DataType.accl_z, 4 }
        };

        /// <summary>
        /// Down sampling rates to limit processing and display of incoming data.
        /// </summary>
        private static readonly Dictionary<DataType, uint> DownSamplingRate = new Dictionary<DataType, uint> {
            { DataType.accl_x, 1 },
            { DataType.accl_y, 1 },
            { DataType.accl_z, 1 }
        };


        /// <summary>
        /// A list of the latest output additions from a device connection.
        /// Refreshes in the bluetooth service layer after every device connection.
        /// </summary>
        /// <value>The latest output additions.</value>
        private readonly List<Tuple<int, DataType>> _recentOutputAdditions = new List<Tuple<int, DataType>>();
        public List<Tuple<int, DataType>> RecentOutputAdditions => _recentOutputAdditions;

        public List<int> CurrentOutputs { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:UHMS.Core.Services.SensorDataService"/> class.
        /// Processes incoming sensor data by handling characteristic updates by the sensor devices.
        /// </summary>
        /// <param name="log">Log.</param>
        public SensorDataService(IMvxLog log, IDataLoggingService dataLoggingService)
        {
            _log = log;
            _dataLoggingService = dataLoggingService;
            // Setup the data collections to be ready for new data.
            _dataTime = new Dictionary<int, uint>();

            GraphDataBuffer = new ConcurrentDictionary<int, ConcurrentQueue<DataProfile>>();
            CurrentOutputs = new List<int>();

            DataErrorCount = new Dictionary<int, int>
            {
                { 0, 0 },
                { 1, 0 }
            };
        }

        public void DataUpdate(byte[] data, DataType type, SensorDevice senderDevice)
        {
            var senderDeviceType = senderDevice.Type;
            // Create a new list with a collection for each data type for holding incoming data.
            var incomingData = new Dictionary<DataType, List<DataProfile>>();
            foreach (DataType dataType in ValidSensorDataTypes)
            {
                incomingData.Add(dataType, new List<DataProfile>());
            }

            var outputID = Tuple.Create(senderDevice.SlotIndex, type).GetHashCode();
            //_log.Debug($"{senderDevice.SlotIndex}");
            // Parse incoming data.
            if (senderDeviceType == DeviceType.Stroke)
            {
                if (type == DataType.accl)
                {
                    try {
                        for (int i = 0; i < 4; i ++) {
                            outputID = Tuple.Create(senderDevice.SlotIndex, DataType.accl_z).GetHashCode();
                            incomingData[DataType.accl_z].Add(new DataProfile(type, _dataTime[outputID]++, (Int16)((data[1+ i* 6] << 8) | data[0 + i * 6])));

                            outputID = Tuple.Create(senderDevice.SlotIndex, DataType.accl_x).GetHashCode();
                            incomingData[DataType.accl_x].Add(new DataProfile(type, _dataTime[outputID]++, (Int16)((data[3 + i * 6] << 8) | data[2 + i * 6])));

                            outputID = Tuple.Create(senderDevice.SlotIndex, DataType.accl_y).GetHashCode();
                            incomingData[DataType.accl_y].Add(new DataProfile(type, _dataTime[outputID]++, (Int16)((data[5 + i * 6] << 8) | data[4 + i * 6])));
                        }


                    } catch (IndexOutOfRangeException) {
                        _log.Error($"Invalid data range received from device {senderDevice.SlotIndex}. Count: {data.Length} Data: {string.Join(" ", data)}");
                        if (senderDevice.SlotIndex >= 0)
                            DataErrorCount[senderDevice.SlotIndex]++;
                        return;
                    } catch (KeyNotFoundException) {
                        _log.Error($"Invalid key detected for device {senderDevice.SlotIndex}. Keys in scope: outputID {outputID}");
                        if (senderDevice.SlotIndex >= 0)
                            DataErrorCount[senderDevice.SlotIndex]++;
                        return;
                    } 

                }
            }

            //Task.Run(async () => await _dataLoggingService.WritePackage(incomingData, senderDeviceType));
            //_dataLoggingService.AddCollectionAsync(incomingData, senderDeviceType);
            // DataRefreshPointers for each data collection of data points.
            foreach (KeyValuePair<DataType, List<DataProfile>> incomingDataforType in incomingData)
            {
                DataType dataType = incomingDataforType.Key;
                List<DataProfile> dataPoints = incomingDataforType.Value;
                outputID = Tuple.Create(senderDevice.SlotIndex, dataType).GetHashCode();

                if (dataPoints.Count > 0)
                {
                    // The Limit of the databuffer queue.
                    int queueDataPointLimit = _blePacketLimit * NumDataPointsPerPacket[dataType] / (int)DownSamplingRate[dataType];

                    // If the downsampling amount was specified. (i.e. accl_z)
                    if (DownSamplingRate[dataType] > 1)
                    {
                        // Calculate how many data points should be left after the downsampling.
                        int numOfDownsampledDataPoints = NumDataPointsPerPacket[dataType] / (int)DownSamplingRate[dataType];

                        for (int i = 0; i < dataPoints.Count; i += (int)DownSamplingRate[dataType])
                        {
                            int dataCount = Math.Min((int)DownSamplingRate[dataType], dataPoints.Count - i);
                            var partialData = dataPoints.GetRange(i, dataCount);
                            if (partialData.Count <= 0) continue;

                            DataProfile downsampledData = partialData[partialData.Count-1];

                            // Remove a datapoint from the queue if it's beyond the databuffer queue point limit.
                            if (GraphDataBuffer[outputID].Count > queueDataPointLimit)
                            {
                                GraphDataBuffer[outputID].TryDequeue(out DataProfile dequeuedData);
                            }

                            // Add the new downsampled data.
                            GraphDataBuffer[outputID].Enqueue(downsampledData);
                        }

                        DataProfile downsample = dataPoints[dataPoints.Count - 1];

                        // Remove a datapoint from the queue if it's beyond the databuffer queue point limit.
                        if (GraphDataBuffer[outputID].Count > queueDataPointLimit)
                        {
                            GraphDataBuffer[outputID].TryDequeue(out DataProfile dequeuedData);
                        }

                        // Add the new downsampled data.
                        GraphDataBuffer[outputID].Enqueue(downsample);
                    }
                    // Else the downsampling amount was not specified.
                    else
                    {
                        foreach (var dataPoint in dataPoints)
                        {
                            // Remove a datapoint from the queue if it's beyond the databuffer queue point limit.
                            if (GraphDataBuffer[outputID].Count > queueDataPointLimit)
                            {
                                GraphDataBuffer[outputID].TryDequeue(out DataProfile dequeuedData);
                            }
                            // Add the new data point.
                            GraphDataBuffer[outputID].Enqueue(dataPoint);
                        }
                    }
                }
            }

            var timeStamp =((data[27] << 24) | (data[26] << 16) | (data[25] << 8) | data[24]) / 4.0;
            _dataLoggingService.UpdateTimeStamp(timeStamp);

        }

        /// <summary>
        /// Downsamples the given collection data to a single DataProfile by averaging all values. 
        /// </summary>
        /// <returns>The DataProfile with all values averaged.</returns>
        /// <param name="type">Data type.</param>
        /// <param name="dataPoints">A collection of DataProfiles that represent data points to be downsampled.</param>
        private DataProfile DownsampleByAverage(DataType type, List<DataProfile> dataPoints)
        {
            double sum = 0;
            for (int i = 0; i < dataPoints.Count; i++)
                sum += dataPoints[i].Value;
            double average = sum / DownSamplingRate[type];

            uint index = dataPoints[0].Index;
            return new DataProfile(type, index, average);
        }

        /// <summary>
        /// Enqueues the data and starts the data threads to commence write to file.
        /// </summary>
        /// <param name="data">Data.</param>
        public void EnqueueData(Collection<DataProfile> data)
        {
            if (!DataWriter1.IsAlive)
            {
                foreach (var d in data)
                    queue1.Enqueue(d);
                if (!DataWriter2.IsAlive)
                {
                    DataWriter1 = new Thread(() => PopAndWrite(queue1)) { IsBackground = true };
                    DataWriter1.Start();
                }
            }
            else if (!DataWriter2.IsAlive)
            {
                foreach (var d in data)
                    queue2.Enqueue(d);
                if (!DataWriter1.IsAlive)
                {
                    DataWriter2 = new Thread(() => PopAndWrite(queue2)) { IsBackground = true };
                    DataWriter2.Start();
                }
            }
            else
            {
                _log.Debug("DATA IS LEAKING");
            }
        }

        public void AddToDataBuffer(int OutputID)
        {
            if (GraphDataBuffer.ContainsKey(OutputID))
                ClearDataBufferForDataType(OutputID);
            else
                GraphDataBuffer.TryAdd(OutputID, new ConcurrentQueue<DataProfile>());

            if (_dataTime.ContainsKey(OutputID))
                _dataTime[OutputID] = 0;
            else
                _dataTime.Add(OutputID, 0);
        }

        public void RemoveFromDataBuffer(int OutputID)
        {
            if (GraphDataBuffer.ContainsKey(OutputID))
                ClearDataBufferForDataType(OutputID);
            if (_dataTime.ContainsKey(OutputID))
                _dataTime[OutputID] = 0;
        }

        public void ClearDataBufferForDataType(int OutputID)
        {
            while (GraphDataBuffer[OutputID].Count > 0)
            {
                GraphDataBuffer[OutputID].TryDequeue(out DataProfile data);
            }
        }

        /// <summary>
        /// Dequeues the data and writes into the filesystem.
        /// </summary>
        /// <param name="q">Q.</param>
        public void PopAndWrite(Queue<DataProfile> q)
        {
            _log.DebugFormat("Writing data into file. Queue Count: {0}", q.Count);
            while (q.Count != 0)
            {
                DataProfile data = q.Dequeue();
                //_fileManagementModel.WriteToFile(data).Wait();
            }
        }
    }
}
