using System;
using System.Diagnostics;
using System.Linq;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public abstract class ReductGenerator : IReductGenerator
    {
        #region Members

        private IPermutationGenerator permutationGenerator;
        private int[][] fieldGroups;
        private int reductIdSequence;

        protected readonly Stopwatch timer = new Stopwatch();
        protected readonly object mutex = new object();

        #endregion Members

        #region Properties

        public Args Parameters { get; set; }
        public Args InnerParameters { get; set; }

        public virtual IReductStore ReductPool { get; protected set; }
        public virtual bool UseCache { get; private set; }
        public virtual PermutationCollection Permutations { get; protected set; }
        public virtual DataStore DataStore { get; private set; }
        public virtual double Epsilon { get; set; }
        public virtual int ReductionStep { get; set; }

        public virtual int[][] FieldGroups
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
                    lock (mutex)
                    {
                        if (permutationGenerator == null)
                        {
                            permutationGenerator = new PermutatioGeneratorFieldGroup(this.FieldGroups);
                        }
                    }
                }

                return this.permutationGenerator;
            }

            set
            {
                lock (mutex)
                {
                    this.permutationGenerator = value;
                }
            }
        }

        public virtual int NextReductId
        {
            get { return this.reductIdSequence + 1; }
        }

        public virtual long ReductGenerationTime
        {
            get { return timer.ElapsedMilliseconds; }
        }

        #endregion Properties

        #region Constructors

        protected ReductGenerator()
        {
            this.InitDefaultParameters();
        }

        #endregion Constructors

        #region Methods

        protected abstract void Generate();

        protected abstract IReduct CreateReductObject(int[] fieldIds, double epsilon, string id);

        protected abstract IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection eqClasses);

        public abstract IReduct CreateReduct(int[] permutation, double epsilon, double[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null);

        public virtual void Run()
        {
            timer.Reset();
            timer.Start();
            this.Generate();
            timer.Stop();
        }

        protected virtual IReductStore CreateReductStore(int initialSize = 0)
        {
            return initialSize != 0 ? new ReductStore(initialSize) : new ReductStore();
        }

        public virtual IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles = Int32.MaxValue)
        {
            ReductStoreCollection reductStoreCollection = new ReductStoreCollection(1);
            reductStoreCollection.AddStore(this.ReductPool);
            return reductStoreCollection;
        }

        protected int GetNextReductId()
        {
            lock (mutex)
            {
                reductIdSequence++;
                return reductIdSequence;
            }
        }

        public virtual void InitDefaultParameters()
        {
            this.UseCache = false;
            this.Epsilon = 0;
            this.ReductionStep = 1;
        }

        public virtual void initFromDataStore(DataStore data)
        {
            int[] fieldIds = data.GetStandardFields();
            this.fieldGroups = new int[fieldIds.Length][];
            for (int i = 0; i < fieldIds.Length; i++)
            {
                this.fieldGroups[i] = new int[1];
                this.fieldGroups[i][0] = fieldIds[i];
            }
        }

        public virtual void InitFromArgs(Args args)
        {
            this.Parameters = args;

            if (args.Exist(ReductGeneratorParamHelper.TrainData))
            {
                this.DataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.TrainData);
                this.initFromDataStore(this.DataStore);
            }

            if (args.Exist(ReductGeneratorParamHelper.PermuatationGenerator))
            {
                this.PermutationGenerator = (IPermutationGenerator)args.GetParameter(ReductGeneratorParamHelper.PermuatationGenerator);
            }
            else
            {
                if (this.DataStore != null)
                {
                    this.PermutationGenerator = new PermutationGenerator(this.DataStore);
                }
            }

            if (args.Exist(ReductGeneratorParamHelper.PermutationCollection))
            {
                this.Permutations = (PermutationCollection)args.GetParameter(ReductGeneratorParamHelper.PermutationCollection);
            }
            else if (args.Exist(ReductGeneratorParamHelper.NumberOfReducts))
            {
                int numberOfReducts = (int)args.GetParameter(ReductGeneratorParamHelper.NumberOfReducts);
                this.Permutations = this.PermutationGenerator.Generate(numberOfReducts);
            }
            else if (args.Exist(ReductGeneratorParamHelper.NumberOfPermutations))
            {
                int numberOfPermutations = (int)args.GetParameter(ReductGeneratorParamHelper.NumberOfPermutations);
                this.Permutations = this.PermutationGenerator.Generate(numberOfPermutations);
            }
            else
            {
                this.Permutations = this.PermutationGenerator.Generate(1);
            }

            if (args.Exist("USECACHE"))
                this.UseCache = true;

            if (args.Exist(ReductGeneratorParamHelper.Epsilon))
                this.Epsilon = (double)args.GetParameter(ReductGeneratorParamHelper.Epsilon);

            if (args.Exist(ReductGeneratorParamHelper.ReductionStep))
                this.ReductionStep = (int)args.GetParameter(ReductGeneratorParamHelper.ReductionStep);

            if (args.Exist(ReductGeneratorParamHelper.InnerParameters))
                this.InnerParameters = (Args)args.GetParameter(ReductGeneratorParamHelper.InnerParameters);

            //TODO FieldGroups
        }

        #endregion Methods
    }
}