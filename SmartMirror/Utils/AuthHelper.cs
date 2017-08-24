using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmartMirror.Utils
{
    public class AuthHelper
    {
        public static string CLIENT_ID = "53280cb3-3587-43fe-a264-234ce1d22524";
        public static string REDIRECT = "https://localhost:44300";
        public static string AUTHORITY = "https://login.microsoftonline.com/common";
        public static string GRAPH_RESOURCE = "https://graph.microsoft.com";

        public static async Task<AuthResult> AcquireTokenByDeviceCodeAsync(DeviceCodeResult device_code)
        {
            AuthResult result = null;
            while (result == null)
            {
                HttpClient client = new HttpClient();
                string payloadString = $"resource={device_code.Resource}&client_id={CLIENT_ID}&grant_type=device_code&code={device_code.DeviceCode}";
                StringContent payload = new StringContent(payloadString, Encoding.UTF8, "application/x-www-form-urlencoded");
                using (var resp = await client.PostAsync("https://login.microsoftonline.com/common/oauth2/token", payload))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<AuthResult>(json);
                    }
                    else
                    {
                        var json = JObject.Parse(await resp.Content.ReadAsStringAsync());
                        if (json.SelectToken("error").Value<string>().Equals("authorization_pending"))
                        {
                            var timer = Task.Delay(1000);
                            await timer;
                        }
                        else if (json.SelectToken("error").Value<string>().Equals("code_expired"))
                        {
                            return null;
                        }
                        else
                            throw new Exception("Unknown error");
                    }
                }
            }
            return result;
        }

        public static async Task<AuthResult> AcquireTokenWithRefreshTokenAsync(string refresh_token, string resource)
        {
            HttpClient client = new HttpClient();
            string payloadString = $"resource={resource}&client_id={CLIENT_ID}&grant_type=refresh_token&refresh_token={refresh_token}&scope=openid";
            StringContent payload = new StringContent(payloadString, Encoding.UTF8, "application/x-www-form-urlencoded");
            using (var resp = await client.PostAsync("https://login.microsoftonline.com/common/oauth2/token", payload))
            {
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    AuthResult result = JsonConvert.DeserializeObject<AuthResult>(json);
                    return result;
                }
                else
                    return null;
            }
        }
    }
}
