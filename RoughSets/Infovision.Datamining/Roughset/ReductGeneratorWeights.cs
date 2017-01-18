using System;
using Raccoon.Core;
using Raccoon.MachineLearning.Weighting;

namespace Raccoon.MachineLearning.Roughset
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

            if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
                this.weightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {            
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, epsilon, this.WeightGenerator.Weights);
            r.Id = id;
            return r;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection equivalenceClasses)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, epsilon, this.WeightGenerator.Weights, equivalenceClasses);
            r.Id = id;
            return r;
        }

        protected virtual WeightGenerator CreateWeightGenerator()
        {
            WeightGeneratorConstant wGen = new WeightGeneratorConstant(this.DataStore);
            wGen.Value = 1.0 / this.DataStore.NumberOfRecords;
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
            WeightGenerator wGen = new WeightGeneratorMajority(this.DataStore);
            this.DataStore.SetWeights(wGen.Weights);
            return wGen;
        }
    }

    public class ApproximateReductMajorityWeightsFactory : ApproximateReductMajorityFactory
    {
        public override string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ApproximateReductMajorityWeights; }
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
            WeightGenerator wGen = new WeightGeneratorRelative(this.DataStore);
            this.DataStore.SetWeights(wGen.Weights);
            return wGen;
        }
    }

    public class ApproximateReductRelativeWeightsFactory : ApproximateReductRelativeFactory
    {
        public override string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ApproximateReductRelativeWeights; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneratorWeightsRelative rGen = new ReductGeneratorWeightsRelative();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }
}