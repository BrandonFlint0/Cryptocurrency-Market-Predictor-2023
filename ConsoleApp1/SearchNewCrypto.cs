using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using System.Threading.Tasks;
using System.Diagnostics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using System.Collections;
using System.Net;
using System.Globalization;
using Tweetinvi.Core.Events;
using Newtonsoft.Json;
using System.Net.Http;


namespace TwittercheckProg
{
    class SearchNewCrypto
    {

        public static async Task CompleteNewSearch(string[] args)
        {
            var NewCryptoList = new List<NewCryptocurrency>();


            // Search for new cryptos
            using (var client = new HttpClient())
            {
                // Set the address for the API
                client.BaseAddress = new Uri("https://pro-api.coinmarketcap.com/");

                // Add the API key to the request headers ( Replace "..." with api key
                client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", "...");

                // Send a GET request to the API ( Change limit=1000 based on the number of cryptos you want to save )
                var CMCresponse = client.GetAsync("v1/cryptocurrency/listings/latest?start=1&limit=1000").Result;

                // Read the JSON response from the API
                var json = CMCresponse.Content.ReadAsStringAsync().Result;

                // Deserialize the JSON into a dynamic object
                dynamic CMCdata = JsonConvert.DeserializeObject(json);

                // Initialize a new list of Cryptocurrency objects
                var cryptocurrencies = new List<NewCryptocurrency>();

                // Iterate over the data and add each cryptocurrency to the list
                foreach (var coin in CMCdata.data)
                {
                    cryptocurrencies.Add(new NewCryptocurrency
                    {
                        Symbol = coin.symbol,
                        Name = coin.name,
                        Price = coin.quote.USD.price,
                        MarketCap = coin.quote.USD.market_cap,
                        DateAdded = coin.date_added
                    });
                }

                //Record todays date
                DateTime today = DateTime.Today.Date;

                // Print the list of cryptocurrencies
                foreach (var coin in cryptocurrencies)
                {

                    string format = "MM/dd/yyyy HH:mm:ss";
                    DateTime CHCdateTime = DateTime.ParseExact(coin.DateAdded, format, CultureInfo.InvariantCulture);

                    TimeSpan difference = today - CHCdateTime.Date;

                    if (difference.TotalDays < 60)
                    {
                        // recorded date is within 30 days of today
                        Console.WriteLine("{0} {1}: ${2:0.00} {3}", coin.Name, coin.Symbol, coin.Price, coin.DateAdded);
                        NewCryptoList.Add(new NewCryptocurrency
                        {
                            Symbol = coin.Symbol,
                            Name = coin.Name,
                            Price = coin.Price,
                            MarketCap = coin.MarketCap,
                            DateAdded = coin.DateAdded
                        });
                    }
                    else
                    {
                        // recorded date is more than 30 days from today
                    }
                }
            }



            // Replace "..." with twitter api access keys
            var appClient = new TwitterClient("...", "...");

            // Start the authentication process
            var authenticationRequest = await appClient.Auth.RequestAuthenticationUrlAsync();

            // Authenticate through twitter
            Process.Start(new ProcessStartInfo(authenticationRequest.AuthorizationURL)
            {
                UseShellExecute = true
            });

            // Enter pin given by twitter
            Console.WriteLine("Please enter the code and press enter.");
            var pinCode = Console.ReadLine();

            // Save authentication key to reuse existing authentication to make requests
            var userCredentials = await appClient.Auth.RequestCredentialsFromVerifierCodeAsync(pinCode, authenticationRequest);
            var userClient = new TwitterClient(userCredentials);
            var user = await userClient.Users.GetAuthenticatedUserAsync();
            Console.WriteLine("Authenticated");

            // Get the tweets available on the user's home page

            //List for results
            //name, url, post contents,
            List<string> WhoMadeThePostList = new List<string>();
            List<string> UrlList = new List<string>();
            List<string> ContentsList = new List<string>();


            // Search for tweets that contain the specified keyword
            foreach (var coin in NewCryptoList)
            {
                //Console.WriteLine("{0}",coin.Symbol);
                var searchTerm = coin.Name;
                var matchingTweets = await userClient.Search.SearchTweetsAsync(searchTerm);

                // Iterate through the results and print the text of each tweet
                foreach (var tweet in matchingTweets)
                {
                    //Console.WriteLine(tweet.FullText);
                    WhoMadeThePostList.Add(tweet.CreatedBy.ToString());
                    UrlList.Add(tweet.Url);
                    ContentsList.Add(tweet.FullText);
                }
            }

            //set credentials to have access to sheets doc
            string[] Scopes = { SheetsService.Scope.Spreadsheets };

            // replace "..." with google auth key json file. (Allows you to access google sheets without signing in)
            string jsonPath = @"...";
            var credentials = GoogleCredential.FromJson(File.ReadAllText(jsonPath)).CreateScoped(Scopes);
            string ApplicationName = "Twitter data";


            // spreadsheetId | Replace "..." with spreadsheetid
            string spreadsheetId = "...";


            // call the google sheets service
            SheetsService service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = ApplicationName,
            });

