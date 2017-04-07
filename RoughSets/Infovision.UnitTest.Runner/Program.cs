using NRough.Data;
using NRough.MachineLearning.Classification.DecisionTrees.Pruning;
using NRough.Tests.MachineLearning.Classification.DecisionTrees;
using NRough.Core;
using NRough.Core.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NRough.MachineLearning;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.RCode;
using System.Diagnostics;

namespace NRough.UnitTest.Runner
{   
    public class Program
    {
        public static void Main(string[] args)
        {
            
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

            //ClassificationResult.OutputColumns = @"ds;model;eps;acc;attr;numrul;dthm;dtha";
            ClassificationResult.OutputColumns = @"ds;model;eps;acc;recallmacro;precisionmacro;attr;numrul;dthm;dtha";

            int numOfTests = 20;
            if(args.Length == 1)
                numOfTests = Int32.Parse(args[0]);

            List<string> fileNames = new List<string>(new string[] {
            //    @"mylogfile_20170311102703.txt"
            });

            fileNames.AddRange(Test_Benchmark2(numOfTests, false));
            fileNames.AddRange(Test_CV2(numOfTests, false));                        
                        
            //ProcessResultFiles(fileNames);
        }

        public static void Test_Benchmark(int tests)
        {
            DecisionTreeReductCompare test = new DecisionTreeReductCompare();
            string trainFile, testFile, reductFactoryKey;
            DataFormat fileFormat;

            MethodBase method = typeof(DecisionTreeReductCompare).GetMethod("ErrorImpurityTest");
            object[] testCases = method.GetCustomAttributes(typeof(TestCaseAttribute), true);

            using (var cc = new ConsoleCopy("mylogfile_"+DateTime.Now.ToString("yyyyMMddHHmmss") +".txt"))
            {                
                Console.WriteLine(ClassificationResult.TableHeader());

                for (int i = 0; i < tests; i++)
                {
                    foreach (var testCase in testCases)
                    {
                        trainFile = (string)((TestCaseAttribute)testCase).Arguments[0];
                        testFile = (string)((TestCaseAttribute)testCase).Arguments[1];
                        fileFormat = (DataFormat)((TestCaseAttribute)testCase).Arguments[2];                        
                        reductFactoryKey = (string)((TestCaseAttribute)testCase).Arguments[3];
                        
                        test.ErrorImpurityTest(trainFile, testFile, fileFormat, reductFactoryKey);
                    }
                }
            }
        }        

        public static void Test_CV(int tests)
        {
            DecisionTreeReductCompare test = new DecisionTreeReductCompare();
            string dataFile, reductFactoryKey;
            DataFormat fileFormat;
            int folds;

            MethodBase method = typeof(DecisionTreeReductCompare).GetMethod("ErrorImpurityTest_CV");
            object[] testCases = method.GetCustomAttributes(typeof(TestCaseAttribute), true);

            using (var cc = new ConsoleCopy("mylogfile_CV_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt"))
            {                
                Console.WriteLine(ClassificationResult.TableHeader());
                for (int i = 0; i < tests; i++)
                {
                    foreach (var testCase in testCases)
                    {
                        dataFile = (string)((TestCaseAttribute)testCase).Arguments[0];
                        fileFormat = (DataFormat)((TestCaseAttribute)testCase).Arguments[1];                        
                        reductFactoryKey = (string)((TestCaseAttribute)testCase).Arguments[2];
                        folds = (int)((TestCaseAttribute)testCase).Arguments[3];

                        test.ErrorImpurityTest_CV(dataFile, fileFormat, reductFactoryKey, folds);
                    }
                }
            }
        }

        public static IEnumerable<string> Test_Benchmark2(int tests, bool processResultFile)
        {
            List<string> resultFiles = new List<string>();

            RoughDecisionTreeTest test = new RoughDecisionTreeTest();
            MethodBase method = typeof(RoughDecisionTreeTest).GetMethod("DecisionTreeBenchmarkSplittedData");
            object[] testCases = method.GetCustomAttributes(typeof(TestCaseAttribute), true);
            foreach (var testCase in testCases)
            {
                string fileName = "mylogfile_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt";
                using (var cc = new ConsoleCopy(fileName))
                {
                    Console.WriteLine(ClassificationResult.TableHeader());
                    for (int i = 0; i < tests; i++)
                    {
                        var trainFile = (string)((TestCaseAttribute)testCase).Arguments[0];
                        var testFile = (string)((TestCaseAttribute)testCase).Arguments[1];
                        var fileFormat = (DataFormat)((TestCaseAttribute)testCase).Arguments[2];
                        var reductFactoryKey = (string)((TestCaseAttribute)testCase).Arguments[3];

                        test.DecisionTreeBenchmarkSplittedData(trainFile, testFile, fileFormat, reductFactoryKey);
                    }
                    resultFiles.Add(fileName);
                }

                if (processResultFile)
                    ProcessResultFiles(new string[] { fileName });
            }

            return resultFiles;
        }

