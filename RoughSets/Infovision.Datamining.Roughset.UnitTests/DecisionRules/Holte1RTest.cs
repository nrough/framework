using Infovision.Data;
using Infovision.Datamining.Roughset.DecisionRules;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Roughset.UnitTests.DecisionRules
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
            DataStore data = DataStore.Load(trainFile, FileFormat.Rses1);
            foreach (var fieldInfo in data.DataStoreInfo.Fields) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, FileFormat.Rses1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            Holte1R holte1R = new Holte1R();
            holte1R.Learn(data, attributes);

            Console.WriteLine(Classifier.DefaultClassifer.Classify(holte1R, test));
        }

        [Test, Repeat(1)]
        [TestCase(@"Data\playgolf2.train", @"Data\playgolf2.train")]
        public void DiscretizationTest(string trainFile, string testFile)
        {
            DataStore data = DataStore.Load(trainFile, FileFormat.Rses1);            
            DataStore test = DataStore.Load(testFile, FileFormat.Rses1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();

            Holte1R holte1R = new Holte1R();
            holte1R.Learn(data, attributes);

            Console.WriteLine(Classifier.DefaultClassifer.Classify(holte1R, test));
        }

        [Test, Repeat(1)]
        public void DecisionTreeRough_GermanCredit()
        {            
            int numOfFolds = 5;
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            int[] attributes = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray();
            DataStore train = null, test = null;
            
            DataStoreSplitter splitter = new DataStoreSplitter(data, numOfFolds);
            for (int f = 0; f < numOfFolds; f++)
            {
                splitter.Split(ref train, ref test, f);

                Holte1R oneR = new Holte1R();                
                oneR.Learn(train, attributes);                
                Console.WriteLine(Classifier.DefaultClassifer.Classify(oneR, test));             
            }
        }
    }


}
