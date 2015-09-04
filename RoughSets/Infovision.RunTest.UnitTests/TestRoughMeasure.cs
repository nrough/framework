using System;
using System.IO;
using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Test;
using NUnit.Framework;

namespace Infovision.RunTest.UnitTests
{
    [TestFixture]
    public class TestRoughMeasure
    {
        private string trainFilename;
        private string testFilename;
        private int numberOfReducts;
        private int numberOfPermutations;
        private int numberOfTests;
        private int numberOfFolds;
        private string resultFilename;
        private string autoSaveFilename;

        private Program program;
        private ParameterCollection parameterList;
        private RoughMeasureTest test;

        const int PARM_POS_TRAIN_NAME = 0;
        const int PARM_POS_TEST_NAME = 1;
        const int PARM_POS_NUMBER_OF_REDUCTS = 2;
        const int PARM_POS_NUMBER_OF_PERMUTATIONS = 3;
        const int PARM_POS_NUMBER_OF_FOLDS = 4;
        const int PARM_POS_FOLD_NUMBER = 5;
        const int PARM_POS_NUMBER_OF_TESTS = 6;
        const int PARM_POS_REDUCT_TYPE = 7;
        const int PARM_POS_APPROXIMATION_DEGREE = 8;
        const int PARM_POS_REDUCT_MEASURE = 9;
        const int PARM_POS_IDENTIFICATION_TYPE = 10;
        const int PARM_POS_VOTE_TYPE = 11;

        const int FILE_POS_TRAIN_NAME = 1;
        const int FILE_POS_TEST_NAME = 3;
        const int FILE_POS_NUMBER_OF_REDUCTS = 5;
        const int FILE_POS_NUMBER_OF_PERMUTATIONS = 6;
        const int FILE_POS_NUMBER_OF_FOLDS = 8;
        const int FILE_POS_FOLD_NUMBER = 9;
        const int FILE_POS_NUMBER_OF_TESTS = 10;
        const int FILE_POS_APPROXIMATION_DEGREE = 11;
        const int FILE_POS_REDUCT_TYPE = 12;
        const int FILE_POS_REDUCT_MEASURE = 7;
        const int FILE_POS_IDENTIFICATION_TYPE = 13;
        const int FILE_POS_VOTE_TYPE = 14;

        public TestRoughMeasure()
        {
            trainFilename = @"Data\playgolf.train";
            testFilename = @"Data\playgolf.train";
            numberOfReducts = 10;
            numberOfPermutations = 10;
            numberOfTests = 10;
            numberOfFolds = 5;
            resultFilename = Path.ChangeExtension(trainFilename, "result");
            autoSaveFilename = Path.ChangeExtension(trainFilename, "bin");

            program = new Program();
            parameterList = program.RelativeTestParmList(trainFilename,
                                                        testFilename,
                                                        numberOfReducts,
                                                        numberOfPermutations,
                                                        numberOfTests,
                                                        numberOfFolds);

            test = new RoughMeasureTest(parameterList);
        }

        [Test]
        public void TestEqualsOperator()
        {
            DataStore d1 = DataStore.Load(new DataReaderFileRses(trainFilename));
            DataStore d2 = DataStore.Load(new DataReaderFileRses(trainFilename));

            Assert.AreNotEqual(true, d1 == d2);
        }

        [Test]
        public void TestEqualsMethod()
        {
            DataStore d1 = DataStore.Load(new DataReaderFileRses(trainFilename));
            DataStore d2 = DataStore.Load(new DataReaderFileRses(trainFilename));

            Assert.AreEqual(false, d1.Equals(d2));
        }
       
        private void DeleteFiles()
        {
            try
            {
                File.Delete(resultFilename);
                File.Delete(autoSaveFilename);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        [Test]
        public void TestSerialization()
        {
            DeleteFiles();
            
            TestRunner testRunner = new TestRunner(resultFilename, test);
            testRunner.OpenLog(resultFilename);
            RunNTests(testRunner, 4);
            testRunner.SaveResults(autoSaveFilename);
            testRunner.CloseLog();

            RoughMeasureTest test2 = new RoughMeasureTest(parameterList);
            TestRunner testRunner2 = new TestRunner(resultFilename, null, autoSaveFilename);

            Assert.AreEqual(testRunner.LastSavedParameterVector, testRunner2.LastSavedParameterVector);

            RunNTests(testRunner2, 1);
            testRunner.CloseLog();

            Assert.AreNotEqual(testRunner.LastSavedParameterVector, testRunner2.CurrentParameterVector);
        }

        private string GetLastFileRecord(string fileName)
        {
            string lastLine = String.Empty;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string line = streamReader.ReadLine();
                    while (!String.IsNullOrEmpty(line))
                    {
                        lastLine = line;
                        line = streamReader.ReadLine();
                    }
                }
            }
            return lastLine;
        }

