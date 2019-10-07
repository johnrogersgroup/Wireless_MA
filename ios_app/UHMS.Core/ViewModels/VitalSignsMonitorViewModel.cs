using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using UHMS.Core.Models.Data;
using UHMS.Core.Services;
using Plugin.BLE.Abstractions.EventArgs;

namespace UHMS.Core.ViewModels
{
    /// <summary>
    /// Vital signs monitor view model. Handles the preparation of the graphical representation data of the vital sign data.
    /// </summary>
    public class VitalSignsMonitorViewModel : MvxViewModel
    {
        private readonly IBluetoothService _bluetoothService;
        private readonly IDeviceSlotService _deviceSlotService;

        /// <summary>
        /// The data types currently available to be graphed. Each of these types have a corresponding graph view in the ui.
        /// </summary>
        private readonly List<Tuple<int, DataType>> _graphDataTypes = new List<Tuple<int, DataType>>
        {
            Tuple.Create(0, DataType.accl_x),
            Tuple.Create(0, DataType.accl_y),
            Tuple.Create(0, DataType.accl_z),
            Tuple.Create(1, DataType.accl_x),
            Tuple.Create(1, DataType.accl_y),
            Tuple.Create(1, DataType.accl_z),
        };

        /// <summary>
        /// The data sync groups.
        /// Each HashSetgroup specifies the type of graph data to be synced, which clears and moves the refresh cursor together.
        /// A data type, even if it doesn't need to synced with anything else, must be in a group to refresh properly.
        /// </summary>
        private readonly List<HashSet<int>> _dataSyncGroups = new List<HashSet<int>>() {
            new HashSet<int>
            {
                Tuple.Create(0, DataType.accl_x).GetHashCode(),
                Tuple.Create(0, DataType.accl_y).GetHashCode(),
                Tuple.Create(0, DataType.accl_z).GetHashCode(),
                Tuple.Create(1, DataType.accl_x).GetHashCode(),
                Tuple.Create(1, DataType.accl_y).GetHashCode(),
                Tuple.Create(1, DataType.accl_z).GetHashCode(),
            },
        };

        private readonly HashSet<DataType> limitDataTypes = new HashSet<DataType>()
        {
            DataType.gyro_x,
            DataType.gyro_y,
            DataType.gyro_z,
        };

        /// <summary>
        /// The graph render rate. Refresh the UI at a 34hz interval.
        /// </summary>
        private readonly int _graphRenderRate = 1000 / 34;

        private readonly IMvxLog _log;
        private readonly ISensorDataService _sensorDataService;
        private readonly IUserDialogs _userDialogs;

        /// <summary>
        /// Signals to a CancellationToken (Propagator) to notify to cancel async tasks.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// The list of currently active graphs that are being rendered.
        /// </summary>
        private readonly HashSet<int> _currentActiveGraphs = new HashSet<int>();

        /// <summary>
        /// The set of graphs that have reached the end and are waiting for other graphs to finish.
        /// Each set is grouped by groups set in _dataSyncGroups.
        /// </summary>
        private readonly List<HashSet<int>> _finishedGraphs = new List<HashSet<int>>();

        /// <summary>
        /// The graph view models. Handles the drawing of the data.
        /// </summary>
        public Dictionary<int, LiveGraphViewModel> _graphs;

        public Dictionary<int, GraphOutput> DataOutputs = new Dictionary<int, GraphOutput>();

        private List<SlotViewModel> _deviceSlots = new List<SlotViewModel>(2);
        public List<SlotViewModel> DeviceSlots => _deviceSlots;

        private Task _renderGraphTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UHMS.Core.ViewModels.VitalSignsMonitorViewModel"/> class.
        /// </summary>
        /// <param name="sensorDataService">Sensor data service.</param>
        /// <param name="userDialogs">User dialogs.</param>
        /// <param name="log">Log.</param>
        public VitalSignsMonitorViewModel(IBluetoothService bluetoothService, ISensorDataService sensorDataService, IDeviceSlotService deviceSlotService,
            IUserDialogs userDialogs, IMvxLog log)
        {
            _bluetoothService = bluetoothService;
            _sensorDataService = sensorDataService;
            _deviceSlotService = deviceSlotService;
            _userDialogs = userDialogs;
            _log = log;

            InitializeGraphs();
            AttatchBluetoothEventHandlers();
            InsertSlots();
        }
        /// <summary>
        /// The collection of graph data to be displayed for each type of data.
        /// </summary>
        private ConcurrentDictionary<string, ObservableCollection<DataProfile>> _graphData { get; set; }

