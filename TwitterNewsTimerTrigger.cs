using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace TwitterAzureFunction
{
    public class TwitterUser
    {
        public string Username { get; set; }
        public string UserId { get; set; }
    }

    public class TwitterNewsTimerTrigger
    {
        private readonly TwitterUser[] newsSources = new[]
        {
            new TwitterUser { Username = "ktnnewske", UserId = "1057902407655526400" },
            new TwitterUser { Username = "ntvkenya", UserId = "25985333" },
            new TwitterUser { Username = "thestarkenya", UserId = "343326011" },
            new TwitterUser { Username = "citizentvkenya", UserId = "70394965" },
            new TwitterUser { Username = "standardkenya", UserId = "53037279" },
            new TwitterUser { Username = "nationafrica", UserId = "25979455" }
        };

        private readonly TwitterApiRequest twitterApiRequest = new TwitterApiRequest();

        [FunctionName("TwitterTimerTrigger")]
        public async Task RunAsync([TimerTrigger("*/30 * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            IConfiguration config = new ConfigurationBuilder()
                 .SetBasePath(context.FunctionAppDirectory)
                 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                 .AddEnvironmentVariables()
                 .Build();

            string apiKey = config["ApiKey"];
            string blobStorageConnectionString = config["AzureBlobStorageConnectionString"];
            string blobStorageContainerName = config["AzureBlobStorageContainerName"];
            string count = "3";

            var blobContainerClient = new BlobContainerClient(blobStorageConnectionString, blobStorageContainerName);

            foreach (var user in newsSources)
            {
                var tweets = await twitterApiRequest.GetTweets(user.UserId, count, apiKey, log);
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}. Username: {user.Username}");

                BlobClient tweetBlobClient = blobContainerClient.GetBlobClient($"{user.Username}/{DateTime.Now.ToString("yyyyMMddTHHmmssZ")}");

                // Prepare data for upload to blob storage
                await tweetBlobClient.UploadAsync(BinaryData.FromString(tweets), overwrite: true);

                // Implement a 20-second delay
                await Task.Delay(20000);
            }
        }
    }
}
