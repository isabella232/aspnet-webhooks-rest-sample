using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GraphWebhooks.Startup))]

namespace GraphWebhooks
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
