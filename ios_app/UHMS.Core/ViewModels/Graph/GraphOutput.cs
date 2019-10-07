using System;
using UHMS.Core.Models.Data;

namespace UHMS.Core.ViewModels
{
    public struct GraphOutput
    {
        public readonly int DeviceSlotIndex;

        public readonly int SyncGroupIndex;

        public readonly DataType OutputType;

        public readonly string UIBindingName;

        public GraphOutput(int deviceSlotIndex, int syncGroupIndex, DataType outputType, string uiBindingName)
        {
            DeviceSlotIndex = deviceSlotIndex;
            SyncGroupIndex = syncGroupIndex;
            OutputType = outputType;
            UIBindingName = uiBindingName;
        }


    }
}