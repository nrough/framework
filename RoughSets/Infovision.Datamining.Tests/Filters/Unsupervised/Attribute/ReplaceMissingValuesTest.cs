using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining.Filters.Unsupervised.Attribute;
using Infovision.Datamining.Roughset;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Tests.Filters.Unsupervised.Attribute
{
    [TestFixture]
    public class ReplaceMissingValuesTest
    {
        [Test]
        public void ComputeTest()
        {
            //Console.WriteLine("ReplaceMissingValuesTest.ComputeTest()");

            DataStore trnData = DataStore.Load(@"Data\soybean-large.data", FileFormat.Csv);            
            DataStore tstData = DataStore.Load(@"Data\soybean-large.test", FileFormat.Csv, trnData.DataStoreInfo);
            trnData.SetDecisionFieldId(1);
            tstData.SetDecisionFieldId(1);

            if (!Directory.Exists(@"temp"))
                Directory.CreateDirectory(@"temp");
            
            trnData.WriteToCSVFileExt(@"temp\missingvalsorig.trn", ",");
            tstData.WriteToCSVFileExt(@"temp\missingvalsorig.tst", ",");

            DataStore trnCompleteData = new ReplaceMissingValues().Compute(trnData);
            //DataStore tstCompleteDate = new ReplaceMissingValues().Compute(tstData, trnData);
            DataStore tstCompleteDate = tstData;

            trnCompleteData.WriteToCSVFileExt(@"temp\missingvals.trn", ",");
            tstCompleteDate.WriteToCSVFileExt(@"temp\missingvals.tst", ",");

            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.DataStore, trnData);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoostingVarEps);
            parms.SetParameter(ReductGeneratorParamHelper.IdentificationType, (RuleQualityFunction)RuleQuality.ConfidenceW);
            parms.SetParameter(ReductGeneratorParamHelper.VoteType, (RuleQualityFunction)RuleQuality.ConfidenceW);
            parms.SetParameter(ReductGeneratorParamHelper.MaxIterations, 1);            
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(trnData));
            parms.SetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, false);

            ReductEnsembleBoostingVarEpsGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleBoostingVarEpsGenerator;
            reductGenerator.Generate();

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

            /*
            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                      parms.GetParameter(ReductGeneratorParamHelper.FactoryKey),
                                      reductGenerator.IdentyficationType,
                                      reductGenerator.VoteType,
                                      reductGenerator.MinReductLength,
                                      reductGenerator.UpdateWeights,
                                      "Majority",
                                      reductGenerator.CheckEnsembleErrorDuringTraining,
                                      1,
                                      reductGenerator.MaxIterations,
                                      reductGenerator.IterationsPassed,
                                      reductGenerator.NumberOfWeightResets,
                                      resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                      resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                      reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
            */
        }
    }
}
