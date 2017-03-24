/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

 using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;
using System.Security.Claims;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using GraphWebhooks.TokenStorage;

namespace GraphWebhooks.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public void SignIn()
        {
            if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        // Remove all cache entries for this user and send an OpenID Connect sign-out request.
        public void SignOut()
        {
            string userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            string tenantId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string authority = $"{ Startup.AadInstance }/{ tenantId }";

            AuthenticationContext authContext = new AuthenticationContext(
                authority,
                new SampleTokenCache(userObjectId));
            authContext.TokenCache.Clear();
            HttpContext.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        }
    }
}
