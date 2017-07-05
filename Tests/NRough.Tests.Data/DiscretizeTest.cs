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

using System;
using System.Collections.Generic;
using System.Linq;
using NRough.MachineLearning.Roughsets;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Discretization;
using NRough.Data;
using NRough.Data.Benchmark;

namespace NRough.Tests.Data
{
    [TestFixture]
    internal class DiscretizeTest
    {
        public static IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles("Data",
                new string[] {
                    "german",
                    "sat"
                });
        }

        [Test, TestCaseSource("GetDataFiles")]
        public void UpdateColumnTest(KeyValuePair<string, BenchmarkData> kvp)
        {
            double epsilon = 0.05;
            int numberOfPermutations = 20;

            BenchmarkData benchmark = kvp.Value;
            DataStore data = null, train = null, test = null;
            DataSplitter splitter = null;
            AttributeInfo localFieldInfoTrain, localFieldInfoTest;

            if (benchmark.CrossValidationActive)
            {
                data = DataStore.Load(benchmark.DataFile, benchmark.DataFormat);
                splitter = new DataSplitter(data, benchmark.CrossValidationFolds);
            }
            else
            {
                train = DataStore.Load(benchmark.TrainFile, benchmark.DataFormat);
                test = DataStore.Load(benchmark.TestFile, benchmark.DataFormat, train.DataStoreInfo);
            }

            for (int i = 0; i < benchmark.CrossValidationFolds; i++)
            {
                if (splitter != null)                    
                    splitter.Split(out train, out test, i);                

                Args args = new Args();
                args.SetParameter(ReductFactoryOptions.DecisionTable, train);
                args.SetParameter(ReductFactoryOptions.Epsilon, epsilon);
                args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
                PermutationCollection permutations = ReductFactory.GetPermutationGenerator(args).Generate(numberOfPermutations);
                args.SetParameter(ReductFactoryOptions.PermutationCollection, permutations);

                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Run();

                IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection();
                Console.WriteLine("Average reduct length: {0}", reductStoreCollection.GetAvgMeasure(new ReductMeasureLength()));
                IReductStore reductStore = reductStoreCollection.FirstOrDefault();

                RoughClassifier classifier = new RoughClassifier(
                    reductStoreCollection,
                    RuleQualityMethods.ConfidenceW,
                    RuleQualityMethods.ConfidenceW,
                    train.DataStoreInfo.DecisionInfo.InternalValues());

                ClassificationResult classificationResult = classifier.Classify(test);
                Console.WriteLine("Accuracy: {0}", classificationResult.Accuracy);                

                
                foreach (AttributeInfo field in benchmark.GetNumericFields())
                {
                    var discretizer = new DiscretizeEqualFreqency();

                    localFieldInfoTrain = train.DataStoreInfo.GetFieldInfo(field.Id);
                    localFieldInfoTrain.IsNumeric = false;
                    localFieldInfoTrain.IsOrdered = true;
                                        
                    long[] oldValues = train.GetColumnInternal(field.Id);
                    discretizer.Compute(oldValues, null, null);
                    long[] newValues = discretizer.Apply(oldValues);
                    localFieldInfoTrain.Cuts = discretizer.Cuts;
                    
                    localFieldInfoTrain.DataType = typeof(int);
                    train.UpdateColumn(field.Id, Array.ConvertAll(newValues, x => (object)x));

                    localFieldInfoTest = test.DataStoreInfo.GetFieldInfo(field.Id);
                    localFieldInfoTest.IsNumeric = false;
                    localFieldInfoTest.IsOrdered = true;

                    newValues = discretizer.Apply(test.GetColumnInternal(field.Id));
                    localFieldInfoTest.DataType = typeof(long);
                    localFieldInfoTest.Cuts = localFieldInfoTrain.Cuts;
                    test.UpdateColumn(field.Id, Array.ConvertAll(newValues, x => (object)x), localFieldInfoTrain);
                }            

                args = new Args();
                args.SetParameter(ReductFactoryOptions.DecisionTable, train);
                args.SetParameter(ReductFactoryOptions.Epsilon, epsilon);
                args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
                args.SetParameter(ReductFactoryOptions.PermutationCollection, permutations);

                reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Run();

                reductStoreCollection = reductGenerator.GetReductStoreCollection();
                Console.WriteLine("Average reduct length: {0}", reductStoreCollection.GetAvgMeasure(new ReductMeasureLength()));

                reductStore = reductStoreCollection.FirstOrDefault();
                foreach (IReduct reduct in reductStore)
                {
                    reduct.EquivalenceClasses.ToString2();
                }

                classifier = new RoughClassifier(
                    reductStoreCollection,
                    RuleQualityMethods.ConfidenceW,
                    RuleQualityMethods.ConfidenceW,
                    train.DataStoreInfo.DecisionInfo.InternalValues());

                classificationResult = classifier.Classify(test);
                Console.WriteLine("Accuracy: {0}", classificationResult.Accuracy);                
                
            }
        }
    }
}