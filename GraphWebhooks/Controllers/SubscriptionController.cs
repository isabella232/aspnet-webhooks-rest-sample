/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

using System;
using System.Web.Mvc;
using GraphWebhooks.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GraphWebhooks.Helpers;
using System.Security.Claims;
using System.Collections.Generic;

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
            HttpResponseMessage response;
            try
            {
                response = await SubscriptionHelper.CreateSubscription();
            }
            catch (Exception e)
            {
                ViewBag.Message = BuildErrorMessage(e);
                return View("Error", e);
            }

            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Error", new { message = response.StatusCode, debug = await response.Content.ReadAsStringAsync() });
            }

            string stringResult = await response.Content.ReadAsStringAsync();
            SubscriptionViewModel viewModel = new SubscriptionViewModel()
            {
                Subscription = JsonConvert.DeserializeObject<Subscription>(stringResult)
            };

            return View("Subscription", viewModel);
        }

        // Delete the current webhooks subscription and sign out the user.
        [Authorize]
        public async Task<ActionResult> DeleteSubscription()
        {
            var subscriptions = SubscriptionCache.GetSubscriptionCache().DeleteAllSubscriptions();

            foreach (var subscription in subscriptions)
            {
                HttpResponseMessage response = await SubscriptionHelper.DeleteSubscription(subscription.Key);

                if (!response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Error", new { message = response.StatusCode, debug = response.Content.ReadAsStringAsync() });
                }
            }

            return RedirectToAction("SignOut", "Account");
        }

        public string BuildErrorMessage(Exception e)
        {
            string message = e.Message;
            if (e is AdalSilentTokenAcquisitionException) message = "Unable to get an access token. You may need to sign in again.";
            return message;
        }
    }
}