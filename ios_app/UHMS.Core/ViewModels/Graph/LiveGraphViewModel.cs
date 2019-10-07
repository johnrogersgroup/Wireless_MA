using System;
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
    /// Graph view model that renders a single graph view that holds a single series.
    /// </summary>
    public class LiveGraphViewModel : MvxViewModel
    {
        /// <summary>
        /// The data point limits ("width") for each graph. Limits how many data points will be available for display.
        /// </summary>
        private static readonly Dictionary<DataType, uint> _dataPointLimits = new Dictionary<DataType, uint>
        {
            {DataType.accl_x, 300},
            {DataType.accl_y, 300},
            {DataType.accl_z, 300}
        };

        /// <summary>
        /// The width for the blank indicator for each graph. Different values for graphs with different data point limits.
        /// </summary>
        private static readonly Dictionary<DataType, uint> _refreshIndicatorWidths = new Dictionary<DataType, uint>
        {
            {DataType.accl_x, 3},
            {DataType.accl_y, 3},
            {DataType.accl_z, 3}
        };

        private readonly DataType _type;

        /// <summary>
        /// The graph data.
        /// </summary>
        private ObservableCollection<DataProfile> _data;


        ///// <summary>
        ///// Collection of cursors (a data point index position) that keeps track of the current position in terms of the graph collection.
        ///// </summary>
        private int _refreshCursorPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UHMS.Core.ViewModels.LiveGraphViewModel"/> class.
        /// </summary>
        /// <param name="type">Type.</param>
        public LiveGraphViewModel(DataType type)
        {
            _type = type;

            SetupGraph();
            FillGraphWithNaNData();
        }

        /// <summary>
        /// Gets the graph data observable collection.
        /// </summary>
        /// <value>The data.</value>
        public ObservableCollection<DataProfile> Data => _data;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:UHMS.Core.ViewModels.LiveGraphViewModel"/>'s cursor is at the end of the graph.
        /// </summary>
        /// <value><c>true</c> if cursor is at end; otherwise, <c>false</c>.</value>
        public bool CursorIsAtEnd => _refreshCursorPosition == _dataPointLimits[_type] - 1;

        /// <summary>
        /// Setups the graph collection and related variables.
        /// </summary>
        private void SetupGraph()
        {
            _data = new ObservableCollection<DataProfile>();
            _refreshCursorPosition = 0;
        }

        /// <summary>
        /// Updates the graph with a new data point.
        /// </summary>
        /// <param name="newData">New data.</param>
        public void UpdateData(DataProfile newData)
        {
            var newDataIndex = _refreshCursorPosition;
            var newDataValue = newData.Value;

            InvokeOnMainThread(() => { _data[newDataIndex].Value = newDataValue; });
        }

        /// <summary>
        /// Updates the graph refresh indicator to a new position based on the cursor index.
        /// </summary>
        public void UpdateRefreshIndicator()
        {
            var indicatorPointIndex = _refreshCursorPosition + 1;
            var indicatorWidth = _refreshIndicatorWidths[_type];
            var lastValidIndex = _dataPointLimits[_type] - 1;

            InvokeOnMainThread(() =>
            {
                for (var i = 0; i <= indicatorWidth; i++)
                {
                    var isAtEndOfGraph = indicatorPointIndex >= lastValidIndex;
                    if (isAtEndOfGraph) return;

                    _data[indicatorPointIndex].Value = double.NaN;
                    indicatorPointIndex++;
                }
            });
        }

        /// <summary>
        /// Moves the refresh cursor on unit in the positive direction.
        /// </summary>
        public void MoveRefreshCursor()
        {
            _refreshCursorPosition++;
        }

        /// <summary>
        /// Resets the refresh cursor to the starting position, 0.
        /// </summary>
        public void ResetRefreshCursor()
        {
            _refreshCursorPosition = 0;
        }

        /// <summary>
        /// Fills the graph with NaN data. This will be displayed in the graph ui as a blank graph.
        /// </summary>
        private void FillGraphWithNaNData()
        {
            for (uint i = 0; i < _dataPointLimits[_type]; i++) _data.Add(new DataProfile(_type, i, double.NaN));
        }

        /// <summary>
        /// Clears all the data points from the graph data list and resets the graph state to be ready for a new session.
        /// </summary>
        public void ClearGraph()
        {
            // Reset helper index variables. Gets the graph ready for a new sensor connection.
            _refreshCursorPosition = 0;

            // Clear existing data points.
            InvokeOnMainThread(() =>
            {
                _data.Clear();
                FillGraphWithNaNData();
            });
        }
    }
}