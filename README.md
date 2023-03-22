# Azure Function Repo: Twitter API Query
## About
Working on social media analysis, I created an Azure function that allows users to query the Twitter API using RapidAPI and retrieve the latest tweets of a specific user. The function is built in C# and deployed on Azure Functions.

## Technical Requirements
The function is HTTP-triggered, meaning that users can send HTTP requests to the function's endpoint and receive a response containing the specified user's latest tweets. The function authenticates the user's request with Twitter API keys, fetches the latest tweets of the user, and returns them in JSON format. The function is built using the following technologies and tools:

- Backend: C# and .NET
- Serverless Compute: Azure Functions
- API Integration: RapidAPI
- Twitter API Authentication: OAuth 1.0a
- Twitter API Query: GET statuses/user_timeline

## Installation and Setup
To use the Twitter API query function, follow these steps:

1. Clone the repository
2. Install dependencies using NuGet Package Manager
3. Update the appsettings.json file with your RapidAPI and Twitter API keys
4. Publish the function to Azure Functions

## Usage
Users can query the Twitter API and retrieve the latest tweets of a specific user by sending an HTTP GET request to the function's endpoint with required parameters, including the username and count of tweets to return. The function will return the latest tweets of the user in JSON format.

### Example
To try out the Twitter API query function, replace <username> and <count> with your desired values in the following URL and send an HTTP GET request to the endpoint:

#### https://<function-app-name>.azurewebsites.net/api/tweets?username=username&count=count
The function will return the latest tweets of the specified user in JSON format.

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
