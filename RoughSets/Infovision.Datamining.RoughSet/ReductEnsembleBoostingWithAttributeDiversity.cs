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

		public override IReduct GetNextReduct(double[] weights, int minimumLength, int maximumLength)
		{
			if (minimumLength > maximumLength)
				throw new ArgumentOutOfRangeException();

			Permutation permutation = new PermutationGeneratorEnsemble(this.DataStore, this.GetReductGroups()).Generate(1)[0];
			int maxLen = System.Math.Min(maximumLength, this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));
			int minLen = System.Math.Max(minimumLength, 0);
			int cutoff = RandomSingleton.Random.Next(minLen, maxLen + 1);

			int[] attributes = new int[cutoff];
			for (int i = 0; i < cutoff; i++)
				attributes[i] = permutation[i];

			return this.CreateReduct(attributes, this.Epsilon, weights);
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
