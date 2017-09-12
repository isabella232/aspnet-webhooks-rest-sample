using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace GraphWebhooks.Helpers
{
    public class HttpHelper
    {
        internal static async Task<HttpResponseMessage> SendAsync(string endpoint, HttpMethod httpMethod, object content = null)
        {
            // Get an access token.
            string accessToken = await AuthHelper.GetAccessTokenAsync();

            // Build the request.
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // This sample subscribes to get notifications when the user receives an email.
                HttpRequestMessage request = new HttpRequestMessage(httpMethod, endpoint);

                if (content != null)
                {
                    string contentString = JsonConvert.SerializeObject(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    request.Content = new StringContent(contentString, System.Text.Encoding.UTF8, "application/json");                    
                }

                return await client.SendAsync(request);
            }
        }
    }
}