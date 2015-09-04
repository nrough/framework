using System;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductGeneratorWeights : ReductGeneratorMeasure
    {
        #region Constructors

        public ReductGeneratorWeights(DataStore dataStore)
            : base(dataStore)
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

        protected override IReduct CreateReductObject(int[] fieldIds)
        {
            return new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights);
        }

        #endregion
    }

    [Serializable]
    public class ReductGeneratorWeightsMajority : ReductGeneratorWeights
    {
        private WeightGenerator weightGenerator;

        public ReductGeneratorWeightsMajority(DataStore dataStore)
            : base(dataStore)
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

        public ReductGeneratorWeightsRelative(DataStore dataStore)
            : base(dataStore)
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
