# Twitter News Aggregator
## About
The project aims to create a news aggregator for popular news handles in Kenya. The goal is to provide a one-stop-shop for users to access the latest news updates from trusted news sources without the need to browse through multiple profiles or scroll through numerous tweets.

The project is being implemented using Azure functions, a serverless computing service that allows developers to run their code in the cloud without the need to manage infrastructure. I will implement different types of functions, each with a unique trigger dependning on the functionality I am looking to achieve. The news aggregator provides users with a quick and convinient access to real-time news updates from multiple sources. This project looks to improve user productivity by cutting the amount of time spent scrolling social media feeds to get news updates, allowing them to focus on more valuable activities. The aggregator will help users stay informed about the latest happenings in Kenya from trusted sources, enhancing their ability to make informed decisions based on up-to-date information. Overall, the project provides a convenient and efficient way for users to stay up-to-date on the latest news from multiple sources in real-time.

## Technical Requirements
- Backend: C# and .NET
- Serverless Compute: Azure Functions
- API Integration: RapidAPI
- Twitter API Authentication: OAuth 1.0a
- Twitter API Query: GET statuses/user_timeline

## Installation and Setup
The project configurations are currently set to run locally but will be updated as I continue working on the project. To set up the project locally:

1. Clone the repository
2. Install dependencies using NuGet Package Manager
3. Update the local.settings.json file with your RapidAPI and Twitter API keys
4. Run the function

## Usage
The project is an ongoing build so each day's work is progressively added as a new branch. The latest branch is day3.

#### The project currently has two functions:

1. Http Triggered function
This function allows users to query the Twitter API and retrieve the latest tweets of a specific user by sending an HTTP GET request to the function's endpoint with required parameters, including the username and count of tweets to return. The function will return the latest tweets of the user in JSON format.

2. Timer Triggered function
This function automatically gets twitter request from selected news providers and logs at intervals of 20 seconds. The function is currently configured to run every 5 mins.

## Contributing
I welcome contributions from other developers. To contribute, please follow these steps:

1. Fork the repository
2. Make your changes
3. Create a pull request


## Credits
I would like to credit the following sources:

- Azure Functions documentation
- RapidAPI documentation
- Twitter API documentation

## License
This project is licensed under the MIT License.

## Contact
If you have any questions or concerns about the Twitter API query function, please contact me at oluochodhiambo11@gmail.com.
