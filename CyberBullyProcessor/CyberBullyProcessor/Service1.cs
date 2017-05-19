using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CyberBullyProcessor
{
    public partial class CyberBullyProcessor : ServiceBase
    {
        public CyberBullyProcessor()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("CyberBullyProcessor"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "CyberBullyProcessor", "Application");
            }
            eventLog1.Source = "CyberBullyProcessor";
            eventLog1.Log = "Application";

            MachineID = GetMachineGuid();
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }

        public string GetMachineGuid()
        {
            string location = @"SOFTWARE\Microsoft\Cryptography";
            string name = "MachineGuid";

            using (RegistryKey localMachineX64View =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey rk = localMachineX64View.OpenSubKey(location))
                {
                    if (rk == null)
                        throw new KeyNotFoundException(
                            string.Format("Key Not Found: {0}", location));

                    object machineGuid = rk.GetValue(name);
                    if (machineGuid == null)
                        throw new IndexOutOfRangeException(
                            string.Format("Index Not Found: {0}", name));

                    return machineGuid.ToString();
                }
            }
        }

        private System.Diagnostics.EventLog eventLog1;
        //List<Thread> threads;
        Int32 processCounter = 0;
        protected Int32 ThreadCounter = 0;
        String MachineID;
        Boolean processing = false;

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart");
            //threads = new List<Thread>();

            // Set up a timer to trigger every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 10; // 60 seconds
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            //eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information);
            if (!processing)
            {
                processing = true;
                ProcessLoop();
            }
        }

        static ManualResetEventSlim waiter = new ManualResetEventSlim(false);

        protected void ProcessLoop()
        {
            //eventLog1.WriteEntry("ProcessLoop");
            try
            {
                using (var conn = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=CyberbullyDB;Integrated Security=False;User ID=sa;Password="))
                using (var command = new SqlCommand("GetOpenProcesses", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.CommandTimeout = 0;
                    conn.Open();

                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            //eventLog1.WriteEntry("CommentID " + rdr["CommentID"].ToString());

                            Int32 commentID = Convert.ToInt32(rdr["CommentID"].ToString());

                            while(ThreadCounter >= Environment.ProcessorCount - 1)
                            {
                                //Thread.Sleep(1000);
                                waiter.Reset();
                                waiter.Wait();
                            }

                            using (var conn1 = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=CyberbullyDB;Integrated Security=False;User ID=sa;Password="))
                            using (var command1 = new SqlCommand("LockProcess", conn1) { CommandType = CommandType.StoredProcedure })
                            {
                                String ProcessID = MachineID + "." + Process.GetCurrentProcess().Id + "." + processCounter++;
                                command1.Parameters.Add(new SqlParameter("@CommentID", commentID));
                                command1.Parameters.Add(new SqlParameter("@ProcessID", ProcessID));
                                command1.CommandTimeout = 0;
                                conn1.Open();

                                using (SqlDataReader rdr1 = command1.ExecuteReader())
                                {
                                    while (rdr1.Read())
                                    {
                                        eventLog1.WriteEntry("CommentID " + rdr["CommentID"].ToString() + " Taken By " + rdr1["ProcessID"].ToString());
                                        if (rdr1["ProcessID"].ToString() == ProcessID)
                                        {
                                            Thread newThread = new Thread(delegate () { ThreadFunction(commentID); });
                                            newThread.IsBackground = true;
                                            ThreadCounter++;
                                            newThread.Start();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                eventLog1.WriteEntry("GetOpenProcesses Failed with: " + e.ToString());
            }
            finally
            {
                processing = false;
            }
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In onStop.");
        }

        protected void UnlockProcess(Int32 test)
        {
            using (var conn1 = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=CyberbullyDB;Integrated Security=False;User ID=sa;Password="))
            using (var command1 = new SqlCommand("UnlockProcess", conn1) { CommandType = CommandType.StoredProcedure })
            {
                command1.Parameters.Add(new SqlParameter("@CommentID", test));
                command1.CommandTimeout = 0;
                conn1.Open();

                command1.BeginExecuteNonQuery();
            }
        }

        protected void ThreadFunction(Int32 test)
        {
            eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " starting.");

            String comment = "";
            String cleancomment = "";
            String[] OneGrams;
            List<String> NGrams = new List<String>();
            Int32 Length;
            Int32 NumCaps = 0;
	        Decimal PercentCaps = 0;
            Int32 NGram = 3;
            
            try
            {
                using (var conn = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=CyberbullyDB;Integrated Security=False;User ID=sa;Password="))
                using (var command = new SqlCommand("GetComment", conn) {CommandType = CommandType.StoredProcedure})
                {
                    command.Parameters.Add(new SqlParameter("@CommentID", test));
                    conn.Open();

                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            comment = rdr["Comment"].ToString();
                            cleancomment = rdr["CleanedComment"].ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " GetComment Failed with: " + e.ToString());
                UnlockProcess(test);
                ThreadCounter--;
                waiter.Set();
                Thread.CurrentThread.Abort();
            }

            OneGrams = cleancomment.Split('|');

            for (Int32 i = 1; i <= NGram; i++)
            {
                for (Int32 n = 1; n < OneGrams.Length - i - 1; n++)
                {
                    String adder = OneGrams[n];

                    for(Int32 count = i - 1; count >= 1; count--)
                    {
                        adder += "|" + OneGrams[n + i - count];
                    }

                    NGrams.Add(adder);
                }
            }

            try
            {
                using (var conn = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=CyberbullyDB;Integrated Security=False;User ID=sa;Password="))
                using (var command = new SqlCommand("ClearNGramPercent", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@CommentID", test));
                    conn.Open();

                    Int32 rdr = command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " ClearNGramPercent Failed with: " + e.ToString());
                UnlockProcess(test);
                ThreadCounter--;
                waiter.Set();
                Thread.CurrentThread.Abort();
            }

            foreach (String Gram in NGrams)   
            {
                if (Gram == "")
                    continue;

                try
                {
                    using (var conn = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=CyberbullyDB;Integrated Security=False;User ID=sa;Password="))
                    using (var command = new SqlCommand("InsertNGram", conn) { CommandType = CommandType.StoredProcedure })
                    {
                        command.Parameters.Add(new SqlParameter("@NGram", Gram));
                        conn.Open();

                        Int32 rdr = command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " InsertNGram Failed with: " + e.ToString());
                    UnlockProcess(test);
                    ThreadCounter--;
                    waiter.Set();
                    Thread.CurrentThread.Abort();
                }

                Decimal commentLength = cleancomment.Length;
                Decimal commentNoGramLength = cleancomment.Replace("|" + Gram + "|", "|").Length;
                Decimal gramLength = Gram.Length + 1;
                Decimal Contain = (commentLength - commentNoGramLength) / gramLength;
                Decimal commentWords = cleancomment.Replace("|", "").Length;
                Decimal gramWords = Gram.Replace("|", "").Length;
                Decimal Percent = Contain / ((commentLength - commentWords - 1) / (gramLength - gramWords));

                try
                {
                    using (var conn = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=CyberbullyDB;Integrated Security=False;User ID=sa;Password="))
                    using (var command = new SqlCommand("InsertNGramPercent", conn) { CommandType = CommandType.StoredProcedure })
                    {
                        command.Parameters.Add(new SqlParameter("@CommentID", test));
                        command.Parameters.Add(new SqlParameter("@NGram", Gram));
                        command.Parameters.Add(new SqlParameter("@Pct", Percent));
                        conn.Open();

                        Int32 rdr = command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " InsertNGramPercent Failed with: " + e.ToString());
                    UnlockProcess(test);
                    ThreadCounter--;
                    waiter.Set();
                    Thread.CurrentThread.Abort();
                }
            }

            Length = comment.Length;

            foreach (Char i in comment)
            {
                if (Char.IsUpper(i))
                    NumCaps++;
            }

            PercentCaps = (Decimal)NumCaps/(Decimal)Length;

            try
            {
                using (var conn = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=CyberbullyDB;Integrated Security=False;User ID=sa;Password="))
                using (var command = new SqlCommand("UpdateStats", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@CommentID", test));
	                command.Parameters.Add(new SqlParameter("@PercentCaps", PercentCaps));
	                conn.Open();

                    Int32 rdr = command.ExecuteNonQuery();
                    
                }  
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " UpdateStats Failed with: " + e.ToString());
                UnlockProcess(test);
                ThreadCounter--;
                waiter.Set();
                Thread.CurrentThread.Abort();
            }

            ThreadCounter--;
            waiter.Set();
            eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " completed.");
        }
    }
}
