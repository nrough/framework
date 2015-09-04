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
	public class ReductEnsembleBoostingWithDiversityGenerator : ReductEnsembleBoostingGenerator
	{						
		private Dictionary<string, int> clusterInstances;
		private List<double[]> clusterInstances2;
		
		public Func<double[], double[], double> Distance { get; set; }
		public Func<int[], int[], DistanceMatrix, double[][], double> Linkage { get; set; }
		public Func<IReduct, double[], double[]> ReconWeights { get; set; }
		public int NumberOfReductsToTest { get; set; }
		public AgregateFunction AgregateFunction { get; set; }

		public ReductEnsembleBoostingWithDiversityGenerator()
			: base()
		{			
			clusterInstances = new Dictionary<string, int>();
			clusterInstances2 = new List<double[]>();
		}

		public ReductEnsembleBoostingWithDiversityGenerator(DataStore data)
			: base(data)		
		{				
			clusterInstances = new Dictionary<string, int>();
			clusterInstances2 = new List<double[]>();
		}

		public override void SetDefaultParameters()
		{
			base.SetDefaultParameters();

			this.Distance = Similarity.Manhattan;
			this.Linkage = ClusteringLinkage.Average;
			this.ReconWeights = ReductEnsembleReconWeightsHelper.GetErrorReconWeights;
			this.NumberOfReductsToTest = 10;
			this.AgregateFunction = AgregateFunction.Max;
		}

		public override void InitFromArgs(Args args)
		{
			base.InitFromArgs(args);

			if (args.Exist(ReductGeneratorParamHelper.Distance))
				this.Distance = (Func<double[], double[], double>)args.GetParameter(ReductGeneratorParamHelper.Distance);

			if (args.Exist(ReductGeneratorParamHelper.Linkage))
				this.Linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args.GetParameter(ReductGeneratorParamHelper.Linkage);

			if (args.Exist(ReductGeneratorParamHelper.ReconWeights))
				this.ReconWeights = (Func<IReduct, double[], double[]>)args.GetParameter(ReductGeneratorParamHelper.ReconWeights);

			if (args.Exist(ReductGeneratorParamHelper.NumberOfReductsToTest))
				this.NumberOfReductsToTest = (int)args.GetParameter(ReductGeneratorParamHelper.NumberOfReductsToTest);

			if (args.Exist(ReductGeneratorParamHelper.AgregateFunction))
				this.AgregateFunction = (AgregateFunction)args.GetParameter(ReductGeneratorParamHelper.AgregateFunction);
		}

		private IReduct GetNextReductLocal(double[] weights, int minimumLength, int maximumLength)
		{
			Permutation permutation = new PermutationGeneratorEnsemble(this.DataStore, this.GetReductGroups()).Generate(1)[0];
			int length = System.Math.Min(maximumLength, permutation.Length - 1);
			int minLen = System.Math.Max(minimumLength - 1, 0);
			int cutoff = RandomSingleton.Random.Next(minLen, length);

			int[] attributes = new int[cutoff + 1];
			for (int i = 0; i <= cutoff; i++)
				attributes[i] = permutation[i];

			ReductGeneralizedMajorityDecision reduct = new ReductGeneralizedMajorityDecision(this.DataStore, attributes, weights, 0);
			reduct.Id = this.GetNextReductId().ToString();
			reduct.Reduce(attributes, this.MinReductLength);

			if (reduct.Attributes.Count < minimumLength)
				throw new InvalidProgramException("Reduct length is less than minimum length");

			return reduct;
		}
		
		public override IReduct GetNextReduct(double[] weights, int minimumLength, int maximumLength)
		{
			if (clusterInstances2.Count == 0 || this.NumberOfReductsToTest == 1)
				return base.GetNextReduct(weights, minimumLength, maximumLength);
			
			DistanceMatrix distanceMatrix = new DistanceMatrix(this.Distance);

			int[] cluster1 = new int[this.clusterInstances2.Count];
			for (int i = 0; i < this.clusterInstances2.Count; i++)
			{
				cluster1[i] = i;
				for (int j = i + 1; j < this.clusterInstances2.Count; j++)
				{
					distanceMatrix[i, j] = this.Distance(this.clusterInstances2[i], this.clusterInstances2[j]);
				}
			}

			double[][] mergedInstances = this.clusterInstances2.ToArray();
			Array.Resize<double[]>(ref mergedInstances, mergedInstances.Length + this.NumberOfReductsToTest);

			IReduct[] candidates = new IReduct[this.NumberOfReductsToTest];
			double distMax = Double.MinValue;
			double distMin = Double.MaxValue;
			int maxIndex = -1;
			int minIndex = -1;
			int[] oneElementcluster = new int[1];
									
			for (int i = 0; i < this.NumberOfReductsToTest; i++)
			{                                                
				candidates[i] = base.GetNextReduct(weights, minimumLength, maximumLength);
				double[] condidateVector = this.ReconWeights(candidates[i], weights);

				for (int j = 0; j < this.clusterInstances2.Count; j++)
					distanceMatrix[j, this.clusterInstances2.Count + i] = this.Distance(this.clusterInstances2[j], condidateVector);

				mergedInstances[this.clusterInstances2.Count + i] = condidateVector;
				oneElementcluster[0] = this.clusterInstances2.Count + i;

				double clusterDistance = this.Linkage(this.clusterInstances.Values.ToArray(), 
													  oneElementcluster, 
													  distanceMatrix,
													  mergedInstances);
				if (distMax < clusterDistance)
				{
					distMax = clusterDistance;
					maxIndex = i;
				}

				if (distMin > clusterDistance)
				{
					distMin = clusterDistance;
					minIndex = i;
				}
			}

			switch (this.AgregateFunction)
			{
				case AgregateFunction.Min:
					return candidates[minIndex];

				case AgregateFunction.Max:
					return candidates[maxIndex];
			}
			
			return candidates[maxIndex];
		}

		protected override void AddModel(IReductStore model, double modelWeight)
		{
			base.AddModel(model, modelWeight);

			foreach (IReduct r in model)
			{
				clusterInstances.Add(r.Id, clusterInstances2.Count);
				clusterInstances2.Add(this.ReconWeights(r, r.Weights));
			}
		}
	}

	public class ReductEnsembleBoostingWithDiversityFactory : IReductFactory
	{
		public virtual string FactoryKey
		{
			get { return ReductFactoryKeyHelper.ReductEnsembleBoostingWithDiversity; }
		}

		public virtual IReductGenerator GetReductGenerator(Args args)
		{
			ReductEnsembleBoostingWithDiversityGenerator rGen = new ReductEnsembleBoostingWithDiversityGenerator();
			rGen.InitFromArgs(args);
			return rGen;
		}

		public virtual IPermutationGenerator GetPermutationGenerator(Args args)
		{
			DataStore dataStore = (DataStore)args.GetParameter(ReductGeneratorParamHelper.DataStore);
			return new PermutationGenerator(dataStore);
		}
	}
}

