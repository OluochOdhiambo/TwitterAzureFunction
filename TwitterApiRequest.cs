using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TwitterAzureFunction
{
    public class TweetData
    {
        public string TweetText { get; set; }

        public int Retweets { get; set; }

        public int Favorites { get; set; }

        public string userId { get; set; }

        public string createdAt { get; set; }
    }

    public class TwitterApiRequest
    {
        public async Task<string> GetTweets(string userId, string count, string apiKey, ILogger log)
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

                    return json;
                }
                else
                {
                    // GET tweet entries
                    dynamic tweetTimeline = tweetJsonResponse.data.user.result.timeline.timeline;

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

                    return json;

                }

            }
        }
    }
}