        /// <summary>
        /// Gets the list of data to display as graphs.
        /// </summary>
        /// <value>The data list.</value>
        public ConcurrentDictionary<string, ObservableCollection<DataProfile>> GraphData => _graphData;

        /// <summary>
        /// Check if all graphs within a sync group are finished
        /// </summary>
        /// <returns><c>true</c>, if all the active graphs in a group have finished, <c>false</c> otherwise.</returns>
        /// <param name="type">Data Type.</param>
        private bool _allGraphsHaveFinished(int groupIndex)
        {
            var syncGroup = _dataSyncGroups[groupIndex];

            var activeGraphs = new List<DataType>();
            // Check if each type is active AND is finished
            foreach (var outputID in syncGroup)
                if (_currentActiveGraphs.Contains(outputID) && !_finishedGraphs[groupIndex].Contains(outputID)) 
                    return false;
            return true;
        }

        /// <summary>
        /// Initializes the graph data structures to be used in the ui.
        /// </summary>
        private void InitializeGraphs()
        {
            _graphs = new Dictionary<int, LiveGraphViewModel>();

            _graphData = new ConcurrentDictionary<string, ObservableCollection<DataProfile>>();

            foreach (var type in _graphDataTypes)
            {
                var outputID = type.GetHashCode();
                var dataType = type.Item2;
                var uiBindingTypeName = type.Item1 + "_" + dataType.ToString();

                _graphs[outputID] = new LiveGraphViewModel(type.Item2);
                _graphData.TryAdd(uiBindingTypeName, _graphs[outputID].Data);
            }

            for (int i = 0; i < _dataSyncGroups.Count; i++) {
                _finishedGraphs.Add(new HashSet<int>());
            }
        }

        /// <summary>
        /// Attaches event handlers specific to graph update data to bluetooth events.
        /// </summary>
        private void AttatchBluetoothEventHandlers()
        {
            _bluetoothService.Adapter.DeviceConnected += OnDeviceConnectedAsync;
            _bluetoothService.Adapter.DeviceDisconnected += OnDeviceDisconnected;
            _bluetoothService.Adapter.DeviceConnectionLost += OnDeviceDisconnected;
        }

