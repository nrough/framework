using System;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductGeneratorWeights : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorWeights()
            : base()
        {
            this.InformationMeasure = (IInformationMeasure)InformationMeasureBase.Construct(InformationMeasureType.ObjectWeights);
        }

        #endregion

        #region Properties

        protected abstract WeightGenerator WeightGenerator
        {
            get;
        }

        #endregion

        #region Methods

        protected override IReduct CreateReductObject(int[] fieldIds, double approxDegree, string id)
        {
            ReductWeights r = new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights, approxDegree);
            r.Id = id;
            return r;
        }

        #endregion
    }

    [Serializable]
    public class ReductGeneratorWeightsMajority : ReductGeneratorWeights
    {
        private WeightGenerator weightGenerator;

        public ReductGeneratorWeightsMajority()
            : base()
        {
        }

        protected override WeightGenerator WeightGenerator
        {
            get
            {
                if (this.weightGenerator == null)
                {
                    this.weightGenerator = new WeightGeneratorMajority(this.DataStore);
                }

                return this.weightGenerator;
            }
        }
    }

    [Serializable]
    public class ReductGeneratorWeightsRelative : ReductGeneratorWeights
    {
        private WeightGenerator weightGenerator;

        public ReductGeneratorWeightsRelative()
            : base()
        {
        }

        protected override WeightGenerator WeightGenerator
        {
            get
            {
                if (this.weightGenerator == null)
                {
                    this.weightGenerator = new WeightGeneratorRelative(this.DataStore);
                }

                return this.weightGenerator;
            }
        }
    }
}
