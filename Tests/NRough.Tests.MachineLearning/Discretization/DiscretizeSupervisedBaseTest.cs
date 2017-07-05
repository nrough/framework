// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using NRough.Core;
using NRough.Data;
using NRough.MachineLearning.Discretization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Discretization
{
    public class DiscretizeSupervisedBaseTest : DiscretizeBaseTest
    {        
        protected long[] labels = {
                        1, 1, 1, 2, 2, 2, 2, 2,
                        1, 1, 1, 1, 1, 1, 1, 1,
                        2, 2, 2, 2, 2, 3, 3, 3 };

        public override IDiscretizer GetDiscretizer()
        {
            DiscretizeSupervisedBase result = new DiscretizeSupervisedBase();
            result.NumberOfBuckets = 10;
            return result;
        }

        [TestCase(@"Data\sat.trn", @"Data\sat.tst", DataFormat.RSES1)]
        [TestCase(@"Data\pendigits.trn", @"Data\pendigits.tst", DataFormat.RSES1)]
        [TestCase(@"Data\optdigits.trn", @"Data\optdigits.tst", DataFormat.RSES1)]
        [TestCase(@"Data\letter.trn", @"Data\letter.tst", DataFormat.RSES1)]
        [TestCase(@"Data\vowel.trn", @"Data\vowel.tst", DataFormat.CSV)]
        public void DiscretizeData(string fileTrain, string fileTest, DataFormat fileFormat)
        {
            Console.WriteLine(fileTrain);

            DataStore train = DataStore.Load(fileTrain, fileFormat);
            DataStore test = DataStore.Load(fileTest, fileFormat, train.DataStoreInfo);

            train.Dump(@"C:\" + fileTrain + ".int", " ", false);
            test.Dump(@"C:\" + fileTest + ".int", " ", false);

            train.DumpExt(@"C:\" + fileTrain + ".ext", " ", false, true);
            test.DumpExt(@"C:\" + fileTest + ".ext", " ", false, true);

            var discretizer = new DecisionTableDiscretizer(this.GetDiscretizer());

            discretizer.FieldsToDiscretize
                = train.DataStoreInfo.SelectAttributeIds(a => a.IsStandard)
                .Where(fieldId => train.DataStoreInfo.GetFieldInfo(fieldId).IsNumeric);

            discretizer.Discretize(train, train.Weights);
            DecisionTableDiscretizer.Discretize(test, train);

            train.DumpExt(@"C:\" + fileTrain + ".disc", " ", false, true);
            test.DumpExt(@"C:\" + fileTest + ".disc", " ", false, true);
        }

        [TestCase(@"Data\letter.trn", @"Data\letter.tst", DataFormat.RSES1)]
        public void LetterBug(string fileTrain, string fileTest, DataFormat fileFormat)
        {
            Console.WriteLine(fileTrain);

            DataStore train = DataStore.Load(fileTrain, fileFormat);
            DataStore test = DataStore.Load(fileTest, fileFormat, train.DataStoreInfo);

            var discretizer = new DecisionTableDiscretizer(this.GetDiscretizer());

            discretizer.FieldsToDiscretize = new int[] { 2 };
            discretizer.Discretize(train, train.Weights);
        }

        [TestCase(@"Data\german.data", DataFormat.CSV, null)]        
        public virtual void DiscretizeTest(string filename, DataFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);                        
            IEnumerable<int> numericFields = fields == null ? data.DataStoreInfo
                .SelectAttributeIds(a => a.IsStandard && a.CanDiscretize()) : fields;

            var discretizer = new DecisionTableDiscretizer(this.GetDiscretizer());
            discretizer.FieldsToDiscretize = numericFields;
            discretizer.Discretize(data, data.Weights);
            
            foreach (var kvp in discretizer.FieldDiscretizer)
                Console.WriteLine("Field {0} Cuts {1}", kvp.Key, (kvp.Value == null || kvp.Value.Cuts == null) ? "All" : kvp.Value.ToString());

            foreach (int fieldId in numericFields)
                Assert.IsNotNull(data.DataStoreInfo.GetFieldInfo(fieldId).Cuts, "Training file {0} Field {1}", filename, fieldId);
        }

        [TestCase(@"Data\german.data", DataFormat.CSV, null)]
        public virtual void CreateDiscretizedDataTableTest(string filename, DataFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);
            DataSplitter splitter = new DataSplitter(data, 5);            

            DataStore trainData = null, testData = null;
            splitter.Split(out trainData, out testData, 0);

            IEnumerable<int> numericFields = fields == null ? trainData.DataStoreInfo
                .SelectAttributeIds(a => a.IsStandard && a.CanDiscretize()) : fields;

            var discretizer = new DecisionTableDiscretizer(this.GetDiscretizer());
            discretizer.FieldsToDiscretize = numericFields;
            discretizer.Discretize(trainData, trainData.Weights);
            DecisionTableDiscretizer.Discretize(testData, trainData);

            foreach (var kvp in discretizer.FieldDiscretizer)
                Console.WriteLine("Field {0} Cuts {1}", kvp.Key, kvp.Value == null ? "All" : kvp.Value.ToString());
            
            foreach (int fieldId in numericFields)
            {
                Assert.IsNotNull(trainData.DataStoreInfo.GetFieldInfo(fieldId).Cuts, "Training file {0} Field {1}", filename, fieldId);
                Assert.IsNotNull(testData.DataStoreInfo.GetFieldInfo(fieldId).Cuts, "Test file {0} Field {1}", filename, fieldId);
            }            
        }

        [Test]
        public void ComputeTest()
        {
            IDiscretizerSupervised discretizer = this.GetDiscretizer() as IDiscretizerSupervised;
            if (discretizer == null)
                throw new InvalidOperationException();
            discretizer.Compute(data, labels, null);
            Assert.IsNotNull(discretizer.Cuts);
        }
    }
}
