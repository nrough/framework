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
    [TestFixture]
    public class DataStoreDiscretizationTest
    {
        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2 })]
        public void DiscretizeTest(string filename, DataFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);

            IEnumerable<int> numericFields = fields == null ? data.DataStoreInfo
                .SelectAttributeIds(a => a.IsStandard && a.CanDiscretize()) : fields;

            var discretizer = new DecisionTableDiscretizer();            
            discretizer.Discretize(data);
            
            foreach (int fieldId in numericFields)
                Assert.IsNotNull(data.DataStoreInfo.GetFieldInfo(fieldId).Cuts, "Training file {0} Field {1}", filename, fieldId);

            var discretizers = discretizer.FieldDiscretizer;
            foreach (int fieldId in numericFields)
            {
                Console.WriteLine("Field {0}", fieldId);
                Console.WriteLine(discretizers[fieldId]);
            }
        }

        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2 })]
        public void AddNewColumnsTest(string filename, DataFormat fileFormat, IEnumerable<int> fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);
            int numberOfFields = data.DataStoreInfo.NumberOfFields;

            IEnumerable<int> numericFields = fields == null ? data.DataStoreInfo
                .SelectAttributeIds(a => a.IsStandard && a.CanDiscretize()): fields;

            var discretizer =
                new DecisionTableDiscretizer(
                    new DiscretizeSupervisedBase()
                    {
                        NumberOfBuckets = 5
                    })
                {
                    RemoveColumnAfterDiscretization = true,
                    UpdateDataColumns = false,
                    AddColumnsBasedOnCuts = true,
                    UseBinaryCuts = true
                };

            discretizer.FieldsToDiscretize = numericFields;
            discretizer.AddColumnsBasedOnCuts = true;
            discretizer.Discretize(data);            
        }

        [TestCase(@"Data\german.data", DataFormat.CSV, new int[] { 2 })]
        public void CreateDiscretizedDataTableTest(string filename, DataFormat fileFormat, int[] fields)
        {
            DataStore data = DataStore.Load(filename, fileFormat);
            DataSplitter splitter = new DataSplitter(data, 5);

            DataStore trainData = null, testData = null;
            splitter.Split(out trainData, out testData, 0);

            int numberOfFields = trainData.DataStoreInfo.NumberOfFields;
            IEnumerable<int> numericFields = fields == null ? trainData.DataStoreInfo
                .SelectAttributeIds(a => a.IsStandard && a.CanDiscretize()) : fields;

            var discretizer = new DecisionTableDiscretizer();
            discretizer.FieldsToDiscretize = numericFields;
            discretizer.AddColumnsBasedOnCuts = true;
            discretizer.Discretize(trainData);
            
            DecisionTableDiscretizer.Discretize(testData, trainData);

            Assert.AreEqual(numberOfFields + 1, trainData.DataStoreInfo.NumberOfFields);
            Assert.AreEqual(numberOfFields + 1, testData.DataStoreInfo.NumberOfFields);
        }
    }
}
