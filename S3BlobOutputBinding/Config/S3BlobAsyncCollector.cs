using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace SampleExtension.Config
{
    internal class S3BlobAsyncCollector : IAsyncCollector<S3BlobMessage>
    {
        private S3BlobConfiguration config;
        private S3BlobAttribute attr;
        private static IAmazonS3 s3Client;

        public S3BlobAsyncCollector(S3BlobConfiguration config, S3BlobAttribute attr)
        {
            this.config = config;
            this.attr = attr;
            s3Client = new AmazonS3Client(this.attr.AWSAccessKeyID,this.attr.AWSSecretAccessKey,Amazon.RegionEndpoint.USWest2);
        }
            
        public async Task AddAsync(S3BlobMessage item, CancellationToken cancellationToken = default(CancellationToken))
        {
            var mergedItem = MergeMessageProperties(item, config, attr);
            await SendS3BlobMessage(mergedItem, attr);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        // combine properties to create final message that will be sent
        private static S3BlobMessage MergeMessageProperties(S3BlobMessage item, S3BlobConfiguration config, S3BlobAttribute attr)
        {
            var result = new S3BlobMessage();
            result.BucketName = FirstOrDefault(item.BucketName, attr.BucketName, config.BucketName) ;
            result.S3BlobPath = FirstOrDefault(item.S3BlobPath, attr.S3BlobPath, config.S3BlobPath); ;
            result.Data = item.Data;
            return result;
        }
        private static string FirstOrDefault(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrEmpty(v));
        }
        private static async Task SendS3BlobMessage(S3BlobMessage mergedItem, S3BlobAttribute attribute)
        {
            try
            {
               await s3Client.UploadObjectFromStreamAsync(mergedItem.BucketName, mergedItem.S3BlobPath, mergedItem.Data, null, default(System.Threading.CancellationToken));
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") ||
                    ex.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("Caught Exception: " + ex.Message);
                    Console.WriteLine("Response Status Code: " + ex.StatusCode);
                    Console.WriteLine("Error Code: " + ex.ErrorCode);
                    Console.WriteLine("Request ID: " + ex.RequestId);
                }
            }
        }
    }
}
