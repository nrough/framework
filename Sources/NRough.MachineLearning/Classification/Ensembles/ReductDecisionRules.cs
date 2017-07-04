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
using NRough.Core;
using NRough.Data;
using NRough.MachineLearning.Roughsets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.Ensembles
{
    [Serializable]
    public class ReductDecisionRules : EnsembleBase
    {
        public Args ReductGeneratorArgs { get; set; }
        public RuleQualityMethod DecisionIdentificationMethod { get; set; }
        public RuleQualityMethod RuleVotingMethod { get; set; }

        private IList<IReduct> reducts;

        public ReductDecisionRules()
        {
            InitDefault();
        }

        public ReductDecisionRules(string modelName)
            : base(modelName)
        {
            InitDefault();
        }

        public ReductDecisionRules(IReduct reduct)
            : this()
        {
            reducts.Add(reduct);
        }

        public ReductDecisionRules(IEnumerable<IReduct> reductCollection)
            : this()
        {
            foreach (var reduct in reductCollection)
                reducts.Add(reduct);
        }

        private void InitDefault()
        {
            reducts = new List<IReduct>();
            Args parm = new Args();
            parm.SetParameter(ReductFactoryOptions.ReductType,
                ReductTypes.ApproximateDecisionReduct);
            parm.SetParameter(ReductFactoryOptions.FMeasure,
                (FMeasure)FMeasures.MajorityWeighted);
            parm.SetParameter(ReductFactoryOptions.Epsilon, 0.05);
            parm.SetParameter(ReductFactoryOptions.NumberOfReducts, 10);

            ReductGeneratorArgs = parm;

            DecisionIdentificationMethod = RuleQualityMethods.Confidence;
            RuleVotingMethod = RuleQualityMethods.SingleVote;
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            if (reducts == null || reducts.Count == 0)
            {
                if (ReductGeneratorArgs == null)
                    throw new InvalidOperationException("ReductGeneratorArgs == null");

                Args localArgs = (Args) ReductGeneratorArgs.Clone();
                localArgs.SetParameter(ReductFactoryOptions.DecisionTable, data);

                IReductGenerator generator = ReductFactory.GetReductGenerator(localArgs);
                generator.Run();

                reducts = new List<IReduct>(generator.GetReducts());
            }

            //Create rules and save them for Classify method

            
            return Classifier.Default.Classify(this, data);
        }
    }
}
