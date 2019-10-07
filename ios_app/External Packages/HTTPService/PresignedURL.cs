using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace HTTPService
{
    public class PresignedURL
    {
        [JsonProperty(PropertyName = "expiration_time")]
        public string ExpirationTime { get; set; }

        [JsonProperty(PropertyName = "upload_url")]
        public string UploadURL { get; set; }
    }
}
