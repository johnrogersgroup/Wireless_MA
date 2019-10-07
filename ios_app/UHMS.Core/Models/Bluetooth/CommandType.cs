using System;
namespace UHMS.Core.Models.Bluetooth
{
    public enum CommandType
    {
        NoCommand,
        StartSession,
        StopSession,
        GetInfo,
        StartSessionRead
    }
}
