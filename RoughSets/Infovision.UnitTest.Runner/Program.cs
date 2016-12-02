using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Roughset.DecisionTrees.Pruning;
using Infovision.Datamining.Roughset.UnitTests.DecisionTrees;
using Infovision.Utils;
using Infovision.Utils.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.UnitTest.Runner
{   
    public class Program
    {
        public static void Main(string[] args)
        {
            //Test_Benchmark(4);
            //Test_CV(4);

            ProcessResultFiles();
        }

        public static void Test_Benchmark(int tests)
        {
            DecisionTreeReductCompare test = new DecisionTreeReductCompare();
            string trainFile, testFile, reductFactoryKey;
            PruningType pruningType;

            MethodBase method = typeof(DecisionTreeReductCompare).GetMethod("ErrorImpurityTest");
            object[] testCases = method.GetCustomAttributes(typeof(TestCaseAttribute), true);

            using (var cc = new ConsoleCopy("mylogfile_"+DateTime.Now.ToString("yyyyMMddHHmmss") +".txt"))
            {
                ClassificationResult.OutputColumns = @"ds;model;t;eps;ens;acc;attr;numrul;dthm;dtha";
                Console.WriteLine(ClassificationResult.ResultHeader());

                for (int i = 0; i < tests; i++)
                {
                    foreach (var testCase in testCases)
                    {
                        trainFile = (string)((TestCaseAttribute)testCase).Arguments[0];
                        testFile = (string)((TestCaseAttribute)testCase).Arguments[1];
                        pruningType = (PruningType)((TestCaseAttribute)testCase).Arguments[2];
                        reductFactoryKey = (string)((TestCaseAttribute)testCase).Arguments[3];

                        test.ErrorImpurityTest(trainFile, testFile, pruningType, reductFactoryKey);
                    }
                }
            }
        }

        public static void Test_CV(int tests)
        {
            DecisionTreeReductCompare test = new DecisionTreeReductCompare();
            string dataFile, reductFactoryKey;
            PruningType pruningType;
            FileFormat fileFormat;
            int folds;

            MethodBase method = typeof(DecisionTreeReductCompare).GetMethod("ErrorImpurityTest_CV");
            object[] testCases = method.GetCustomAttributes(typeof(TestCaseAttribute), true);

            using (var cc = new ConsoleCopy("mylogfile_CV_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt"))
            {
                ClassificationResult.OutputColumns = @"ds;model;t;eps;ens;acc;attr;numrul;dthm;dtha";
                Console.WriteLine(ClassificationResult.ResultHeader());

                for (int i = 0; i < tests; i++)
                {
                    foreach (var testCase in testCases)
                    {
                        dataFile = (string)((TestCaseAttribute)testCase).Arguments[0];
                        fileFormat = (FileFormat)((TestCaseAttribute)testCase).Arguments[1];
                        pruningType = (PruningType)((TestCaseAttribute)testCase).Arguments[2];
                        reductFactoryKey = (string)((TestCaseAttribute)testCase).Arguments[3];
                        folds = (int)((TestCaseAttribute)testCase).Arguments[4];

                        test.ErrorImpurityTest_CV(dataFile, fileFormat, pruningType, reductFactoryKey, folds);
                    }
                }
            }
        }

        public static void ProcessResultFiles()
        {
            List<string> fileNames = new List<string>(new string[] {
                @"d:\temp\1\mylogfile_CV_20161128064439.txt",
                @"d:\temp\1\mylogfile_CV_20161130122938.txt",
                @"d:\temp\1\mylogfile_20161130004421.txt",
                @"d:\temp\1\mylogfile_20161126193020.txt",
                @"d:\temp\1\mylogfile_CV_20161125183714.txt",                
                @"d:\temp\1\mylogfile_CV_20161122000641.txt",
                @"d:\temp\1\mylogfile_20161121203530.txt",
                @"d:\temp\1\mylogfile_CV_20161121152623.txt",
                @"d:\temp\1\mylogfile_20161121110634.txt",
                @"d:\temp\1\mylogfile_CV_20161121080710.txt",
                @"d:\temp\1\mylogfile_20161121001141.txt",
                @"d:\temp\1\mylogfile_CV_20161120211514.txt",
                @"d:\temp\1\mylogfile_20161120104404.txt",
                @"d:\temp\1\mylogfile_CV_20161120074024.txt",
                @"d:\temp\1\mylogfile_CV_20161119200826.txt",
                @"d:\temp\1\mylogfile_20161119231109.txt"
                });

            DataTable dtc = ClassificationResult.ReadResults(fileNames, '|');

            if (dtc.Columns.Contains("Column1"))
                dtc.Columns.Remove("Column1");

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
            }

            dtc.Dumb(@"D:\temp\1\results_all.csv", ";", true);

            //dt = ClassificationResult.AverageResults(dt);
            ClassificationResult.AggregateResults(dtc, "acc").Dumb(@"D:\temp\1\acc.csv", ";", true);
            ClassificationResult.AggregateResults(dtc, "attr").Dumb(@"D:\temp\1\attr.csv", ";", true);
            ClassificationResult.AggregateResults(dtc, "numrul").Dumb(@"D:\temp\1\numrul.csv", ";", true);
            ClassificationResult.AggregateResults(dtc, "dthm").Dumb(@"D:\temp\1\dthm.csv", ";", true); 
            ClassificationResult.AggregateResults(dtc, "dtha").Dumb(@"D:\temp\1\dtha.csv", ";", true);

            ClassificationResult.AverageResults(dtc).Dumb(@"D:\temp\1\results_avg.csv", ";", true);



            //ClassificationResult.PlotR(dt);


            //TODO Compare results of different models
            //TODO Calculate Kappa, Test significance, etc.
        }

    }
}
