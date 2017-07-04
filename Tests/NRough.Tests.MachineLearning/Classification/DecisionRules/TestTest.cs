//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.MachineLearning.Classification.DecisionLookup;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning.Classification;

namespace NRough.Tests.MachineLearning.Classification.DecisionRules
{
    [TestFixture]
    class TestTest
    {
        [Test, Repeat(1)]
        [TestCase(@"Data\monks-2.train", @"Data\monks-2.test")]
        public void NumberOfRulesVsAccuracy(string trainFile, string testFile)
        {
            DataStore data = DataStore.Load(trainFile, DataFormat.RSES1);
            foreach (var fieldInfo in data.DataStoreInfo.Attributes) fieldInfo.IsNumeric = false;
            DataStore test = DataStore.Load(testFile, DataFormat.RSES1, data.DataStoreInfo);
            int[] attributes = data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard).ToArray();

            WeightGenerator weightGenerator = new WeightGeneratorMajority(data);
            PermutationCollection permutationCollection = new PermutationCollection(100, attributes);                

            Args parms = new Args();
            parms.SetParameter<DataStore>(ReductFactoryOptions.DecisionTable, data);
            parms.SetParameter<string>(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
            parms.SetParameter<WeightGenerator>(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parms.SetParameter<double>(ReductFactoryOptions.Epsilon, 0.20);
            parms.SetParameter<PermutationCollection>(ReductFactoryOptions.PermutationCollection, permutationCollection);
            parms.SetParameter<bool>(ReductFactoryOptions.UseExceptionRules, false);
            parms.SetParameter<int>(ReductFactoryOptions.NumberOfReducts, 100);
            parms.SetParameter<int>(ReductFactoryOptions.NumberOfReductsToTest, 100);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();

            foreach (var reductStore in generator.GetReductStoreCollection())
                foreach (var reduct in reductStore)
                {
                    DecisionLookupMajority decTable = new DecisionLookupMajority();
                    decTable.Learn(data, reduct.Attributes.ToArray());
                    ClassificationResult result = Classifier.Default.Classify(decTable, test);
                    Console.WriteLine("{0} {1} {2}", reduct, result.Error, result.NumberOfRules);
                }
        }
    }
}
