using Microsoft.Owin;
using Owin;
using System;
using System.Net;

[assembly: OwinStartupAttribute(typeof(GraphWebhooks.Startup))]

namespace GraphWebhooks
{
    public partial class Startup
    {
        private static readonly Uri BaseUri = new Uri("https://graph.microsoft.com");

        private static readonly int ConnectionLeaseTimeoutValue = 180000; // 3 minutes = 3*60*1000 milliseconds
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();

            ServicePoint point = ServicePointManager.FindServicePoint(BaseUri);
            point.ConnectionLeaseTimeout = ConnectionLeaseTimeoutValue;
        }
    }
}
