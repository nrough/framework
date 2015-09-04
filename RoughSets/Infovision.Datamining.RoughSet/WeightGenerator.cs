
using System;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class WeightGenerator
    {
        private DataStore dataStore = null;
        private double[] weights;
        
        protected WeightGenerator(DataStore dataStore)
        {
            this.dataStore = dataStore;
            this.weights = new double[dataStore.NumberOfRecords];
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
                return this.weights; 
            }
            protected set { this.weights = value; }
        }

        protected bool CalcFlag
        {
            get;
            set;
        }

        #endregion

        #region Methods
        
        protected virtual void Generate()
        {
            this.CalcFlag = true;
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

        protected override void Generate()
        {
            this.CalcFlag = true;

            double sum = 0;
            double weight = 0;
            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
            {
                weight = RandomSingleton.Random.Next(1, this.DataStore.NumberOfRecords + 1) * this.DataStore.NumberOfRecords;
                sum += weight;
                this.Weights[i] = weight;
            }

            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
            {
                this.Weights[i] = this.Weights[i] / sum;
            }
        }
    }

    [Serializable]
    public class WeightGeneratorMajority : WeightGenerator
    {
        public WeightGeneratorMajority(DataStore dataStore)
            : base(dataStore)
        {
        }

        protected override void Generate()
        {
            this.CalcFlag = true;

            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
            {
                this.Weights[i] = 1.0 / this.DataStore.NumberOfRecords;
            }
        }
    }

    [Serializable]
    public class WeightGeneratorRelative : WeightGenerator
    {
        public WeightGeneratorRelative(DataStore dataStore)
            : base(dataStore)
        {
        }

        protected override void Generate()
        {
            this.CalcFlag = true;

            double weight = 0;
            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
            {
                weight = 1.0
                    / (this.DataStore.DataStoreInfo.NumberOfObjectsWithDecision(this.DataStore.GetDecisionValue(i))
                        * this.DataStore.DataStoreInfo.NumberOfDecisionValues);

                this.Weights[i] = weight;
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

        protected override void Generate()
        {
            this.CalcFlag = true;
            for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
            {
                this.Weights[i] = value;
            }            
        }
    }
}