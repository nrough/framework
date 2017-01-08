using Infovision.Data;
using Infovision.MachineLearning.Discretization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Tests.Discretization
{
    public class DiscretizeUnsupervisedTest : DiscretizeBaseTest
    {        
        public void CreateDiscretizedDataTableTest()
        {
            int[] numericFields = new int[] { 2, 5, 8, 11, 13, 16, 18 };
            DataStore data = DataStore.Load(@"Data\german.data", FileFormat.Csv);
            foreach (int fieldId in numericFields)
            {
                data.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric = true;
            }
            DataStoreSplitter splitter = new DataStoreSplitter(data, 5);

            DataStore trainData = null, testData = null;
            splitter.Split(ref trainData, ref testData, 0);

            IDiscretizationUnsupervised discretizer = this.GetDiscretizer() as IDiscretizationUnsupervised;
            if (discretizer == null)
                throw new InvalidOperationException("discretizer == null");
            
            DataStoreDiscretizer descretizer = new DataStoreDiscretizer(discretizer);
            descretizer.Fields2Discretize = numericFields;
            descretizer.Discretize(trainData, trainData.Weights);
            DataStoreDiscretizer.Discretize(testData, trainData);

            foreach (int fieldId in numericFields)
            {
                Assert.IsNotNull(trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts, String.Format("Field {0}", fieldId));
                Assert.Greater(trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts.Length, 0, String.Format("Field {0}", fieldId));
            }
        }
        
        public void CalculateCutPointsByEqualFrequencyBinningTest()
        {
            IDiscretizationUnsupervised discretizer = this.GetDiscretizer() as IDiscretizationUnsupervised;
            if (discretizer == null)
                throw new InvalidOperationException("discretizer == null");

            discretizer.Compute(data, null, null);
            ShowInfo(discretizer, data, dataNotExisting);
        }

        public void ShowInfo(IDiscretization discretizer, long[] dataExisting, long[] dataNotExisting)            
        {
            Console.WriteLine(discretizer.ToString());

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", data[i], discretizer.Apply(dataExisting[i]));

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", dataNotExisting[i], discretizer.Apply(dataNotExisting[i]));
        }
    }
}
