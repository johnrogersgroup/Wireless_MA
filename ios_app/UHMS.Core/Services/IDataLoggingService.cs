using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UHMS.Core.Models.Bluetooth;
using UHMS.Core.Models.Data;

namespace UHMS.Core.Services
{
    public interface IDataLoggingService
    {
        Task Open(long epochTime);

        Task Close();

        void UpdateTimeStamp(double timeStamp);
        void WriteEvent(int eventNum);

        void ParsePage(byte[] data, FileStream fileStream);

        void WriteHeader(FileStream fileStream);

        FileStream GetFileStream(uint epochTime, byte deviceType);
    }
}
