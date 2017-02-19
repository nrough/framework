using System;
using Raccoon.Data;
using Raccoon.Core;
using Raccoon.MachineLearning.Permutations;

namespace Raccoon.MachineLearning.Roughset
{
    public class ReductEnsembleBoostingVarEpsWithAttributeDiversityGenerator : ReductEnsembleBoostingVarEpsGenerator
    {
        public ReductEnsembleBoostingVarEpsWithAttributeDiversityGenerator()
            : base()
        {
        }

        public ReductEnsembleBoostingVarEpsWithAttributeDiversityGenerator(DataStore data)
            : base(data)
        {
        }

        public override IReduct GetNextReduct(double[] weights)
        {
            Permutation permutation = new PermutationGeneratorEnsemble(this.DecisionTable, this.GetReductGroups()).Generate(1)[0];
            return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
        }
    }

    public class ReductEnsembleBoostingVarEpsWithAttributeDiversityFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductTypes.ReductEnsembleBoostingVarEpsWithAttributeDiversity; }
        }

        public virtual IReductGenerator GetReductGenerator(Args args)
        {
            ReductEnsembleBoostingVarEpsWithAttributeDiversityGenerator rGen = new ReductEnsembleBoostingVarEpsWithAttributeDiversityGenerator();
            rGen.InitFromArgs(args);
            return rGen;
        }

        public virtual IPermutationGenerator GetPermutationGenerator(Args args)
        {
            throw new NotImplementedException("GetPermutationGenerator(Args args) method is not implemented");
        }
    }
}