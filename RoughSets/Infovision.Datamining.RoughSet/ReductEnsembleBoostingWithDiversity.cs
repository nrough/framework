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
		public static string FactoryKey = ReductFactoryKeyHelper.ReductEnsembleBoostingWithDiversity;

		private IReduct[] reducts;		
		private int reductCounter;
		private bool reductsCalculated;
		private double[] weights;        
		
		public Func<double[], double[], double> Distance { get; set; }
		public Func<int[], int[], DistanceMatrix, double[][], double> Linkage { get; set; }
		public int NumberOfReductsToTest { get; set; }

		public ReductEnsembleBoostingWithDiversityGenerator()
			: base()
		{
			reductCounter = 0;
			reductsCalculated = false;
		}

		public ReductEnsembleBoostingWithDiversityGenerator(DataStore data)
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
			this.NumberOfReductsToTest = 10;
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
			base.Generate();                        
		}						

		protected override IReduct GetNextReduct(double[] weights, int minimumLength, int maximumLength)
		{            
			//TODO build instances from current reduct pool with their weights
			//TODO create m instances with current weight vector after error calculation
			//TODO create a cluster from reduct pool
			//TODO for each one element cluster created from each of m new reducts find the one that is most diverse
			//TODO return most diverse reduct

			//TODO is it possible to leverage dendrogram to find k most diverse reducts			

            IReduct[] candidates = new IReduct[this.NumberOfReductsToTest];
            for (int i = 0; i < this.NumberOfReductsToTest; i++)
                candidates[i] = base.GetNextReduct(weights, minimumLength, maximumLength);



			return base.GetNextReduct(weights, minimumLength, maximumLength);
		}

		protected override void AddModel(IReductStore model, double modelWeight)
		{
			base.AddModel(model, modelWeight);

            //TODO if reduct is selected as model add this to a cluster in order not to construct it every time we want to get new reduct
		}		
	}

	public class ReductEnsembleBoostingWithDiversityFactory : IReductFactory
	{
		public virtual string FactoryKey
		{
			get { return ReductEnsembleBoostingWithDiversityGenerator.FactoryKey; }
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

