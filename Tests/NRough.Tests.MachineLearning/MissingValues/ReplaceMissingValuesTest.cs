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
using System.IO;
using NRough.Data;
using NRough.MachineLearning.Roughsets;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.MissingValues;

namespace NRough.Tests.MachineLearning.MissingValues
{
    [TestFixture]
    public class ReplaceMissingValuesTest
    {
        [Test]
        public void ComputeTest()
        {
            DataStore trnData = DataStore.Load(@"Data\soybean-large.data", DataFormat.CSV);
            DataStore tstData = DataStore.Load(@"Data\soybean-large.test", DataFormat.CSV, trnData.DataStoreInfo);
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