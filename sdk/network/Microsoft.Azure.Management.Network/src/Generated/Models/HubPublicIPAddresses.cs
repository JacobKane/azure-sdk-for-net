// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Management.Network.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Public IP addresses associated with azure firewall.
    /// </summary>
    public partial class HubPublicIPAddresses
    {
        /// <summary>
        /// Initializes a new instance of the HubPublicIPAddresses class.
        /// </summary>
        public HubPublicIPAddresses()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the HubPublicIPAddresses class.
        /// </summary>
        /// <param name="addresses">The number of Public IP addresses
        /// associated with azure firewall.</param>
        /// <param name="count">Private IP Address associated with azure
        /// firewall.</param>
        public HubPublicIPAddresses(IList<AzureFirewallPublicIPAddress> addresses = default(IList<AzureFirewallPublicIPAddress>), int? count = default(int?))
        {
            Addresses = addresses;
            Count = count;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the number of Public IP addresses associated with
        /// azure firewall.
        /// </summary>
        [JsonProperty(PropertyName = "addresses")]
        public IList<AzureFirewallPublicIPAddress> Addresses { get; set; }

        /// <summary>
        /// Gets or sets private IP Address associated with azure firewall.
        /// </summary>
        [JsonProperty(PropertyName = "count")]
        public int? Count { get; set; }

    }
}
