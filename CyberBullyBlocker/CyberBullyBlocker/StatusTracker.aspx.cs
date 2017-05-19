using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CyberBullyBlocker
{
    public partial class StatusTracker : System.Web.UI.Page
    {
        static Int32 MinAverageProcessing = 0;
        static Int32 MaxAverageProcessing = 0;
        static Int32 MinAverageAnalysis = 0;
        static Int32 MaxAverageAnalysis = 0;
        static Boolean FinishedProcessing = false;
        protected void Page_Load(object sender, EventArgs e)
        {
            Head.Text = "Reloads every 60 sec.";

            Int32 ProcessingRemaining = GetProcessingRemaining();
            Tracker.Text = "<br>Processing Remaining: " + ProcessingRemaining.ToString();

            //if (ProcessingRemaining > 0)
            //    FinishedProcessing = false;
            //else if(ProcessingRemaining == 0 && !FinishedProcessing)
            //{
            //    Boolean stop = StopProcService();
            //    if (stop)
            //    {
            //        StartService();
            //        FinishedProcessing = true;
            //    }
            //}

            Int32 AverageProcessing = GetAverageProcessing();
            if (MinAverageProcessing == 0 || (MinAverageProcessing > AverageProcessing && AverageProcessing != 0))
                MinAverageProcessing = AverageProcessing;
            if ((MaxAverageProcessing < AverageProcessing && AverageProcessing != 0))
                MaxAverageProcessing = AverageProcessing;

            Tracker.Text += "<br>Average Processing: " + AverageProcessing.ToString() + "ms";
            Tracker.Text += "<br>Minimum Average Processing: " + MinAverageProcessing.ToString() + "ms";
            Tracker.Text += "<br>Maximum Average Processing: " + MaxAverageProcessing.ToString() + "ms";
            DateTime now = DateTime.Now;
            DateTime finishProcessing = DateTime.Now.AddMilliseconds(ProcessingRemaining * AverageProcessing);
            DateTime maxFinishProcessing = DateTime.Now.AddMilliseconds(ProcessingRemaining * MaxAverageProcessing);
            TimeSpan spanProcessing = (finishProcessing - now);
            TimeSpan maxSpanProcessing = (maxFinishProcessing - now);
            Tracker.Text += "<br>Estimated Time Remaining: " + String.Format("{0}:{1}:{2}:{3}:{4}", spanProcessing.Days, spanProcessing.Hours, spanProcessing.Minutes, spanProcessing.Seconds, spanProcessing.Milliseconds);
            Tracker.Text += "<br>Max Time Remaining: " + String.Format("{0}:{1}:{2}:{3}:{4}", maxSpanProcessing.Days, maxSpanProcessing.Hours, maxSpanProcessing.Minutes, maxSpanProcessing.Seconds, maxSpanProcessing.Milliseconds);

            Int32 AnalysisRemaining = GetAnalysisRemaining();
            Tracker.Text += "<br><br>Analysis Remaining: " + AnalysisRemaining.ToString();

            Int32 AverageAnalysis = GetAverageAnalysis();
            if (MinAverageAnalysis == 0 || (MinAverageAnalysis > AverageAnalysis && AverageAnalysis != 0))
                MinAverageAnalysis = AverageAnalysis;
            if ((MaxAverageAnalysis < AverageAnalysis && AverageAnalysis != 0))
                MaxAverageAnalysis = AverageAnalysis;

            Tracker.Text += "<br>Average Analysis: " + AverageAnalysis.ToString() + "ms";
            Tracker.Text += "<br>Minimum Average Analysis: " + MinAverageAnalysis.ToString() + "ms";
            Tracker.Text += "<br>Maximum Average Analysis: " + MaxAverageAnalysis.ToString() + "ms";
            //Tracker.Text += "<br>Correct: " + GetCorrect().ToString();
            //Tracker.Text += "<br>Incorrect: " + GetIncorrect().ToString();
            now = DateTime.Now;
            DateTime finishAnalysis = DateTime.Now.AddMilliseconds(AnalysisRemaining * AverageAnalysis);
            DateTime maxFinishAnalysis = DateTime.Now.AddMilliseconds(AnalysisRemaining * MaxAverageAnalysis);
            TimeSpan spanAnalysis = (finishAnalysis - now);
            TimeSpan maxSpanAnalysis = (maxFinishAnalysis - now);
            Tracker.Text += "<br>Estimated Time Remaining: " + String.Format("{0}:{1}:{2}:{3}:{4}", spanAnalysis.Days, spanAnalysis.Hours, spanAnalysis.Minutes, spanAnalysis.Seconds, spanAnalysis.Milliseconds);
            Tracker.Text += "<br>Max Time Remaining: " + String.Format("{0}:{1}:{2}:{3}:{4}", maxSpanAnalysis.Days, maxSpanAnalysis.Hours, maxSpanAnalysis.Minutes, maxSpanAnalysis.Seconds, maxSpanAnalysis.Milliseconds);
        }

        protected Int32 GetProcessingRemaining()
        {
            Int32 result = 0;

            String SQL = "SELECT COUNT(CommentID) AS Remaining FROM Comment (NOLOCK) WHERE Processed = 0";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
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

        protected Int32 GetAverageProcessing()
        {
            Int32 result = 0;

            String SQL = "SELECT DATEDIFF(ms, (SELECT MIN(StartProcess) FROM Comment (NOLOCK)), (SELECT MAX(EndProcess) FROM COMMENT (NOLOCK)))/COUNT(CommentID) AS Average FROM Comment (NOLOCK) WHERE Processed = 1";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
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

        protected Int32 GetAnalysisRemaining()
        {
            Int32 result = 0;

            String SQL = "SELECT COUNT(CommentID) AS Remaining FROM Comment (NOLOCK) WHERE Analyzed = 0 AND Processed = 1";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
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

        protected Int32 GetAverageAnalysis()
        {
            Int32 result = 0;

            String SQL = "SELECT DATEDIFF(ms, (SELECT MIN(StartAnalysis) FROM Comment (NOLOCK) WHERE Analyzed = 1), (SELECT MAX(EndAnalysis) FROM COMMENT (NOLOCK) WHERE Analyzed = 1))/COUNT(CommentID) AS Average FROM Comment (NOLOCK) WHERE Analyzed = 1";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
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

        protected Int32 GetCorrect()
        {
            Int32 result = 0;

            String SQL = "SELECT COUNT(CommentID) AS Correct FROM Comment (NOLOCK) WHERE (TrainValue > 0 AND Bullying = 1) OR (TrainValue < 0 AND Bullying = -1)";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
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

        protected Int32 GetIncorrect()
        {
            Int32 result = 0;

            String SQL = "SELECT COUNT(CommentID) AS Correct FROM Comment (NOLOCK) WHERE (TrainValue > 0 AND Bullying = -1) OR (TrainValue < 0 AND Bullying = 1)";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
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

        protected void Reset_Click(object sender, EventArgs e)
        {
            /*Boolean stop = StopService();
            if (stop)
            {
                StoreResult();
                IncreaseWrongComments();
                ResetCounter();
                StartService();
            }*/

            MinAverageProcessing = 0;
            MaxAverageProcessing = 0;
            MinAverageAnalysis = 0;
            MaxAverageAnalysis = 0;
            FinishedProcessing = false;
        }

        protected Boolean StopService()
        {
            if (impersonateValidUser("administrator", "PHD-SERVER", ""))
            {
                ServiceController service = new ServiceController("CyberBullyAnalyzer");
                try
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(10000);

                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }
                catch
                {
                    undoImpersonation();
                    return false;
                }
            }

            undoImpersonation();
            return true;
        }

        protected Boolean StopProcService()
        {
            if (impersonateValidUser("administrator", "PHD-SERVER", ""))
            {
                ServiceController service = new ServiceController("CyberBullyProcessor");
                try
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(10000);

                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }
                catch
                {
                    undoImpersonation();
                    return false;
                }
            }

            undoImpersonation();
            return true;
        }

        protected void StoreResult()
        {
            String SQL = "INSERT INTO Results (TrainingSetSize, NGramLevel, NGramPercent, knnLevel, SVMType, SVMKernal, C, Degree, Coef0, RunNumber, CorrectPositive, CorrectNegative, WrongPositive, WrongNegative) VALUES((SELECT COUNT(CommentID) FROM TrainingSet), 3, 7, 14, 'C_SVC', 'POLY', 1, 2, 1, (SELECT ISNULL(MAX(RunNumber), 0) + 1 FROM Results), (SELECT COUNT(CommentID) FROM Comment WHERE TrainValue > 0 AND Bullying = 1), (SELECT COUNT(CommentID) FROM Comment WHERE TrainValue < 0 AND Bullying = -1), (SELECT COUNT(CommentID) FROM Comment WHERE TrainValue > 0 AND Bullying = -1), (SELECT COUNT(CommentID) FROM Comment WHERE TrainValue < 0 AND Bullying = 1))";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
                using (var command = new SqlCommand(SQL, conn))
                {
                    conn.Open();

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception) { }
        }

        protected void IncreaseWrongComments()
        {
            String SQL = "UPDATE Comment SET TrainValue = (ABS(TrainValue) + 1) * (TrainValue / ABS(TrainValue)) WHERE (TrainValue < 0 AND Bullying > 0) OR (TrainValue > 0 AND Bullying < 0)";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
                using (var command = new SqlCommand(SQL, conn))
                {
                    conn.Open();

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception) { }
        }

        protected void ResetCounter()
        {
            String SQL = "UPDATE Comment SET StartAnalysis = NULL, InAnalysis = 0, EndAnalysis = NULL, Analyzed = 0, Bullying = 0, Probability = NULL";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CBDB"].ConnectionString))
                using (var command = new SqlCommand(SQL, conn))
                {
                    conn.Open();

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception) { }
        }

        protected void StartService()
        {
            if (impersonateValidUser("administrator", "PHD-SERVER", ""))
            {
                ServiceController service = new ServiceController("CyberBullyAnalyzer");
                try
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(10000);

                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
                catch
                {}
            }

            undoImpersonation();
        }

        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_PROVIDER_DEFAULT = 0;

        WindowsImpersonationContext impersonationContext;

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(String lpszUserName,
        String lpszDomain,
        String lpszPassword,
        int dwLogonType,
        int dwLogonProvider,
        ref IntPtr phToken);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,
        int impersonationLevel,
        ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);


        private bool impersonateValidUser(String userName, String domain, String password)
        {
            WindowsIdentity tempWindowsIdentity;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            if (RevertToSelf())
            {
                if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE,
                LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        impersonationContext = tempWindowsIdentity.Impersonate();
                        if (impersonationContext != null)
                        {
                            CloseHandle(token);
                            CloseHandle(tokenDuplicate);
                            return true;
                        }
                    }
                }
            }
            if (token != IntPtr.Zero)
                CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);
            return false;
        }

        private void undoImpersonation()
        {
            impersonationContext.Undo();
        }
    }
}