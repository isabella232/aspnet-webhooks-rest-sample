/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

 using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Owin;
using GraphWebhooks.TokenStorage;

namespace GraphWebhooks
{
    public partial class Startup
    {
        public static string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        public static string ClientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        public static string AadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        public static string GraphResourceId = ConfigurationManager.AppSettings["ida:ResourceId"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions { });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = ClientId,
                    Authority = $"{ AadInstance }/common",
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        // instead of using the default validation (validating against a single issuer value, as we do in line of business apps), 
                        // we inject our own multitenant validation logic
                        ValidateIssuer = false,
                        // If the app needs access to the entire organization, then add the logic
                        // of validating the Issuer here.
                        // IssuerValidator
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthorizationCodeReceived = (context) =>
                        {

                            // If there is a code in the OpenID Connect response, redeem it for an access token and store it away.
                            var code = context.Code;
                            string userObjectId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                            string tenantId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                            string authority = $"{ AadInstance }/{ tenantId }";

                            AuthenticationContext authContext = new AuthenticationContext(
                                authority, 
                                new SampleTokenCache(userObjectId));

                            authContext.AcquireTokenByAuthorizationCodeAsync(
                                code, 
                                new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)),
                                new ClientCredential(ClientId, ClientSecret), 
                                GraphResourceId);

                            return Task.FromResult(0);
                        },
                        RedirectToIdentityProvider = (context) =>
                        {
                            // This ensures that the address used for sign in and sign out is picked up dynamically from the request.
                            // This allows you to deploy your app (to Azure Web Sites, for example) without having to change settings
                            // Remember that the base URL of the address used here must be provisioned in Azure AD beforehand.
                            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                            context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                            context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;

                            return Task.FromResult(0);
                        },
                        AuthenticationFailed = (context) =>
                        {
                            // Suppress the exception if you don't want to see the error.
                            context.HandleResponse();
                            string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                            string message = context.Exception.Message;
                            context.Response.Redirect(appBaseUrl + $"/error/index?message={ message }");

                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}
