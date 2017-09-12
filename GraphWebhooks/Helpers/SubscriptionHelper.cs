using GraphWebhooks.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace GraphWebhooks.Helpers
{
    public class SubscriptionHelper
    {
        internal static async Task<HttpResponseMessage> CreateSubscription()
        {
            string subscriptionsEndpoint = "https://graph.microsoft.com/v1.0/subscriptions/";

            Subscription subscription = new Subscription
            {
                Resource = "me/mailFolders('Inbox')/messages",
                ChangeType = "created",
                NotificationUrl = ConfigurationManager.AppSettings["ida:NotificationUrl"],
                ClientState = Guid.NewGuid().ToString(),
                //ExpirationDateTime = DateTime.UtcNow + new TimeSpan(0, 0, 4230, 0) // current maximum timespan for messages
                ExpirationDateTime = DateTime.UtcNow + new TimeSpan(0, 0, 15, 0) // shorter duration useful for testing
            };

            // Send the `POST subscriptions` request and parse the response. 
            HttpResponseMessage response = await HttpHelper.SendAsync(subscriptionsEndpoint, HttpMethod.Post, subscription);

            if (!response.IsSuccessStatusCode)
            {
                return response;
            }

            string stringResult = await response.Content.ReadAsStringAsync();
            var createdSubscription = JsonConvert.DeserializeObject<Subscription>(stringResult);
            var subscriptionDetails = new SubscriptionDetails(
                    createdSubscription.Id,
                    createdSubscription.ClientState,
                    ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value,
                    ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value);

            // This sample temporarily stores the current subscription ID, client state, user object ID, and tenant ID. 
            // This info is required so the NotificationController, which is not authenticated, can retrieve an access token from the cache and validate the subscription.
            // Production apps typically use some method of persistent storage.
            SubscriptionCache.GetSubscriptionCache().SaveSubscriptionInfo(subscriptionDetails);

            return response;
        }


        internal static async Task<HttpResponseMessage> RenewSubscription(string subscriptionId)
        {

            string subscriptionsEndpoint = "https://graph.microsoft.com/v1.0/subscriptions/" + subscriptionId;

            Subscription subscription = new Subscription
            {
                ExpirationDateTime = DateTime.UtcNow + new TimeSpan(0, 0, 5, 0) // shorter duration useful for testing
            };

            // Send the `POST subscriptions` request and parse the response.
            HttpResponseMessage response = await HttpHelper.SendAsync(subscriptionsEndpoint, new HttpMethod("PATCH"), subscription);
            response.EnsureSuccessStatusCode();
            return response;
        }
        
        internal static async Task<HttpResponseMessage> CheckSubscription(string subscriptionId)
        {
            string subscriptionsEndpoint = "https://graph.microsoft.com/v1.0/subscriptions/" + subscriptionId;

            HttpResponseMessage response = await HttpHelper.SendAsync(subscriptionsEndpoint, HttpMethod.Get);
            return response;
        }

        internal static async Task<HttpResponseMessage> DeleteSubscription(string subscriptionId)
        {
            string subscriptionsEndpoint = "https://graph.microsoft.com/v1.0/subscriptions/" + subscriptionId;

            HttpResponseMessage response = await HttpHelper.SendAsync(subscriptionsEndpoint, HttpMethod.Delete);
            return response;
        }
    }
}