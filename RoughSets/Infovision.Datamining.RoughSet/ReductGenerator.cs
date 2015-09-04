using System;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductGenerator : IReductGenerator
    {
        #region Globals
        
        private IReductStore reductPool;
        private IPermutationGenerator permutationGenerator;
        private PermutationCollection permutationList;
        private DataStore dataStore = null;
        private double approximationLevel = 0;
        private int[][] fieldGroups;
        private int reductIdSequence;
        private bool useCache;

        protected object syncRoot = new object();

        #endregion
        
        #region Properties        

        public IReductStore ReductPool
        {
            get
            {
                return this.reductPool;
            }

            protected set
            {
                this.reductPool = value;
            }
        }

        public bool UseCache
        {
            get
            {
                return this.useCache;
            }
        }

        public PermutationCollection Permutations
        {
            get { return this.permutationList; }
        }

        public DataStore DataStore
        {
            get { return dataStore; }
        }

        public double ApproximationDegree
        {
            get { return approximationLevel; }
            set
            {
                if (value < 0.0 || value > 1.0)
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

        protected virtual IPermutationGenerator PermutationGenerator
        {
            get
            {
                if (permutationGenerator == null)
                {
                    lock (syncRoot)
                    {
                        if (permutationGenerator == null)
                        {
                            permutationGenerator = new PermutatioGeneratorFieldGroup(this.FieldGroups);
                        }
                    }
                }

                return this.permutationGenerator;
            }
        }

        #endregion

        #region Constructors        

        protected ReductGenerator()
        {
            this.SetDefaultParameters();            
        }

        #endregion 

        #region Methods               

        public abstract void Generate();        
        protected abstract IReductStore CreateReductStore();
        protected abstract IReduct CreateReductObject(int[] fieldIds, double approximationDegree, string id);

        public virtual IReductStoreCollection GetReductGroups(int numberOfEnsembles)
        {
            ReductStoreCollection reductStoreCollection = new ReductStoreCollection();
            reductStoreCollection.AddStore(this.ReductPool);
            return reductStoreCollection;
        }
        
        protected int GetNextReductId()
        {
            reductIdSequence++;
            return reductIdSequence;
        }

        public virtual void SetDefaultParameters()
        {
            this.useCache = false;
        }

        public virtual void initFromDataStore(DataStore data)
        {
            int[] fieldIds = data.DataStoreInfo.GetFieldIds(FieldTypes.Standard);
            this.fieldGroups = new int[fieldIds.Length][];
            for (int i = 0; i < fieldIds.Length; i++)
            {
                this.fieldGroups[i] = new int[1];
                this.fieldGroups[i][0] = fieldIds[i];
            }
        }

        public virtual void InitFromArgs(Args args)
        {                        
            if (args.Exist("DataStore"))
            {
                this.dataStore = (DataStore)args.GetParameter("DataStore");
                this.initFromDataStore(dataStore);
            }            
            
            if (args.Exist("PermutationCollection"))
            {
                this.permutationList = (PermutationCollection)args.GetParameter("PermutationCollection");
            }
            else if (args.Exist("NumberOfReducts"))
            {
                int numberOfReducts = (int)args.GetParameter("NumberOfReducts");
                this.permutationList = this.PermutationGenerator.Generate(numberOfReducts);
            }
            else if (args.Exist("NumberOfPermutations"))
            {
                int numberOfPermutations = (int)args.GetParameter("NumberOfPermutations");
                this.permutationList = this.PermutationGenerator.Generate(numberOfPermutations);
            }

            if (this.permutationList == null)
            {
                throw new NullReferenceException("PermutationCollection is null");
            }
            
            if (args.Exist("USECACHE"))
                this.useCache = true;
        }

        #endregion
    }
}
