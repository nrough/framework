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
	public class ReductEnsembleBoostingWithDendrogramGenerator : ReductEnsembleBoostingGenerator
	{		
		private IReduct[] reducts;		
		private int reductCounter;
		private bool reductsCalculated;
		private double[] weights;
		private HierarchicalClustering hCluster;
		
		public Func<double[], double[], double> Distance { get; set; }
		public Func<int[], int[], DistanceMatrix, double[][], double> Linkage { get; set; }

		public ReductEnsembleBoostingWithDendrogramGenerator()
			: base()
		{
			reductCounter = 0;
			reductsCalculated = false;
		}

		public ReductEnsembleBoostingWithDendrogramGenerator(DataStore data)
			: base(data)		
		{
			reductCounter = 0;
			reductsCalculated = false;
		}

		public override void SetDefaultParameters()
		{
			base.SetDefaultParameters();
			this.Distance = Similarity.Manhattan;
			this.Linkage = ClusteringLinkage.Average;
		}

		public override void InitFromArgs(Args args)
		{
			base.InitFromArgs(args);

			if (args.Exist("Distance"))
				this.Distance = (Func<double[], double[], double>)args.GetParameter("Distance");

			if (args.Exist("Linkage"))
				this.Linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args.GetParameter("Linkage");
		}

		public override void Generate()
		{
			this.GenerateReducts();
			this.BuildDendrogram();			
						
			base.Generate();                        
		}

		private void BuildDendrogram()
		{
			hCluster = new HierarchicalClustering();
			hCluster.Distance = this.Distance;
			hCluster.Linkage = this.Linkage;
			
			Dictionary<int, double[]> instances = new Dictionary<int,double[]>(this.MaxIterations);
			for (int i = 0; i < reducts.Length; i++)
				instances.Add(i, ReductEnsembleReconWeightsHelper.GetDefaultReconWeights(reducts[i], weights));

			hCluster.Instances = instances;
			hCluster.Compute();            
		}
		
		private void GenerateReducts()
		{            
			reductsCalculated = false;

			weights = new double[this.DataStore.NumberOfRecords];
			for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
				weights[i] = 1.0 / this.DataStore.NumberOfRecords;

			this.reducts = new IReduct[this.MaxIterations];				
			for (int i = 0; i < this.MaxIterations; i++)
				this.reducts[i] = this.GetNextReduct(weights, this.MinReductLength, this.MaxReductLength);								

			HierarchicalCluster cluster = new HierarchicalCluster(0);
			cluster.AddMemberObject(0);
			int[] oneElementCluster = new int[1];

			for (int i = 1; i < this.MaxIterations; i++)
			{
				double d = Double.MinValue;
				int bestIndex = -1;

				for (int j = i + 1; j < this.MaxIterations; j++)
				{                                        
					oneElementCluster[0] = j;
					
					double reductDistance = hCluster.GetClusterDistance(cluster.MemberObjects.ToArray(), oneElementCluster);
					if (d < reductDistance)
					{
						d = reductDistance;
						bestIndex = j;
					}
				}

				cluster.AddMemberObject(bestIndex);

				IReduct tmp = reducts[i];
				reducts[i] = reducts[bestIndex];
				reducts[bestIndex] = tmp;                
			}

			reductCounter = 0;
			reductsCalculated = true;
		}

		protected override IReduct GetNextReduct(double[] weights, int minimumLength, int maximumLength)
		{
			if (reductsCalculated)								
				return reducts[reductCounter++];
			  
			return base.GetNextReduct(weights, minimumLength, maximumLength);
		}						
	}

	public class ReductEnsembleBoostingWithDendrogramFactory : IReductFactory
	{
		public virtual string FactoryKey
		{
			get { return ReductFactoryKeyHelper.ReductEnsembleBoostingWithDendrogram; }
		}

		public virtual IReductGenerator GetReductGenerator(Args args)
		{
			ReductEnsembleBoostingWithDendrogramGenerator rGen = new ReductEnsembleBoostingWithDendrogramGenerator();
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
