Cryptocurrency-Market-Predictor

**Overview**
Cryptocurrency-Market-Predictor is a C# project designed to fetch and analyse cryptocurrency data from Twitter and CoinMarketCap, then export this data to Google Sheets for further analysis. It can search for tweets related to existing and newly listed cryptocurrencies, providing insights into market sentiments and trends.

**Features**
- Fetches cryptocurrency data from Twitter using the Tweetinvi library.
- Fetches cryptocurrency listings from CoinMarketCap.
- Exports data to Google Sheets for analysis.

**Libraries and Tools Required**
- Tweetinvi: A C# library that provides an interface to the Twitter API.
- Google APIs Client Library for .NET: Allows integration with Google services, including Sheets.
- Newtonsoft.Json: A popular high-performance JSON framework for .NET.
- .NET Framework: Targeting .NET Framework 4.7.2 or higher is recommended for compatibility with all dependencies.

**Getting Started**
To run this project, you'll need to have Visual Studio installed with support for C# development. Clone the repository, and ensure all NuGet packages are restored correctly.

1. Set up Twitter API credentials: Obtain your Twitter API keys and access tokens by creating a Twitter developer account and a new app.
2. Set up Google API credentials: Enable the Google Sheets API in your Google Cloud Platform project and download the credentials JSON file.
3. Update the placeholders in the code with your actual API keys and the path to your Google credentials JSON file.
4. Compile and run the program.

**Important Note**
As of early 2023, this project has been deprecated due to changes in Twitter's API costs. The project has not been updated to accommodate these changes or any potential changes in the Google Sheets API and the CoinMarketCap API.

**Disclaimer**
This project is intended for educational purposes only. Users are responsible for complying with Twitter's, Google's, and CoinMarketCap's terms of service, including any limitations on API usage.
