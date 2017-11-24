// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using System;
using System.IO;

namespace SampleExtension
{
    /// <summary>
    /// Attribute used to bind a parameter to a S3Blob. Message will be posted to S3Blob when the 
    /// method completes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class S3BlobAttribute : Attribute
    {
        private readonly string _s3BlobPath;

        /// <summary>Initializes a new instance of the <see cref="S3BlobAttribute"/> class.</summary>
        /// <param name="s3BlobPath">The path of the blob to which to bind.</param>
        public S3BlobAttribute(string s3BlobPath)
        {
            _s3BlobPath = s3BlobPath;
        }

        /// <summary>
        /// Sets the AWSAccessKeyID for the current outgoing S3Blob message. May include binding parameters.
        /// </summary>
        [AppSetting(Default = "AWSAccessKeyID")]
        public string AWSAccessKeyID { get; set; }
        /// <summary>
        /// Sets the AWSSecretAccessKey for the current outgoing S3Blob message. May include binding parameters.
        /// </summary>
        [AppSetting(Default = "AWSSecretAccessKey")]
        public string AWSSecretAccessKey { get; set; }

        /// <summary>
        /// Sets the BucketName for the current outgoing S3Blob message. May include binding parameters.
        /// </summary>
        [AppSetting(Default = "BucketName")]
        public string BucketName { get; set; }

        /// <summary>
        /// Sets the Path for the current S3Blob message. May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string S3BlobPath { get { return _s3BlobPath; } }

        /// <summary>
        /// Sets the data Stream for the outgoing request. May include binding parameters.
        /// </summary>
        public Stream Data { get; set; }
    }
}
