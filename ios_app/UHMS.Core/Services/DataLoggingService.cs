using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MvvmCross.Logging;
using UHMS.Core.Models.Data;
using System.Diagnostics;

namespace UHMS.Core.Services
{
    public class DataLoggingService : IDataLoggingService
    {
        private readonly IMvxLog _log;
        private bool _objectOpen = false;
        private Dictionary<int, FileStream> FileStreams { get; set; }
        private string _dataLogPath;
        private Stopwatch _stopWatch;
        private double _lastTimestamp = 0;
        private const int SYNC_FREQ = 100;
        private int _sync_cnt = 0;
        //private readonly int _syncTimer = 1000;

        //private System.Timers.Timer consumerTimer;
        BlockingCollection<double> timeStampBlockingCollection;
        public DataLoggingService(IMvxLog log)
        {
            _log = log;
            FileStreams = new Dictionary<int, FileStream>();
           timeStampBlockingCollection = new BlockingCollection<double>();

            // Setup stopwatch.
            _stopWatch = new Stopwatch();
            _stopWatch.Start();

            //Task.Run(() => ProcessCollectionAsync(0));
            //Task.Run(() => ProcessCollectionAsync(1));
            Task.Run(() => ProcessTimeSyncCollectionAsync());

            _log.Debug("Data logging service loaded.");
        }

        private void SyncTimer(object sender, ElapsedEventArgs e)
        {

        }

        public async Task Open(long epochTime)
        {
            if (_objectOpen)
                return;
            _log.Debug("Logger opened");
            await CreateNewLogFile(epochTime);
        }

        public async Task Close()
        {
            if (!_objectOpen)
                return;

            _objectOpen = false;

            foreach (var stream in FileStreams)
            {
                FileStreams[stream.Key].Close();
                _log.Debug($"Stream with {stream.Key} as a key is being closed.");
            }
            FileStreams.Clear();
        }

        private async Task CreateNewLogFile(long epochTime)
        {
            var createdTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local).ToString("yy-MM-dd-HH_mm_ss");

            _dataLogPath = Path.Combine($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Downloads", createdTime);
            _log.Debug($"Data log path is : {_dataLogPath}");

            // Creates directory if it does not exists.
            Directory.CreateDirectory(_dataLogPath);

            //for (int i = 0; i < 2; i++)
            //{
            //    var deviceType = (i == 0) ? "left" : "right";
            //    var fileName = $"dev_{deviceType}.tsv";
            //    AddOrUpdateFileStream(i, $"{_dataLogPath}/{fileName}");
            //    var header = $"local time\taccel x\taccel y\taccel z\n";
            //    await WriteStringAsync(header, FileStreams[i]);
            //}

            // Add event Log
            AddOrUpdateFileStream(2, $"{_dataLogPath}/event.tsv");
            var eheader = $"local time\tevent\n";
            await WriteStringAsync(eheader, FileStreams[2]);
            _objectOpen = true;
            //_stopWatch.Restart();
            //_repeatedTimer.Start();
        }

