using System.IO;
using Raccoon.Data;
using Raccoon.MachineLearning.Roughset;
using Raccoon.Core;
using NUnit.Framework;
using Raccoon.MachineLearning.Weighting;
using Raccoon.MachineLearning.Classification;
using Raccoon.MachineLearning.MissingValues;

namespace Raccoon.MachineLearning.Tests.MissingValues
{
    [TestFixture]
    public class ReplaceMissingValuesTest
    {
        [Test]
        public void ComputeTest()
        {
            DataStore trnData = DataStore.Load(@"Data\soybean-large.data", FileFormat.Csv);
            DataStore tstData = DataStore.Load(@"Data\soybean-large.test", FileFormat.Csv, trnData.DataStoreInfo);
            trnData.SetDecisionFieldId(1);
            tstData.SetDecisionFieldId(1);

            if (!Directory.Exists(@"temp"))
                Directory.CreateDirectory(@"temp");

            trnData.DumpExt(@"temp\missingvalsorig.trn", ",");
            tstData.DumpExt(@"temp\missingvalsorig.tst", ",");

            DataStore trnCompleteData = new ReplaceMissingValues().Compute(trnData);
            DataStore tstCompleteDate = tstData;

            trnCompleteData.DumpExt(@"temp\missingvals.trn", ",");
            tstCompleteDate.DumpExt(@"temp\missingvals.tst", ",");

            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, trnData);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoostingVarEps);
            parms.SetParameter(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.ConfidenceW);
            parms.SetParameter(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.ConfidenceW);
            parms.SetParameter(ReductGeneratorParamHelper.MaxIterations, 1);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(trnData));
            parms.SetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, false);

            Args inner = new Args();
            inner.SetParameter(ReductGeneratorParamHelper.TrainData, trnData);
            inner.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajorityWeights);
            inner.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, 1);
            inner.SetParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(trnData));

            parms.SetParameter(ReductGeneratorParamHelper.InnerParameters, inner);

            ReductEnsembleBoostingVarEpsGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingVarEpsGenerator;
            reductGenerator.Run();

            RoughClassifier classifierTrn = new RoughClassifier(
                reductGenerator.GetReductGroups(),
                reductGenerator.IdentyficationType,
                reductGenerator.VoteType,
                trnData.DataStoreInfo.GetDecisionValues());
            ClassificationResult resultTrn = classifierTrn.Classify(trnData, null);

            RoughClassifier classifierTst = new RoughClassifier(
                reductGenerator.GetReductGroups(),
                reductGenerator.IdentyficationType,
                reductGenerator.VoteType,
                trnData.DataStoreInfo.GetDecisionValues());
            ClassificationResult resultTst = classifierTst.Classify(tstData, null);
        }
    }
}