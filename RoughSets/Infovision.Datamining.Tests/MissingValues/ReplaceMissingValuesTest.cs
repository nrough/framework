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
            DataStore trnData = DataStore.Load(@"Data\soybean-large.data", FileFormat.CSV);
            DataStore tstData = DataStore.Load(@"Data\soybean-large.test", FileFormat.CSV, trnData.DataStoreInfo);
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
            parms.SetParameter(ReductFactoryOptions.DecisionTable, trnData);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsembleBoostingVarEps);
            parms.SetParameter(ReductFactoryOptions.IdentificationType, (RuleQualityMethod)RuleQualityMethods.ConfidenceW);
            parms.SetParameter(ReductFactoryOptions.VoteType, (RuleQualityMethod)RuleQualityMethods.ConfidenceW);
            parms.SetParameter(ReductFactoryOptions.MaxIterations, 1);
            parms.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorMajority(trnData));
            parms.SetParameter(ReductFactoryOptions.CheckEnsembleErrorDuringTraining, false);

            Args inner = new Args();
            inner.SetParameter(ReductFactoryOptions.DecisionTable, trnData);
            inner.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajorityWeights);
            inner.SetParameter(ReductFactoryOptions.NumberOfReducts, 1);
            inner.SetParameter(ReductFactoryOptions.WeightGenerator, new WeightGeneratorMajority(trnData));

            parms.SetParameter(ReductFactoryOptions.InnerParameters, inner);

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