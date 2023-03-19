using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TwitterAzureFunction
{
    public static class Function
    {
        [FunctionName("index")]
        public static IActionResult GetHomePage([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, ExecutionContext context)
        {
            var path = Path.Combine(context.FunctionAppDirectory, "content", "index.html");
            return new ContentResult
            {
                Content = File.ReadAllText(path),
                ContentType = "text/html",
            };
        }

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
            [SignalRConnectionInfo(HubName = "serverless")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("broadcast")]
        public static async Task Broadcast([TimerTrigger("*/5 * * * *")] TimerInfo myTimer,
        [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            IConfiguration config = new ConfigurationBuilder()
                 .SetBasePath(context.FunctionAppDirectory)
                 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                 .AddEnvironmentVariables()
                 .Build();

            string blobStorageConnectionString = config["AzureBlobStorageConnectionString"];
            string blobStorageContainerName = config["AzureBlobStorageContainerName"];

            var blobContainerClient = new BlobContainerClient(blobStorageConnectionString, blobStorageContainerName);

            List<TweetData> blobTweets = new List<TweetData>();

            List<string> blobDirectories = new List<string>() { "citizentvkenya/", "ktnnewske/", "nationafrica/", "ntvkenya/", "standardkenya/", "thestarkenya/" };

            foreach (var item in blobDirectories)
            {
                await FetchBlobs.ListBlobsHierarchicalListing(blobContainerClient, item, blobTweets, blobStorageConnectionString, blobStorageContainerName, log);
            }


            string json = JsonConvert.SerializeObject(blobTweets, Formatting.Indented);

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "newMessage",
                    Arguments = new[] { json },
                });
        }
    }
}