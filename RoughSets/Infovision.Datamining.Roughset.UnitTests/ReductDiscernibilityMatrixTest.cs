using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

using NUnit.Framework;
using NUnit;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    class ReductDiscernibilityMatrixTest
    {
        DataStore dataStoreTrain = null;
        DataStore dataStoreTest = null;

        DataStoreInfo dataStoreTrainInfo = null;
        string output = "matrix.out";

        public ReductDiscernibilityMatrixTest()
        {
            string trainFileName = @"Data\dna_modified.trn";
            string testFileName = @"Data\dna_modified.tst";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }

        [Test]
        public void MeasureRelative()
        {
            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductRelativeWeights);
            parms.SetParameter(ReductGeneratorParamHelper.DataStore, dataStoreTrain);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, ReductFactory.GetPermutationGenerator(parms).Generate(10));
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, 20 / 100.0m);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Generate();
            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection(Int32.MaxValue);

            RoughClassifier classifier = new RoughClassifier(reductStoreCollection, RuleQuality.CoverageW, RuleQuality.CoverageW, dataStoreTrain.DataStoreInfo.GetDecisionValues());
            ClassificationResult result = classifier.Classify(dataStoreTrain, null);
            
            using (FileStream fileStream = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (StreamWriter resultFile = new StreamWriter(fileStream))
                {
                    foreach (IReductStore rs in reductStoreCollection)
                    {
                        foreach (IReduct red in rs)
                        {
                            decimal quality = InformationMeasureBase.Construct(InformationMeasureType.ObjectWeights).Calc(red);
                            resultFile.WriteLine("{0} : {1}", red.ToString(), quality);
                        }
                    }

                    resultFile.WriteLine();

                    for (int objectIdx = 0; objectIdx < dataStoreTrain.NumberOfRecords; objectIdx++)
                    //foreach (int objectIdx in dataStoreTrain.GetObjectIndexes())
                    {
                        resultFile.Write("{0,5}:", objectIdx);
                        foreach (IReductStore rs in reductStoreCollection)
                        {
                            foreach (IReduct red in rs)
                            {
                                resultFile.Write(" {0,5}", classifier.IsObjectRecognizable(dataStoreTrain, objectIdx, red));
                            }
                        }
                        resultFile.Write(Environment.NewLine);
                    }
                }
            }
        }

    }
}
