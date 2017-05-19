using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace CommentScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            finishedVids = new List<String>();
            
            while (true)
            {
                try
                {
                    new Program().RunYouTube().Wait();
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

        static List<String> finishedVids;

        private async Task RunYouTube()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "",//Enter your API Key here
                    ApplicationName = "CommentScanner"
                });

            var playlistItems = youtubeService.PlaylistItems.List("snippet");
            playlistItems.PlaylistId = "PLrEnWoR732-BHrPp_Pm8_VleD68f9s14-";
            playlistItems.MaxResults = 50;

            String nextPageToken = "";
            PlaylistItemListResponse playlistResponse;
            
            do
            {
                playlistResponse = await playlistItems.ExecuteAsync();

                foreach (var playlistResult in playlistResponse.Items)
                {
                    if (!finishedVids.Contains(playlistResult.Snippet.ResourceId.VideoId))
                    {
                        finishedVids.Add(playlistResult.Snippet.ResourceId.VideoId);

                        var commentThreadsList = youtubeService.CommentThreads.List("snippet");
                        commentThreadsList.VideoId = playlistResult.Snippet.ResourceId.VideoId;
                        commentThreadsList.MaxResults = 100;
                        commentThreadsList.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;

                        CommentThreadListResponse commentThreadsResponse;

                        do
                        {
                            commentThreadsResponse = await commentThreadsList.ExecuteAsync();

                            foreach (var commentThreadsResult in commentThreadsResponse.Items)
                            {

                                SqlInsert(commentThreadsResult, "YouTube");

                                if (commentThreadsResult.Snippet.TotalReplyCount != 0)
                                {
                                    var commentList = youtubeService.Comments.List("snippet");
                                    commentList.ParentId = commentThreadsResult.Id;
                                    commentList.MaxResults = 100;
                                    commentList.TextFormat = CommentsResource.ListRequest.TextFormatEnum.PlainText;

                                    CommentListResponse commentResponse;

                                    do
                                    {
                                        commentResponse = await commentList.ExecuteAsync();

                                        foreach (var commentResult in commentResponse.Items)
                                        {
                                            SqlInsert(commentResult, "YouTube");
                                        }

                                        nextPageToken = commentResponse.NextPageToken;
                                        commentList.PageToken = nextPageToken;
                                    } while (nextPageToken != null);
                                }

                            }

                            nextPageToken = commentThreadsResponse.NextPageToken;
                            commentThreadsList.PageToken = nextPageToken;
                        } while (nextPageToken != null);
                    }
                }

                nextPageToken = playlistResponse.NextPageToken;
                playlistItems.PageToken = nextPageToken;
            } while (nextPageToken != null);

            finishedVids = new List<string>();
        }

        private static void SqlInsert(CommentThread passed, String source)
        {
            String comment = passed.Snippet.TopLevelComment.Snippet.TextDisplay.Replace("'", "`");

            if (comment.Substring(0, 1) == "+" && comment.Substring(1, 1) != " ")
            {
                Int32 index = comment.IndexOf(' ');
                comment = comment.Substring(index);
            }
            
            try
            {
                using (var conn = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=RawComments;Integrated Security=False;User ID=sa;Password="))//Enter Connection String
                using (var command = new SqlCommand("InsertComment", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@Id", passed.Snippet.TopLevelComment.Id));
                    command.Parameters.Add(new SqlParameter("@Comment", comment));
                    command.Parameters.Add(new SqlParameter("@likeCount", passed.Snippet.TopLevelComment.Snippet.LikeCount));
                    command.Parameters.Add(new SqlParameter("@Source", source));
                    conn.Open();

                    Int32 rdr = command.ExecuteNonQuery();

                }
            }
            catch (Exception)
            {  }
        }

        private static void SqlInsert(Comment passed, String source)
        {
            String comment = passed.Snippet.TextDisplay.Replace("'", "`");

            if (comment.Substring(0, 1) == "+" && comment.Substring(1, 1) != " ")
            {
                Int32 index = comment.IndexOf(' ');
                comment = comment.Substring(index);
            }

            try
            {
                using (var conn = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=RawComments;Integrated Security=False;User ID=sa;Password="))//Enter Connection String
                using (var command = new SqlCommand("InsertComment", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@Id", passed.Id));
                    command.Parameters.Add(new SqlParameter("@Comment", comment));
                    command.Parameters.Add(new SqlParameter("@likeCount", passed.Snippet.LikeCount));
                    command.Parameters.Add(new SqlParameter("@Source", source));
                    conn.Open();

                    Int32 rdr = command.ExecuteNonQuery();

                }
            }
            catch (Exception)
            { }
        }
    }
}
