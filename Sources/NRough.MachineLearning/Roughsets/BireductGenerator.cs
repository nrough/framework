//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;
using System.Linq;
using NRough.Data;
using NRough.Core;
using NRough.MachineLearning.Permutations;

namespace NRough.MachineLearning.Roughsets
{
    [Serializable]
    public class BireductGenerator : ReductGenerator
    {
        #region Properties

        protected override IPermutationGenerator PermutationGenerator
        {
            get { return new PermutationAttributeObjectGenerator(this.DecisionTable, this.Epsilon); }
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
            Bireduct r = new Bireduct(this.DecisionTable, fieldIds, epsilon);
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
            Bireduct bireduct = this.CreateReductObject(this.DecisionTable.GetStandardFields(),
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
            get { return ReductTypes.Bireduct; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            BireductGenerator rGen = new BireductGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductFactoryOptions.DecisionTable);

            if (args.Exist(ReductFactoryOptions.Epsilon))
                return new PermutationAttributeObjectGenerator(dataStore, args.GetParameter<double>(ReductFactoryOptions.Epsilon));

            return new PermutationAttributeObjectGenerator(dataStore);
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
            get { return new PermutationGeneratorFieldObjectRelative(this.DecisionTable, this.Epsilon); }
        }

        #endregion Properties
    }

    public class BireductRelativeFactory : BireductFactory
    {
        public override string FactoryKey
        {
            get { return ReductTypes.BireductRelative; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            BireductRelativeGenerator rGen = new BireductRelativeGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public override IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductFactoryOptions.DecisionTable);

            if (args.Exist(ReductFactoryOptions.Epsilon))
                return new PermutationGeneratorFieldObjectRelative(dataStore, (double)args.GetParameter(ReductFactoryOptions.Epsilon));

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
            BireductGamma r = new BireductGamma(this.DecisionTable, epsilon);
            r.Id = id;
            return r;
        }

        protected override IReduct CalculateReduct(int[] permutation, IReductStore reductStore)
        {
            BireductGamma bireduct = this.CreateReductObject(this.DecisionTable.GetStandardFields(),
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
            get { return ReductTypes.GammaBireduct; }
        }

        public override IReductGenerator GetReductGenerator(Args args)
        {
            BireductGammaGenerator rGen = new BireductGammaGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }
    }
}