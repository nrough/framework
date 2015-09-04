
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
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
                this.Weights[i] = (double)1 / (double)this.DataStore.NumberOfRecords;
            }
        }
    }

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
                weight = (double)1
                    / ((double)this.DataStore.DataStoreInfo.NumberOfObjectsWithDecision(this.DataStore.GetDecisionValue(i))
                                                * (double)this.DataStore.DataStoreInfo.NumberOfDecisionValues);

                this.Weights[i] = weight;
            }
        }
    }
}