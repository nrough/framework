﻿using System;

using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

namespace Infovision.TestAssembly
{
    public class TestReductFactory : Infovision.Datamining.Roughset.IReductFactory
    {
        public string FactoryKey
        {
            get { return "TestReduct"; }
        }

        public IReductGenerator GetReductGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            TestReductGenerator rGen = new TestReductGenerator(dataStore);
            rGen.InitFromArgs(args);
            return rGen;
        }

        public IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter("DataStore");
            return new PermutationGeneratorReverse(dataStore);
        }

        public IReductStore GetReductStore(Args args)
        {
            return new ReductStore();
        }
    }

    public class TestReductGenerator : Infovision.Datamining.Roughset.IReductGenerator
    {        
        private IReductStore reductPool;

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

        public double Epsilon
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

        public virtual void InitFromArgs(Args args)
        {

        }

        public virtual void Generate()
        {
            this.ReductPool = new ReductStore();            
        }

        public virtual IReduct CreateReduct(Permutation permutation)
        {
            throw new NotImplementedException("CreteReduct() method was not implemented.");
        }

        public virtual IReductStoreCollection GetReductGroups(int numberOfEnsembles)
        {
            ReductStoreCollection reductStoreCollection = new ReductStoreCollection();
            reductStoreCollection.AddStore(this.ReductPool);
            return reductStoreCollection;
        }
    }
}
