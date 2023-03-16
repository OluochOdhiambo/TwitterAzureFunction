using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs.Models;
using Azure;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Globalization;

namespace TwitterAzureFunction
{
    public class BlobDetail
    {
        public string BlobUser { get; set; }

        public string Blob { get; set; }
    }

    public static class FetchTweets
    {
        public static async Task DownloadToText(BlobClient blobClient)
        {
            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            string downloadedData = downloadResult.Content.ToString();
            Console.WriteLine("Downloaded data:", downloadedData);
        }

        private static async Task ListBlobsHierarchicalListing(BlobContainerClient container,
                                                       string prefix,
                                                       int? segmentSize,
                                                       List<BlobDetail> blobDetails, string blobStorageConnectionString, string blobStorageContainerName, ILogger log)
        {
            try
            {
                // Call the listing operation and return pages of the specified size.
                var resultSegment = container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
                    .AsPages(default, segmentSize);

                // Enumerate the blobs returned for each page. Get only last two blobs.
                int count = 0;
                await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
                {
                    var sortedBlobs = blobPage.Values
                        .OrderByDescending(b => DateTimeOffset.ParseExact(
                            Path.GetFileNameWithoutExtension(b.Blob.Name),
                            "yyyyMMddTHHmmssZ",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal));

                    // Iterate over the blobs.
                    foreach (BlobHierarchyItem blobhierarchyItem in sortedBlobs)
                    {
                        // Write out the name of the blob.
                        Console.WriteLine("Blob name: {0}", blobhierarchyItem.Blob.Name);
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
                            log.LogInformation($"downloaded tweet: {downloadedData}");
                            // Handle the downloaded data as needed
                        }
                        catch (RequestFailedException ex)
                        {
                            Console.WriteLine("Error downloading blob: {0}", ex.Message);
                            // Handle the error as appropriate for your application
                        }

                        blobDetails.Add(blob);
                        count++;
                    }

                    if (count >= 2)
                    {
                        break;
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
            int segmentSize = 2;

            var blobContainerClient = new BlobContainerClient(blobStorageConnectionString, blobStorageContainerName);

            List<BlobDetail> blobDetails = new List<BlobDetail>();

            List<string> blobDirectories = new List<string>() { "citizentvkenya/", "ktnnewske/", "nationafrica/", "ntvkenya/", "standardkenya/", "thestarkenya/"};

            List<string> blobFiles = new List<string>();

            foreach (var item in blobDirectories)
            {
                await ListBlobsHierarchicalListing(blobContainerClient, item, segmentSize, blobDetails, blobStorageConnectionString, blobStorageContainerName, log);
            }


            string json = JsonConvert.SerializeObject(blobDetails, Formatting.Indented);
            
            return new OkObjectResult(json);
        }
    }
}
