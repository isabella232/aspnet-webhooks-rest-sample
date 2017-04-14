/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Claims;
using System.Threading.Tasks;
using GraphWebhooks.TokenStorage;

namespace GraphWebhooks.Helpers
{
    class AuthHelper
    {
        private static SampleTokenCache tokenCache;
        private static string aadInstance = Startup.AadInstance;
        private static string appId = Startup.ClientId;
        private static string appSecret = Startup.ClientSecret;
        private static string graphResourceId = Startup.GraphResourceId;

        // Used by SubscriptionController to get an access token from the cache.
        public static async Task<string> GetAccessTokenAsync()
        {        
            string userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            string tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            string authority = $"{ aadInstance }/{ tenantId }";
            tokenCache = new SampleTokenCache(userObjectId);

            AuthenticationContext authContext = new AuthenticationContext(authority, tokenCache);
            try
            {
                AuthenticationResult authResult = await authContext.AcquireTokenSilentAsync(
                    graphResourceId,
                    new ClientCredential(appId, appSecret),
                    new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));

                return authResult.AccessToken;
            }
            catch (AdalException e)
            {
                throw e;
            }
        }

        // Used by NotificationController (which is not authenticated) to get an access token from the cache.
        public static async Task<string> GetAccessTokenForSubscriptionAsync(string userObjectId, string tenantId)
        {
            string authority = $"{ aadInstance }/{ tenantId }";
            tokenCache = new SampleTokenCache(userObjectId);

            AuthenticationContext authContext = new AuthenticationContext(authority, tokenCache);
            try
            {
                AuthenticationResult authResult = await authContext.AcquireTokenSilentAsync(
                    graphResourceId,
                    new ClientCredential(appId, appSecret),
                    new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));

                return authResult.AccessToken;
            }
            catch (AdalException e)
            {
                throw e;
            }
        }
    }
}
