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
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.TrainData);
            TestReductGenerator rGen = new TestReductGenerator(dataStore);
            rGen.InitFromArgs(args);
            return rGen;
        }

        public IPermutationGenerator GetPermutationGenerator(Args args)
        {
            DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.TrainData);
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

        public override decimal Epsilon
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

        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id)
        {
            throw new NotImplementedException();
        }

        protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id, EquivalenceClassCollection eqClasses)
        {
            throw new NotImplementedException();
        }

        protected override void Generate()
        {
            this.ReductPool = new ReductStore();            
        }

        public override IReduct CreateReduct(int[] attributes, decimal epsilon, decimal[] weights, IReductStore reductStore = null)
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
