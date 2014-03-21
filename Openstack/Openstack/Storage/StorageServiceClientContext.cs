﻿// /* ============================================================================
// Copyright 2014 Hewlett Packard
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ============================================================================ */

namespace Openstack.Storage
{
    using System.Threading;
    using Openstack.Identity;

    /// <summary>
    /// Wrapper class that provides a context for the various storage clients.
    /// </summary>
    public class StorageServiceClientContext
    {
        /// <summary>
        /// Gets or sets a credential that can be used to connect to the remote Openstack service.
        /// </summary>
        public IOpenstackCredential Credential { get; set; }

        /// <summary>
        /// Gets or sets a cancellation token that can be used when connecting to the remote Openstack service.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets the name of the storage service.
        /// </summary>
        public string StorageServiceName { get; set; }

        /// <summary>
        /// Gets or sets the region to use when connecting to the remote service.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Creates a new instance of the StorageServiceClientContext class.
        /// </summary>
        /// <param name="credential">The credential for this context.</param>
        /// <param name="cancellationToken">The cancellation token for this context.</param>
        /// <param name="serviceName">The name of the storage service.</param>
        /// <param name="region">The region of the storage service.</param>
        internal StorageServiceClientContext(IOpenstackCredential credential, CancellationToken cancellationToken, string serviceName, string region)
        {
            this.Credential = credential;
            this.CancellationToken = cancellationToken;
            this.StorageServiceName = serviceName;
            this.Region = region;
        }
    }
}