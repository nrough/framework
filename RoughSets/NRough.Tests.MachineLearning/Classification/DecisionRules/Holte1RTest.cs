﻿using NRough.Data;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Classification.DecisionRules;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Classification.DecisionRules
{
    [TestFixture]
    public class Holte1RTest
    {        
        [Test, Repeat(1)]
        //[TestCase(@"Data\monks-1.train", @"Data\monks-1.test")]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test")]
        //[TestCase(@"Data\monks-3.train", @"Data\monks-3.test")]
        //[TestCase(@"Data\dna_modified.trn", @"Data\dna_modified.test")]
        //[TestCase(@"Data\spect.train", @"Data\spect.test")]
        //[TestCase(@"Data\dna.train", @"Data\dna.test")]
        public void ClassiferTest(string trainFile, string testFile)
        {            
            DataStore data = DataStore.Load(trainFile, DataFormat.RSES1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, DataFormat.RSES1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();
            //int[] attributes = new int[] { 3 };

            Holte1R oneR = new Holte1R();
            oneR.Learn(data, attributes);

            Console.WriteLine(Classifier.Default.Classify(oneR, test));

            foreach (var decisionList in oneR.GetRules())
                Console.WriteLine(decisionList.ToString(data.DataStoreInfo));
        }

        [Test, Repeat(1)]
        [TestCase(@"Data\playgolf2.train", @"Data\playgolf2.train")]
        public void DiscretizationTest(string trainFile, string testFile)
        {
            DataStore data = DataStore.Load(trainFile, DataFormat.RSES1);            
            DataStore test = DataStore.Load(testFile, DataFormat.RSES1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();

            Holte1R oneR = new Holte1R();
            oneR.Learn(data, attributes);

            Console.WriteLine(Classifier.Default.Classify(oneR, test));

            foreach (var decisionList in oneR.GetRules())
                Console.WriteLine(decisionList.ToString(data.DataStoreInfo));
        }

        [Test, Repeat(1)]
        public void DecisionTreeRough_GermanCredit()
        {            
            int numOfFolds = 10;
            DataStore data = DataStore.Load(@"Data\german.data", DataFormat.CSV);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldGroup.Standard).ToArray();
            DataStore train = null, test = null;
            
            DataSplitter splitter = new DataSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {
                splitter.Split(out train, out test, f);

                Holte1R oneR = new Holte1R();                
                oneR.Learn(train, attributes);                
                Console.WriteLine(Classifier.Default.Classify(oneR, test));
                
                foreach (var decisionList in oneR.GetRules())
                    Console.WriteLine(decisionList.ToString(data.DataStoreInfo));
            }
        }
    }


}