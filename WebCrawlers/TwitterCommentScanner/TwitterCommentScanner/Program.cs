using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tweetinvi;
using Tweetinvi.Core;
using Tweetinvi.Core.Credentials;
using Tweetinvi.Core.Enum;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Core.Interfaces;
using Tweetinvi.Core.Interfaces.Controllers;
using Tweetinvi.Core.Interfaces.DTO;
using Tweetinvi.Core.Interfaces.Models;
using Tweetinvi.Core.Interfaces.Streaminvi;
using Tweetinvi.Core.Parameters;
using Tweetinvi.Json;
using SavedSearch = Tweetinvi.SavedSearch;
using Stream = Tweetinvi.Stream;

namespace TwitterCommentScanner
{
    class Program
    {
        private static ISampleStream _sampleStream;
        
        static void Main(string[] arg)
        {
            Auth.SetUserCredentials("", "", "", "");//Enter your Twitter Credentials

            ExceptionHandler.SwallowWebExceptions = false;
            _sampleStream = Stream.CreateSampleStream(Auth.Credentials);
            _sampleStream.AddTweetLanguageFilter(Tweetinvi.Core.Enum.Language.English);
            _sampleStream.TweetReceived += (sender, args) =>
            {
                SqlInsert(args.Tweet);

            };
            _sampleStream.StreamStarted += (sender, args) =>
            {  };

            _sampleStream.StreamStopped += (sender, args) =>
            {
                var exception = args.Exception;
                var disconnectMessage = args.DisconnectMessage;

                if (exception != null)
                    Console.WriteLine(exception.ToString());
                if (disconnectMessage != null)
                    Console.WriteLine(disconnectMessage.ToString());
            };
            while(true)
            {
                try
                {
                    new Program().RunTwitter().Wait();
                }
                catch (AggregateException ex)
                {
                    foreach (var e in ex.InnerExceptions)
                    {
                        Console.WriteLine("Error: " + e.Message);
                    }
                }
            }
        }

        private async Task RunTwitter()
        {
            _sampleStream.StartStream();
        }

        private static void SqlInsert(ITweet passed)
        {
            String comment = passed.ToString();
            String id = passed.Id.ToString();

            foreach(String word in comment.Split(' '))
            {
                if (word.Length > 0 && word.Substring(0, 1) == "@")
                    comment = comment.Replace(word, "@");
            }

            try
            {
                using (var conn = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=RawComments;Integrated Security=False;User ID=sa;Password="))//Enter Connection String
                using (var command = new SqlCommand("InsertComment", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@Id", id));
                    command.Parameters.Add(new SqlParameter("@Comment", comment));
                    command.Parameters.Add(new SqlParameter("@likeCount", passed.FavouriteCount));
                    command.Parameters.Add(new SqlParameter("@Source", "Twitter"));
                    conn.Open();

                    Int32 rdr = command.ExecuteNonQuery();

                }
            }
            catch (Exception)
            { }
        }
    }
}
