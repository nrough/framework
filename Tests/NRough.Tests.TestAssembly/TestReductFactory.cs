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
using NRough.Data;
using NRough.MachineLearning.Roughsets;
using NRough.Core;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning;

namespace NRough.Tests.TestAssembly
{
    public class TestReductFactory : NRough.MachineLearning.Roughsets.IReductFactory
    {
        public string FactoryKey
        {
            get { return "TestReduct"; }
        }

        public IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductFactoryOptions.DecisionTable);
            TestReductGenerator rGen = new TestReductGenerator(dataStore);
            rGen.InitFromArgs(args);
            return rGen;
        }

        public IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductFactoryOptions.DecisionTable);
            return new PermutationGeneratorReverse(dataStore);
        }

        public IReductStore GetReductStore(Args args)
        {
            return new ReductStore();
        }
    }

    public class TestReductGenerator : ReductGenerator
    {
        private IReductStore reductPool;

        public override IReductStore ReductPool
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

        public override double Epsilon
        {
            get;
            set;
        }

        public TestReductGenerator()
        {
        }

        public TestReductGenerator(DataStore dataStore)
        {
        }

        public override void InitFromArgs(Args args)
        {
        }

        public override void Run()
        {
            this.Generate();
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
        {
            throw new NotImplementedException();
        }

        protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id, EquivalenceClassCollection eqClasses)
        {
            throw new NotImplementedException();
        }

        protected override void Generate()
        {
            this.ReductPool = new ReductStore();
        }

        public override IReduct CreateReduct(int[] attributes, double epsilon, double[] weights, IReductStore reductStore = null, IReductStoreCollection reductStoreCollection = null)
        {
            throw new NotImplementedException("CreteReduct() method was not implemented.");
        }

        public override IReductStoreCollection GetReductStoreCollection(int numberOfEnsembles)
        {
            ReductStoreCollection reductStoreCollection = new ReductStoreCollection(1);
            reductStoreCollection.AddStore(this.ReductPool);
            return reductStoreCollection;
        }
    }
}