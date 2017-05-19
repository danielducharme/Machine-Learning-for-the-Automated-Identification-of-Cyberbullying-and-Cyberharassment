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
    public partial class Classification : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Name"] = Request.QueryString["Name"];

            Head.Text = Session["Name"] + " Classification";
            Reminder.Text = "Bullying Reminder:\r\n\tIs the comment obscene?\r\n\tWas the comment intended to seriously alarm, annoy or bother the person?\r\n\tAnd does the comment serve a legitimate purpose?";
            if (Session["Name"].ToString() == "ToS")
                Reminder.Text = "Bullying Reminder:\r\n\tIs the comment discussing politics or religion?";

            Counter.Text = "Bullying: " + GetBullying().ToString();

            if(Page.IsPostBack == false)
                Comment.Text = GetComment();
        }

        protected void Bully_Click(object sender, EventArgs e)
        {
            UpdateComment(-1);
            Comment.Text = GetComment();
        }

        protected void NotBully_Click(object sender, EventArgs e)
        {
            UpdateComment(1);
            Comment.Text = GetComment();
        }

        protected String GetComment()
        {
            String result = "";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CDB"].ConnectionString))
                using (var command = new SqlCommand("GetRandomComment", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@Name", Session["Name"]));
                    conn.Open();

                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            Session["ID"] = Convert.ToInt32(rdr["RecordID"]);
                            result = rdr["Comment"].ToString();
                        }
                    }
                }
            }
            catch (Exception) { }

            return result;
        }

        protected Int32 GetBullying()
        {
            Int32 result = 0;

            String SQL;

            if (Session["Name"].ToString() == "Twitter")
                SQL = "SELECT COUNT(Id) AS Bully FROM Comments WHERE DanielClassification = -1 AND Source = 'Twitter'";
            else
                SQL = "SELECT COUNT(Id) AS Bully FROM Comments WHERE " + Session["Name"].ToString() + "Classification = -1 AND Source = 'YouTube'";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CDB"].ConnectionString))
                using (var command = new SqlCommand(SQL, conn))
                {
                    conn.Open();

                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            result = rdr.GetInt32(0);
                        }
                    }
                }
            }
            catch (Exception) { }

            return result;
        }

        protected void UpdateComment(Int32 Result)
        {
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CDB"].ConnectionString))
                using (var command = new SqlCommand("UpdateComment", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@Name", Session["Name"]));
                    command.Parameters.Add(new SqlParameter("@RecordID", Session["ID"]));
                    command.Parameters.Add(new SqlParameter("@Result", Result));
                    conn.Open();

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception) { }
        }
    }
}