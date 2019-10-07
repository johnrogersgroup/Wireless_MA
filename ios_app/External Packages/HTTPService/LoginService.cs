using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HTTPService
{

    class LoginPostInfo
    {
        [JsonProperty(PropertyName = "user_name")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
    }

    public class LoginService
    {
        private readonly EndpointConfig config;
        private readonly string endpoint = "/login";


        public LoginService(EndpointConfig c)
        {
            config = c;
        }

        public async Task<Tokens> PerformLoginAsync(string username, string password)
        {
            using (var httpClient = new HttpClient { BaseAddress = new Uri(config.BasePath) })
            {
                var user = new LoginPostInfo() { UserName = username, Password = password };
                var jsonUser = JsonConvert.SerializeObject(user);
                var content = new StringContent(jsonUser.ToString(), Encoding.UTF8, "application/json");


                using (var resp = await httpClient.PostAsync(endpoint, content))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var tokensJSON = await resp.Content.ReadAsStringAsync();
                        Tokens tokens = JsonConvert.DeserializeObject<Tokens>(tokensJSON);

                        return tokens;
                    }
                    else { return null; }
                }
            }
        }
    }
}
