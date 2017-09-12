using GraphWebhooks.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace GraphWebhooks.Helpers
{
    public class SubscriptionCache
    {
        static SubscriptionCache cache = null;

        Timer timer;
        private SubscriptionCache()
        {
            // Renew subscriptions every 10 minute.
            Timer renewalTimer = new Timer(10 * 60 * 1000)
            {
                AutoReset = false
            };
            renewalTimer.Elapsed += OnRenewal;
            renewalTimer.Start();
            this.timer = renewalTimer;
        }

        public static SubscriptionCache GetSubscriptionCache()
        {
            if(cache != null)
            {
                return cache;
            }

            cache = new SubscriptionCache();
            return cache;
        }


        private async void OnRenewal(object sender, ElapsedEventArgs e)
        {
            Dictionary<string, SubscriptionDetails> subscriptionstore = HttpRuntime.Cache.Get("subscription_store") as Dictionary<string, SubscriptionDetails>;

            foreach (var item in subscriptionstore)
            {
                var response = await SubscriptionHelper.CheckSubscription(item.Key);
                if (response.IsSuccessStatusCode)
                {
                    await SubscriptionHelper.RenewSubscription(item.Key as string);
                }
                else
                {
                    await SubscriptionHelper.CreateSubscription();
                }
            }

            timer.Start();
        }
        

        // This sample temporarily stores the current subscription ID, client state, user object ID, and tenant ID. 
        // This info is required so the NotificationController can retrieve an access token from the cache and validate the subscription.
        // Production apps typically use some method of persistent storage.
        public void SaveSubscriptionInfo(SubscriptionDetails subscriptionDetails)
        {
            if (HttpRuntime.Cache["subscription_store"] == null)
            {
                Dictionary<string, SubscriptionDetails> subscriptionstore = new Dictionary<string, SubscriptionDetails>();
                subscriptionstore.Add(subscriptionDetails.SubscriptionId, subscriptionDetails);
                HttpRuntime.Cache.Add("subscription_store",
                    subscriptionstore,
                    null, DateTime.MaxValue, new TimeSpan(24, 0, 0), System.Web.Caching.CacheItemPriority.NotRemovable, null);
            }
            else
            {
                Dictionary<string, SubscriptionDetails> subscriptionstore = HttpRuntime.Cache.Get("subscription_store") as Dictionary<string, SubscriptionDetails>;
                subscriptionstore.Add(subscriptionDetails.SubscriptionId, subscriptionDetails);
            }
        }

        public SubscriptionDetails GetSubscriptionInfo(string subscriptionId)
        {
            Dictionary<string, SubscriptionDetails> subscriptionstore = HttpRuntime.Cache.Get("subscription_store") as Dictionary<string, SubscriptionDetails>;
            return subscriptionstore[subscriptionId];
        }

        public Dictionary<string, SubscriptionDetails> DeleteAllSubscriptions()
        {
            return HttpRuntime.Cache.Remove("subscription_store") as Dictionary<string, SubscriptionDetails>;

        }
    }
}