/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

using System;
using System.Web;
using System.Web.Mvc;
using GraphWebhooks.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GraphWebhooks.Utils;

namespace GraphWebhooks.Controllers
{
    public class SubscriptionController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        // Create a webhook subscription.
        [Authorize, HandleAdalException]
        public async Task<ActionResult> CreateSubscription()
        {
            // Get an access token and add it to the client. 
            // This sample stores the refreshToken, so get the AuthenticationResult that has the access token and refresh token.
            AuthenticationResult authResult = await AuthHelper.GetAccessTokenAsync();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Build the request.
            // This sample subscribes to get notifications when the user receives an email.
            string subscriptionsEndpoint = "https://graph.microsoft.com/v1.0/subscriptions/";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, subscriptionsEndpoint);
            var subscription = new Subscription
            {
                Resource = "me/mailFolders('Inbox')/messages",
                ChangeType = "created",
                NotificationUrl = ConfigurationManager.AppSettings["ida:NotificationUrl"],
                ClientState = Guid.NewGuid().ToString(),
                ExpirationDateTime = DateTime.UtcNow + new TimeSpan(0, 0, 4230, 0)
            };

            string contentString = JsonConvert.SerializeObject(subscription, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.Content = new StringContent(contentString, System.Text.Encoding.UTF8, "application/json");

            // Send the request and parse the response.
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

                // Parse the JSON response.
                string stringResult = await response.Content.ReadAsStringAsync();
                SubscriptionViewModel viewModel = new SubscriptionViewModel
                {
                    Subscription = JsonConvert.DeserializeObject<Subscription>(stringResult)
                };

                // This app temporarily stores the current subscription ID, refresh token, and client state. 
                // These are required so the NotificationController, which is not authenticated, can retrieve an access token keyed from the subscription ID.
                // Production apps typically use some method of persistent storage.
                HttpRuntime.Cache.Insert("subscriptionId_" + viewModel.Subscription.Id,
                    Tuple.Create(viewModel.Subscription.ClientState, authResult.RefreshToken), null, DateTime.MaxValue, new TimeSpan(24, 0, 0), System.Web.Caching.CacheItemPriority.NotRemovable, null);

                // Save the latest subscription ID, so we can delete it later.
                Session["SubscriptionId"] = viewModel.Subscription.Id;
                return View("Subscription", viewModel);
            }
            else
            {
                return RedirectToAction("Index", "Error", new { message = response.StatusCode, debug = await response.Content.ReadAsStringAsync() });
            }

        }

        // Delete the current webhooks subscription and sign out the user.
        [Authorize, HandleAdalException]
        public async Task<ActionResult> DeleteSubscription()
        {
            string subscriptionId = (string)Session["SubscriptionId"];

            if (!string.IsNullOrEmpty(subscriptionId))
            {
                string serviceRootUrl = "https://graph.microsoft.com/v1.0/subscriptions/";

                // Get an access token and add it to the client.
                AuthenticationResult authResult = await AuthHelper.GetAccessTokenAsync();

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Send the 'DELETE /subscriptions/id' request.
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, serviceRootUrl + subscriptionId);
                HttpResponseMessage response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Error", new { message = response.StatusCode, debug = response.Content.ReadAsStringAsync() });
                }
            }
            return RedirectToAction("SignOut", "Account");
        }
    }
}