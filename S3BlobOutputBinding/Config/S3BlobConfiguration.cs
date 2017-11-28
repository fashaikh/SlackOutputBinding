// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Amazon.S3;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Threading;

namespace SampleExtension.Config
{
    /// <summary>
    /// Extension for binding <see cref="S3BlobMessage"/>.
    /// </summary>
    public class S3BlobConfiguration : IExtensionConfigProvider
    {
        private INameResolver _nameResolver;
        private IConverterManager _converterManager;
        private static IAmazonS3 s3Client;

        #region Global configuration defaults

        /// <summary>
        /// BucketName
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// FilePath
        /// </summary>
        public string S3BlobPath { get; set; }
        #endregion

        public void Initialize(ExtensionConfigContext context)
    {
            _nameResolver = context.Config.NameResolver;
            _converterManager = context.Config.ConverterManager;

            // add converter between JObject and S3BlobMessage
            // Allows a user to bind to IAsyncCollector<JObject>, and the sdk will convert that to IAsyncCollector<S3BlobMessage>
            context.AddConverter<JObject, S3BlobMessage>(input => input.ToObject<S3BlobMessage>());

            // Add a binding rule for Collector
            var rule = context.AddBindingRule<S3BlobAttribute>();
            rule.BindToCollector<S3BlobMessage>(attr => new S3BlobAsyncCollector(this, attr));

            rule.//SetPostResolveHook(ToBlobDescr).
                BindToInput<S3BlobMessage>(BuildItemFromAttr);
        }

        // All {} and %% in the Attribute have been resolved by now. 
        private S3BlobMessage BuildItemFromAttr(S3BlobAttribute attribute)
        {
            s3Client = new AmazonS3Client(attribute.AWSAccessKeyID, attribute.AWSSecretAccessKey, Amazon.RegionEndpoint.USWest2);
               
            return new S3BlobMessage
            {
                BucketName = attribute.BucketName,
                Data = s3Client.GetObjectStreamAsync(attribute.BucketName, attribute.S3BlobPath,null,CancellationToken.None).GetAwaiter().GetResult(),
                S3BlobPath = attribute.S3BlobPath 
            };
        }


        private ParameterDescriptor ToBlobDescr(S3BlobAttribute attr, ParameterInfo parameter, INameResolver nameResolver)
        {
            // Resolve the connection string to get an account name. 
            var accountName = attr.BucketName;

            var resolved = nameResolver.ResolveWholeString(attr.S3BlobPath);

            string containerName = resolved;
            string blobName = null;
            int split = resolved.IndexOf('/');
            if (split > 0)
            {
                containerName = resolved.Substring(0, split);
                blobName = resolved.Substring(split + 1);
            }

            return new S3BlobParameterDescriptor
            {
                Name = parameter.Name,
                AccountName = accountName,
                ContainerName = containerName,
                BlobName = blobName
            };
        }

    }
}
