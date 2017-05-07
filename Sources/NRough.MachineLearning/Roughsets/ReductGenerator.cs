﻿using System;
using System.Diagnostics;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NRough.MachineLearning.Permutations;

namespace NRough.MachineLearning.Roughsets
{
    [Serializable]
    public abstract class ReductGenerator : IReductGenerator
    {
        #region Members

        private IPermutationGenerator permutationGenerator;
        private int[][] fieldGroups;
        private int reductIdSequence;

        [NonSerialized]
        protected readonly Stopwatch timer = new Stopwatch();

        protected readonly object mutex = new object();

        #endregion Members

        #region Properties

        public Args Parameters { get; set; }
        public Args InnerParameters { get; set; }

        public virtual IReductStore ReductPool { get; protected set; }
        public virtual bool UseCache { get; private set; }
        public virtual PermutationCollection Permutations { get; set; }
        public virtual DataStore DecisionTable { get; set; }
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

        protected bool IsComputed { get; set; }

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

            IsComputed = true;
        }

        protected virtual IReductStore CreateReductStore(int initialSize = 0)
        {
            return initialSize != 0 ? new ReductStore(initialSize) : new ReductStore();
        }

        public virtual IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles = Int32.MaxValue)
        {
            if (!IsComputed)
                this.Run();

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

            if (args.Exist(ReductFactoryOptions.DecisionTable))
            {
                this.DecisionTable = (DataStore)args.GetParameter(ReductFactoryOptions.DecisionTable);
                this.initFromDataStore(this.DecisionTable);
            }

            if (args.Exist(ReductFactoryOptions.PermuatationGenerator))
            {
                this.PermutationGenerator = (IPermutationGenerator)args.GetParameter(ReductFactoryOptions.PermuatationGenerator);
            }
            else
            {
                if (this.DecisionTable != null)
                {
                    this.PermutationGenerator = new PermutationGenerator(this.DecisionTable);
                }
            }

            if (args.Exist(ReductFactoryOptions.PermutationCollection))
            {
                this.Permutations = (PermutationCollection)args.GetParameter(ReductFactoryOptions.PermutationCollection);
            }
            else if (args.Exist(ReductFactoryOptions.NumberOfReducts))
            {
                int numberOfReducts = (int)args.GetParameter(ReductFactoryOptions.NumberOfReducts);
                this.Permutations = this.PermutationGenerator.Generate(numberOfReducts);
            }
            else if (args.Exist(ReductFactoryOptions.NumberOfPermutations))
            {
                int numberOfPermutations = (int)args.GetParameter(ReductFactoryOptions.NumberOfPermutations);
                this.Permutations = this.PermutationGenerator.Generate(numberOfPermutations);
            }
            else
            {
                this.Permutations = this.PermutationGenerator.Generate(1);
            }

            if (args.Exist("USECACHE"))
                this.UseCache = true;

            if (args.Exist(ReductFactoryOptions.Epsilon))
                this.Epsilon = (double)args.GetParameter(ReductFactoryOptions.Epsilon);

            if (args.Exist(ReductFactoryOptions.ReductionStep))
                this.ReductionStep = (int)args.GetParameter(ReductFactoryOptions.ReductionStep);

            if (args.Exist(ReductFactoryOptions.InnerParameters))
                this.InnerParameters = (Args)args.GetParameter(ReductFactoryOptions.InnerParameters);

            //TODO FieldGroups
        }

        #endregion Methods
    }
}