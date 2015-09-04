using System;

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
        private IReductStoreCollection reductStoreCollection;
        private IReductStore reductPool;
        private double approximationLevel = 0;

        public IReductStoreCollection ReductStoreCollection
        {
            get
            {
                return this.reductStoreCollection;
            }

            protected set
            {
                this.reductStoreCollection = value;
            }
        }

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

        public double ApproximationDegree
        {
            get { return this.approximationLevel; }
            set { this.approximationLevel = value; }
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
            this.ReductStoreCollection = new ReductStoreCollection();
            this.ReductStoreCollection.AddStore(this.ReductPool);
        }
    }
}
