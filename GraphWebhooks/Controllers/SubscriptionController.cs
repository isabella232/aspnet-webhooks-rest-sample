/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

using System;
using System.Web;
using System.Web.Mvc;
using GraphWebhooks.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GraphWebhooks.Controllers
{
    public class SubscriptionController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        // Create webhook subscriptions.
        [Authorize]
        public async Task<ActionResult> CreateSubscription()
        {

            // Get an access token and add it to the client.
            AuthenticationResult authResult = null;
            try
            {
                string userObjId = System.Security.Claims.ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                string tenantId = System.Security.Claims.ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                string authority = string.Format(System.Globalization.CultureInfo.InvariantCulture, ConfigurationManager.AppSettings["ida:AADInstance"], tenantId);
                AuthenticationContext authContext = new AuthenticationContext(authority, false);
                ClientCredential credential = new ClientCredential(ConfigurationManager.AppSettings["ida:ClientId"], ConfigurationManager.AppSettings["ida:ClientSecret"]);
                try
                {
                    authResult = await authContext.AcquireTokenSilentAsync("https://graph.microsoft.com", credential,
                                    new UserIdentifier(userObjId, UserIdentifierType.UniqueId));
                }
                catch (AdalSilentTokenAcquisitionException)
                {
                    Request.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Subscription/CreateSubscription" },
                                                OpenIdConnectAuthenticationDefaults.AuthenticationType);
                    return new EmptyResult();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { message = ex.Message, debug = ex.StackTrace });
            }

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Build the request.
            // This sample subscribes to get notifications when the user receives an email.
            string subscriptionsEndpoint = "https://graph.microsoft.com/stagingbeta/subscriptions/";
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

                // Save the latest subscription ID, so we can delete it later and filter the view on it.
                Session["SubscriptionId"] = viewModel.Subscription.Id;
                return View("Subscription", viewModel);

            }
            else
            {
                return RedirectToAction("Index", "Error", new { message = response.StatusCode, debug = await response.Content.ReadAsStringAsync() });
            }

        }

        // Delete the current webhooks subscription and sign out the user.
        [Authorize]
        public async Task<ActionResult> DeleteSubscription()
        {
            string subscriptionId = (string)Session["SubscriptionId"];

            if (!string.IsNullOrEmpty(subscriptionId))
            {
                string serviceRootUrl = "https://graph.microsoft.com/stagingbeta/subscriptions/";

                // Get an access token and add it to the client.
                string accessToken;
                try
                {
                    string userObjId = System.Security.Claims.ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                    string tenantId = System.Security.Claims.ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                    string authority = string.Format(System.Globalization.CultureInfo.InvariantCulture, ConfigurationManager.AppSettings["ida:AADInstance"], tenantId);
                    AuthenticationContext authContext = new AuthenticationContext(authority, false);
                    ClientCredential credential = new ClientCredential(ConfigurationManager.AppSettings["ida:ClientId"], ConfigurationManager.AppSettings["ida:ClientSecret"]);
                    AuthenticationResult authResult = null;
                    try
                    {
                        authResult = await authContext.AcquireTokenSilentAsync("https://graph.microsoft.com", credential,
                                        new UserIdentifier(userObjId, UserIdentifierType.UniqueId));
                    }
                    catch (AdalSilentTokenAcquisitionException)
                    {
                        Request.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Subscription/DeleteSubscription" },
                                                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
                        return new EmptyResult();
                    }
                    accessToken = authResult?.AccessToken;

                }
                catch (Exception ex)
                {
                    return RedirectToAction("Index", "Error", new { message = ex.Message, debug = "" });
                }

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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