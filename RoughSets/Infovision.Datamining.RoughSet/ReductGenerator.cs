using System;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductGenerator : IReductGenerator
    {
        #region Globals

        private DataStore dataStore = null;
        private double approximationLevel = 0;
        private int[][] fieldGroups;
        protected object syncRoot = new object();

        #endregion

        #region Constructors

        private ReductGenerator()
        {
        }
        
        protected ReductGenerator(DataStore dataStore)
        {
            this.dataStore = dataStore;

            int[] fieldIds = dataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard);
            this.fieldGroups = new int[fieldIds.Length][];
            for (int i = 0; i < fieldIds.Length; i++)
            {
                this.fieldGroups[i] = new int[1];
                this.fieldGroups[i][0] = fieldIds[i];
            }
        }

        #endregion 

        #region Properties

        public DataStore DataStore
        {
            get { return dataStore; }
        }

        public double ApproximationLevel
        {
            get { return approximationLevel; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException("The approximation ratio value must be in range [0; 1)");

                approximationLevel = value;
            }
        }

        public int[][] FieldGroups
        {
            get { return this.fieldGroups; }
            set
            {
                this.fieldGroups = new int[value.Length][];
                for (int i = 0; i < value.Length; i++)
                {
                    this.fieldGroups[i] = new int[value[i].Length];
                    Buffer.BlockCopy(value[i], 0, this.fieldGroups[i], 0, value[i].Length * sizeof(int));
                }   
            }
        }

        #endregion

        #region Methods
        
        public abstract IReductStore Generate(Args args);
        protected abstract IReductStore CreateReductStore(Args args);
        protected abstract IReduct CreateReductObject(int[] fieldIds);

        #endregion
    }
}
