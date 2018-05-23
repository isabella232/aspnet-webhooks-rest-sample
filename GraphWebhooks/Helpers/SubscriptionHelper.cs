using GraphWebhooks.Models;
using Microsoft.Graph;
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
        internal static string CurrentUserId
        {
            get
            {
                return ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            }
        }

        internal static async Task<Subscription> CreateSubscription(string baseUrl, string userId = null)
        {
            var graphClient = GraphHelper.GetAuthenticatedClient(string.IsNullOrEmpty(userId) ? CurrentUserId : userId, baseUrl);

            var subscription = new Subscription
            {
                Resource = "me/mailFolders('Inbox')/messages",
                ChangeType = "created",
                NotificationUrl = ConfigurationManager.AppSettings["ida:NotificationUrl"],
                ClientState = Guid.NewGuid().ToString(),
                ExpirationDateTime = DateTime.UtcNow + new TimeSpan(0, 0, 15, 0) // shorter duration useful for testing
            };

            var newSubscription = await graphClient.Subscriptions.Request().AddAsync(subscription);

            // This sample temporarily stores the current subscription ID, client state, and user object ID.
            // This info is required so the NotificationController, which is not authenticated, can retrieve
            // an access token from the cache and validate the subscription.
            // Production apps typically use some method of persistent storage.
            var subscriptionDetails = new SubscriptionDetails(
                    newSubscription.Id,
                    newSubscription.ClientState,
                    CurrentUserId,
                    baseUrl);

            SubscriptionCache.GetSubscriptionCache().SaveSubscriptionInfo(subscriptionDetails);

            return newSubscription;

            //SubscriptionStore.SaveSubscriptionInfo(newSubscription.Id,
            //    newSubscription.ClientState,
            //    ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value);

            //// Send the `POST subscriptions` request and parse the response. 
            //HttpResponseMessage response = await HttpHelper.SendAsync(subscriptionsEndpoint, HttpMethod.Post, subscription);

            //if (!response.IsSuccessStatusCode)
            //{
            //    return response;
            //}

            //string stringResult = await response.Content.ReadAsStringAsync();
            //var createdSubscription = JsonConvert.DeserializeObject<Subscription>(stringResult);
            //var subscriptionDetails = new SubscriptionDetails(
            //        createdSubscription.Id,
            //        createdSubscription.ClientState,
            //        ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value,
            //        ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value);

            //// This sample temporarily stores the current subscription ID, client state, user object ID, and tenant ID. 
            //// This info is required so the NotificationController, which is not authenticated, can retrieve an access token from the cache and validate the subscription.
            //// Production apps typically use some method of persistent storage.
            //SubscriptionCache.GetSubscriptionCache().SaveSubscriptionInfo(subscriptionDetails);

            //return response;
        }


        internal static async Task<Subscription> RenewSubscription(string subscriptionId, string userId, string baseUrl)
        {
            var graphClient = GraphHelper.GetAuthenticatedClient(userId, baseUrl);

            Subscription subscription = new Subscription
            {
                ExpirationDateTime = DateTime.UtcNow + new TimeSpan(0, 0, 15, 0) // shorter duration useful for testing
            };

            return await graphClient.Subscriptions[subscriptionId].Request().UpdateAsync(subscription);
        }
        
        internal static async Task<Subscription> CheckSubscription(string subscriptionId, string userId, string baseUrl)
        {
            var graphClient = GraphHelper.GetAuthenticatedClient(userId, baseUrl);
            return await graphClient.Subscriptions[subscriptionId].Request().GetAsync();
        }

        internal static async Task DeleteSubscription(string subscriptionId, string baseUrl)
        {
            var graphClient = GraphHelper.GetAuthenticatedClient(CurrentUserId, baseUrl);
            await graphClient.Subscriptions[subscriptionId].Request().DeleteAsync();
        }
    }
}