using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using NUnit.Framework;
using Infovision.Datamining.Filters.Supervised.Attribute;

namespace Infovision.Datamining.Tests.Filters.Supervised.Attribute
{
    
    [TestFixture]
    public class DiscretizationTest
    {
        int[] data = { 5, 5, 5, 5, 5, 7, 8, 9, 
                        10, 12, 13, 14, 16, 40, 41, 42, 
                        43, 44, 45, 45, 46, 47, 48, 49 };

        int[] labels = { 1, 1, 1, 1, 1, 1, 1, 1, 
                        1, 1, 1, 2, 2, 2, 2, 2, 
                        2, 2, 2, 2, 2, 3, 3, 3 };

        int[] dataNotExisting = { 0, 1, 2, 3, 4, 5, 6, 
                                    11, 15, 16, 17, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34, 
                                    50, 60, 70, 80 };

        //[TestCase(false, false)]
        //[TestCase(false, true)]
        //[TestCase(true, false)]
        [TestCase(true, true)]
        public void CreateDiscretizedDataTableTest(bool useBetterEncoding, bool useKononenko)
        {
            int[] numericFields = new int[] { 2, 5, 13 }; //8, 11, 16, 18
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            foreach (int fieldId in numericFields)
            {
                data.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric = true;
            }

            DataStoreSplitter splitter = new DataStoreSplitter(data, 5);

            DataStore trainData = null, testData = null;
            splitter.ActiveFold = 0;
            splitter.Split(ref trainData, ref testData);

            Infovision.Datamining.Filters.Supervised.Attribute.DataStoreDiscretizer descretizer = new Infovision.Datamining.Filters.Supervised.Attribute.DataStoreDiscretizer()
            {
                UseBetterEncoding = useBetterEncoding,
                UseKononenko = useKononenko,
                Fields2Discretize = numericFields
            };
            
            descretizer.Discretize(ref trainData, ref testData);

            foreach (int fieldId in numericFields)
            {
                if (trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts.Length == 0)
                    Console.WriteLine(String.Format("Field {0}", fieldId));                
            }
        }


        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void ComputeTest(bool useBetterEncoding, bool useKononenko)
        {
            Discretization<int, int> disc = new Discretization<int, int>()
            {
                UseBetterEncoding = useBetterEncoding,
                UseKononenko = useKononenko
            };

            disc.Compute(data, labels, null);

            ShowInfo<int, int>(disc, data, dataNotExisting);
        }        

        public void ShowInfo<A, L>(Discretization<A, L> disc, A[] dataExisting, A[] dataNotExisting)
            where A : struct, IComparable, IFormattable, IComparable<A>, IEquatable<A>
            where L : struct, IComparable, IFormattable, IComparable<L>, IEquatable<L>                     
        {
            Console.WriteLine(disc.ToString());

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", data[i], disc.Search(dataExisting[i]));

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", dataNotExisting[i], disc.Search(dataNotExisting[i]));
        }
    }   
}
