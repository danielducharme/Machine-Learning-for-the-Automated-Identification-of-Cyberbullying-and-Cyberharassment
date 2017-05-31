using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibSVMsharp;
using LibSVMsharp.Core;
using LibSVMsharp.Extensions;
using LibSVMsharp.Helpers;
using Microsoft.Win32;
using Accord.Statistics.Kernels;

namespace CyberBullyAnalyzer
{
    public partial class CyberBullyAnalyzer : ServiceBase
    {
        public CyberBullyAnalyzer()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("CyberBullyAnalyzer"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "CyberBullyAnalyzer", "Application");
            }
            eventLog1.Source = "CyberBullyAnalyzer";
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
        SVMModel model;
        Boolean Trained = false, InTraining = false;
        Int32 processCounter = 0;
        String MachineID;
        Boolean processing = false;

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart");

            // Set up a timer to trigger every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 10; // 60 seconds
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            //eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information);
            if (!Trained && !InTraining)
            {
                InTraining = true;
                TrainModel();
            }
            if (Trained && !processing)
            {
                processing = true;
                ProcessLoop();
            }
        }

        protected String knnStep(List<String> dSet, String entry, Int32 k)
        {
            k = k + 1;

            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";
            
            String[] list = entry.Trim().Split(' ');

            Double y = Convert.ToDouble(list[0].Trim(), provider);

            List<Double> attributes = new List<Double>();
            for (int i = 1; i < list.Length; i++)
            {
                String[] temp = list[i].Split(':');
                Double Value = Convert.ToDouble(temp[1].Trim(), provider);
                attributes.Add(Value);
            }

            List<Double[]> dSet2 = new List<Double[]>();
            foreach(String row in dSet)
            {
                String[] rowList = row.Trim().Split(' ');

                Double rowy = Convert.ToDouble(rowList[0].Trim(), provider);

                List<Double> rowattributes = new List<Double>();
                for(int rowi = 1; rowi < rowList.Length; rowi++)
                {
                    String[] temp = rowList[rowi].Split(':');
                    Double Value = Convert.ToDouble(temp[1].Trim(), provider);
                    Value = Value - attributes[rowi - 1];
                    Value = Value * Value;
                    rowattributes.Add(Value);
                }

                Double[] newRow = new Double[2];
                newRow[0] = rowattributes.Sum();
                newRow[1] = rowy;
                dSet2.Add(newRow);
            }

            dSet2  = dSet2.OrderBy(arr => arr[0]).ToList();

            Int32 neighbors = 0;
            Int32 count = 0;

            foreach (Double[] row in dSet2)
            {
                if (y != row[1])
                    neighbors++;

                count++;
                if(count >= k)
                    break;
            }

            if (neighbors > 1)
                return entry;
            return null;
        }

        protected List<String> knnBuilder(List<String> dataSetName, Int32 k, Boolean normalizedData)
        {
            if (k < 1)
                return dataSetName;

            List<String> output = new List<String>();

            foreach (String row in dataSetName)
            {
                if (knnStep(dataSetName, row, k) == row)
                    output.Add(row);
            }

            return output;
        }

        public class CrossValidator
        {
            public Int32 type;
            public Int32 kernel;
            public Decimal C;
            public Decimal Nu;
            public Decimal Gamma;
            public Int32 Degree;
            public Decimal Coef0;
            public Decimal accuracy;
            public Decimal minAccuracy;
            public Decimal maxAccuracy;
            public Int32 traintimetaken;
            public Int32 testtimetaken;
            public Int32 correctPositive;
            public Int32 correctNegative;
            public Int32 wrongPositive;
            public Int32 wrongNegative;
        }

		protected static Double Bootstrap(List<String> traininglist, List<String> testinglist, SVMParameter parameter)
		{
			NumberFormatInfo provider = new NumberFormatInfo();
			provider.NumberDecimalSeparator = ".";

			SVMProblem train = new SVMProblem();

			foreach (String row in traininglist)
			{
				String[] list = row.Trim().Split(' ');

				Double y = Convert.ToDouble(list[0].Trim(), provider);

				List<SVMNode> nodes = new List<SVMNode>();
				for (int i = 1; i < list.Length; i++)
				{
					string[] temp = list[i].Split(':');
					SVMNode node = new SVMNode();
					node.Index = Convert.ToInt32(temp[0].Trim());
					node.Value = Convert.ToDouble(temp[1].Trim(), provider);
					nodes.Add(node);
				}

				train.Add(nodes.ToArray(), y);
			}

			SVMModel CVModel;

			CVModel = SVM.Train(train, parameter);

			SVMProblem test = new SVMProblem();

			foreach (String row in testinglist)
			{
				String[] list = row.Trim().Split(' ');

				Double y = Convert.ToDouble(list[0].Trim(), provider);

				List<SVMNode> nodes = new List<SVMNode>();
				for (int i = 1; i < list.Length; i++)
				{
					string[] temp = list[i].Split(':');
					SVMNode node = new SVMNode();
					node.Index = Convert.ToInt32(temp[0].Trim());
					node.Value = Convert.ToDouble(temp[1].Trim(), provider);
					nodes.Add(node);
				}

				test.Add(nodes.ToArray(), y);
			}


			Double[] results = test.Predict(CVModel);
			return test.EvaluateClassificationProblem(results);
		}

		public static List<List<String>> splitList(List<String> locations, Int32 nSize)
		{
			Int32[] nums = Enumerable.Range(0, locations.Count).ToArray();
			Random rnd = new Random();

			// Shuffle the array
			for (Int32 i = 0; i < nums.Length; ++i)
			{
				Int32 randomIndex = rnd.Next(nums.Length);
				Int32 temp = nums[randomIndex];
				nums[randomIndex] = nums[i];
				nums[i] = temp;
			}

			List<List<String>> list = new List<List<String>>();

			for (Int32 i = 0; i < locations.Count; i += nSize)
			{
				List<String> newString = new List<String>();

				for (Int32 j = i; j < i + nSize && j < locations.Count; j++)
					newString.Add(locations[nums[j]]);

				list.Add(newString);
			}

			return list;
		}

		protected static Double ReimplementedCV(List<String> trainingset, Int32 folds, SVMParameter parameter)
		{
			NumberFormatInfo provider = new NumberFormatInfo();
			provider.NumberDecimalSeparator = ".";

			List<List<String>> cvList = splitList(trainingset, (trainingset.Count / folds));
			Double[] cv = new Double[cvList.Count()];

			List<String> traininglist = new List<String>();

			for (Int32 fold = 0; fold < cvList.Count(); fold++)
			{
				for (Int32 trainfold = 0; trainfold < cvList.Count(); trainfold++)
				{
					if (fold != trainfold)
					{
						traininglist.AddRange(cvList[trainfold]);
					}
				}

				SVMProblem boot = new SVMProblem();

				foreach (String row in traininglist)
				{
					String[] list = row.Trim().Split(' ');

					Double y = Convert.ToDouble(list[0].Trim(), provider);

					List<SVMNode> nodes = new List<SVMNode>();
					for (int i = 1; i < list.Length; i++)
					{
						string[] temp = list[i].Split(':');
						SVMNode node = new SVMNode();
						node.Index = Convert.ToInt32(temp[0].Trim());
						node.Value = Convert.ToDouble(temp[1].Trim(), provider);
						nodes.Add(node);
					}

					boot.Add(nodes.ToArray(), y);
				}

				SVMModel CVModel;

				CVModel = SVM.Train(boot, parameter);

				SVMProblem test = new SVMProblem();

				foreach (String row in cvList[fold])
				{
					String[] list = row.Trim().Split(' ');

					Double y = Convert.ToDouble(list[0].Trim(), provider);

					List<SVMNode> nodes = new List<SVMNode>();
					for (int i = 1; i < list.Length; i++)
					{
						string[] temp = list[i].Split(':');
						SVMNode node = new SVMNode();
						node.Index = Convert.ToInt32(temp[0].Trim());
						node.Value = Convert.ToDouble(temp[1].Trim(), provider);
						nodes.Add(node);
					}

					test.Add(nodes.ToArray(), y);
				}


				Double[] results = test.Predict(CVModel);
				cv[fold] = test.EvaluateClassificationProblem(results);

				traininglist.Clear();
			}

			return cv.Average();
		}

		protected void ThreadSafeCrossValidation(SVMProblem training, CrossValidator cval, List<String> knnList, SVMProblem testing)
        {
            DateTime Start = DateTime.Now;
            SVMParameter parameter = new SVMParameter();

            if (cval.type == 1)
            {
                parameter.Type = SVMType.C_SVC;
                parameter.C = Math.Pow(2, (Double)cval.C);
            }
            else
            {
                parameter.Type = SVMType.NU_SVC;
                parameter.Nu = (Double)cval.Nu;
            }

            if (cval.kernel == 1)
                parameter.Kernel = SVMKernelType.LINEAR;
            else if (cval.kernel == 2)
            {
                parameter.Kernel = SVMKernelType.POLY;
                parameter.Gamma = Math.Pow(2, (Double)cval.Gamma);
                parameter.Degree = cval.Degree;
                parameter.Coef0 = (Double)cval.Coef0;
            }
            else if (cval.kernel == 3)
            {
                parameter.Kernel = SVMKernelType.RBF;
                parameter.Gamma = Math.Pow(2, (Double)cval.Gamma);
            }

			cval.accuracy = Convert.ToDecimal(ReimplementedCV(knnList, 10, parameter));

			DateTime Training = DateTime.Now;
            cval.traintimetaken = Convert.ToInt32(Training.Subtract(Start).TotalMilliseconds);

			List<Decimal> a = new List<Decimal>();
			List<Decimal> b = new List<Decimal>();
			Random rnd = new Random();
			NumberFormatInfo provider = new NumberFormatInfo();
			provider.NumberDecimalSeparator = ".";

			Decimal err = Convert.ToDecimal(0.368 * Bootstrap(knnList, knnList, parameter));
			Int32 accErrCount = 0;

			for (Int32 bootstrap = 1; bootstrap <= 200; bootstrap++)
			{
				Console.SetCursorPosition(0, Console.CursorTop);
				Console.Write("Getting Bootstrap Set...{0}", bootstrap);

				List<String> tempDataList = new List<String>(knnList.Count);
				List<String> testDataList = new List<String>(knnList.Count);

				Int32[] nums = new Int32[knnList.Count];

				// Shuffle the array
				for (Int32 i = 0; i < knnList.Count; i++)
				{
					nums[i] = rnd.Next(0, knnList.Count);
				}

				for (Int32 i = 0; i < knnList.Count; i++)
				{
					tempDataList.Add(knnList[nums[i]]);
				}

				for (Int32 i = 0; i < knnList.Count; i++)
				{
					Boolean found = false;
					for (Int32 j = 0; j < knnList.Count && !found; j++)
						found = nums[j] == i;

					if (!found)
						testDataList.Add(knnList[i]);
				}

				Decimal testerr = Convert.ToDecimal(Bootstrap(tempDataList, testDataList, parameter));
				a.Add(err + 0.632m * testerr);
				b.Add(testerr);
			}
			Console.WriteLine("");

			a = a.OrderBy(arr => arr).ToList();
			b = b.OrderBy(arr => arr).ToList();

			cval.minAccuracy = a[5 - 1];
			cval.maxAccuracy = a[195 - 1];
			cval.Nu = -1;

			if (cval.accuracy < cval.minAccuracy)
			{
				cval.minAccuracy = b[5 - 1];
				cval.maxAccuracy = b[195 - 1];
				cval.Nu = -2;

				if (cval.accuracy < cval.minAccuracy)
				{
					ThreadCounter--;
					waiter.Set();
					return;
				}
			}

			Training = DateTime.Now;

			SVMModel testmodel = SVM.Train(training, parameter);

			for (Int32 test = 0; test < testing.Length; test++)
			{
				Double result;
				Console.SetCursorPosition(0, Console.CursorTop);
				Console.Write("Testing...{0}", test);

				try
				{
					result = SVM.Predict(testmodel, testing.X[test]);

					if (result == 1 && testing.Y[test] == 1)
						cval.correctPositive++;
					else if (result == 1 && testing.Y[test] == -1)
						cval.wrongNegative++;
					else if (result == -1 && testing.Y[test] == 1)
						cval.wrongPositive++;
					else if (result == -1 && testing.Y[test] == -1)
						cval.correctNegative++;
				}
				catch (ArgumentNullException) { break; }
			}
			Console.WriteLine("");

			testmodel = null;

			DateTime Finish = DateTime.Now;

			cval.testtimetaken = Convert.ToInt32(Finish.Subtract(Training).TotalMilliseconds);

			UpdateResults(RecordCount, NGramLevel, NGramPercent, knnLevel, cval);
			ThreadCounter--;
			waiter.Set();
		}

        protected void UpdateResults(Int32 TrainingSetSize, Int32 NGramLevel, Int32 NGramPercent, Int32 knnLevel, CrossValidator cval)
        {
            try
            {
                using (var conn = new SqlConnection(SqlSecurity.SqlConnectionString))
                using (var command = new SqlCommand("InsertResult", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@TrainingSetSize", TrainingSetSize));
                    command.Parameters.Add(new SqlParameter("@NGramLevel", NGramLevel));
                    command.Parameters.Add(new SqlParameter("@NGramPercent", NGramPercent));
                    command.Parameters.Add(new SqlParameter("@knnLevel", knnLevel));
                    if (cval.type == 1)
                    {
                        command.Parameters.Add(new SqlParameter("@SVMType", "C_SVC"));
                        command.Parameters.Add(new SqlParameter("@C", cval.C));
                        command.Parameters.Add(new SqlParameter("@Nu", DBNull.Value));
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@SVMType", "NU_SVC"));
                        command.Parameters.Add(new SqlParameter("@C", DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Nu", cval.Nu));
                    }


                    if (cval.kernel == 1)
                    {
                        command.Parameters.Add(new SqlParameter("@SVMKernal", "LINEAR"));
                        command.Parameters.Add(new SqlParameter("@G", DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Degree", DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Coef0", DBNull.Value));
                    }
                    else if (cval.kernel == 2)
                    {
                        command.Parameters.Add(new SqlParameter("@SVMKernal", "POLY"));
                        command.Parameters.Add(new SqlParameter("@G", cval.Gamma));
                        command.Parameters.Add(new SqlParameter("@Degree", cval.Degree));
                        command.Parameters.Add(new SqlParameter("@Coef0", cval.Coef0));
                    }
                    else if (cval.kernel == 3)
                    {
                        command.Parameters.Add(new SqlParameter("@SVMKernal", "RBF"));
                        command.Parameters.Add(new SqlParameter("@G", cval.Gamma));
                        command.Parameters.Add(new SqlParameter("@Degree", DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Coef0", DBNull.Value));
                    }
                    
                    command.Parameters.Add(new SqlParameter("@Accuracy", cval.accuracy));
                    command.Parameters.Add(new SqlParameter("@MinAccuracy", cval.minAccuracy));
                    command.Parameters.Add(new SqlParameter("@MaxAccuracy", cval.maxAccuracy));
                    command.Parameters.Add(new SqlParameter("@TrainTimeTaken", cval.traintimetaken));
                    command.Parameters.Add(new SqlParameter("@TestTimeTaken", cval.testtimetaken));
                    command.Parameters.Add(new SqlParameter("@CorrectPositive", cval.correctPositive));
                    command.Parameters.Add(new SqlParameter("@CorrectNegative", cval.correctNegative));
                    command.Parameters.Add(new SqlParameter("@WrongPositive", cval.wrongPositive));
                    command.Parameters.Add(new SqlParameter("@WrongNegative", cval.wrongNegative));
                    conn.Open();

                    Int32 rdr = command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        protected Boolean ResultsExist(Int32 TrainingSetSize, Int32 NGramLevel, Int32 NGramPercent, Int32 knnLevel, CrossValidator cval)
        {
            try
            {
                using (var conn = new SqlConnection(SqlSecurity.SqlConnectionString))
                using (var command = new SqlCommand("ResultExists", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@TrainingSetSize", TrainingSetSize));
                    command.Parameters.Add(new SqlParameter("@NGramLevel", NGramLevel));
                    command.Parameters.Add(new SqlParameter("@NGramPercent", NGramPercent));
                    command.Parameters.Add(new SqlParameter("@knnLevel", knnLevel));
                    if (cval.type == 1)
                    {
                        command.Parameters.Add(new SqlParameter("@SVMType", "C_SVC"));
                        command.Parameters.Add(new SqlParameter("@C", cval.C));
                        command.Parameters.Add(new SqlParameter("@Nu", DBNull.Value));
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@SVMType", "NU_SVC"));
                        command.Parameters.Add(new SqlParameter("@C", DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Nu", cval.Nu));
                    }


                    if (cval.kernel == 1)
                    {
                        command.Parameters.Add(new SqlParameter("@SVMKernal", "LINEAR"));
                        command.Parameters.Add(new SqlParameter("@G", DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Degree", DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Coef0", DBNull.Value));
                    }
                    else if (cval.kernel == 2)
                    {
                        command.Parameters.Add(new SqlParameter("@SVMKernal", "POLY"));
                        command.Parameters.Add(new SqlParameter("@G", cval.Gamma));
                        command.Parameters.Add(new SqlParameter("@Degree", cval.Degree));
                        command.Parameters.Add(new SqlParameter("@Coef0", cval.Coef0));
                    }
                    else if (cval.kernel == 3)
                    {
                        command.Parameters.Add(new SqlParameter("@SVMKernal", "RBF"));
                        command.Parameters.Add(new SqlParameter("@G", cval.Gamma));
                        command.Parameters.Add(new SqlParameter("@Degree", DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Coef0", DBNull.Value));
                    }
                    conn.Open();

                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                Decimal line = Convert.ToDecimal(rdr[0]);

                                cval.accuracy = line;
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        protected Int32 NGramLevel = 1;
        protected Int32 NGramPercent = 100;
        protected Int32 RecordCount = 0;
        protected Int32 knnLevel = 0;
        protected Int32 ThreadCounter = 0;

        protected void OptimizeModel()
        {
            SVMProblem training = new SVMProblem();
            List<SVMProblem> trainings;
            SVMProblem testing = new SVMProblem();
            List<String> initialDataList;
            List<String> initialTestingList = new List<String>();
            List<String> knnList = new List<String>();

            SVMParameter parameter = new SVMParameter();

            List<CrossValidator> cvals = new List<CrossValidator>();
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";

            Console.WriteLine("Start");
            for (Int32 wrap = 1; wrap <= 10; wrap++)
            {
                Int32 NumberOfComments = 175;
                //for (Int32 NumberOfComments = 25; NumberOfComments <= 250; NumberOfComments += 25)
                {
                    NGramLevel = 3;
                    //for (NGramLevel = 1; NGramLevel <= 10; NGramLevel++)
                    {
                        NGramPercent = 7;
                        //for (NGramPercent = 1; NGramPercent <= 10; NGramPercent = NGramPercent + 1)
                        {
                            initialDataList = new List<String>();
                            initialTestingList = new List<String>();
                            try
                            {
                                Console.WriteLine("Getting Training Set NGramLevel: {0} NGramPercent: {1} NumberOfComments: {2}", NGramLevel.ToString(), NGramPercent.ToString(), NumberOfComments.ToString());
                                using (var conn = new SqlConnection(SqlSecurity.SqlConnectionString))
                                using (var command = new SqlCommand("GetTrainingSet", conn) { CommandType = CommandType.StoredProcedure })
                                {
                                    command.Parameters.Add(new SqlParameter("@MaxNGramLevel", NGramLevel));
                                    command.Parameters.Add(new SqlParameter("@NGramPercent", NGramPercent));
                                    command.Parameters.Add(new SqlParameter("@NumberOfComments", NumberOfComments));
                                    command.CommandTimeout = 0;
                                    conn.Open();

                                    using (SqlDataReader rdr = command.ExecuteReader())
                                    {
                                        while (rdr.Read())
                                        {
                                            initialDataList.Add(rdr["Data"].ToString());
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed with error {0}", e.Message);
                                InTraining = false;
                                return;
                            }

                            if (initialDataList[0] == "")
                            {
                                Console.WriteLine("No Data");
                                InTraining = false;
                                return;
                            }

                            Console.WriteLine("Getting Testing Set");
                            try
                            {
                                using (var conn = new SqlConnection(SqlSecurity.SqlConnectionString))
                                using (var command = new SqlCommand("GetTestingSet", conn) { CommandType = CommandType.StoredProcedure })
                                {
                                    command.CommandTimeout = 0;
                                    conn.Open();

                                    using (SqlDataReader rdr = command.ExecuteReader())
                                    {
                                        while (rdr.Read())
                                        {
                                            initialTestingList.Add(rdr["Data"].ToString());
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed with error {0}", e.Message);
                                InTraining = false;
                                return;
                            }

                            if (initialTestingList.Count == 0 || initialTestingList[0] == "")
                            {
                                Console.WriteLine("No Tests");
                                InTraining = false;
                                return;
                            }

                            RecordCount = NumberOfComments * 2;

                            Decimal knnPercent = 0.04M;
                            //for (Decimal knnPercent = 0.01M; knnPercent <= 0.1M; knnPercent = knnPercent + 0.01M)
                            {
                                training = new SVMProblem();
                                trainings = new List<SVMProblem>();
                                testing = new SVMProblem();
                                cvals = new List<CrossValidator>();
                                knnLevel = (Int32)Math.Round(RecordCount * knnPercent);

                                Console.WriteLine("Creating knn Set knnLevel: {0}", knnLevel.ToString());
                                knnList = knnBuilder(initialDataList, knnLevel, false);

                                Double[][] test = new Double[knnList.Count][];
                                Int32 indexer = 0;

                                foreach (String row in knnList)
                                {
                                    String[] list = row.Trim().Split(' ');

                                    Double y = Convert.ToDouble(list[0].Trim(), provider);

                                    List<SVMNode> nodes = new List<SVMNode>();
                                    List<Double> Estimater = new List<Double>();
                                    for (int i = 1; i < list.Length; i++)
                                    {
                                        string[] temp = list[i].Split(':');
                                        SVMNode node = new SVMNode();
                                        node.Index = Convert.ToInt32(temp[0].Trim());
                                        node.Value = Convert.ToDouble(temp[1].Trim(), provider);
                                        nodes.Add(node);
                                        Estimater.Add(Convert.ToDouble(temp[1].Trim(), provider));
                                    }

                                    training.Add(nodes.ToArray(), y);
                                    test[indexer] = Estimater.ToArray();
                                    indexer++;
                                }

                                foreach (String row in initialTestingList)
                                {
                                    String[] list = row.Trim().Split(' ');

                                    Double y = Convert.ToDouble(list[0].Trim(), provider);

                                    List<SVMNode> nodes = new List<SVMNode>();
                                    for (int i = 1; i < list.Length; i++)
                                    {
                                        string[] temp = list[i].Split(':');
                                        SVMNode node = new SVMNode();
                                        node.Index = Convert.ToInt32(temp[0].Trim());
                                        node.Value = Convert.ToDouble(temp[1].Trim(), provider);
                                        nodes.Add(node);
                                    }

                                    testing.Add(nodes.ToArray(), y);
                                }

                                Gaussian sigmaEstimate = Gaussian.Estimate(test);

                                Console.WriteLine("Getting cvals");
                                Int32 kernel = 2;
                                //for (Int32 kernel = 1; kernel <= 3; kernel++)
                                {
                                    if (kernel == 1)
                                        parameter.Kernel = SVMKernelType.LINEAR;
                                    else if (kernel == 2)
                                        parameter.Kernel = SVMKernelType.POLY;
                                    else if (kernel == 3)
                                        parameter.Kernel = SVMKernelType.RBF;

                                    Int32 type = 1;
                                    //for (Int32 type = 1; type <= 2; type++)
                                    {
                                        if (type == 1)
                                            parameter.Type = SVMType.C_SVC;
                                        else
                                            parameter.Type = SVMType.NU_SVC;

                                        Int32 CParam = 1;
                                        //for (Int32 CParam = 0; CParam <= 7; CParam++)
                                        {
                                            Decimal NuParam = 999M;
                                            //for (Decimal NuParam = 0.1M; NuParam <= 0.7M; NuParam += 0.1M)
                                            {
                                                Int32 DegreeParam = 2;
                                                //for (Int32 DegreeParam = 1; DegreeParam <= 5; DegreeParam += 1)
                                                {
                                                    Int32 CoefParam = 1;
                                                    //for (Int32 CoefParam = 0; CoefParam <= 1; CoefParam++)
                                                    {
                                                        if (type == 1)
                                                            NuParam = 999;
                                                        else
                                                            CParam = 999;

                                                        if (kernel == 1)
                                                        {
                                                            DegreeParam = 999;
                                                            CoefParam = 999;
                                                        }
                                                        else if (kernel == 3)
                                                        {
                                                            DegreeParam = 999;
                                                            CoefParam = 999;
                                                        }

                                                        CrossValidator cval = new CrossValidator();
                                                        cval.type = type;
                                                        cval.kernel = kernel;
                                                        cval.C = CParam;
                                                        cval.Gamma = Convert.ToDecimal(sigmaEstimate.Gamma); ;
                                                        cval.Nu = NuParam;
                                                        cval.Degree = DegreeParam;
                                                        cval.Coef0 = CoefParam;

                                                        cvals.Add(cval);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                Console.WriteLine("Creating threads");
                                foreach (CrossValidator cval in cvals)
                                {
                                    while (ThreadCounter >= Environment.ProcessorCount - 1)
                                    {
                                        //Thread.Sleep(1000);
                                        waiter.Reset();
                                        waiter.Wait();
                                    }

                                    if (!ResultsExist(RecordCount, NGramLevel, NGramPercent, knnLevel, cval))
                                    {
                                        Console.WriteLine("Creating thread: Count: " + RecordCount.ToString() + " NGramLevel: " + NGramLevel.ToString() + " NGramPercent: " + NGramPercent.ToString() + " knnLevel: " + knnLevel.ToString());
                                        Thread newThread = new Thread(delegate () { ThreadSafeCrossValidation(training, cval, knnList, testing); });
                                        newThread.IsBackground = true;
                                        ThreadCounter++;
                                        newThread.Start();
                                    }
                                }

                                Console.WriteLine("All threads created");

                                while (ThreadCounter > 0)
                                {
                                    //Thread.Sleep(1000);
                                    waiter.Reset();
                                    waiter.Wait();
                                }
                            }
                        }
                    }
                }
            }

            Environment.Exit(0);
        }

        protected void TrainModel()
        {
            eventLog1.WriteEntry("Training the model");
            InTraining = true;
            //OptimizeModel();
            SVMProblem training = new SVMProblem();
            List<String> initialDataList;
            List<String> knnList = new List<String>();

            SVMParameter parameter = new SVMParameter();
            
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";

            Int32 NumberOfComments = 175;
            NGramLevel = 3;
            NGramPercent = 7;
            initialDataList = new List<String>();
            try
            {
                Console.WriteLine("Getting Training Set NGramLevel: {0} NGramPercent: {1} NumberOfComments: {2}", NGramLevel.ToString(), NGramPercent.ToString(), NumberOfComments.ToString());
                using (var conn = new SqlConnection(SqlSecurity.SqlConnectionString))
                using (var command = new SqlCommand("GetTrainingSet", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@MaxNGramLevel", NGramLevel));
                    command.Parameters.Add(new SqlParameter("@NGramPercent", NGramPercent));
                    command.Parameters.Add(new SqlParameter("@NumberOfComments", NumberOfComments));
                    command.CommandTimeout = 0;
                    conn.Open();

                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            initialDataList.Add(rdr["Data"].ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed with error {0}", e.Message);
                InTraining = false;
                return;
            }

            if (initialDataList[0] == "")
            {
                Console.WriteLine("No Data");
                InTraining = false;
                return;
            }

            RecordCount = NumberOfComments * 2;

            Decimal knnPercent = 0.04M;
            training = new SVMProblem();
            knnLevel = (Int32)Math.Round(RecordCount * knnPercent);

            Console.WriteLine("Creating knn Set knnLevel: {0}", knnLevel.ToString());
            knnList = knnBuilder(initialDataList, knnLevel, false);

            Double[][] train = new Double[knnList.Count][];
            Int32 indexer = 0;

            foreach (String row in knnList)
            {
                String[] list = row.Trim().Split(' ');

                Double y = Convert.ToDouble(list[0].Trim(), provider);

                List<SVMNode> nodes = new List<SVMNode>();
                List<Double> Estimater = new List<Double>();
                for (int i = 1; i < list.Length; i++)
                {
                    string[] temp = list[i].Split(':');
                    SVMNode node = new SVMNode();
                    node.Index = Convert.ToInt32(temp[0].Trim());
                    node.Value = Convert.ToDouble(temp[1].Trim(), provider);
                    nodes.Add(node);
                    Estimater.Add(Convert.ToDouble(temp[1].Trim(), provider));
                }

                training.Add(nodes.ToArray(), y);
                train[indexer] = Estimater.ToArray();
                indexer++;
            }

            Gaussian sigmaEstimate = Gaussian.Estimate(train);
            
            Int32 bestKernel = 2;
            Int32 bestType = 1;
            Int32 bestC = 1;
            Decimal bestNu = 999M;
            Int32 bestDegree = 2;
            Int32 bestCoef0 = 1;
            Decimal bestGamma = Convert.ToDecimal(sigmaEstimate.Gamma); ;

            parameter = new SVMParameter();

            if (bestType == 1)
            {
                parameter.Type = SVMType.C_SVC;
                parameter.C = Math.Pow(2, (Double)bestC);
            }
            else
            {
                parameter.Type = SVMType.NU_SVC;
                parameter.Nu = (Double)bestNu;
            }

            if (bestKernel == 1)
                parameter.Kernel = SVMKernelType.LINEAR;
            else if (bestKernel == 2)
            {
                parameter.Kernel = SVMKernelType.POLY;
                parameter.Gamma = Math.Pow(2, (Double)bestGamma);
                parameter.Degree = bestDegree;
                parameter.Coef0 = (Double)bestCoef0;
            }
            else if (bestKernel == 3)
            {
                parameter.Kernel = SVMKernelType.RBF;
                parameter.Gamma = Math.Pow(2, (Double)bestGamma);
            }

            //parameter.Probability = true;
            
            try
            {
                model = SVM.Train(training, parameter);
            }
            catch(Exception e)
            {
                eventLog1.WriteEntry("Model failed: " + e.ToString());
                InTraining = false;
                return;
            }

            Trained = true;
            eventLog1.WriteEntry("Model is trained");
        }

        static ManualResetEventSlim waiter = new ManualResetEventSlim(false);

        protected void ProcessLoop()
        {
            //eventLog1.WriteEntry("ProcessLoop");
            try
            {
                using (var conn = new SqlConnection(SqlSecurity.SqlConnectionString))
                using (var command = new SqlCommand("GetOpenAnalysis", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.CommandTimeout = 0;
                    conn.Open();

                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            //eventLog1.WriteEntry("CommentID " + rdr["CommentID"].ToString());

                            Int32 commentID = Convert.ToInt32(rdr["CommentID"].ToString());

                            while (ThreadCounter >= Environment.ProcessorCount - 1)
                            {
                                //Thread.Sleep(1000);
                                waiter.Reset();
                                waiter.Wait();
                            }

                            using (var conn1 = new SqlConnection(SqlSecurity.SqlConnectionString))
                            using (var command1 = new SqlCommand("LockAnalysis", conn1) { CommandType = CommandType.StoredProcedure })
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
                                        //.WriteEntry("CommentID " + rdr["CommentID"].ToString() + " Taken By " + rdr1["ProcessID"].ToString());
                                        if (rdr1["ProcessID"].ToString() == ProcessID)
                                        {
                                            Thread newThread = new Thread(delegate () { ThreadFunction(commentID); });
                                            newThread.Name = ProcessID;
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
            catch (Exception e)
            {
                eventLog1.WriteEntry("GetOpenAnalysis Failed with: " + e.ToString());
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

        protected void UnlockAnalysis(Int32 test)
        {
            using (var conn1 = new SqlConnection("Data Source=PHD-SERVER;Initial Catalog=CyberbullyDB;Integrated Security=False;User ID=sa;Password="))
            using (var command1 = new SqlCommand("UnlockAnalysis", conn1) { CommandType = CommandType.StoredProcedure })
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

            SVMProblem testing = new SVMProblem();
            Double result = -2;
            Double[] confidenceScores = new Double[model.ClassCount];

            try
            {
                using (var conn = new SqlConnection(SqlSecurity.SqlConnectionString))
                using (var command = new SqlCommand("GetCommentData", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@CommentID", test));
                    command.CommandTimeout = 0;
                    conn.Open();

                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            NumberFormatInfo provider = new NumberFormatInfo();
                            provider.NumberDecimalSeparator = ".";

                            String line = rdr["Data"].ToString();

                            String[] list = line.Trim().Split(' ');

                            Double y = Convert.ToDouble(list[0].Trim(), provider);

                            List<SVMNode> nodes = new List<SVMNode>();
                            for (int i = 1; i < list.Length; i++)
                            {
                                string[] temp = list[i].Split(':');
                                SVMNode node = new SVMNode();
                                node.Index = Convert.ToInt32(temp[0].Trim());
                                node.Value = Convert.ToDouble(temp[1].Trim(), provider);
                                nodes.Add(node);
                            }

                            testing.Add(nodes.ToArray(), y);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " GetCommentData Failed with: " + e.ToString());
                UnlockAnalysis(test);
                ThreadCounter--;
                waiter.Set();
                Thread.CurrentThread.Abort();
            }

            //eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " Testing.");

            try
            { 
                result = SVM.Predict(model, testing.X[0]);
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Test failed: " + e.ToString());
                UnlockAnalysis(test);
                ThreadCounter--;
                waiter.Set();
                Thread.CurrentThread.Abort();
            }

            //eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " result: " + result.ToString() + ".");

            try
            {
                using (var conn = new SqlConnection(SqlSecurity.SqlConnectionString))
                using (var command = new SqlCommand("UpdateResult", conn) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@CommentID", test));
                    command.Parameters.Add(new SqlParameter("@Result", Convert.ToInt32(result)));
                    //command.Parameters.Add(new SqlParameter("@Probability", confidenceScores.Max()));
                    conn.Open();

                    Int32 rdr = command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                eventLog1.WriteEntry("Thread " + Thread.CurrentThread.Name + " UpdateResult Failed.");
                UnlockAnalysis(test);
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
