using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
	public class ReductEnsembleBoostingVarEpsGenerator : ReductEnsembleBoostingGenerator
	{
		private decimal m0;
		private InformationMeasureWeights informationMeasure;		

		private InformationMeasureWeights InformationMeasure
		{
			get
			{
				if (this.informationMeasure == null)
					this.informationMeasure = new InformationMeasureWeights();
				return this.informationMeasure;
			}
		}

		public decimal M0
		{
			get { return this.m0; }
			protected set { this.m0 = value; }
		}
		
		public ReductEnsembleBoostingVarEpsGenerator()
			: base()
		{
		}

		public ReductEnsembleBoostingVarEpsGenerator(DataStore data)
			: base(data)		
		{						
		}
		
		public override IReduct GetNextReduct(decimal[] weights, int minimumLength, int maximumLength)
		{
			//this means empty reduct
			if(minimumLength == 0 && maximumLength == 0)
				return this.CreateReduct(new int[] {}, this.Epsilon, weights);
			
			Permutation permutation = new PermutationGenerator(this.DataStore).Generate(1)[0];            
			return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
		}

		public override IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights)
		{
			ReductWeights reduct = new ReductWeights(this.DataStore, new int[]{}, weights, this.Epsilon);
			reduct.Id = this.GetNextReductId().ToString();
			this.Reach(reduct, permutation, null);
			this.Reduce(reduct, permutation, null);
			return reduct;
		}

		protected virtual void Reach(IReduct reduct, int[] permutation, IReductStore reductStore)
		{
			for (int i = 0; i < permutation.Length; i++)
			{
				reduct.AddAttribute(permutation[i]);
				if (this.IsReduct(reduct, reductStore))
					return;
			}
		}

		protected virtual void Reduce(IReduct reduct, int[] permutation, IReductStore reductStore)
		{
			int len = permutation.Length - 1;
			for (int i = len; i >= 0; i--)
			{
				int attributeId = permutation[i];
				if (reduct.TryRemoveAttribute(attributeId))
					if (!this.IsReduct(reduct, reductStore))
						reduct.AddAttribute(attributeId);
			}
		}

		/// <summary>
		/// Checks if M(Bw) – M(0) >= (1-epsilon) * (M(Aw)-M(0))
		/// which is equivalent to M(Bw) >= (1 - epsilon) * M(Aw) + epsilon * M(0)
		/// </summary>
		/// <param name="reduct"></param>
		/// <returns></returns>
		public virtual bool CheckIsReduct(IReduct reduct)
		{            
			decimal mB = this.GetPartitionQuality(reduct);
			decimal mA = this.GetDataSetQuality(reduct);

			if (Decimal.Round(mB - this.m0, 17) >= Decimal.Round((Decimal.One - this.Epsilon) * (mA - m0), 17))
				return true;

			return false;
		}

		protected virtual bool IsReduct(IReduct reduct, IReductStore reductStore)
		{                        
			bool isReduct = false;
			if (reductStore != null && reductStore.IsSuperSet(reduct))
				isReduct = true;
			else
				isReduct = this.CheckIsReduct(reduct);            

			return isReduct;
		}

		protected virtual decimal GetPartitionQuality(IReduct reduct)
		{
			return this.InformationMeasure.Calc(reduct);
		}
		
		protected virtual decimal GetDataSetQuality(IReduct reduct)
		{
			ReductWeights allAttributesReduct = new ReductWeights(this.DataStore, this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), reduct.Weights, reduct.Epsilon);
			return this.GetPartitionQuality(allAttributesReduct);
		}

		public override void SetDefaultParameters()
		{
			base.SetDefaultParameters();
						
			this.Epsilon = (decimal) (0.5 * this.Threshold);
		}

		public override void InitFromArgs(Args args)
		{
			base.InitFromArgs(args);
			
			IReduct emptyReduct = this.CreateReduct(new int[] { }, this.Epsilon, WeightGenerator.Weights);
			this.M0 = this.GetPartitionQuality(emptyReduct);

			if (!args.Exist(ReductGeneratorParamHelper.Epsilon))
			{
				int K = this.DataStore.DataStoreInfo.NumberOfDecisionValues;
				this.Epsilon = (Decimal.One / (decimal)K) * (decimal)this.Threshold;
			}
		}
	}

	public class ReductEnsembleBoostingVarEpsFactory : IReductFactory
	{
		public virtual string FactoryKey
		{
			get { return ReductFactoryKeyHelper.ReductEnsembleBoostingVarEps; }
		}

		public virtual IReductGenerator GetReductGenerator(Args args)
		{
			ReductEnsembleBoostingVarEpsGenerator rGen = new ReductEnsembleBoostingVarEpsGenerator();
			rGen.InitFromArgs(args);
			return rGen;
		}

		public virtual IPermutationGenerator GetPermutationGenerator(Args args)
		{
			throw new NotImplementedException("GetPermutationGenerator(Args args) method is not implemented");
		}
	}
}
