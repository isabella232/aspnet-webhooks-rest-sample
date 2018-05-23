/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

using System;
using System.Runtime.Caching;
using System.Web;

namespace GraphWebhooks.Helpers
{
    public class SubscriptionStore
    {
        private static ObjectCache cache = MemoryCache.Default;
        private static CacheItemPolicy defaultPolicy = new CacheItemPolicy();

        public string SubscriptionId { get; set; }
        public string ClientState { get; set; }
        public string UserId { get; set; }

        private SubscriptionStore(string subscriptionId, Tuple<string, string> parameters)
        {
            SubscriptionId = subscriptionId;
            ClientState = parameters.Item1;
            UserId = parameters.Item2;
        }

        // This sample temporarily stores the current subscription ID, client state, user object ID, and tenant ID. 
        // This info is required so the NotificationController can retrieve an access token from the cache and validate the subscription.
        // Production apps typically use some method of persistent storage.
        public static void SaveSubscriptionInfo(string subscriptionId, string clientState, string userId)
        {
            cache.Set(
                new CacheItem($"subscriptionId_{subscriptionId}", Tuple.Create(clientState, userId)), 
                defaultPolicy);
        }

        public static SubscriptionStore GetSubscriptionInfo(string subscriptionId)
        {
            Tuple<string, string> subscriptionParams = cache.Get($"subscriptionId_{subscriptionId}") as Tuple<string, string>;
            return new SubscriptionStore(subscriptionId, subscriptionParams);
        }
    }
}