            //Clear the spreadsheet
            ClearValuesRequest requestBody = new ClearValuesRequest();
            ClearValuesResponse response = service.Spreadsheets.Values.Clear(requestBody, spreadsheetId, "NewCryptos").Execute();

            //Begin Exporting to sheets
            //Export who made the post into column B
            String range = "NewCryptos!B1:B" + WhoMadeThePostList.Count;
            ValueRange valueRange = new ValueRange();

            List<IList<Object>> data = new List<IList<Object>>();
            for (int i = 0; i < WhoMadeThePostList.Count(); i++)
            {
                data.Add(new List<Object> { WhoMadeThePostList[i] });
            }
            valueRange.Values = data;

            //update cells with user input
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = updateRequest.Execute();


            //Export the URL into column C
            String range2 = "NewCryptos!C1:C" + UrlList.Count;
            ValueRange valueRange2 = new ValueRange();

            List<IList<Object>> data2 = new List<IList<Object>>();
            for (int i = 0; i < UrlList.Count(); i++)
            {
                data2.Add(new List<Object> { UrlList[i] });
            }
            valueRange2.Values = data2;

            //update cells with user input
            var updateRequest2 = service.Spreadsheets.Values.Update(valueRange2, spreadsheetId, range2);
            updateRequest2.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse2 = updateRequest2.Execute();


            //Export the contents into column D
            String range3 = "NewCryptos!D1:D" + ContentsList.Count;
            ValueRange valueRange3 = new ValueRange();

            List<IList<Object>> data3 = new List<IList<Object>>();
            for (int i = 0; i < ContentsList.Count(); i++)
            {
                data3.Add(new List<Object> { ContentsList[i] });
            }
            valueRange3.Values = data3;

            //update cells with user input
            var updateRequest3 = service.Spreadsheets.Values.Update(valueRange3, spreadsheetId, range3);
            updateRequest3.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse3 = updateRequest3.Execute();


            //Export the date and time into column A
            var dateTime = DateTime.Now;
            string formattedDateTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string DateAndTimeFinal = "Date and time " + formattedDateTime;

            var value4 = new List<IList<object>> { new List<object>() { DateAndTimeFinal } };
            var range4 = "NewCryptos!A1:A1";
            ValueRange requestBody4 = new ValueRange();
            requestBody4.Values = value4;
            requestBody4.Range = range4;
            requestBody4.MajorDimension = "ROWS";

            // update the range
            var updateRequest4 = service.Spreadsheets.Values.Update(requestBody4, spreadsheetId, range4);
            updateRequest4.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var updateResponse4 = updateRequest4.Execute();

            //Completed successfully
            Console.WriteLine("Done!");
        }
    }
    public class NewCryptocurrency
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public long MarketCap { get; set; }
        public string DateAdded { get; set; }
    }


}
