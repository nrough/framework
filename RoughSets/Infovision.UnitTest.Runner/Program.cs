using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Roughset.DecisionTrees.Pruning;
using Infovision.Datamining.Roughset.UnitTests.DecisionTrees;
using Infovision.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            Test_CV(5);
            Test_Benchmark(5);
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
                ClassificationResult.OutputColumns = @"ds;m;t;eps;ens;acc;attr;numrul;dthm;dtha";
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
                ClassificationResult.OutputColumns = @"ds;m;t;eps;ens;acc;attr;numrul;dthm;dtha";
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

        public void ProcessResultFile(string filename, string separator, string columns)
        {
            //TODO Average over numer of tests
            //TODO Calculate Mean, StdDev, Variance
            //TODO Compare results of different models
            //TODO Calculate Kappa, Test significance, etc.
        }

    }
}
