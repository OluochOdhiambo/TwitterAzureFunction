using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TwitterAzureFunction
{
    public static class TwitterFunction
    {
        [FunctionName("TwitterFunction")]
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

            string apiKey = config["ApiKey"];

            // string apiKey = Environment.GetEnvironmentVariable("ApiKey");

            string username = req.Query["username"];
            string count = req.Query["count"];
            int num;
            count = int.TryParse(count, out num) ? num.ToString() : "1";

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            username = username ?? data?.username;

            if (int.Parse(count) > 3)
            {
                return new NotFoundObjectResult("A maximum tweet count of 3 is permitted per request");
            }

            if (username != null)
            {
                log.LogInformation($"Username: {username}.");

                // GET USER ID
                var userIdClient = new HttpClient();
                var userIdRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://twitter135.p.rapidapi.com/UserByScreenName/?username={username}"),
                    Headers =
                {
                    { "X-RapidAPI-Key", apiKey },
                    { "X-RapidAPI-Host", "twitter135.p.rapidapi.com" },
                },
                };

                using (var response = await userIdClient.SendAsync(userIdRequest))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    dynamic jsonResponse = JsonConvert.DeserializeObject(body);

                    string userId = jsonResponse.data.user.result.rest_id;

                    log.LogInformation($"UserID: {userId}.");

                    if (userId != null)
                    {
                        // GET TWEETS
                        var tweetClient = new HttpClient();
                        var tweetRequest = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri($"https://twitter135.p.rapidapi.com/UserTweets/?id={userId}&count={int.Parse(count)}"),
                            Headers =
                            {
                                { "X-RapidAPI-Key", apiKey },
                                { "X-RapidAPI-Host", "twitter135.p.rapidapi.com" },
                            },
                        };

                        using (var tweetResponse = await tweetClient.SendAsync(tweetRequest))
                        {
                            tweetResponse.EnsureSuccessStatusCode();
                            var tweetBody = await tweetResponse.Content.ReadAsStringAsync();

                            // Deserialize JSON response
                            dynamic tweetJsonResponse = JsonConvert.DeserializeObject(tweetBody);

                            int instructionsCount = tweetJsonResponse.data.user.result.timeline.timeline.instructions.Count;

                            if (instructionsCount == 3)
                            {
                                // GET tweet entries
                                dynamic tweetTimeline = tweetJsonResponse.data.user.result.timeline.timeline;

                                dynamic lastTweetResult = tweetTimeline.instructions[instructionsCount - 1].entry;

                                if (int.Parse(count) > 1 && int.Parse(count) < 4)
                                {
                                    IEnumerable<dynamic> otherTweetResults = ((JArray)tweetTimeline.instructions[instructionsCount - 2].entries)
                                        .Select(entry => (dynamic)entry)
                                        .Take(int.Parse(count) - 1);


                                    IEnumerable<dynamic> allTweets = otherTweetResults.Concat(new dynamic[] { lastTweetResult });

                                    // Create a list of Tweets
                                    List<TweetData> tweetList = allTweets.Select(t => new TweetData
                                    {
                                        TweetText = t.content.itemContent.tweet_results.result.legacy.full_text,
                                        Retweets = t.content.itemContent.tweet_results.result.legacy.retweet_count,
                                        Favorites = t.content.itemContent.tweet_results.result.legacy.favorite_count,
                                        userId = userId,
                                        createdAt = t.content.itemContent.tweet_results.result.legacy.created_at
                                    }).ToList();

                                    string json = JsonConvert.SerializeObject(tweetList, Formatting.Indented);

                                    return new OkObjectResult(json);
                                }
                                else
                                {
                                    // Extract tweet data from the lastTweetResult
                                    var tweet = new TweetData
                                    {
                                        TweetText = lastTweetResult.content.itemContent.tweet_results.result.legacy.full_text,
                                        Retweets = lastTweetResult.content.itemContent.tweet_results.result.legacy.retweet_count,
                                        Favorites = lastTweetResult.content.itemContent.tweet_results.result.legacy.favorite_count,
                                        userId = userId,
                                        createdAt = lastTweetResult.content.itemContent.tweet_results.result.legacy.created_at
                                    };

                                    var tweetJson = JsonConvert.SerializeObject(tweet);
                                    return new ContentResult
                                    {
                                        ContentType = "application/json",
                                        Content = tweetJson
                                    };
                                }
                                

                            }
                            else
                            {
                                // GET tweet entries
                                dynamic tweetTimeline = tweetJsonResponse.data.user.result.timeline.timeline;

                                if (int.Parse(count) > 1)

                                {

                                    IEnumerable<dynamic> allTweets = ((JArray)tweetTimeline.instructions[instructionsCount - 1].entries)
                                        .Select(entry => (dynamic)entry)
                                        .Take(int.Parse(count));


                                    // Create a list of Tweets
                                    List<TweetData> tweetList = allTweets.Select(t => new TweetData
                                    {
                                        TweetText = t.content.itemContent.tweet_results.result.legacy.full_text,
                                        Retweets = t.content.itemContent.tweet_results.result.legacy.retweet_count,
                                        Favorites = t.content.itemContent.tweet_results.result.legacy.favorite_count,
                                        userId = userId,
                                        createdAt = t.content.itemContent.tweet_results.result.legacy.created_at
                                    }).ToList();

                                    string json = JsonConvert.SerializeObject(tweetList, Formatting.Indented);

                                    return new OkObjectResult(json);
                                }
                                else
                                {
                                    // Extract tweet data from the lastTweetResult
                                    var tweet = new TweetData
                                    {
                                        TweetText = tweetTimeline.instructions[instructionsCount - 1].entries[0].content.itemContent.tweet_results.result.legacy.full_text,
                                        Retweets = tweetTimeline.instructions[instructionsCount - 1].entries[0].content.itemContent.tweet_results.result.legacy.retweet_count,
                                        Favorites = tweetTimeline.instructions[instructionsCount - 1].entries[0].content.itemContent.tweet_results.result.legacy.favorite_count,
                                        userId = userId,
                                        createdAt = tweetTimeline.instructions[instructionsCount - 1].entries[0].content.itemContent.tweet_results.result.legacy.created_at
                                    };

                                    var tweetJson = JsonConvert.SerializeObject(tweet);
                                    return new ContentResult
                                    {
                                        ContentType = "application/json",
                                        Content = tweetJson
                                    };
                                }
                            }

                        }
                    }
                    else
                    {
                        return new NotFoundObjectResult("User not found.");
                    }
                }
            }
            else
            {
                return new NotFoundObjectResult("Please specify a username in the get request url or in the post request body.");
            }
            
        }

    }
}
