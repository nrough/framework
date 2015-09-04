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
            return (IReductGenerator)new TestReductGenerator(dataStore);
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
        private double approximationLevel = 0;
        
        public TestReductGenerator()
        {
        }

        public TestReductGenerator(DataStore dataStore)
        {
        }

        public double ApproximationDegree
        {
            get { return this.approximationLevel; }
            set { this.approximationLevel = value; }
        }

        public IReductStoreCollection Generate(Args args)
        {
            IReductStoreCollection reductStoreCollection = new ReductStoreCollection();
            reductStoreCollection.AddStore(new ReductStore());
            return reductStoreCollection;            
        }
    }
}
