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

using System;
using NRough.Core;
using NRough.MachineLearning.Weighting;

namespace NRough.MachineLearning.Roughsets
{
    [Serializable]
    public class ReductGeneratorWeights : ReductGeneratorMeasure
    {
        private WeightGenerator weightGenerator;

        #region Constructors

        public ReductGeneratorWeights()
            : base()
        {
            this.InformationMeasure = InformationMeasureWeights.Instance;
        }

        #endregion Constructors

        #region Properties

        public virtual WeightGenerator WeightGenerator
        {
            get
            {
                if (this.weightGenerator == null)
                {
                    this.weightGenerator = CreateWeightGenerator();
                }

                return this.weightGenerator;
            }

            set
            {
                this.weightGenerator = value;
            }
        }

        #endregion Properties

        #region Methods

        public override void InitFromArgs(Core.Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductFactoryOptions.WeightGenerator))
                this.weightGenerator = (WeightGenerator)args.GetParameter(ReductFactoryOptions.WeightGenerator);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {            
            ReductWeights r = new ReductWeights(this.DecisionTable, fieldIds, epsilon, this.WeightGenerator.Weights);
            r.Id = id;
            return r;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection equivalenceClasses)
        {
            ReductWeights r = new ReductWeights(this.DecisionTable, fieldIds, epsilon, this.WeightGenerator.Weights, equivalenceClasses);
            r.Id = id;
            return r;
        }

        protected virtual WeightGenerator CreateWeightGenerator()
        {
            WeightGeneratorConstant wGen = new WeightGeneratorConstant(this.DecisionTable);
            wGen.Value = 1.0 / this.DecisionTable.NumberOfRecords;
            return wGen;
        }

        #endregion Methods
    }

    [Serializable]
    public class ReductGeneratorWeightsMajority : ReductGeneratorWeights
    {
        public ReductGeneratorWeightsMajority()
            : base()
        {
        }

        protected override WeightGenerator CreateWeightGenerator()
        {
            WeightGenerator wGen = new WeightGeneratorMajority(this.DecisionTable);
            this.DecisionTable.SetWeights(wGen.Weights);
            return wGen;
        }
    }

    public class ApproximateReductMajorityWeightsFactory : ApproximateReductMajorityFactory
    {
        public override string FactoryKey
        {
            get { return ReductTypes.ApproximateReductMajorityWeights; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneratorWeightsMajority rGen = new ReductGeneratorWeightsMajority();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }

    [Serializable]
    public class ReductGeneratorWeightsRelative : ReductGeneratorWeights
    {
        public ReductGeneratorWeightsRelative()
            : base()
        {
        }

        protected override WeightGenerator CreateWeightGenerator()
        {
            WeightGenerator wGen = new WeightGeneratorRelative(this.DecisionTable);
            this.DecisionTable.SetWeights(wGen.Weights);
            return wGen;
        }
    }

    public class ApproximateReductRelativeWeightsFactory : ApproximateReductRelativeFactory
    {
        public override string FactoryKey
        {
            get { return ReductTypes.ApproximateReductRelativeWeights; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneratorWeightsRelative rGen = new ReductGeneratorWeightsRelative();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }
}