        /// <summary>
        /// Adds or updates filestream in variable <c>FileStreams</c>.
        /// </summary>
        /// <param name="type">Key to find which filestream should be updated.</param>
        /// <param name="path">Path of new file that is being written.</param>
        private async void AddOrUpdateFileStream(int type, string path)
        {
            _log.Debug($"Following file is being updated: {path}");
            if (!File.Exists(path))
            {
                if (FileStreams.ContainsKey(type))
                {
                    await FileStreams[type].FlushAsync();
                    FileStreams[type] = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true);

                }
                else
                {
                    FileStreams.Add(type, new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true));
                }
            }
            else
            {
                _log.Debug("File already exists.");
            }
        }

        /// <summary>
        /// Writes given string content into the stream.
        /// </summary>
        /// <param name="content">Content to be written on streamed file.</param>
        /// <param name="stream">Stream that is being written.</param>
        private async Task WriteStringAsync(string content, FileStream stream)
        {
            var cont = new UTF8Encoding(true).GetBytes(content);
            await stream.WriteAsync(cont, 0, cont.Length);
        }

        private void WriteString(string content, FileStream stream)
        {
            var cont = new UTF8Encoding(true).GetBytes(content);
            stream.Write(cont, 0, cont.Length);
        }

        public void UpdateTimeStamp(double timeStamp)
        {
            timeStampBlockingCollection.Add(timeStamp);
        }

        private void ProcessTimeSyncCollectionAsync()
        {
            _log.Debug("Came to Process Collection Async");

            foreach (var timestamp in timeStampBlockingCollection.GetConsumingEnumerable())
            {
                    _stopWatch.Restart();
                    _lastTimestamp = timestamp;

            }
        }

        public void WriteEvent(int eventNum)
        {

            if (_objectOpen)
            {
                // so that we can have an accurate time
                var cur_timestamp = _lastTimestamp + _stopWatch.ElapsedMilliseconds;
                var dataString = $"{cur_timestamp}\t{eventNum}\n";
                WriteString(dataString, FileStreams[2]);
            }
        }

        public void WritePackage(Dictionary<DataType, List<DataProfile>> dataProfiles, int deviceType)
        {
            if (_objectOpen)
            {
                var dataString = "";
                try
                {
                    for (int i = 0; i < 24; i++)
                    {
                        var timeStamp = dataProfiles[DataType.timestamp][0].Value - (23 - i) * 1.25;

                        if (timeStamp < 0) continue;

                        dataString += $"{timeStamp}\t";
                        if (i % 8 == 0)
                        {
                            dataString += $"{dataProfiles[DataType.accl_x][i / 8].Value}\t";
                            dataString += $"{dataProfiles[DataType.accl_y][i / 8].Value}\t";
                            dataString += $"{dataProfiles[DataType.accl_z][i].Value}\n";
                        }
                        else
                        {
                            dataString += $"\t\t{dataProfiles[DataType.accl_z][i].Value}\n";
                        }

                    }

                    WriteString(dataString, FileStreams[deviceType]);
                    //_log.Debug("File Writing");
                }
                catch (Exception e)
                {
                    _log.Debug(e.ToString());
                }
            }
        }

        public void ParsePage(byte[] data, FileStream fileStream)
        {
            if (data.Length != 2048)
            {
                _log.Debug("Data exceeds or less to be processed.");
                return;
            }

            // Getting information from header.
            var dataType = data[0];
            var dataWidth = data[1];
            var magic_num = ((data[3] << 8) + data[2]);
            var epochTime = ((data[7] << 24) + (data[6] << 16) + (data[5] << 8) + data[4]) / 4.0;

            if(magic_num != 0x374A)
            {
                _log.Debug($"MAGIC Number not matching {magic_num}");
                return;
            }
            // Parsing data for each data types(accl.x, accl.y, accl.z) into string.
            var body = $"";
            int counter = 8;

            if(dataType == 2)
            {
                for (int i = 0; i < 170; i++)
                {

                    body += (epochTime - (170 - 1 - i) * 5.00488) + "\t";

                    body += $"{(short)(data[counter] + (data[counter + 1] << 8))}\t";
                    body += $"{(short)(data[counter + 2] + (data[counter + 3] << 8))}\t";
                    body += $"{(short)(data[counter + 4] + (data[counter + 5] << 8))}\t";
                    body += $"{(short)(data[counter + 6] + (data[counter + 7] << 8))}\t";
                    body += $"{(short)(data[counter + 8] + (data[counter + 9] << 8))}\t";
                    body += $"{(short)(data[counter + 10] + (data[counter + 11] << 8))}\n";
                    counter += 12;

                }
            }
            else
            {
                for (int i = 0; i < (data.Length - 8) / 20 * 8; i++)
                {
                    var cur_time = (epochTime - ((data.Length - 8) / 20 * 8 - i - 1) * 0.61035);

                    body += cur_time + "\t";

                    if (i % 8 == 7)
                    {
                        body += $"{(short)(data[counter + 2] + (data[counter + 3] << 8))}\t"; // x
                        body += $"{(short)(data[counter + 4] + (data[counter + 5] << 8))}\t"; //y
                        body += $"{(short)(data[counter] + (data[counter + 1] << 8))}\n"; //z
                        counter += 6;
                    }
                    else
                    {
                        body += $"\t\t{(short)(data[counter++] + (data[counter++] << 8))}\n";
                    }

                }
            }

            var cont = new UTF8Encoding(true).GetBytes(body);
            fileStream.Write(cont, 0, cont.Length);
        }

        
        public void WriteHeader(FileStream fileStream)
        {
            var header = $"local time\taccel x\taccel y\taccel z\tgyro x\tgyro y\tgyro z\n";
            var content = new UTF8Encoding(true).GetBytes(header);
            fileStream.Write(content, 0, content.Length);
        }

        public FileStream GetFileStream(uint epochTime, byte deviceType)
        {
            // Convert epoque time to date time
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            var createdTime = dateTime.AddSeconds(epochTime).ToString("yy-MM-dd-HH_mm_ss");

            var downloadPath = Path.Combine($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Downloads", createdTime);
            Directory.CreateDirectory(downloadPath);
            var fileName = "dev_" + (deviceType == 0x00 ? "left" : "right") + ".tsv"; //dev_left.tsv or dev_right.tsv
            var fileStream = new FileStream($"{downloadPath}/{fileName}", FileMode.OpenOrCreate);
            return fileStream;
        }
    }
}
