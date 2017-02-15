using Raccoon.Data;
using Raccoon.MachineLearning.Discretization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.MachineLearning.Tests.Discretization
{
    public abstract class DiscretizeUnsupervisedTest : DiscretizeBaseTest
    {
        [TestCase(@"Data\german.data", FileFormat.Csv, null)]
        public void CreateDiscretizedDataTableTest(string filename, FileFormat fileFormat, int[] fields)
        {            
            DataStore data = DataStore.Load(filename, fileFormat);            
            DataSplitter splitter = new DataSplitter(data, 5);

            DataStore trainData = null, testData = null;
            splitter.Split(out trainData, out testData, 0);

            IDiscretizerUnsupervised discretizer = this.GetDiscretizer() as IDiscretizerUnsupervised;
            if (discretizer == null)
                throw new InvalidOperationException("discretizer == null");
            
            DataStoreDiscretizer dataDescretizer = new DataStoreDiscretizer(discretizer);
            dataDescretizer.Fields2Discretize = fields;
            dataDescretizer.Discretize(trainData, trainData.Weights);
            DataStoreDiscretizer.Discretize(testData, trainData);

            foreach (int fieldId in fields)
            {
                Assert.IsNotNull(trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts, String.Format("Field {0}", fieldId));
                Assert.Greater(trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts.Length, 0, String.Format("Field {0}", fieldId));
            }
        }

        [Test]
        public void ComputeTest()
        {
            IDiscretizerUnsupervised discretizer = this.GetDiscretizer() as IDiscretizerUnsupervised;
            if (discretizer == null)
                throw new InvalidOperationException();
            discretizer.Compute(data, null, null);
            Assert.IsNotNull(discretizer.Cuts);
        }        
    }
}
