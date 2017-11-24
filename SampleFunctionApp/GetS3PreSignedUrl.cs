using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Text;
using AWSSignatureV4_S3_Sample.Signers;
using System.Collections.Generic;
using AWSSignatureV4_S3_Sample.Util;

namespace SampleFunctionApp
{
    public static class GetS3PreSignedUrl
    {
        /// <summary>
        /// Reads the bucketName and objectKey from an HTTP trigger input
        /// and generates the SASUri/S3 Pre signed uri for the blob
        /// </summary>

        [FunctionName("GetS3PreSignedUrl")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string bucketName = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "bucketName", true) == 0)
                .Value;
            string objectKey = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "objectKey", true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            bucketName = bucketName ?? data?.bucketName;
            objectKey = objectKey ?? data?.objectKey;

            return (bucketName == null|| objectKey==null)
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass bucketName and objectKey on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, GetPreSignedUrl(bucketName,objectKey));
        }

        //http://docs.aws.amazon.com/AmazonS3/latest/API/sig-v4-examples-using-sdks.html#sig-v4-examples-using-sdk-dotnet
        private static string GetPreSignedUrl(string bucketName, string objectKey)
        {
            var region = "us-westus-2";
            // Construct a virtual hosted style address with the bucket name part of the host address,
            // placing the region into the url if we're not using us-east-1.
            var regionUrlPart = string.Empty;
            if (!string.IsNullOrEmpty(region))
            {
                if (!region.Equals("us-east-1", StringComparison.OrdinalIgnoreCase))
                    regionUrlPart = string.Format("-{0}", region);
            }

            var endpointUri = string.Format("https://{0}.s3{1}.amazonaws.com/{2}",
                                        bucketName,
                                        regionUrlPart,
                                        objectKey);

            // construct the query parameter string to accompany the url
            var queryParams = new StringBuilder();

            // for SignatureV4, the max expiry for a presigned url is 7 days, expressed
            // in seconds
            var expiresOn = DateTime.UtcNow.AddDays(2);
            var period = Convert.ToInt64((expiresOn.ToUniversalTime() - DateTime.UtcNow).TotalSeconds);
            queryParams.AppendFormat("{0}={1}", AWS4SignerBase.X_Amz_Expires, HttpHelpers.UrlEncode(period.ToString()));

            var headers = new Dictionary<string, string>();

            var signer = new AWS4SignerForQueryParameterAuth
            {
                EndpointUri = new Uri(endpointUri),
                HttpMethod = "GET",
                Service = "s3",
                Region = "us-west-2"
            };

            var authorization = signer.ComputeSignature(headers,
                                                queryParams.ToString(),
                                                "UNSIGNED-PAYLOAD",
                                                Environment.GetEnvironmentVariable("AWSAccessKeyID"),
                                                Environment.GetEnvironmentVariable("AWSSecretAccessKey"));

            // build the presigned url to incorporate the authorization element
            var urlBuilder = new StringBuilder(endpointUri.ToString());

            // add our query params
            urlBuilder.AppendFormat("?{0}", queryParams.ToString());

            // and finally the Signature V4 authorization string components
            urlBuilder.AppendFormat("&{0}", authorization);

            var presignedUrl = urlBuilder.ToString();
            return presignedUrl;
        }
    }
}