        public static IEnumerable<string> Test_CV2(int tests, bool processResultFile)
        {
            List<string> resultFiles = new List<string>();
            RoughDecisionTreeTest test = new RoughDecisionTreeTest();            
            MethodBase method = typeof(RoughDecisionTreeTest).GetMethod("DecisionTreeWithCV");
            object[] testCases = method.GetCustomAttributes(typeof(TestCaseAttribute), true);                            
            foreach (var testCase in testCases)
            {                
                string fileName = "mylogfile_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt";
                using (var cc = new ConsoleCopy(fileName))
                {
                    Console.WriteLine(ClassificationResult.TableHeader());
                    for (int i = 0; i < tests; i++)
                    {
                        var dataFile = (string)((TestCaseAttribute)testCase).Arguments[0];
                        var fileFormat = (DataFormat)((TestCaseAttribute)testCase).Arguments[1];
                        var reductFactoryKey = (string)((TestCaseAttribute)testCase).Arguments[2];
                        var folds = (int)((TestCaseAttribute)testCase).Arguments[3];

                        test.DecisionTreeWithCV(dataFile, fileFormat, reductFactoryKey, folds);
                    }
                    resultFiles.Add(fileName);                    
                }

                if (processResultFile)
                    ProcessResultFiles(new string[] { fileName });
            }
            
            return resultFiles;
        }

        public static IEnumerable<string> Test_Benchmark3(int tests, bool processResultFile)
        {
            List<string> resultFiles = new List<string>();

            RoughDecisionTreeTest test = new RoughDecisionTreeTest();
            MethodBase method = typeof(RoughDecisionTreeTest).GetMethod("DecisionTreeBenchmarkSplittedData");
            object[] testCases = method.GetCustomAttributes(typeof(TestCaseAttribute), true);
            foreach (var testCase in testCases)
            {
                string fileName = "mylogfile_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt";
                using (var cc = new ConsoleCopy(fileName))
                {
                    Console.WriteLine(ClassificationResult.TableHeader());
                    for (int i = 0; i < tests; i++)
                    {
                        var trainFile = (string)((TestCaseAttribute)testCase).Arguments[0];
                        var testFile = (string)((TestCaseAttribute)testCase).Arguments[1];
                        var fileFormat = (DataFormat)((TestCaseAttribute)testCase).Arguments[2];
                        var reductFactoryKey = (string)((TestCaseAttribute)testCase).Arguments[3];

                        test.DecisionTreeBenchmarkSplittedData(trainFile, testFile, fileFormat, reductFactoryKey);
                    }
                    resultFiles.Add(fileName);
                }

                if (processResultFile)
                    ProcessResultFiles(new string[] { fileName });
            }

            return resultFiles;
        }

