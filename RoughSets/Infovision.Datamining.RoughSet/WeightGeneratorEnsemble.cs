using System;
using Infovision.Data;

namespace Infovision.Datamining.Roughset
{
    public abstract class WeightGeneratorEnsemble : WeightGeneratorRelative
    {
        public WeightGeneratorEnsemble(DataStore dataStore)
            : base(dataStore)
        {
            this.DiscernibilityMatrix = new int[dataStore.NumberOfRecords];            
            this.NumberOfReducts = 0;
        }

        #region Properties

        protected int NumberOfReducts
        {
            get;
            set;
        }

        protected int[] DiscernibilityMatrix
        {
            get;
            set;
        }


        #endregion

        #region Methods

        public void NewReduct(IReduct reduct)
        {
            this.CalcFlag = false;
            this.NumberOfReducts++;

            //TODO Calculate max according to the type of 
            foreach (EquivalenceClass e in reduct.EquivalenceClassMap)
            {
                double maxValue = 0;
                long maxDecision = -1;
                foreach (long decisionValue in e.DecisionValues)
                {
                    double sum = 0;
                    foreach (int objectIdx in e.GetObjectIndexes(decisionValue))
                    {
                        sum += reduct.Weights[objectIdx];
                    }
                    if (sum > maxValue + (0.0001 / (double)reduct.ObjectSetInfo.NumberOfRecords))
                    {
                        maxValue = sum;
                        maxDecision = decisionValue;
                    }
                }

                //Objects with maxDecision get 1 in the discernibility matrix
                foreach (int objectIdx in e.GetObjectIndexes(maxDecision))
                {
                    this.DiscernibilityMatrix[objectIdx]++;
                }
            }
        }

        protected override void Generate()
        {
            this.CalcFlag = true;
            double sum = 0;

            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
            {
                this.Weights[i] = this.CalcObjectWeight(i);
                sum += this.Weights[i];
            }

            //Normalize
            if (sum != 0)
                for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
                    this.Weights[i] = this.Weights[i] / sum;
        }

        protected abstract double CalcObjectWeight(int objectIdx);

        #endregion
        
    }

    public class WeightGeneratorEnsembleRelative : WeightGeneratorEnsemble
    {
       
        #region Constructors

        public WeightGeneratorEnsembleRelative(DataStore dataStore)
            : base(dataStore)
        {
        }

        #endregion

        #region Methods

        protected override double CalcObjectWeight(int objectIdx)
        {            
            double result = 1.0
                    / (this.DataStore.DataStoreInfo.NumberOfObjectsWithDecision(this.DataStore.GetDecisionValue(objectIdx))
                                                * this.DataStore.DataStoreInfo.NumberOfDecisionValues);

            return this.NumberOfReducts > 0 ? result * (1.0 - (this.DiscernibilityMatrix[objectIdx] / this.NumberOfReducts)) : result;
        }

        #endregion
    }

    public class WeightGeneratorEnsembleMajority : WeightGeneratorEnsemble
    {

        #region Constructors

        public WeightGeneratorEnsembleMajority(DataStore dataStore)
            : base(dataStore)
        {
        }

        #endregion

        #region Methods

        protected override double CalcObjectWeight(int objectIdx)
        {
            double result;

            result = (double) 1 / (double) this.DataStore.NumberOfRecords;
            return this.NumberOfReducts > 0 ? result * (1 - (this.DiscernibilityMatrix[objectIdx] / this.NumberOfReducts)) : result;
        }

        #endregion
    }
}
