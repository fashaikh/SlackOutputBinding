using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;

namespace SQSDemo
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("local.settings.json");

            Configuration = builder.Build();

            //SQS FIFO queues are designed to guarantee that messages are processed exactly once, in the exact order that they are sent

            var sqsConfig = new AmazonSQSConfig();
           
            sqsConfig.ServiceURL = Configuration["Values:AWSSQSServiceURL"];
            Console.WriteLine( $"Determining Queue in {sqsConfig.ServiceURL} ...");
            var sqsClient = new AmazonSQSClient(Configuration["Values:AWSAccessKeyID"], Configuration["Values:AWSSecretAccessKey"], sqsConfig);
            var createQueueRequest = new CreateQueueRequest();
            var attrs = new Dictionary<string, string>();
            attrs.Add(QueueAttributeName.FifoQueue, "true");
            createQueueRequest.Attributes = attrs;

            createQueueRequest.QueueName = Configuration["Values:SQSFIFOQueueName"];
       
            var createQueueResponse = sqsClient.CreateQueueAsync(createQueueRequest).GetAwaiter().GetResult();

            var myQueueURL = createQueueResponse.QueueUrl;
            Console.WriteLine($"FIFO Queue is {myQueueURL}");
            Console.WriteLine($"=======================================================================");
            Console.WriteLine($"");

            var sendMessageRequest = new SendMessageRequest();
            sendMessageRequest.QueueUrl = myQueueURL;
            var receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = myQueueURL;

            while (true)
            {
                Console.Write("Enter Message to Enqueue:");
                var inputMessage = Console.ReadLine();
                sendMessageRequest.MessageBody = inputMessage;
                //Adding these two for FIFO queues Read more here https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/FIFO-queues.html
                sendMessageRequest.MessageGroupId = Guid.NewGuid().ToString();
                sendMessageRequest.MessageDeduplicationId = Guid.NewGuid().ToString();
                var sendMessageResponse = sqsClient.SendMessageAsync(sendMessageRequest).GetAwaiter().GetResult();
                Console.WriteLine($"MessageId {sendMessageResponse.MessageId} Enqueued with MD5 {sendMessageResponse.MD5OfMessageBody}");
                Console.WriteLine($"Polling queue in 3 Seconds ...");
                Thread.Sleep(3000);

                var receiveMessageResponse = sqsClient.ReceiveMessageAsync(receiveMessageRequest).GetAwaiter().GetResult();
                foreach (var message in receiveMessageResponse.Messages)
                {
                    Console.WriteLine($"Received MessageId {message.MessageId}:{message.Body}");

                    // Good. Now we need to delete it from the queue.
                    var deleteMessageRequest = new DeleteMessageRequest();
                    deleteMessageRequest.QueueUrl = myQueueURL;
                    deleteMessageRequest.ReceiptHandle = message.ReceiptHandle;
                    var response = sqsClient.DeleteMessageAsync(deleteMessageRequest).GetAwaiter().GetResult();
                    Console.WriteLine($"Message Delete Response: {response.HttpStatusCode.ToString()}");
                }
                Console.WriteLine($"Done Reading messages.");
                Console.WriteLine($"=======================================================================");
            }

        }
    }
}
