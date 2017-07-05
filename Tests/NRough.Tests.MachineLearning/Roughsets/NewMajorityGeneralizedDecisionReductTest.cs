// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

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
