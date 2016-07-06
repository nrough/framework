using System;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
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

        public override IReduct GetNextReduct(decimal[] weights)
        {
            Permutation permutation = new PermutationGeneratorEnsemble(this.DataStore, this.GetReductGroups()).Generate(1)[0];
            return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
        }
    }

    public class ReductEnsembleBoostingWithAttributeDiversityFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ReductEnsembleBoostingWithAttributeDiversity; }
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