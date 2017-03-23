using GraphWebhooks.TokenStorage;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GraphWebhooks.Helpers
{
    class AuthHelper
    {
        //private static string userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
        //private static string tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
        //private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        //private static string resourceId = ConfigurationManager.AppSettings["ida:ResourceId"];
        //private static ClientCredential credential = new ClientCredential(ConfigurationManager.AppSettings["ida:ClientId"], ConfigurationManager.AppSettings["ida:ClientSecret"]);
        private static SampleTokenCache tokenCache;

        private static string aadInstance = Startup.AadInstance;
        private static string appId = Startup.ClientId;
        private static string appSecret = Startup.ClientSecret;
        private static string graphResourceId = Startup.GraphResourceId;

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

        public static async Task<string> GetAccessTokenForSubscriptionAsync(string userObjectId, string tenantId)
        {
            //string authority = string.Format(System.Globalization.CultureInfo.InvariantCulture, aadInstance, "common");

            //AuthenticationContext authContext = new AuthenticationContext(authority, false);
            //AuthenticationResult authResult = await authContext.AcquireTokenByRefreshTokenAsync(
            //    refreshToken, 
            //    credential,
            //    resourceId);
            //return authResult.AccessToken;
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
