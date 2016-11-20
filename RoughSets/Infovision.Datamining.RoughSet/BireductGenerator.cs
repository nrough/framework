﻿using System;
using System.Linq;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    [Serializable]
    public class BireductGenerator : ReductGenerator
    {
        #region Properties

        protected override IPermutationGenerator PermutationGenerator
        {
            get { return new PermutationGeneratorFieldObject(this.DataStore, this.Epsilon); }
        }

        #endregion Properties

        #region Constructors

        public BireductGenerator()
            : base()
        {
        }

        #endregion Constructors

        #region Methods

        protected override IReductStore CreateReductStore(int initialSize = 0)
        {
            return new BireductStore(initialSize);
        }

        protected override void Generate()
        {
            IReductStore reductStore = this.CreateReductStore();

            foreach (Permutation permutation in this.Permutations)
            {
                Bireduct bireduct = (Bireduct)this.CalculateReduct(permutation.ToArray(), reductStore);
                reductStore.AddReduct(bireduct);
            }

            this.ReductPool = reductStore;
        }

        public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            IReductStore localReductStore = this.CreateReductStore();
            return this.CalculateReduct(permutation, localReductStore);
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            Bireduct r = new Bireduct(this.DataStore, fieldIds, epsilon);
            r.Id = id;
            return r;
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection equivalenceClasses)
        {
            return this.CreateReductObject(fieldIds, epsilon, id);
        }

        //protected virtual IReduct CalculateReduct(Permutation permutation, IReductStore reductStore)
        protected virtual IReduct CalculateReduct(int[] permutation, IReductStore reductStore)
        {
            Bireduct bireduct = this.CreateReductObject(this.DataStore.GetStandardFields(),
                                                        this.Epsilon,
                                                        this.GetNextReductId().ToString()) as Bireduct;

            this.Reach(bireduct, permutation, reductStore);
            return bireduct;
        }

        protected virtual void Reach(Bireduct bireduct, int[] permutation, IReductStore reductStore)
        {
            for (int i = 0; i < permutation.Length; i++)
                if (permutation[i] < 0)
                    bireduct.TryRemoveAttribute(-permutation[i]);
                else
                    bireduct.TryAddObject(permutation[i]);
        }

        #endregion Methods
    }

    public class BireductFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductFactoryKeyHelper.Bireduct; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            BireductGenerator rGen = new BireductGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.TrainData);

            if (args.Exist(ReductGeneratorParamHelper.Epsilon))
                return new PermutationGeneratorFieldObject(dataStore, args.GetParameter<double>(ReductGeneratorParamHelper.Epsilon));

            return new PermutationGeneratorFieldObject(dataStore);
        }
    }

    [Serializable]
    public class BireductRelativeGenerator : BireductGenerator
    {
        #region Constructors

        public BireductRelativeGenerator()
            : base()
        {
        }

        #endregion Constructors

        #region Properties

        protected override IPermutationGenerator PermutationGenerator
        {
            get { return new PermutationGeneratorFieldObjectRelative(this.DataStore, this.Epsilon); }
        }

        #endregion Properties
    }

    public class BireductRelativeFactory : BireductFactory
    {
        public override string FactoryKey
        {
            get { return ReductFactoryKeyHelper.BireductRelative; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            BireductRelativeGenerator rGen = new BireductRelativeGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public override IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.TrainData);

            if (args.Exist(ReductGeneratorParamHelper.Epsilon))
                return new PermutationGeneratorFieldObjectRelative(dataStore, (double)args.GetParameter(ReductGeneratorParamHelper.Epsilon));

            return new PermutationGeneratorFieldObjectRelative(dataStore);
        }
    }

    [Serializable]
    public class BireductGammaGenerator : BireductGenerator
    {
        #region Constructors

        public BireductGammaGenerator()
            : base()
        {
        }

        #endregion Constructors

        #region Methods

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            BireductGamma r = new BireductGamma(this.DataStore, epsilon);
            r.Id = id;
            return r;
        }

        protected override IReduct CalculateReduct(int[] permutation, IReductStore reductStore)
        {
            BireductGamma bireduct = this.CreateReductObject(this.DataStore.GetStandardFields(),
                                                             this.Epsilon,
                                                             this.GetNextReductId().ToString()) as BireductGamma;
            Reach(bireduct, permutation, reductStore);
            return bireduct;
        }

        #endregion Methods
    }

    public class BireductGammaFactory : BireductFactory
    {
        public override string FactoryKey
        {
            get { return ReductFactoryKeyHelper.GammaBireduct; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            BireductGammaGenerator rGen = new BireductGammaGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }
}