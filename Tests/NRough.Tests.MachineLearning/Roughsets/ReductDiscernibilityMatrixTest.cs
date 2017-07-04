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
using System;
using System.IO;
using NRough.Data;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Roughsets;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    internal class ReductDiscernibilityMatrixTest
    {
        private DataStore dataStoreTrain = null;
        private DataStore dataStoreTest = null;

        private DataStoreInfo dataStoreTrainInfo = null;
        private string output = "matrix.out";

        public ReductDiscernibilityMatrixTest()
        {
            //string trainFileName = @"Data\dna_modified.trn";
            string testFileName = @"Data\dna_modified.tst";

            dataStoreTrain = Data.Benchmark.Factory.DnaModifiedTrain();
            //dataStoreTrain = DataStore.Load(trainFileName, DataFormat.RSES1);
            dataStoreTest = DataStore.Load(testFileName, DataFormat.RSES1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }

        [Test]
        public void MeasureRelative()
        {
            Args parms = new Args();
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductRelativeWeights);
            parms.SetParameter(ReductFactoryOptions.DecisionTable, dataStoreTrain);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection, ReductFactory.GetPermutationGenerator(parms).Generate(10));
            parms.SetParameter(ReductFactoryOptions.Epsilon, 20 / 100.0);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();
            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();

            RoughClassifier classifier = new RoughClassifier(reductStoreCollection, RuleQualityMethods.CoverageW, RuleQualityMethods.CoverageW, dataStoreTrain.DataStoreInfo.GetDecisionValues());
            ClassificationResult result = classifier.Classify(dataStoreTrain, null);

            using (FileStream fileStream = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (StreamWriter resultFile = new StreamWriter(fileStream))
                {
                    foreach (IReductStore rs in reductStoreCollection)
                    {
                        foreach (IReduct red in rs)
                        {
                            double quality = InformationMeasureBase.Construct(InformationMeasureType.ObjectWeights).Calc(red);
                            resultFile.WriteLine("{0} : {1}", red.ToString(), quality);
                        }
                    }

                    resultFile.WriteLine();

                    for (int objectIdx = 0; objectIdx < dataStoreTrain.NumberOfRecords; objectIdx++)
                    {
                        resultFile.Write("{0,5}:", objectIdx);
                        foreach (IReductStore rs in reductStoreCollection)
                        {
                            foreach (IReduct red in rs)
                            {
                                resultFile.Write(" {0,5}", RoughClassifier.IsObjectRecognizable(dataStoreTrain, objectIdx, red, RuleQualityMethods.ConfidenceW));
                            }
                        }
                        resultFile.Write(Environment.NewLine);
                    }
                }
            }
        }
    }
}