        private void CompareFileRecordWithParms(ParameterValueVector parameterVector, string [] fields)
        {
            Assert.AreEqual(parameterVector[PARM_POS_TRAIN_NAME].ToString(), fields[FILE_POS_TRAIN_NAME].ToString());
            Assert.AreEqual(parameterVector[PARM_POS_TEST_NAME].ToString(), fields[FILE_POS_TEST_NAME].ToString());
            Assert.AreEqual(parameterVector[PARM_POS_NUMBER_OF_REDUCTS].ToString(), fields[FILE_POS_NUMBER_OF_REDUCTS].ToString());
            Assert.AreEqual(parameterVector[PARM_POS_NUMBER_OF_PERMUTATIONS].ToString(), fields[FILE_POS_NUMBER_OF_PERMUTATIONS].ToString());
            Assert.AreEqual(parameterVector[PARM_POS_NUMBER_OF_FOLDS].ToString(), fields[FILE_POS_NUMBER_OF_FOLDS].ToString());
            Assert.AreEqual(parameterVector[PARM_POS_FOLD_NUMBER].ToString(), fields[FILE_POS_FOLD_NUMBER].ToString());
            Assert.AreEqual(parameterVector[PARM_POS_NUMBER_OF_TESTS].ToString(), fields[FILE_POS_NUMBER_OF_TESTS].ToString());

            Assert.AreEqual(parameterVector[PARM_POS_REDUCT_TYPE].ToString(), fields[FILE_POS_REDUCT_TYPE].ToString());
            Assert.AreEqual(parameterVector[PARM_POS_APPROXIMATION_DEGREE].ToString(), fields[FILE_POS_APPROXIMATION_DEGREE].ToString());
            Assert.AreEqual(parameterVector[PARM_POS_REDUCT_MEASURE].ToString(), fields[FILE_POS_REDUCT_MEASURE].ToString());
            Assert.AreEqual(parameterVector[PARM_POS_IDENTIFICATION_TYPE].ToString(), fields[FILE_POS_IDENTIFICATION_TYPE].ToString());
            Assert.AreEqual(parameterVector[PARM_POS_VOTE_TYPE].ToString(), fields[FILE_POS_VOTE_TYPE].ToString());
        }

        [Test]
        public void TestResultFileAfterSerialization()
        {
            DeleteFiles();

            TestRunner testRunner = new TestRunner(resultFilename, test);
            testRunner.OpenLog(resultFilename);
            RunNTests(testRunner, 1);
            testRunner.SaveResults(autoSaveFilename);
            testRunner.CloseLog();

            RunNTests(testRunner, 2);

            ParameterValueVector currentParamVector = testRunner.CurrentParameterVector;

            string lastLine = GetLastFileRecord(resultFilename);
            string[] fields = lastLine.Split(new Char[] { ' ', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            testRunner = new TestRunner(resultFilename, null, autoSaveFilename);
            testRunner.OpenLog(resultFilename);
            
            CompareFileRecordWithParms(testRunner.LastSavedParameterVector, fields);

            RunNTests(testRunner, 2);
            testRunner.CloseLog();

            Assert.AreEqual(currentParamVector, testRunner.CurrentParameterVector);
        }

        private void RunNTests(TestRunner testRunner, int n)
        {
            for (int i = 0; i < n; i++)
            {
                testRunner.RunSingleTest();
            }
        }

        [Test]
        public void TestReductStoreAfterSerialization()
        {
            DeleteFiles();
            TestRunner testRunner = new TestRunner(resultFilename, test);
            testRunner.OpenLog(resultFilename);
            RunNTests(testRunner, 3);
            testRunner.SaveResults(autoSaveFilename);
            testRunner.CloseLog();

            IReductStoreCollection reductStoreCollection = test.ReductStoreCollection;

            testRunner = new TestRunner(resultFilename, null, autoSaveFilename);

            RoughMeasureTest localTest = testRunner.TestObject as RoughMeasureTest;
            Assert.NotNull(localTest);
            IReductStoreCollection reductStoreCollection2 = localTest.ReductStoreCollection;
            Assert.AreEqual(reductStoreCollection.Count, reductStoreCollection2.Count);

            foreach(IReductStore rs in reductStoreCollection)
                foreach (IReduct r in rs)
                    Assert.AreEqual(true, rs.Exist(r));

            foreach (IReductStore rs in reductStoreCollection2)
                foreach (IReduct r in rs)
                    Assert.AreEqual(true, rs.Exist(r));
        }
    }
}
