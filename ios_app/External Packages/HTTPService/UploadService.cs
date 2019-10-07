using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HTTPService
{
    public class UploadService
    {
        private readonly EndpointConfig config;

        public static async Task<bool> PerformUploadS3DirectAsync(Stream stream, PresignedURL url)
        {
            HttpWebRequest httpRequest = WebRequest.Create(url.UploadURL) as HttpWebRequest;
            httpRequest.Method = "PUT";

            using (Stream uploadStream = httpRequest.GetRequestStream())
            {
                var buffer = new byte[1000000];
                int bytesRead = 0;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await uploadStream.WriteAsync(buffer, 0, bytesRead);
                }
            }

            HttpWebResponse response = await httpRequest.GetResponseAsync() as HttpWebResponse;

            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
        }
    }
}
