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