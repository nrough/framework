using NRough.Core;
using NRough.Data;
using NRough.Data.Filters;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Classification.Ensembles;
using NRough.MachineLearning.Evaluation;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Roughsets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    class NewMajorityGeneralizedDecisionReductTest
    {
        [TestCase(@"Data\dermatology_modified.data", DataFormat.CSV, 5)]
        //[TestCase(@"Data\zoo.dta", DataFormat.RSES1, 5)]
        //[TestCase(@"Data\soybean-small.2.data", DataFormat.RSES1, 5)]
        //[TestCase(@"Data\house-votes-84.2.data", DataFormat.RSES1_1, 5)]
        //[TestCase(@"Data\agaricus-lepiota.2.data", DataFormat.RSES1, 5)]
        //[TestCase(@"Data\breast-cancer-wisconsin.2.data", DataFormat.RSES1, 5)]
        //[TestCase(@"Data\promoters.2.data", DataFormat.RSES1, 5)]
        [TestCase(@"Data\semeion.data", DataFormat.RSES1, 5)]                
        public void DecisionTreeWithCV(string dataFile, DataFormat fileFormat, int folds)
        {
            DataStore data = DataStore.Load(dataFile, fileFormat);
            foreach (var attribute in data.DataStoreInfo.Attributes)
                attribute.IsNumeric = false;
            CrossValidation cv = new CrossValidation(data, folds);
            var permutations = new PermutationCollection(
                20, data.SelectAttributeIds(a => a.IsStandard).ToArray());

            var reductTypes = new string[]
            {
                ReductTypes.ApproximateReductMajority,
                ReductTypes.GeneralizedMajorityDecisionApproximate
            };

            foreach (var reductType in reductTypes)
            {                                
                for (double eps = 0.0; eps <= 0.5; eps += 0.01)
                {                    
                    Args parms = new Args();                    
                    parms.SetParameter<string>(ReductFactoryOptions.ReductType, reductType);
                    parms.SetParameter<double>(ReductFactoryOptions.Epsilon, eps);
                    parms.SetParameter<bool>(ReductFactoryOptions.UseExceptionRules, false);
                    parms.SetParameter<PermutationCollection>(ReductFactoryOptions.PermutationCollection, permutations);

                    var prototype = new ReductDecisionRules(String.Format("{0}-{1}-{2}", reductType, "", ""));
                    prototype.ReductGeneratorArgs = parms;

                    var result = cv.Run<ReductDecisionRules>(prototype, data.GetStandardFields());
                    result.Epsilon = eps;
                    Console.WriteLine(result);
                }

                //TODO remove
                break;
            }                                                
        }
    }
}
