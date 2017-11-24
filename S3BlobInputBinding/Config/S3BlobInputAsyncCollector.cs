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

namespace SampleExtension.Config
{
    internal class S3BlobInputAsyncCollector : IAsyncCollector<S3BlobInputMessage>
    {
        private S3BlobInputConfiguration config;
        private S3BlobInputAttribute attr;
        private static HttpClient client = new HttpClient();

        public S3BlobInputAsyncCollector(S3BlobInputConfiguration config, S3BlobInputAttribute attr)
        {
            this.config = config;
            this.attr = attr;
        }
            
        public async Task AddAsync(S3BlobInputMessage item, CancellationToken cancellationToken = default(CancellationToken))
        {
            var mergedItem = MergeMessageProperties(item, config, attr);
            await SendS3BlobInputMessage(mergedItem, attr);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        // combine properties to create final message that will be sent
        private static S3BlobInputMessage MergeMessageProperties(S3BlobInputMessage item, S3BlobInputConfiguration config, S3BlobInputAttribute attr)
        {
            var result = new S3BlobInputMessage();

            result.Text = FirstOrDefault(item.Text, attr.Text);
            result.Channel = FirstOrDefault(item.Channel, attr.Channel, config.Channel);
            result.Username = FirstOrDefault(item.Username, attr.Username, config.Username);
            result.IconEmoji = FirstOrDefault(item.IconEmoji, attr.IconEmoji, config.IconEmoji);
            result.IsMarkdown = item.IsMarkdown;
            result.AsUser = item.AsUser;

            return result;
        }

        private static string FirstOrDefault(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrEmpty(v));
        }

        private static async Task SendS3BlobInputMessage(S3BlobInputMessage mergedItem, S3BlobInputAttribute attribute)
        {
            try
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.PostAsJsonAsync(attribute.WebHookUrl, mergedItem);
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