        /// <summary>
        /// Event handler for when a device connects to the central bluetooth device.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void OnDeviceConnectedAsync(object sender, DeviceEventArgs e)
        {
            while (_bluetoothService.IsBusy) {
                _log.Debug($"On Device Connected Async wait");
                await Task.Delay(200);
            }
            // Check if the view refresher is alredy running.
            if (_renderGraphTask != null && (_renderGraphTask.IsCompleted == false ||
                                             _renderGraphTask.Status == TaskStatus.Running ||
                                             _renderGraphTask.Status == TaskStatus.WaitingToRun ||
                                             _renderGraphTask.Status == TaskStatus.WaitingForActivation))
            {
                _log.Info("Graph refresh task is already running. ");
                _log.Debug($"Active Graphs: {string.Join(" ", _currentActiveGraphs)}");

                _log.Info("Resetting graph indicators to 0");
                
                // Reset types in the same group for each of the output types from a recent device connection.
                foreach (var outputTuple in _sensorDataService.RecentOutputAdditions) {
                    var outputID = outputTuple.GetHashCode();

                    int syncGroupIndex = -1;
                    for (int i = 0; i < _dataSyncGroups.Count; i++)
                    {
                        var syncGroup = _dataSyncGroups[i];
                        if (syncGroup.Contains(outputID))
                        {   
                            foreach (var type in syncGroup) {
                                if (_currentActiveGraphs.Contains(type)){
                                    _graphs[type].ResetRefreshCursor();
                                    _graphs[type].ClearGraph();
                                }
                            }
                            syncGroupIndex = i;
                            break;
                        }
                    }
                    GraphOutput newOutput = new GraphOutput(outputTuple.Item1, syncGroupIndex, outputTuple.Item2, outputTuple.Item2.ToString());
                    if (!limitDataTypes.Contains(outputTuple.Item2))
                    {
                        DataOutputs.Add(outputTuple.GetHashCode(), newOutput);
                    }
                }
            }
            else
            {
                foreach (var outputTuple in _sensorDataService.RecentOutputAdditions)
                {
                    var outputID = outputTuple.GetHashCode();

                    int syncGroupIndex = -1;
                    for (int i = 0; i < _dataSyncGroups.Count; i++)
                    {
                        var syncGroup = _dataSyncGroups[i];
                        if (syncGroup.Contains(outputID))
                        {
                            syncGroupIndex = i;
                            break;
                        }
                    }
                    GraphOutput newOutput = new GraphOutput(outputTuple.Item1, syncGroupIndex, outputTuple.Item2, outputTuple.Item2.ToString());
                    if (!limitDataTypes.Contains(outputTuple.Item2))
                    {
                        DataOutputs.Add(outputTuple.GetHashCode(), newOutput);
                    }
                }

                _log.Info("Starting graph refresh task.");
                StartMonitor();
            }

            CheckIfASessionIsRunning();
        }

        /// <summary>
        /// Event handler for when a device disconnects. 
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnDeviceDisconnected(object sender, DeviceEventArgs e)
        {
            if (_renderGraphTask != null && _bluetoothService.OutputData.Count == 0)
            {
                _log.Info("Last Device disconnected. Stopping the view refresh task.");
                StopMonitor();

                while (_bluetoothService.IsBusy)
                {
                    await Task.Delay(100);
                }

                _currentActiveGraphs.Clear();
                foreach (var type in _graphDataTypes)
                    _graphs[type.GetHashCode()].ClearGraph();
            }
            else
            {
                while (_bluetoothService.IsBusy)
                {
                    await Task.Delay(100);
                }

                _log.Info("A device is still connected.");
                _log.Debug($"Active Graphs: {string.Join(" ", _currentActiveGraphs)}");
                CleanUpRemainingData();
                _log.Debug($"Active Graphs after Cleanup: {string.Join(" ", _currentActiveGraphs)}");
            }

            CheckIfASessionIsRunning();
        }

        public void CheckIfASessionIsRunning()
        {
            for (int i = 0; i < DeviceSlots.Count; i++)
            {
                if (DeviceSlots[i].SessionStatus == 1)
                {
                    IsRunningSession = true;
                    return;
                }
            }
            IsRunningSession = false;
        }

        private void InsertSlots()
        {
            var slot1 = new SlotViewModel(_deviceSlotService.GetSlotAtIndex(0), _deviceSlotService, _log);
            var slot2 = new SlotViewModel(_deviceSlotService.GetSlotAtIndex(1), _deviceSlotService, _log);
            _deviceSlots.Insert(0, slot1);
            _deviceSlots.Insert(1, slot2);

            IsRunningSession = false;
        }

        private bool _isRunningSession = false;

        public bool IsRunningSession
        {
            get => _isRunningSession;
            set
            {
                SetProperty(ref _isRunningSession, value, "IsRunningSession");
            }
        }

        /// <summary>
        /// Starts the monitor for vital signals, which includes refreshing the graph.
        /// </summary>
        public void StartMonitor()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Token.Register(CleanUpRemainingData);

            _renderGraphTask = Task.Run(async () => await RenderGraphs());
        }

        /// <summary>
        /// Stops the monitor for vital signals and stops the update of all graphs.
        /// </summary>
        public void StopMonitor()
        {
            CleanupCancellationToken();
            _renderGraphTask = null;


        }

