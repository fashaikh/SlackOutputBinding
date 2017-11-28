using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SampleExtension;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace SampleFunctionApp
{
    public static class S3ImageOCR
    {
        /// <summary>
        /// This function reads an image from S3 bucket as a Stream, 
        /// makes a copy of it in S3 and also in Azure Storage Blobs
        /// It also calls the cognitive API and pushes the results to CosmosDB
        /// Additionally it pushes the copy guid name to an Azure storage queue
        /// </summary>
        [FunctionName("S3ImageOCR")]
        public static void Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
        [S3Blob("input/imagetoOCR.png")] S3BlobMessage s3BlobInputMessage,
        [S3Blob("thisischangedbelow")] out S3BlobMessage s3BlobOutputMessage,
        IBinder binderForAzureStorage,
        [Queue("s3images")] out string newImageGuid,
        [DocumentDB("ocrDatabase", "ocrCollection", ConnectionStringSetting = "CosmosDB", CreateIfNotExists = true)] out object ocrData,
        TraceWriter log)
        {
            log.Info($"TimerTriggerFromS3 started at: {DateTime.Now}");
            s3BlobOutputMessage = new S3BlobMessage { Data = new MemoryStream(), BucketName = s3BlobInputMessage.BucketName };
            s3BlobInputMessage.Data.CopyToAsync(s3BlobOutputMessage.Data).GetAwaiter().GetResult();
            log.Info($"Made a copy in S3 at: {DateTime.Now}");

            newImageGuid = Guid.NewGuid().ToString();
            log.Info($"Inserted image name {newImageGuid} in Azure queue at: {DateTime.Now}");

            s3BlobOutputMessage.S3BlobPath = "froms3/" + newImageGuid + ".png";
            s3BlobOutputMessage.Data.Position = 0;
            using (var writer = binderForAzureStorage.BindAsync<Stream>(new BlobAttribute(@"froms3/" + newImageGuid + ".png", FileAccess.Write)).GetAwaiter().GetResult())
            {
                s3BlobOutputMessage.Data.CopyToAsync(writer).GetAwaiter().GetResult();
                writer.FlushAsync().GetAwaiter().GetResult();
            }
            log.Info($"Made a copy in Azure at: {DateTime.Now}");

            s3BlobOutputMessage.Data.Position = 0;
            BinaryReader binaryReader = new BinaryReader(s3BlobOutputMessage.Data);
            
            var ocrResponse = OCRUsingCognitiveAPI(binaryReader.ReadBytes((int)s3BlobOutputMessage.Data.Length));
            log.Info($"Got OCR Response: {string.Join(" - " ,ocrResponse)}");
            ocrData = new { guid = newImageGuid, ocrText = ocrResponse };
            log.Info($"Inserted OCR Response in CosmosDB: {DateTime.Now}");

        }



        //https://github.com/papasoft/azure-functions-twilio-ocr/blob/master/ProcessImage/run.csx
        private static List<string> OCRUsingCognitiveAPI(byte[] byteData)
        {
            var serviceUrl = "https://westus.api.cognitive.microsoft.com/vision/v1.0/ocr?language=unk&detectOrientation=true";
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