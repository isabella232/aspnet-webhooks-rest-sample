/*
 *  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 *  See LICENSE in the source repository root for complete license information.
 */

using System;
using System.Web;

namespace GraphWebhooks.Helpers
{
    public class SubscriptionDetails
    {
        public string SubscriptionId { get; set; }
        public string ClientState { get; set; }
        public string UserId { get; set; }
        public string TenantId { get; set; }

        internal SubscriptionDetails(string subscriptionId, string clientState, string userId, string tenantId)
        {
            SubscriptionId = subscriptionId;
            ClientState = clientState;
            UserId = userId;
            TenantId = tenantId;
        }
    }
}