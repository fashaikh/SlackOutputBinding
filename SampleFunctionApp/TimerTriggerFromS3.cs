using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SampleExtension;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using AWSSignatureV4_S3_Sample.Signers;
using AWSSignatureV4_S3_Sample.Util;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.WebJobs.Extensions.DocumentDB;



namespace SampleFunctionApp
{
    public static class TimerTriggerFromS3
    {
        /// <summary>
        /// This function reads an image from S3 bucket as a Stream, 
        /// makes a copy of it in S3 and also in Azure Storage Blobs
        /// It also calls the cognitive API and pushes the results to CosmosDB
        /// Additionally it pushes the guid name to an Azure storage queue
        /// </summary>
        [FunctionName("TimerTriggerFromS3")]
        public static void Run([TimerTrigger("*/30 * * * * *")]TimerInfo myTimer,
        [S3Blob("input/storagesecurity.png")] S3BlobMessage s3BlobInputMessage,
        [S3Blob("thisischangedbelow")] out S3BlobMessage s3BlobOutputMessage,
        IBinder binderForAzureStorage,
        [Queue("s3images")] out string newImageGuid,
        [DocumentDB("ocrDatabase",
        "ocrCollection",
        ConnectionStringSetting = "CosmosDB",
        CreateIfNotExists = true)] out object ocrData,
        TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            s3BlobOutputMessage = new S3BlobMessage { Data = new MemoryStream(), BucketName = s3BlobInputMessage.BucketName };
            s3BlobInputMessage.Data.CopyToAsync(s3BlobOutputMessage.Data).GetAwaiter().GetResult();
            newImageGuid = Guid.NewGuid().ToString();

            s3BlobOutputMessage.S3BlobPath = "froms3/" + newImageGuid + ".png";
            s3BlobOutputMessage.Data.Position = 0;
            using (var writer = binderForAzureStorage.BindAsync<Stream>(new BlobAttribute(@"froms3/" + newImageGuid + ".png", FileAccess.Write)).GetAwaiter().GetResult())
            {
                s3BlobOutputMessage.Data.CopyToAsync(writer).GetAwaiter().GetResult();
                writer.FlushAsync().GetAwaiter().GetResult();
            }
            s3BlobOutputMessage.Data.Position = 0;
            BinaryReader binaryReader = new BinaryReader(s3BlobOutputMessage.Data);
            
            var ocrResponse = OCRUsingCognitiveAPI(binaryReader.ReadBytes((int)s3BlobOutputMessage.Data.Length));
            ocrData = new { guid = newImageGuid, ocrText = ocrResponse };
        }



        //https://github.com/papasoft/azure-functions-twilio-ocr/blob/master/ProcessImage/run.csx
        private static List<string> OCRUsingCognitiveAPI(byte[] byteData)
        {
            var serviceUrl = "https://westus.api.cognitive.microsoft.com/vision/v1.0/ocr?language=unk&detectOrientation=true";
            //var imagedata = JsonConvert.SerializeObject(new { url = "https://cnn.com" });
            List<string> ocrtext = new List<string>();

            using (var client = new HttpClient())
            {
                string apiKey = System.Environment.GetEnvironmentVariable("OcpApiKey", EnvironmentVariableTarget.Process);
                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    var ocrResponse = client.PostAsync(serviceUrl, content).GetAwaiter().GetResult();

                    dynamic ocr = JsonConvert.DeserializeObject(ocrResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    foreach (var r in ocr.regions)
                    {
                        foreach (var l in r.lines)
                        {
                            foreach (var w in l.words)
                            {
                                ocrtext.Add((string)w.text);
                            }
                         }
                    }

                    return ocrtext;
                }
            }
        }
    }
}