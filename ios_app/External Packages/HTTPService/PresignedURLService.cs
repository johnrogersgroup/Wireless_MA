using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HTTPService
{
    class PresignedURLPostInfo
    {
        [JsonProperty(PropertyName = "bucket_name")]
        public string BucketName { get; set; }

        [JsonProperty(PropertyName = "device_id")]
        public string DeviceID { get; set; }

        [JsonProperty(PropertyName = "file_extension")]
        public string FileExtension { get; set; }

        [JsonProperty(PropertyName = "patient_name")]
        public string PatientName { get; set; }

        [JsonProperty(PropertyName = "patient_doctor_user_name")]
        public string PatientDoctorUserName { get; set; }

        [JsonProperty(PropertyName = "file_name")]
        public string FileName { get; set; }
    }

    public class PresignedURLService
    {
        private readonly EndpointConfig config;
        private readonly string endpoint = "/api/generate_s3_presigned_url";


        public PresignedURLService(EndpointConfig c)
        {
            config = c;
        }

        public async Task<PresignedURL> PerformPresignedURLFetch(Tokens tokens, string bucketName, string deviceID, string fileExt,
            string patientName, string doctorUsername, string fileName)
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

                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var presignedURLInfo = new PresignedURLPostInfo() { BucketName = bucketName, DeviceID = deviceID, FileExtension = fileExt,
                    PatientName = patientName, PatientDoctorUserName = doctorUsername, FileName = sb.ToString()};

                var jsonPresignedURLInfo = JsonConvert.SerializeObject(presignedURLInfo);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", tokens.AccessToken);
                var content = new StringContent(jsonPresignedURLInfo.ToString(), Encoding.UTF8, "application/json");

                using (var resp = await httpClient.PostAsync(endpoint, content))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var presignedURL = await resp.Content.ReadAsStringAsync();
                        PresignedURL url = JsonConvert.DeserializeObject<PresignedURL>(presignedURL);

                        return url;
                    }
                    else { return null; }
                }
            }
        }
    }
}
