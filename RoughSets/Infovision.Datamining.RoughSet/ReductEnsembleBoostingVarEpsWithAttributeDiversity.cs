using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
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

        public override IReduct GetNextReduct(decimal[] weights)
        {
            Permutation permutation = new PermutationGeneratorEnsemble(this.DataStore, this.GetReductGroups()).Generate(1)[0];
            return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
        }
    }

    public class ReductEnsembleBoostingVarEpsWithAttributeDiversityFactory : IReductFactory
    {
        public virtual string FactoryKey
        {
            get { return ReductFactoryKeyHelper.ReductEnsembleBoostingVarEpsWithAttributeDiversity; }
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
