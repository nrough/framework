using System;
using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class AppriximateReductWeightRelativeEnsembleTest
    {
        DataStore dataStoreTrain = null;
        DataStore dataStoreTest = null;

        DataStoreInfo dataStoreTrainInfo = null;

        public AppriximateReductWeightRelativeEnsembleTest()
        {
            string trainFileName = @"monks-1.train";
            string testFileName = @"monks-1.test";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;
        }

        [Test]
        public void ReductRelativeTest()
        {
            double[] weights = new WeightGeneratorEnsembleRelative(dataStoreTrain).Weights;
            ReductWeights allAttributes = new ReductWeights(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), weights);
            double allAttrMeasure = InformationMeasureBase.Construct(InformationMeasureType.Relative).Calc(allAttributes);

            Args parms = new Args(new String[] { "DataStore" }, new Object[] { dataStoreTrain });
            IPermutationGenerator permGen = ReductFactory.GetPermutationGenerator("ApproximateReductRelativeWeightsEnsemble", parms);
            PermutationList permutationList = permGen.Generate(10);
            parms.AddParameter("PermutationList", permutationList);

            ReductGeneratorWeightsEnsembleRelative reductGenerator = new ReductGeneratorWeightsEnsembleRelative(dataStoreTrain);
            reductGenerator.ApproximationLevel = 0.4;
            reductGenerator.NumberOfIterations = 3;

            reductGenerator.Generate(parms);

            foreach (IReductStore reductStore in reductGenerator.PermutationReductStore)
            {
                Assert.IsNotNull(reductStore);
                Assert.GreaterOrEqual(reductStore.Count, 1);
            }

            
        }
    }
}
