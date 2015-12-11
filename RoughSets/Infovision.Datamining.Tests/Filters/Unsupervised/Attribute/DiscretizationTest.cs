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
        
        [Test, Ignore]
        public void CreateDiscretizedDataTableTest()
        {
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            DataStoreSplitter splitter = new DataStoreSplitter(data, 5);

            DataStore trainData = null, testData = null;
            splitter.Split(ref trainData, ref testData);

            int[] numericFields = new int[] { 2, 5, 8, 11, 13, 16, 18 };

            for (int i = 0; i < numericFields.Length; i++ )
                Console.WriteLine(trainData.DataStoreInfo.GetFieldInfo(numericFields[i]).FieldValueType.ToString());
        }
        
        
        
        [Test]
        public void CalculateCutPointsByEqualFrequencyBinningTest()
        {
            int[] data = { 5, 5, 5, 5, 5, 7, 8, 9, 
                           10, 12, 13, 14, 16, 40, 41, 42, 
                           43, 44, 45, 45, 46, 47, 48, 49 };
            
            Discretization<int> disc = new Discretization<int>();
            disc.UseEqualFrequency = true;
            disc.NumberOfBuckets = (int) System.Math.Sqrt(data.Length);
            disc.Compute(data);

            Console.WriteLine(disc.ToString());
            
            //for (int i = 0; i < data.Length; i++)
            //    Console.WriteLine("{0} {1}", data[i], disc.Search(data[i]));

            int[] dataNotExisting = { 0, 1, 2, 3, 4, 5, 6, 
                                      11, 15, 16, 17, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34, 
                                      50, 60, 70, 80 };

            //for (int i = 0; i < data.Length; i++)
            //    Console.WriteLine("{0} {1}", dataNotExisting[i], disc.Search(dataNotExisting[i]));
        }

        [Test]
        public void CalculateCutPointsByEqualWidthBinningTest()
        {
            int[] data = { 5, 5, 5, 5, 5, 7, 8, 9, 
                           10, 12, 13, 14, 16, 40, 41, 42, 
                           43, 44, 45, 45, 46, 47, 48, 49 };

            Discretization<int> disc = new Discretization<int>();
            disc.NumberOfBuckets = 3;
            disc.Compute(data);

            Console.WriteLine(disc.ToString());

            //for (int i = 0; i < data.Length; i++)
            //    Console.WriteLine("{0} {1}", data[i], disc.Search(data[i]));

            int[] dataNotExisting = { 0, 1, 2, 3, 4, 5, 6, 
                                      11, 15, 16, 17, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34, 
                                      50, 60, 70, 80 };

            //for (int i = 0; i < data.Length; i++)
            //    Console.WriteLine("{0} {1}", dataNotExisting[i], disc.Search(dataNotExisting[i]));
        }

        [Test]
        public void CalculateCutPointsByEntropyTest()
        {
            int[] data = { 5, 5, 5, 5, 5, 7, 8, 9, 
                           10, 12, 13, 14, 16, 40, 41, 42, 
                           43, 44, 45, 45, 46, 47, 48, 49 };

            Discretization<int> disc = new Discretization<int>();
            //disc.NumberOfBuckets = 3;
            disc.Compute(data);

            Console.WriteLine(disc.ToString());

            //for (int i = 0; i < data.Length; i++)
            //    Console.WriteLine("{0} {1}", data[i], disc.Search(data[i]));

            int[] dataNotExisting = { 0, 1, 2, 3, 4, 5, 6, 
                                      11, 15, 16, 17, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34, 
                                      50, 60, 70, 80 };

            //for (int i = 0; i < data.Length; i++)
            //    Console.WriteLine("{0} {1}", dataNotExisting[i], disc.Search(dataNotExisting[i]));
        }
    }
}
