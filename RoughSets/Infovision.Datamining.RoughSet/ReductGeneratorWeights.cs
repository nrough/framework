using System;
using Infovision.Data;

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
            this.InformationMeasure = (IInformationMeasure)InformationMeasureBase.Construct(InformationMeasureType.ObjectWeights);
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
                    wGen.Value = 1.0;
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

            if (args.Exist("WeightGenerator"))
                this.weightGenerator = (WeightGenerator)args.GetParameter("WeightGenerator");
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
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
}
