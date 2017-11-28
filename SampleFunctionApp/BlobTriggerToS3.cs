using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SampleExtension;
using System;
using System.IO;

namespace SampleFunctionApp
{
    public static class BlobTriggerToS3
    {
        /// <summary>
        /// Reads from Azure Storage tos3/ container 
        /// and makes a copy in AWS S3 fromazure/ folder
        /// </summary>
        [FunctionName("BlobTriggerToS3")]
        public static void Run(
            [BlobTrigger("tos3/{name}.png")] Stream input,
            [S3Blob("fromazure/{name}.png")] out S3BlobMessage S3BlobMessage,
            TraceWriter log)
        {
            log.Info($"Copying blob from Azure Storage to S3 at {DateTime.Now}") ;
            S3BlobMessage = new S3BlobMessage { Data =  input, BucketName="functions-demo"};
        }


    }
}