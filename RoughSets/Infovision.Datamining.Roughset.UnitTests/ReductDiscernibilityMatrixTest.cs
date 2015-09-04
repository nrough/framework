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
            string trainFileName = @"dna_modified.trn";
            string testFileName = @"dna_modified.tst";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }

        [Test]
        public void MeasureRelative()
        {
            Args parms = new Args(new String[] { "DataStore" }, new Object[] { dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductRelativeWeights", parms);
            PermutationList permutationList = permGen.Generate(10);
            parms.AddParameter("PermutationList", permutationList);

            RoughClassifier classifier = new RoughClassifier();
            classifier.Train(dataStoreTrain, "ApproximateReductRelativeWeights", 20, permutationList);
            IReductStore reductStore = classifier.Classify(dataStoreTrain);
            
            using (FileStream fileStream = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (StreamWriter resultFile = new StreamWriter(fileStream))
                {
                    foreach (IReduct red in reductStore)
                    {
                        double quality = InformationMeasureBase.Construct(InformationMeasureType.ObjectWeights).Calc(red);
                        resultFile.WriteLine("{0} : {1}", red.ToString(), quality);
                    }

                    resultFile.WriteLine();
                    
                    foreach (Int64 objectId in dataStoreTrain.GetObjectIds())
                    {
                        resultFile.Write("{0,5}:", objectId);
                        foreach (IReduct red in reductStore)
                        {
                            resultFile.Write(" {0,5}", classifier.IsObjectRecognizable(dataStoreTrain, objectId, red, IdentificationType.WeightCoverage));
                        }
                        resultFile.Write(Environment.NewLine);
                    }
                }
            }
        }

    }
}
