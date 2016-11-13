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
            DecisionTreeTest test = new DecisionTreeTest();
            string trainFile, testFile, reductFactoryKey;
            PruningType pruningType;

            MethodBase method = typeof(DecisionTreeTest).GetMethod("ErrorImpurityTest");
            object[] testCases = method.GetCustomAttributes(typeof(TestCaseAttribute), true);

            using (var cc = new ConsoleCopy("mylogfile.txt"))
            {

                ClassificationResult.OutputColumns = @"ds;m;t;f;eps;ens;acc;attr;numrul;dthm;dtha;gamma;alpha;beta;desc";
                Console.WriteLine(ClassificationResult.ResultHeader());

                for (int i = 0; i < 25; i++)
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
    }
}
