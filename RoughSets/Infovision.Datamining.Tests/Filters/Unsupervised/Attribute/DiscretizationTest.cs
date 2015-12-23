using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using NUnit.Framework;

namespace Infovision.Datamining.Filters.Unsupervised.Attribute.Tests
{
    [TestFixture]
    public class DiscretizationTest
    {
        static int[] data = { 5, 5, 5, 5, 5, 7, 8, 9, 
                           10, 12, 13, 14, 16, 40, 41, 42, 
                           43, 44, 45, 45, 46, 47, 48, 49 };

        static int[] dataNotExisting = { 0, 1, 2, 3, 4, 5, 6, 
                                      11, 15, 16, 17, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34, 
                                      50, 60, 70, 80 };
       

        [TestCase(true, false, false, 10)]
        [TestCase(false, true, false, 10)]
        [TestCase(false, false, true, 10)]
        public void CreateDiscretizedDataTableTest(bool usingEntropy, bool usingEqualFreq, bool usingEqualWidth, int numberOfBuckets)
        {
            int[] numericFields = new int[] { 2, 5, 8, 11, 13, 16, 18 };            
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            foreach (int fieldId in numericFields)
            {
                data.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric = true;
            }            
            DataStoreSplitter splitter = new DataStoreSplitter(data, 5);

            DataStore trainData = null, testData = null;
            splitter.ActiveFold = 0;
            splitter.Split(ref trainData, ref testData);

            Infovision.Datamining.Filters.Unsupervised.Attribute.DataStoreDiscretizer descretizer = new Infovision.Datamining.Filters.Unsupervised.Attribute.DataStoreDiscretizer()
            {
                NumberOfBins = numberOfBuckets,
                DiscretizeUsingEntropy = usingEntropy,
                DiscretizeUsingEqualFreq = usingEqualFreq,
                DiscretizeUsingEqualWidth = usingEqualWidth,
                Fields2Discretize = numericFields
            };
            
            descretizer.Discretize(ref trainData, ref testData);                                    

            foreach (int fieldId in numericFields)
            {
                Assert.IsNotNull(trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts, String.Format("Field {0}", fieldId));
                Assert.Greater(trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts.Length, 0, String.Format("Field {0}", fieldId));
            }
        }


        [TestCase(true, false,  5)]
        [TestCase(false, true,  3)]
        [TestCase(false, false, 10)]
        public void CalculateCutPointsByEqualFrequencyBinningTest(bool usingEntropy, bool usingEqualFreq, int numberOfBuckets)
        {
            Discretization<int> disc = new Discretization<int>()
            {
                NumberOfBuckets = numberOfBuckets,
                UseEntropy = usingEntropy,
                UseEqualFrequency = usingEqualFreq,
            };

            disc.Compute(data);

            ShowInfo<int>(disc, data, dataNotExisting);
        }

        public void ShowInfo<A>(Discretization<A> disc, A[] dataExisting, A[] dataNotExisting)
            where A : struct, IComparable, IFormattable, IComparable<A>, IEquatable<A>
        {
            Console.WriteLine(disc.ToString());

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", data[i], disc.Search(dataExisting[i]));

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", dataNotExisting[i], disc.Search(dataNotExisting[i]));
        }        
    }
}
