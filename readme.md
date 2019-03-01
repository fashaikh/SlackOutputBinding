# FunctionApp v1 with AWS S3 bucket and queue bindings Cognitive Services and CosmosDB 

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
Reads from Azure Storage `tos3/container` 
and makes a copy in AWS S3 `fromazure/folder` (all in one line)
    The main limitations are:
    1) Cannot trigger on S3 blob creates, only can use them as input or outputs
    2) Cannot use dynamic S3 path names for input, only available for output


## Quick Deploy to Azure
[![Deploy to Azure](http://azuredeploy.net/deploybutton.svg)](https://azuredeploy.net/)

### Step 1 Start FunctionApp Template ![step1 dewploymkent comp lete](https://user-images.githubusercontent.com/2650941/53660706-5cc57380-3c13-11e9-9d35-dff8a18c3b20.PNG)

### Step 2 Get AWS Access Key 
From your AWS account account using these instructions https://www.cloudberrylab.com/resources/blog/how-to-find-your-aws-access-key-id-and-secret-access-key/ 

### Step 3 Get Microsoft Cognitive Services API Key 
From your account or a free one from here https://azure.microsoft.com/try/cognitive-services/

#### To run in Azure, run the ARM Template found in azuredeploy.json and then fill in the app settings with the following values:
- FunctionApp Name : The name of the function App you want to create
- AWSAccessKeyID : With access to the AWS S3 storage account
- AWSSecretAccessKey : Secret for the AWS S3 Access Key. Follow these instructions to get these secrets https://www.cloudberrylab.com/resources/blog/how-to-find-your-aws-access-key-id-and-secret-access-key/ 
- BucketName : Name of the S3 bucket you plan to use. eg functions-demo
- OcpApiKey : from Microsoft Cognitive Services. Get yours here https://azure.microsoft.com/try/cognitive-services/

The deployment screen will look like this:
![stepcosmos](https://user-images.githubusercontent.com/2650941/53660166-16234980-3c12-11e9-93bd-7114298eba4e.PNG)

1) Install storage explorer from https://azure.microsoft.com/en-us/features/storage-explorer/ 
2) Add Azure Storage account : 
![steo 1](https://user-images.githubusercontent.com/2650941/53659980-ba58c080-3c11-11e9-8159-5c2d55306da2.png)
3) Add Cosmosdb account: 
![steo 2](https://user-images.githubusercontent.com/2650941/53659981-baf15700-3c11-11e9-925c-243997bbd121.png)

Check you have access to S3 bucket: 
https://s3.console.aws.amazon.com/s3/buckets/<bucketname>/?region=us-west-2&tab=overview

![steo 3 s3](https://user-images.githubusercontent.com/2650941/53659982-baf15700-3c11-11e9-9fc5-b021ffcaa191.png))


### Step 4 Demos 

#### Demo 1 : S3 to Storage and Cosmos ![Step 3 Execute Function ]

For the demo you can upload an image to input/imagetoOCR.png . 
![steo 6 output](https://user-images.githubusercontent.com/2650941/53659984-baf15700-3c11-11e9-96fe-5c2ce40b0816.png)
Here is a sample image. You can change / add text it or use your own png image
<img width="459" alt="steo 5 sample image" src="https://user-images.githubusercontent.com/2650941/53659985-baf15700-3c11-11e9-9a60-525fcd8b4896.png">
Once this is uploaded you can open the S3ImageOCR function in portal and hit run on it. 
I’ve made it an httptrigger so its easy to demo any changes.

The output can be see in portal and in appInsights.

![steo 6 output](https://user-images.githubusercontent.com/2650941/53659987-baf15700-3c11-11e9-87e0-7ce700f68dbd.png)
Once executed you can also show the OCR output stored in cosmosdb
![steo 7 cosmos output](https://user-images.githubusercontent.com/2650941/53659988-baf15700-3c11-11e9-9129-309c0a5cd746.png)
*Note you can also show the results in storage explorer*

Also a copy is created in S3 output/ bucket, and azure storage:

![steo 9 storage and s3 copy](https://user-images.githubusercontent.com/2650941/53659989-bb89ed80-3c11-11e9-8173-940ad5ce7f93.png)
You can also see the output of QueueTriggerOCRFromS3 which should have the same guid read from the azure storage queue.

#### Demo 2 : Azure Storage to S3 ![Step 3 Execute Function ]
Using storage explorer upload an image to tos3/container:


![demo 2 step 1](https://user-images.githubusercontent.com/2650941/53659978-ba58c080-3c11-11e9-98c0-1ed9e3d9b209.png)
![demo 2 step 2](https://user-images.githubusercontent.com/2650941/53659979-ba58c080-3c11-11e9-9e42-351b001cf4c0.png)
 
Shows the execution of “BlobTriggerToS3” function 
The output file will be in fromazure folder

#### Demo 3 : Get S3 PreSigned URL

![steppresigned](https://user-images.githubusercontent.com/2650941/53660167-16bbe000-3c12-11e9-9f0f-1b79404366e3.PNG)

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
    "AzureServicesAuthConnectionString":"RunAs=App;AppId=<>;TenantId=<>;AppKey=<>",
    "AWSAccessKeyID" :"",
    "AWSSecretAccessKey" :"", 
    "BucketName" : "",
    "OcpApiKey" : "",
    "CosmosDB":""
    }
}
```
