using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GraphWebhooks.Utils
{
    class AuthHelper
    {
        private static string userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
        private static string tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string resourceId = ConfigurationManager.AppSettings["ida:ResourceId"];
        private static ClientCredential credential = new ClientCredential(ConfigurationManager.AppSettings["ida:ClientId"], ConfigurationManager.AppSettings["ida:ClientSecret"]);

        public static async Task<AuthenticationResult> GetAccessTokenAsync()
        {
            string authority = string.Format(System.Globalization.CultureInfo.InvariantCulture, aadInstance, tenantId);

            AuthenticationContext authContext = new AuthenticationContext(authority, false);            
            AuthenticationResult authResult = await authContext.AcquireTokenSilentAsync(
                resourceId, 
                credential,
                new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));
            return authResult;
        }

        public static async Task<string> GetAccessTokenFromRefreshTokenAsync(string refreshToken)
        {
            string authority = string.Format(System.Globalization.CultureInfo.InvariantCulture, aadInstance, "common");

            AuthenticationContext authContext = new AuthenticationContext(authority, false);
            AuthenticationResult authResult = await authContext.AcquireTokenByRefreshTokenAsync(
                refreshToken, 
                credential,
                resourceId);
            return authResult.AccessToken;
        }
    }
}
