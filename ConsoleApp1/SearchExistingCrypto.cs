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


namespace TwittercheckProg
{
    class SearchExistingCrypto
    {
        // Crypto Import
        public static async Task CompleteExistingSearch(string[] args)
        {
            string fileName = @"C:/Users/Brandon/Desktop/ExportCryptos/cryptocurrencies.txt";
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File not found.");
                return;
            }
            var cryptoList = new List<Cryptocurrency>();
            using (var reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    string[] token = line.Split(':');
                    string[] coin = token[0].Split('(');
                    double price = Convert.ToDouble(token[1].Replace("$", string.Empty));
                    cryptoList.Add(new Cryptocurrency
                    {
                        Symbol = coin[1].TrimEnd(')'),
                        Name = coin[0].Trim(),
                        Price = price
                    });
                }
            }
            Console.WriteLine("File imported successfully!");

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
            var homeTimelineTweets = await userClient.Timelines.GetHomeTimelineAsync();

            //List for results
            //name, url, post contents,
            List<string> WhoMadeThePostList = new List<string>();
            List<string> UrlList = new List<string>();
            List<string> ContentsList = new List<string>();


            // Search for tweets that contain the specified keyword
            foreach (var coin in cryptoList)
            {
                //Console.WriteLine("{0}",coin.Symbol);
                var searchTerm = coin.Symbol;
                var matchingTweets = homeTimelineTweets.Where(t => t.FullText.Contains(searchTerm));

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
            ClearValuesResponse response = service.Spreadsheets.Values.Clear(requestBody, spreadsheetId, "ExistingCryptos").Execute();

            //Begin Exporting to sheets

            //Export who made the post into column B
            String range = "ExistingCryptos!B1:B" + WhoMadeThePostList.Count;
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
            String range2 = "ExistingCryptos!C1:C" + UrlList.Count;
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
            String range3 = "ExistingCryptos!D1:D" + ContentsList.Count;
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
            var range4 = "ExistingCryptos!A1:A1";
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
}
