# FunctionApp v1 with with AWS S3 and CosmosDB 

This is code for an [Azure Function](https://azure.microsoft.com/en-us/services/functions/) .
The v1 S3 bindings are modeled off of https://github.com/lindydonna/SlackOutputBinding . 

Once installed it creates a Function App linked to an S3 bucket account and a CosmosDB database.

#### TimerTriggerFromS3:
This function runs every 30 seconds and reads an image from S3 bucket as a Stream, 
makes a copy of it in S3 and also in Azure Storage Blobs
It also calls the cognitive API and pushes the results to CosmosDB
Additionally it pushes the guid name to an Azure storage queue
 
#### GetS3PreSignedUrl
Creates a pre signed Url for any bucketName and objectKey in the S3 bucket you own

#### BlobTriggerToS3:
Reads from Azure Storage tos3/ container 
and makes a copy in AWS S3 fromazure/ folder (all in one line)
    The main limitations are:
    1) Cannot trigger on S3 blob creates, only can use them as input or outputs
    2) Cannot use dynamic S3 path names for input, only available for output


## Quick Deploy to Azure

[![Deploy to Azure](http://azuredeploy.net/deploybutton.svg)](https://azuredeploy.net/)
### Step 1 Deploy FunctionApp Template ![Step 1](https://user-images.githubusercontent.com/2650941/53583547-a4c29880-3b36-11e9-808c-0b2937a43691.PNG)

#### To run in Azure, run the ARM Template found in azuredeploy.json and then fill in the app settings with the following values:
- FunctionApp Name : The name of the function App you want to create
- AWSAccessKeyID : With access to the AWS S3 storage account
- AWSSecretAccessKey : Secret for the AWS S3 Access Key. Follow these instructions to get these secrets https://www.cloudberrylab.com/resources/blog/how-to-find-your-aws-access-key-id-and-secret-access-key/ 
- BucketName : Name of the S3 bucket you plan to use. eg functions-demo
- OcpApiKey : from Microsoft Cognitive Services. Get yours here https://labs.cognitive.microsoft.com/en-US/sign-up?ReturnUrl=/en-us/subscriptions


### Step 2 Execute Function : ![Step 3 Execute Function ](https://user-images.githubusercontent.com/2650941/53583986-83ae7780-3b37-11e9-947f-5c5ce6c6e999.png)

### To run locally open in VSCode and fill in the following values in the local.settings.json file 
```
{
    "IsEncrypted": false,
    "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "",
    "BlobStorageConnectionString": "",
    "BlobStorageAccountName":"",
    "BlobStorageContainerName": "",
    "BlobStorageBlobName": "",
    "tenantId" : "",
    "AzureServicesAuthConnectionString":"RunAs=App;AppId=<>;TenantId=<>;AppKey=<>"
    }
}
```
