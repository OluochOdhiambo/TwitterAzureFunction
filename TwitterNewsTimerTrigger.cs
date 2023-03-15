using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
        public async Task RunAsync([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            IConfiguration config = new ConfigurationBuilder()
                 .SetBasePath(context.FunctionAppDirectory)
                 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                 .AddEnvironmentVariables()
                 .Build();

            string apiKey = config["ApiKey"];
            string count = "3";

            foreach (var user in newsSources)
            {
                var tweets = await twitterApiRequest.GetTweets(user.UserId, count, apiKey, log);
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}. Username: {user.Username}");
                log.LogInformation($"Tweets: {tweets}");


                // Implement a 20-second delay
                await Task.Delay(20000);
            }
        }
    }
}