        public static void ProcessResultFiles(IEnumerable<string> fileNames)
        {                       
            DataTable dtc = ClassificationResult.ReadResults(fileNames, '|');

            if (dtc.Columns.Contains("Column1"))
                dtc.Columns.Remove("Column1");            
                        /*
            dtc.Columns.Add("pruning", typeof(string));
            foreach (DataRow row in dtc.Rows)
            {
                string model = row.Field<string>("model");
                if (model.Substring(model.Length - 4, 4) == "NONE")
                {
                    row["pruning"] = "NONE";
                    row["model"] = model.Substring(0, model.Length - 5);
                }
                else if (model.Substring(model.Length - 3, 3) == "REP")
                {
                    row["pruning"] = "REP";
                    row["model"] = model.Substring(0, model.Length - 4);
                }
                else if (model.Substring(model.Length - 3, 3) == "EBP")
                {
                    row["pruning"] = "EBP";
                    row["model"] = model.Substring(0, model.Length - 4);
                }
            }
            */

                        dtc.Dumb(@"results_all.csv", ";", true);

            //dt = ClassificationResult.AverageResults(dt);
            //ClassificationResult.AverageResults(dtc).Dumb(@"results_avg.csv", ";", true);

            var dt_acc = ClassificationResult.AggregateResults(dtc, "acc").Dumb(@"acc.csv", ";", true);
            DataView view = new DataView(dt_acc);
            DataTable distinctValues = view.ToTable(true, "ds");
            foreach (DataRow row in distinctValues.Rows)
            {                
                view.RowFilter = String.Empty;
                view.RowFilter = String.Format("ds = '{0}'", row["ds"].ToString());
                DataTable dt_acc_ds = view.ToTable();

                RProxy.Pdf(String.Format(@"{0}_acc", row["ds"].ToString()));
                RProxy.PlotResultSimple(dt_acc_ds, "eps", "acc_avg", "model", "acc_max", "acc_min", "Accuracy: "+ row["ds"].ToString());
                RProxy.DevOff();
            }

            var dt_attr = ClassificationResult.AggregateResults(dtc, "attr").Dumb(@"attr.csv", ";", true);
            view = new DataView(dt_attr);
            distinctValues = view.ToTable(true, "ds");
            foreach (DataRow row in distinctValues.Rows)
            {            
                view.RowFilter = String.Empty;
                view.RowFilter = String.Format("ds = '{0}'", row["ds"].ToString());
                DataTable dt_attr_ds = view.ToTable();

                RProxy.Pdf(String.Format(@"{0}_attr", row["ds"].ToString()));
                RProxy.PlotResultSimple(dt_attr_ds, "eps", "attr_avg", "model", "attr_max", "attr_min", "#Distinct attributes: " + row["ds"].ToString());
                RProxy.DevOff();
            }

            var dt_numrul = ClassificationResult.AggregateResults(dtc, "numrul").Dumb(@"numrul.csv", ";", true);
            view = new DataView(dt_numrul);
            distinctValues = view.ToTable(true, "ds");
            foreach (DataRow row in distinctValues.Rows)
            {
                view.RowFilter = String.Empty;
                view.RowFilter = String.Format("ds = '{0}'", row["ds"].ToString());
                DataTable dt_attr_ds = view.ToTable();

                RProxy.Pdf(String.Format(@"{0}_numrul", row["ds"].ToString()));
                RProxy.PlotResultSimple(dt_attr_ds, "eps", "numrul_avg", "model", "numrul_max", "numrul_min", "#Rules: " + row["ds"].ToString());
                RProxy.DevOff();
            }

            var dt_dthm = ClassificationResult.AggregateResults(dtc, "dthm").Dumb(@"dthm.csv", ";", true);
            view = new DataView(dt_dthm);
            distinctValues = view.ToTable(true, "ds");
            foreach (DataRow row in distinctValues.Rows)
            {
                view.RowFilter = String.Empty;
                view.RowFilter = String.Format("ds = '{0}'", row["ds"].ToString());
                DataTable dt_attr_ds = view.ToTable();

                RProxy.Pdf(String.Format(@"{0}_dthm", row["ds"].ToString()));
                RProxy.PlotResultSimple(dt_attr_ds, "eps", "dthm_avg", "model", "dthm_max", "dthm_min", "Max rule length: " + row["ds"].ToString());
                RProxy.DevOff();
            }

            var dt_dtha = ClassificationResult.AggregateResults(dtc, "dtha").Dumb(@"dtha.csv", ";", true);
            view = new DataView(dt_dtha);
            distinctValues = view.ToTable(true, "ds");
            foreach (DataRow row in distinctValues.Rows)
            {
                view.RowFilter = String.Empty;
                view.RowFilter = String.Format("ds = '{0}'", row["ds"].ToString());
                DataTable dt_attr_ds = view.ToTable();

                RProxy.Pdf(String.Format(@"{0}_dtha", row["ds"].ToString()));
                RProxy.PlotResultSimple(dt_attr_ds, "eps", "dtha_avg", "model", "dtha_max", "dtha_min", "Avg rule length attributes: " + row["ds"].ToString());
                RProxy.DevOff();
            }

            //TODO Compare results of different models
            //TODO Calculate Kappa, Test significance, etc.
        }

    }
}
