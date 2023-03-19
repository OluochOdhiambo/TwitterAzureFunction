using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TwitterAzureFunction
{
    public class BlobDetail
    {
        public string BlobUser { get; set; }

        public string Blob { get; set; }
    }

    public class FetchBlobs
    {
        public static async Task DownloadToText(BlobClient blobClient)
        {
            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            string downloadedData = downloadResult.Content.ToString();
        }

        public static async Task ListBlobsHierarchicalListing(BlobContainerClient container,
                                                       string prefix,
                                                       List<TweetData> blobTweets, string blobStorageConnectionString, string blobStorageContainerName, ILogger log)
        {
            try
            {
                // Call the listing operation and return pages of the specified size.
                var resultSegment = container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
                    .AsPages(default);

                // Enumerate the blobs returned for each page. Get only last two blobs.
                
                await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
                {
                    var sortedBlobs = blobPage.Values
                        .OrderByDescending(b => DateTimeOffset.ParseExact(
                            Path.GetFileNameWithoutExtension(b.Blob.Name.Split('/')[1]),
                            "yyyyMMddTHHmmssZ",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal));
                    
                    int count = 0;
                    // Iterate over the blobs.
                    foreach (BlobHierarchyItem blobhierarchyItem in sortedBlobs)
                    {
                        // log.LogInformation(JsonConvert.SerializeObject(blobhierarchyItem));
                        // Write out the name of the blob.
                        BlobDetail blob = new BlobDetail
                        {
                            BlobUser = blobhierarchyItem.Blob.Name.Split('/')[0],
                            Blob = blobhierarchyItem.Blob.Name.Split('/')[1],
                        };

                        var blobClient = new BlobClient(blobStorageConnectionString, blobStorageContainerName, blobhierarchyItem.Blob.Name);

                        try
                        {
                            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                            string downloadedData = downloadResult.Content.ToString();
                            // Handle the downloaded data as needed
                            List<TweetData> tweets = JsonConvert.DeserializeObject<List<TweetData>>(downloadedData);
                            blobTweets.AddRange(tweets);

                            count++;
                        }
                        catch (RequestFailedException ex)
                        {
                            Console.WriteLine("Error downloading blob: {0}", ex.Message);
                            // Handle the error as appropriate for your application
                        }


                        if (count >= 2)
                        {
                            break;
                        }

                    }

                }
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
    }
}
