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
    class Program
    {

        public static async Task Main(string[] args)
        {

            Console.WriteLine("Check list of new or current crypto's");
            Console.WriteLine("1. New");
            Console.WriteLine("2. Existing");
            Console.WriteLine("Type 1 or 2 and press enter");
            string answer = Console.ReadLine();

            if (answer != null)
            {
                if (answer == "1")
                {
                    await SearchNewCrypto.CompleteNewSearch(args);


                }
                else if (answer == "2")
                {
                    await SearchExistingCrypto.CompleteExistingSearch(args);
                }
                else
                {
                    Console.WriteLine("Please try again");
                    await Program.Main(args);
                }
            }
            else
            {
                Console.WriteLine("error");
            }


        }






    }
}
