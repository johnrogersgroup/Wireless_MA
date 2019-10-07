using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HTTPService
{
    class SyncServicePutInfo
    {
        [JsonProperty(PropertyName = "file_name")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "file_size")]
        public long FileSize { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string DeviceID { get; set; }

        [JsonProperty(PropertyName = "patient_name")]
        public string PatientName { get; set; }

        [JsonProperty(PropertyName = "patient_doctor_user_name")]
        public string PatientDoctorUserName { get; set; }
    }

    class SyncServiceResponse
    {
        [JsonProperty(PropertyName = "file_exists")]
        public bool FileExists { get; set; }
    }


    public class SyncService
    {
        private readonly EndpointConfig config;
        private readonly string endpoint = "/api/sync";

        public SyncService(EndpointConfig c)
        {
            config = c;
        }

        public async Task<bool> SyncFile(Tokens tokens, string fileName, long fileSize, string deviceID, string patientName, string patientDoctorUserName)
        {
            using (var httpClient = new HttpClient { BaseAddress = new Uri(config.BasePath) })
            {
                HashAlgorithm hashAlgorithm = MD5.Create();
                StringBuilder sb = new StringBuilder();
                var hashArray = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(fileName));
                foreach (byte b in hashArray)
                {
                    sb.Append(b);
                }

                var syncServicePutInfo = new SyncServicePutInfo
                {
                    FileName = sb.ToString(),
                    FileSize = fileSize,
                    DeviceID = deviceID,
                    PatientName = patientName,
                    PatientDoctorUserName = patientDoctorUserName
                };

                var jsonSyncPutInfo = JsonConvert.SerializeObject(syncServicePutInfo);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", tokens.AccessToken);
                var content = new StringContent(jsonSyncPutInfo.ToString(), Encoding.UTF8, "application/json");

                using (var resp = await httpClient.PostAsync(endpoint, content))
                {
                    if (resp.IsSuccessStatusCode) {
                        var doesStringExist = await resp.Content.ReadAsStringAsync();
                        SyncServiceResponse url = JsonConvert.DeserializeObject<SyncServiceResponse>(doesStringExist);
                        return url.FileExists;
                    }
                    else {
                        return false;
                    }
                }
            }
        }
    }
}
