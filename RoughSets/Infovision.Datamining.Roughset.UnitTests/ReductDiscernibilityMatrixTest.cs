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
            Args parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, ReductGeneratorParamHelper.DataStore }, new Object[] { ReductFactoryKeyHelper.ApproximateReductRelativeWeights, dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator(parms);
            PermutationCollection permutationList = permGen.Generate(10);
            parms.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permutationList);

            RoughClassifier classifier = new RoughClassifier();
            classifier.Train(dataStoreTrain, ReductFactoryKeyHelper.ApproximateReductRelativeWeights, 20, permutationList);
            IReductStoreCollection reductStoreCollection = classifier.Classify(dataStoreTrain, RuleQuality.CoverageW, RuleQuality.CoverageW);
            
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
                    
                    foreach (long objectId in dataStoreTrain.GetObjectIds())
                    {
                        resultFile.Write("{0,5}:", objectId);
                        foreach (IReductStore rs in reductStoreCollection)
                        {
                            foreach (IReduct red in rs)
                            {
                                resultFile.Write(" {0,5}", classifier.IsObjectRecognizable(dataStoreTrain, objectId, red));
                            }
                        }
                        resultFile.Write(Environment.NewLine);
                    }
                }
            }
        }

    }
}
