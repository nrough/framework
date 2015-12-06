using System;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class ReductGeneratorWeights : ReductGeneratorMeasure
    {
        private WeightGenerator weightGenerator;

        #region Constructors

        public ReductGeneratorWeights()
            : base()
        {
            this.InformationMeasure = 
                (IInformationMeasure)InformationMeasureBase
                .Construct(InformationMeasureType.ObjectWeights);
        }

        #endregion

        #region Properties

        public virtual WeightGenerator WeightGenerator
        {
            get
            {
                if (this.weightGenerator == null)
                {
                    WeightGeneratorConstant wGen = new WeightGeneratorConstant(this.DataStore);
                    wGen.Value = Decimal.Divide(Decimal.One, this.DataStore.NumberOfRecords);
                    this.weightGenerator = wGen;
                }

                return this.weightGenerator;
            }

            set
            {
                this.weightGenerator = value;
            }
        }

        #endregion

        #region Methods

        public override void InitFromArgs(Utils.Args args)
        {
            base.InitFromArgs(args);

            if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
                this.weightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights, epsilon);
            r.Id = id;
            return r;
        }

        #endregion
    }

    [Serializable]
    public class ReductGeneratorWeightsMajority : ReductGeneratorWeights
    {       
        public ReductGeneratorWeightsMajority()
            : base()
        {
        }

        protected WeightGenerator CreateWeightGenerator()
        {
            return new WeightGeneratorMajority(this.DataStore);
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

        protected WeightGenerator CreateWeightGenerator()
        {
            return new WeightGeneratorRelative(this.DataStore);
        }        
    }

    public class ApproximateReductRelativeWeightsFactory : ApproximateReductRelativeFactory
    {
        public override string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ApproximateReductRelativeWeights;; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            ReductGeneratorWeightsRelative rGen = new ReductGeneratorWeightsRelative();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }   
}
