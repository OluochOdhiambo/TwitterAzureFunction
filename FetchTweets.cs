using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace TwitterAzureFunction
{
    public static class FetchTweets
    {
        [FunctionName("FetchTweets")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            IConfiguration config = new ConfigurationBuilder()
                 .SetBasePath(context.FunctionAppDirectory)
                 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                 .AddEnvironmentVariables()
                 .Build();

            string blobStorageConnectionString = config["AzureBlobStorageConnectionString"];
            string blobStorageContainerName = config["AzureBlobStorageContainerName"];

            var blobContainerClient = new BlobContainerClient(blobStorageConnectionString, blobStorageContainerName);

            List<TweetData> blobTweets = new List<TweetData>();

            List<string> blobDirectories = new List<string>() { "citizentvkenya/", "ktnnewske/", "nationafrica/", "ntvkenya/", "standardkenya/", "thestarkenya/"};

            foreach (var item in blobDirectories)
            {
                await FetchBlobs.ListBlobsHierarchicalListing(blobContainerClient, item, blobTweets, blobStorageConnectionString, blobStorageContainerName, log);
            }


            string json = JsonConvert.SerializeObject(blobTweets, Formatting.Indented);
            
            return new OkObjectResult(blobTweets);
        }
    }
}
