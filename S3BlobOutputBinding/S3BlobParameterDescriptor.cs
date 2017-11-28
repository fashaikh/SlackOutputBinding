// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Protocols;

public class S3BlobParameterDescriptor : ParameterDescriptor
    {
        /// <summary>Gets or sets the name of the storage account.</summary>
        public string AccountName { get; set; }

        /// <summary>Gets or sets the name of the container.</summary>
        public string ContainerName { get; set; }

        /// <summary>Gets or sets the name of the blob.</summary>
        public string BlobName { get; set; }

    }
