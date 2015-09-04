
using System;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class WeightGenerator
    {
        private DataStore dataStore = null;
        protected double[] w;
        
        protected WeightGenerator(DataStore dataStore)
        {
            this.dataStore = dataStore;
            this.w = new double[dataStore.NumberOfRecords];
            this.CalcFlag = false;
        }

        #region Properties

        protected DataStore DataStore
        {
            get { return this.dataStore; }
        }

        public virtual double[] Weights
        {
            get 
            {
                if (!this.CalcFlag)
                    this.Generate();
                return this.w; 
            }
            
            set { this.w = value; }
        }

        protected bool CalcFlag
        {
            get;
            set;
        }

        #endregion

        #region Methods
        
        public virtual void Generate()
        {
            this.CalcFlag = true;
        }

        public virtual void Reset()
        {
            this.CalcFlag = false;
            this.Generate();
        }

        #endregion
        
    }

    [Serializable]
    public class WeightGeneratorRandom : WeightGenerator
    {
        public WeightGeneratorRandom(DataStore dataStore)
            : base(dataStore)
        {
        }

        public override void Generate()
        {
            if (this.CalcFlag == true)
                return;

            base.Generate();

            double sum = 0.0;            
            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
            {
                this.Weights[i] = RandomSingleton.Random.Next(0, this.DataStore.NumberOfRecords);
                sum += this.Weights[i];
            }

            if (sum != 0.0)
            {
                double allocated = 0.0;
                for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
                {
                    this.Weights[i] = this.Weights[i] / sum;

                    if (i == this.DataStore.NumberOfRecords - 1)
                        this.Weights[i] = 1.0 - allocated;

                    allocated += this.Weights[i];
                }
            }
            else
            {
                throw new InvalidOperationException("sum of weights cannot be zero.");
            }
        }
    }

    [Serializable]
    public class WeightGeneratorConstant : WeightGenerator
    {
        private double value = 1.0;

        public double Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public WeightGeneratorConstant(DataStore dataStore)
            : base(dataStore)
        {
        }

        public WeightGeneratorConstant(DataStore dataStore, double value)
            : base(dataStore)
        {
            this.Value = value;
        }

        public override void Generate()
        {
            if (this.CalcFlag == true)
                return;
            base.Generate();
            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
                this.Weights[i] = value;
        }
    }

    [Serializable]
    public class WeightGeneratorMajority : WeightGeneratorConstant
    {
        public WeightGeneratorMajority(DataStore dataStore)
            : base(dataStore, 1.0 / dataStore.NumberOfRecords)
        {
        }        
    }

    [Serializable]
    public class WeightGeneratorRelative : WeightGenerator
    {
        public WeightGeneratorRelative(DataStore dataStore)
            : base(dataStore)
        {
        }

        public override void Generate()
        {
            if (this.CalcFlag == true)
                return;

            base.Generate();          
            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
            {
                this.Weights[i] = 1.0
                    / (this.DataStore.DataStoreInfo.NumberOfObjectsWithDecision(this.DataStore.GetDecisionValue(i))
                        * this.DataStore.DataStoreInfo.NumberOfDecisionValues);                 
            }
        }
    }    
}