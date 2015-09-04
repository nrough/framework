using System;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductGenerator : IReductGenerator
    {
        #region Members
        
        private IReductStore reductPool;
        private IPermutationGenerator permutationGenerator;
        private PermutationCollection permutationList;
        private DataStore dataStore;
        private double epsilon;
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

        public double Epsilon
        {
            get { return epsilon; }
            set
            {
                if (value < 0.0)
                    epsilon = 0.0;
                else if (value > 1.0)
                    epsilon = 1.0;                    
                else
                    epsilon = value;
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

        public int NextReductId
        {
            get { return this.reductIdSequence + 1; }
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
        protected abstract IReduct CreateReductObject(int[] fieldIds, double epsilon, string id);
        public abstract IReduct CreateReduct(int[] permutation, double epsilon, double[] weights);

        protected virtual IReductStore CreateReductStore()
        {
            return new ReductStore();
        }

        public virtual IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles = Int32.MaxValue)
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
            this.epsilon = 0.0;
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
            if (args.Exist(ReductGeneratorParamHelper.DataStore))
            {
                this.dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.DataStore);
                this.initFromDataStore(dataStore);
            }            
            
            if (args.Exist(ReductGeneratorParamHelper.PermutationCollection))
            {
                this.permutationList = (PermutationCollection)args.GetParameter(ReductGeneratorParamHelper.PermutationCollection);
            }
            else if (args.Exist(ReductGeneratorParamHelper.NumberOfReducts))
            {
                int numberOfReducts = (int)args.GetParameter(ReductGeneratorParamHelper.NumberOfReducts);
                this.permutationList = this.PermutationGenerator.Generate(numberOfReducts);
            }
            else if (args.Exist(ReductGeneratorParamHelper.NumberOfPermutations))
            {
                int numberOfPermutations = (int)args.GetParameter(ReductGeneratorParamHelper.NumberOfPermutations);
                this.permutationList = this.PermutationGenerator.Generate(numberOfPermutations);
            }            
            
            if (args.Exist("USECACHE"))
                this.useCache = true;
        }

        #endregion
    }
}
