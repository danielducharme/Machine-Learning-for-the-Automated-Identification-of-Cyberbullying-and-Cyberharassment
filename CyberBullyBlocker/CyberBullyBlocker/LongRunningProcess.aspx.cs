using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CyberBullyBlocker
{
    public partial class LongRunningProcess : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String result = "FAILED!";
            comment = Session["Comment"].ToString();
            
            // Padding to circumvent IE's buffer*
            Response.Write(new string('*', 256));  
            Response.Flush();

            // Initialization
            UpdateProgress(0, "Comment Uploading");
            Int32 id = UploadComment();

            if (id != -1)
            {
                // Gather data.
                UpdateProgress(25, "Comment Processing");
                ProcessComment(id);

                // Process data.
                UpdateProgress(50, "Comment Analyzing");
                AnalyzeComment(id);

                // Clean up.
                UpdateProgress(75, "Grabbing Results");
                result = GetResults(id);
            }

            // Task completed.
            UpdateProgress(100, result);
        }

        protected void UpdateProgress(int PercentComplete, string Message)
        {
            // Write out the parent script callback.
            Response.Write(String.Format(
              "<script>parent.UpdateProgress({0}, '{1}');</script>",
              PercentComplete, Message));
            // To be sure the response isn't buffered on the server.
            Response.Flush();
        }

        protected String comment;

        protected Int32 UploadComment()
        {
            Int32 result = -1;
            
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
                using (var command = new SqlCommand("InsertComment", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@Comment", comment));
                    conn.Open();

                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            result = Convert.ToInt32(rdr["CommentID"]);
                        }
                    }
                }
            }
            catch (Exception) { }

            return result;
        }

        protected void ProcessComment(Int32 id)
        {
            Boolean processed = false;

            while (!processed)
            {
                try
                {
                    using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
                    using (var command = new SqlCommand("GetProcessedStatus", conn) { CommandType = CommandType.StoredProcedure })
                    {
                        command.Parameters.Add(new SqlParameter("@CommentID", id));
                        conn.Open();

                        using (SqlDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                processed = Convert.ToBoolean(rdr["Processed"]);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    processed = true;
                }

                if (!processed)
                {
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        protected void AnalyzeComment(Int32 id)
        {
            Boolean analyzed = false;

            while (!analyzed)
            {
                try
                {
                    using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
                    using (var command = new SqlCommand("GetAnalyzedStatus", conn) { CommandType = CommandType.StoredProcedure })
                    {
                        command.Parameters.Add(new SqlParameter("@CommentID", id));
                        conn.Open();

                        using (SqlDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                analyzed = Convert.ToBoolean(rdr["Analyzed"]);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    analyzed = true;
                }

                if (!analyzed)
                {
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        protected String GetResults(Int32 id)
        {
            String result = "";
            Boolean resultID = false;
            
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
                using (var command = new SqlCommand("GetResult", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@CommentID", id));
                    conn.Open();

                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            resultID = (Convert.ToInt32(rdr["Bullying"]) == -1);
                        }
                    }
                }
            }
            catch (Exception)
            {
                result = "ERROR!";
            }

            if (resultID)
            {
                result = "BULLYING!";
            }
            else
            {
                result = "Not bullying";
            }

            return result;
        }
    }
}