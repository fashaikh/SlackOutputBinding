using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleExtension
{
    public class S3BlobMessage
    {
        [JsonProperty("data")]
        public Stream Data { get; set; }
        [JsonProperty("bucketName")]
        public string BucketName { get; set; }
        [JsonProperty("s3BlobPath")]
        public string S3BlobPath { get; set; }
    }
}
