/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

using GraphWebhooks.TokenStorage;
using Microsoft.Identity.Client;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GraphWebhooks.Helpers
{
    class AuthHelper
    {
        private static string appId = Startup.ClientId;
        private static string appSecret = Startup.ClientSecret;
        public static string[] scopes = Startup.Scopes;

        // Used by SubscriptionController to get an access token from the cache.
        public static async Task<string> GetAccessTokenAsync(string redirect)
        {        
            string userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            return await GetAccessTokenForSubscriptionAsync(userObjectId, redirect);
        }

        // Used by NotificationController (which is not authenticated) to get an access token from the cache.
        public static async Task<string> GetAccessTokenForSubscriptionAsync(string userObjectId, string redirect)
        {
            var tokenCache = new SampleTokenCache(userObjectId);

            var cca = new ConfidentialClientApplication(appId, redirect, new ClientCredential(appSecret),
                tokenCache.GetMsalCacheInstance(), null);

            var authResult = await cca.AcquireTokenSilentAsync(scopes, cca.Users.First());

            return authResult.AccessToken;
        }
    }
}
