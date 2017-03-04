using System;
using NRough.Data;
using NRough.Core;
using NRough.MachineLearning.Permutations;
using NRough.Doc;

namespace NRough.MachineLearning.Roughsets
{
    [AssemblyTreeVisible(false)]
    public class ReductEnsembleBoostingWithAttributeDiversityGenerator : ReductEnsembleBoostingGenerator
    {
        public ReductEnsembleBoostingWithAttributeDiversityGenerator()
            : base()
        {
        }

        public ReductEnsembleBoostingWithAttributeDiversityGenerator(DataStore data)
            : base(data)
        {
        }

        public override void InitDefaultParameters()
        {
            base.InitDefaultParameters();
        }

        public override void InitFromArgs(Args args)
        {
            base.InitFromArgs(args);
        }

        public override IReduct GetNextReduct(double[] weights)
        {
            Permutation permutation = new PermutationGeneratorEnsemble(this.DecisionTable, this.GetReductGroups()).Generate(1)[0];
            return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
        }
    }

    [AssemblyTreeVisible(false)]
    public class ReductEnsembleBoostingWithAttributeDiversityFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductTypes.ReductEnsembleBoostingWithAttributeDiversity; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductEnsembleBoostingWithAttributeDiversityGenerator rGen = new ReductEnsembleBoostingWithAttributeDiversityGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            throw new NotImplementedException("GetPermutationGenerator(Args args) method is not implemented");
        }
    }
}