/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

using GraphWebhooks.Helpers;
using GraphWebhooks.Models;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GraphWebhooks.Controllers
{
    public class SubscriptionController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        // Create a webhook subscription.
        [Authorize]
        public async Task<ActionResult> CreateSubscription()
        {
            string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}";
            string userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            var graphClient = GraphHelper.GetAuthenticatedClient(userObjectId, baseUrl);

            var subscription = new Subscription
            {
                Resource = "me/mailFolders('Inbox')/messages",
                ChangeType = "created",
                NotificationUrl = ConfigurationManager.AppSettings["ida:NotificationUrl"],
                // Include baseUrl as part of state (so we can use this in the notification
                // to get an access token)
                ClientState = $"{Guid.NewGuid().ToString()}+{baseUrl}",
                ExpirationDateTime = DateTime.UtcNow + new TimeSpan(0, 0, 15, 0) // shorter duration useful for testing
            };

            try
            {
                var newSubscription = await graphClient.Subscriptions.Request().AddAsync(subscription);

                SubscriptionViewModel viewModel = new SubscriptionViewModel
                {
                    Subscription = newSubscription
                };

                // This sample temporarily stores the current subscription ID, client state, user object ID, and tenant ID. 
                // This info is required so the NotificationController, which is not authenticated, can retrieve an access token from the cache and validate the subscription.
                // Production apps typically use some method of persistent storage.
                SubscriptionStore.SaveSubscriptionInfo(newSubscription.Id,
                    newSubscription.ClientState,
                    ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value,
                    ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value);

                // This sample just saves the current subscription ID to the session so we can delete it later.
                Session["SubscriptionId"] = newSubscription.Id;
                return View("Subscription", viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Message = BuildErrorMessage(ex);
                return View("Error", ex);
            }
        }

        // Delete the current webhooks subscription and sign out the user.
        [Authorize]
        public async Task<ActionResult> DeleteSubscription()
        {
            string subscriptionId = (string)Session["SubscriptionId"];
            string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}";
            string userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            var graphClient = GraphHelper.GetAuthenticatedClient(userObjectId, baseUrl);

            try
            {
                await graphClient.Subscriptions[subscriptionId].Request().DeleteAsync();
                Session.Remove("SubscriptionId");
                return RedirectToAction("SignOut", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.Message = BuildErrorMessage(ex);
                return View("Error", ex);
            }
        }

        public string BuildErrorMessage(Exception e)
        {
            string message = e.Message;
            if (e is MsalUiRequiredException) message = "Unable to get an access token. You may need to sign in again.";
            return message;
        }
    }
}