        /// <summary>
        /// Task to render the graph for all valid data types. Enters the new value for new data points and moves the refresh indicator.
        /// </summary>
        /// <returns>The graphs.</returns>
        public async Task RenderGraphs()
        {
            // Run until cancelled with the token.
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var outputList = _bluetoothService.OutputData.ToArray();
                foreach (var outputID in outputList)
                {
                    try
                    {
                        if (!DataOutputs.ContainsKey(outputID)) continue;
                        if (_sensorDataService.DataErrorCount[DataOutputs[outputID].DeviceSlotIndex] > 5)
                        {
                            int deviceSlotIndex = DataOutputs[outputID].DeviceSlotIndex;
                            string deviceName = _deviceSlotService.DeviceSlots[deviceSlotIndex].ConnectedDevice.Name;
                            _userDialogs.Alert(new AlertConfig
                            {
                                Message = $"Error: Incompatible device detected. {deviceName} (Device {deviceSlotIndex + 1}) has sent invalid data. Disconnecting from the device."
                            });
                            _sensorDataService.DataErrorCount[deviceSlotIndex] = 0;
                            await _bluetoothService.DisconnectDevice(_deviceSlotService.DeviceSlots[deviceSlotIndex].ConnectedDevice);
                            break;
                        }

                        if (!_sensorDataService.GraphDataBuffer.ContainsKey(outputID)) continue;
                        // Get the data pushed from the sensor devices.

                        _sensorDataService.GraphDataBuffer[outputID].TryDequeue(out DataProfile data);
                        if (data == null) continue;

                        // Graph each new data point from the sensor devices.

                        if (_cancellationTokenSource.IsCancellationRequested) break;

                        if (_graphs[outputID].CursorIsAtEnd)
                        {
                            // The other graphs need to be synced
                            int syncGroupNumber = DataOutputs[outputID].SyncGroupIndex;
                            if (syncGroupNumber < 0) _graphs[outputID]?.ResetRefreshCursor();

                            _finishedGraphs[syncGroupNumber].Add(outputID);

                            if (_allGraphsHaveFinished(syncGroupNumber))
                            {
                                foreach (var finishedType in _finishedGraphs[syncGroupNumber])
                                    _sensorDataService.ClearDataBufferForDataType(finishedType);

                                ResetGroupCursorPositions(syncGroupNumber);
                                continue;
                            }
                        }
                        else
                        {
                            _graphs[outputID].MoveRefreshCursor();

                            // Draw the new data point on the graph and move the refresh indicator.
                            _graphs[outputID].UpdateData(data);
                            _graphs[outputID].UpdateRefreshIndicator();

                            // Update the active graphs list to sync the refresh indicator cursors.
                            _currentActiveGraphs.Add(outputID);
                            continue;
                        }
                    }
                    catch (Exception exceptionArg)
                    {
                        _log.DebugException("Exception.", exceptionArg);
                    }
                }
                // Repeat the task
                await Task.Delay(_graphRenderRate, _cancellationTokenSource.Token);
            }

            _log.Debug("Graph refresh has ended prematurely.");
        }

        /// <summary>
        /// Resets all refresh cursor positions to the beginning of the graph, 0.
        /// </summary>
        public void ResetGroupCursorPositions(int groupNumber)
        {

            foreach (var graphType in _finishedGraphs[groupNumber])
            {
                _graphs[graphType]?.ResetRefreshCursor();
            }
            _finishedGraphs[groupNumber].Clear();
        }

        /// <summary>
        /// Cleans up the cancellation token and the task associated with it.
        /// </summary>
        public void CleanupCancellationToken()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Cleans up remaining data left behind for all graph types.
        /// </summary>
        public void CleanUpRemainingData()
        {
            _log.Debug("Cleaning up remaining data");

            var keysToRemove = new List<int>();
            foreach (var output in DataOutputs) {
                var outputID = output.Key;
                if (!_bluetoothService.OutputData.Contains(output.Key)) {
                    _graphs[outputID].ClearGraph();
                    _currentActiveGraphs.Remove(outputID);
                    _sensorDataService.ClearDataBufferForDataType(outputID);
                    keysToRemove.Add(outputID);
                }
            }
            foreach(var key in keysToRemove) {
                DataOutputs.Remove(key);
            }
        }
    }
}