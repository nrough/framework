﻿using NRough.Data;
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
using NRough.Datamining.RCode;

namespace NRough.UnitTest.Runner
{   
    public class Program
    {
        public static void Main(string[] args)
        {
            ClassificationResult.OutputColumns = @"ds;model;t;eps;ens;acc;attr;numrul;dthm;dtha;precisionmicro;precisionmacro;recallmicro;recallmacro;f1scoremicro;f1scoremacro";

            Test_CV(25);
            Test_Benchmark(25);
                                   
            //ProcessResultFiles();
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

        public static void ProcessResultFiles()
        {
            /*
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
                @"d:\temp\1\mylogfile_20161119231109.txt",
                @"d:\temp\1\mylogfile_CV_20161207185711.txt",   // vehicle
                @"d:\temp\1\mylogfile_CV_20161207213043.txt",   // vehicle
                @"d:\temp\1\mylogfile_CV_20161207231310.txt",   // german credit
                @"d:\temp\1\mylogfile_CV_20161208232856.txt",   // dermatology, dermatology_no_age, hypothyroid
                @"d:\temp\1\mylogfile_20161209105906.txt",      // vowel disc
                @"d:\temp\1\mylogfile_CV_20161209225000.txt"    // lymphography
                });
            */

            List<string> fileNames = new List<string>(new string[] {
                //@"d:\temp\1\mylogfile_CV_20161224012153.txt",
                //@"d:\temp\1\mylogfile_20161225080854.txt",
                @"d:\temp\1\mylogfile_CV_20161231160308.txt" //nurse
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
            //ClassificationResult.AverageResults(dtc).Dumb(@"D:\temp\1\results_avg.csv", ";", true);

            var dt_acc = ClassificationResult.AggregateResults(dtc, "acc").Dumb(@"D:\temp\1\acc.csv", ";", true);
            DataView view = new DataView(dt_acc);
            DataTable distinctValues = view.ToTable(true, "ds");
            foreach (DataRow row in distinctValues.Rows)
            {                
                view.RowFilter = String.Empty;
                view.RowFilter = String.Format("ds = '{0}'", row["ds"].ToString());
                DataTable dt_acc_ds = view.ToTable();

                RProxy.Pdf(String.Format(@"D:\\temp\\1\\{0}_acc", row["ds"].ToString()));
                RProxy.PlotResult(dt_acc_ds, "eps", "acc_avg", "model", "pruning", "acc_max", "acc_min", "Accuracy: "+ row["ds"].ToString());
                RProxy.DevOff();
            }

            var dt_attr = ClassificationResult.AggregateResults(dtc, "attr").Dumb(@"D:\temp\1\attr.csv", ";", true);
            view = new DataView(dt_attr);
            distinctValues = view.ToTable(true, "ds");
            foreach (DataRow row in distinctValues.Rows)
            {            
                view.RowFilter = String.Empty;
                view.RowFilter = String.Format("ds = '{0}'", row["ds"].ToString());
                DataTable dt_attr_ds = view.ToTable();

                RProxy.Pdf(String.Format(@"D:\\temp\\1\\{0}_attr", row["ds"].ToString()));
                RProxy.PlotResult(dt_attr_ds, "eps", "attr_avg", "model", "pruning", "attr_max", "attr_min", "#Distinct attributes: " + row["ds"].ToString());
                RProxy.DevOff();
            }

            var dt_numrul = ClassificationResult.AggregateResults(dtc, "numrul").Dumb(@"D:\temp\1\numrul.csv", ";", true);
            view = new DataView(dt_numrul);
            distinctValues = view.ToTable(true, "ds");
            foreach (DataRow row in distinctValues.Rows)
            {
                view.RowFilter = String.Empty;
                view.RowFilter = String.Format("ds = '{0}'", row["ds"].ToString());
                DataTable dt_attr_ds = view.ToTable();

                RProxy.Pdf(String.Format(@"D:\\temp\\1\\{0}_numrul", row["ds"].ToString()));
                RProxy.PlotResult(dt_attr_ds, "eps", "numrul_avg", "model", "pruning", "numrul_max", "numrul_min", "#Rules: " + row["ds"].ToString());
                RProxy.DevOff();
            }

            var dt_dthm = ClassificationResult.AggregateResults(dtc, "dthm").Dumb(@"D:\temp\1\dthm.csv", ";", true);
            view = new DataView(dt_dthm);
            distinctValues = view.ToTable(true, "ds");
            foreach (DataRow row in distinctValues.Rows)
            {
                view.RowFilter = String.Empty;
                view.RowFilter = String.Format("ds = '{0}'", row["ds"].ToString());
                DataTable dt_attr_ds = view.ToTable();

                RProxy.Pdf(String.Format(@"D:\\temp\\1\\{0}_dthm", row["ds"].ToString()));
                RProxy.PlotResult(dt_attr_ds, "eps", "dthm_avg", "model", "pruning", "dthm_max", "dthm_min", "Max rule length: " + row["ds"].ToString());
                RProxy.DevOff();
            }

            var dt_dtha = ClassificationResult.AggregateResults(dtc, "dtha").Dumb(@"D:\temp\1\dtha.csv", ";", true);
            view = new DataView(dt_dtha);
            distinctValues = view.ToTable(true, "ds");
            foreach (DataRow row in distinctValues.Rows)
            {
                view.RowFilter = String.Empty;
                view.RowFilter = String.Format("ds = '{0}'", row["ds"].ToString());
                DataTable dt_attr_ds = view.ToTable();

                RProxy.Pdf(String.Format(@"D:\\temp\\1\\{0}_dtha", row["ds"].ToString()));
                RProxy.PlotResult(dt_attr_ds, "eps", "dtha_avg", "model", "pruning", "dtha_max", "dtha_min", "Avg rule length attributes: " + row["ds"].ToString());
                RProxy.DevOff();
            }

            //TODO Compare results of different models
            //TODO Calculate Kappa, Test significance, etc.
        }

    }
}
