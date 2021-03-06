﻿// 
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

using NRough.Data;
using NRough.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning.Roughsets.Reducts.Comparers;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    public class DecisionTreeReduct : DecisionTreeRough
    {
        public string ReductFactoryKey { get; set; }
        public WeightGenerator WeightGenerator { get; set; }
        public PermutationCollection PermutationCollection { get; set; }
        public IComparer<IReduct> ReductComparer { get; set; }
        public int ReductIterations { get; set; }
        public double ReductEpsilon { get; set; }
        public IReduct Reduct { get; private set; }

        public DecisionTreeReduct()
            : base()
        {
            this.ReductFactoryKey = ReductTypes.ApproximateReductMajorityWeights;
            this.ReductIterations = 1;
            this.Gamma = 0.0;
            this.ReductEpsilon = 0.0;
            this.ReductComparer = null;
        }

        public DecisionTreeReduct(string modelName)
            : base(modelName)
        {
            this.ReductFactoryKey = ReductTypes.ApproximateReductMajorityWeights;
            this.ReductIterations = 1;
            this.Gamma = 0.0;
            this.ReductEpsilon = 0.0;
            this.ReductComparer = null;
        }        

        public override void SetClassificationResultParameters(ClassificationResult result)
        {
            base.SetClassificationResultParameters(result);

            result.Epsilon = this.ReductEpsilon;
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            if (this.WeightGenerator == null)
                this.WeightGenerator = new WeightGeneratorMajority(data);

            if (this.PermutationCollection == null)
                this.PermutationCollection = new PermutationCollection(this.ReductIterations, attributes);

            if (this.ReductComparer == null)
                this.ReductComparer = new ReductAccuracyComparer(data);

            Args parms = new Args(8);
            parms.SetParameter(ReductFactoryOptions.DecisionTable, data);
            parms.SetParameter(ReductFactoryOptions.ReductType, this.ReductFactoryKey);
            parms.SetParameter(ReductFactoryOptions.WeightGenerator, this.WeightGenerator);
            parms.SetParameter(ReductFactoryOptions.Epsilon, this.ReductEpsilon);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection, this.PermutationCollection);
            parms.SetParameter(ReductFactoryOptions.UseExceptionRules, false);
            parms.SetParameter(ReductFactoryOptions.NumberOfReducts, this.ReductIterations);
            parms.SetParameter(ReductFactoryOptions.NumberOfReductsToTest, this.ReductIterations);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();

            IReductStoreCollection reductStores = generator.GetReductStoreCollection();
            if (reductStores.ReductPerStore == true)
                throw new InvalidOperationException("reductStores.ReductPerStore == true is not supported");

            IReductStore reducts = reductStores.FirstOrDefault();
            IReduct reduct = reducts.FilterReducts(1, this.ReductComparer).FirstOrDefault();
            this.Reduct = reduct;

            return base.Learn(data, reduct.Attributes.ToArray());
        }
    }
}