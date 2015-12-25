using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Math;

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

		public override void SetDefaultParameters()
		{
			base.SetDefaultParameters();			
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
