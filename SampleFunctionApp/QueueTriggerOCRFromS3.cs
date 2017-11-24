using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace SampleFunctionApp
{
    public static class QueueTriggerOCRFromS3
    {
        /// <summary>
        /// This function reads the image guids from the s3images queue
        /// which is hydrated by TimerTriggerFromS3
        /// </summary>
        [FunctionName("QueueTriggerOCRFromS3")]
        public static void Run([QueueTrigger("s3images", Connection = "")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
