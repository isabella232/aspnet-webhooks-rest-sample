
namespace GraphWebhooks.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class GraphHttpClient
    {
        private static readonly int MaxRetryAttempts = 5;

        private string accessToken = null;

        private HttpClient httpClient = null;

        public GraphHttpClient(string accessToken)
        {
            this.accessToken = accessToken;
            BuildGraphHttpClient(accessToken);
        }

        // Send HTTP request implementation with retries on transient failures.
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            var attempt = 0;
            do
            {
                attempt++;

                Trace.WriteLine("Request begin UTC time is : " + DateTime.UtcNow);
                response = await httpClient.SendAsync(request);

                // Log requestId and timestamp for contacting MS Graph support.
                Trace.WriteLine("Request end UTC time is : " + DateTime.UtcNow);
                IEnumerable<string> requestIds = response.Headers.GetValues("request-id");
                foreach (string requestId in requestIds)
                {
                    Trace.WriteLine("request-id value is : " + requestId);
                }

                if (((int)response.StatusCode == 429) || ((int)response.StatusCode == 503))
                {
                    // Retry Only After the server specified time period obtained from the response.
                    TimeSpan pauseDuration = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    TimeSpan serverRecommendedPauseDuration = GetServerRecommendedPause(response);
                    if (serverRecommendedPauseDuration > pauseDuration)
                    {
                        pauseDuration = serverRecommendedPauseDuration;
                    }
                    await Task.Delay(pauseDuration);

                    // Create a new HttpClient in case of retries by disposing existing client.
                    BuildGraphHttpClient(accessToken);
                }
                else
                {
                    return response;
                }
            } while (attempt <= MaxRetryAttempts);

            return response;
        }

        private void BuildGraphHttpClient(string accessToken)
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null;
            }

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private TimeSpan GetServerRecommendedPause(HttpResponseMessage response)
        {
            var retryAfter = response?.Headers?.RetryAfter;
            if (retryAfter == null)
                return TimeSpan.Zero;

            return retryAfter.Date.HasValue
                ? retryAfter.Date.Value - DateTime.UtcNow
                : retryAfter.Delta.GetValueOrDefault(TimeSpan.Zero);
        }
    }
}