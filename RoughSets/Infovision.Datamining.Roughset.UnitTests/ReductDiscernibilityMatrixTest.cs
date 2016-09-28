using System;
using System.IO;

using Infovision.Data;
using Infovision.Utils;

using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
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
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, dataStoreTrain);
            parms.SetParameter(ReductGeneratorParamHelper.PermutationCollection, ReductFactory.GetPermutationGenerator(parms).Generate(10));
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, 20 / 100.0);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();
            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();

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
                                resultFile.Write(" {0,5}", RoughClassifier.IsObjectRecognizable(dataStoreTrain, objectIdx, red, RuleQuality.ConfidenceW));
                            }
                        }
                        resultFile.Write(Environment.NewLine);
                    }
                }
            }
        }
    }
}