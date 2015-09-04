using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
	public class ReductEnsembleBoostingGenerator : ReductGenerator
	{
		public static string FactoryKey = ReductFactoryKeyHelper.ReductEnsembleBoosting;
				
		private WeightBoostingGenerator weightGenerator;
		private ReductStoreCollection models;

		public int MaxReductLength { get; set; }
		public double Threshold { get; set; }
		public IdentificationType IdentyficationType { get; set;} 
		public VoteType VoteType {get; set; }
		public int NumberOfReductsInWeakClassifier { get; set; }
		public int MaxIterations { get; set; }
		
		public ReductEnsembleBoostingGenerator()
			: base()
		{
			this.MaxReductLength = 1;
			this.Threshold = 0.5;
			this.IdentyficationType = IdentificationType.WeightConfidence;
			this.VoteType = VoteType.WeightCoverage;
			this.NumberOfReductsInWeakClassifier = 1;
			this.MaxIterations = 100;
		}

		public ReductEnsembleBoostingGenerator(DataStore data)
			: this()		
		{			            
			this.weightGenerator = new WeightBoostingGenerator(data);
		}

		public override void SetDefaultParameters()
		{
			base.SetDefaultParameters();

			this.MaxReductLength = 1;
			this.Threshold = 0.5;
			this.IdentyficationType = IdentificationType.WeightConfidence;
			this.VoteType = VoteType.WeightCoverage;
			this.NumberOfReductsInWeakClassifier = 1;
			this.MaxIterations = 100;
		}

		public override void InitFromArgs(Args args)
		{
			base.InitFromArgs(args);
			
			if (args.Exist(ReductGeneratorParamHelper.MaxReductLength))
				this.MaxReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MaxReductLength);

			if (args.Exist(ReductGeneratorParamHelper.Threshold))
				this.Threshold = (double)args.GetParameter(ReductGeneratorParamHelper.Threshold);

			if (args.Exist(ReductGeneratorParamHelper.IdentificationType))
				this.IdentyficationType = (IdentificationType)args.GetParameter(ReductGeneratorParamHelper.IdentificationType);

			if (args.Exist(ReductGeneratorParamHelper.VoteType))
				this.VoteType = (VoteType)args.GetParameter(ReductGeneratorParamHelper.VoteType);

			if (args.Exist(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier))
				this.NumberOfReductsInWeakClassifier = (int)args.GetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier);

			if (args.Exist(ReductGeneratorParamHelper.MaxIterations))
				this.MaxIterations = (int)args.GetParameter(ReductGeneratorParamHelper.MaxIterations);
		}

		public override void Generate()
		{									
			this.ReductPool = this.CreateReductStore();
			this.weightGenerator = new WeightBoostingGenerator(this.DataStore);
			this.models = new ReductStoreCollection();

			double alphaSum = 0.0;
			int iter = 0;
			double error = -1.0;
			
			do
			{
				iter++;
				IReductStore localReductStore = this.CreateReductStore();
				weightGenerator.Generate();
				for (int i = 0; i < this.NumberOfReductsInWeakClassifier; i++)
				{
					IReduct reduct = this.GetNextReduct(weightGenerator.Weights, this.MaxReductLength);
					localReductStore.AddReduct(reduct);
					
					this.ReductPool.AddReduct(reduct);
				}				

				RoughClassifier classifier = new RoughClassifier();				
				classifier.ReductStore = localReductStore;

				IReductStoreCollection rsCollection = new ReductStoreCollection();
				rsCollection.AddStore(localReductStore);
				classifier.ReductStoreCollection = rsCollection;
				
				classifier.Classify(this.DataStore);
				
				ClassificationResult result = classifier.Vote(this.DataStore, this.IdentyficationType, this.VoteType);
				error = result.WeightUnclassified + result.WeightMisclassified;

				if(error >= this.Threshold)
					break;
												
				double alpha = 0.5 * System.Math.Log((1.0 - error) / error);
				localReductStore.Weight = alpha;
				this.models.AddStore(localReductStore);

				double sum = 0.0;
				for (int i = 0; i < this.DataStore.NumberOfRecords; i++ )
				{

					if (this.DataStore.GetDecisionValue(i) == result.GetResult(this.DataStore.ObjectIndex2ObjectId(i)))
					{
						//decrease weight if object is recognized correctly
						weightGenerator.Weights[i] /= 2;
					}
					else
					{
						//increase weight if object is not recognized
						weightGenerator.Weights[i] *= 2;
					}

					//weightGenerator.Weights[i] *= System.Math.Exp(-alpha);
					
					/*
					weightGenerator.Weights[i] *= System.Math.Exp(
							-alpha
							* this.DataStore.GetDecisionValue(i)
							* result.GetResult(this.DataStore.ObjectIndex2ObjectId(i))
							);
					*/

					sum += weightGenerator.Weights[i];
				}

				//Normalize object w
				for (int i = 0; i < this.DataStore.NumberOfRecords; i++ )
					weightGenerator.Weights[i] /= sum;
				
				alphaSum += alpha;

				if (error == 0.0)
					break;
				
			} while(iter <= this.MaxIterations);
						
			// Normalize w for models confidence
			foreach(IReductStore rs in this.models)
				rs.Weight /= alphaSum;
		}

		public virtual IReductStoreCollection GetReductGroups(int numberOfEnsembles = Int32.MaxValue)
		{
			return this.models;
		}

		private IReduct GetNextReduct(double[] weights, int maximumLength)
		{            			
			Permutation permutation = new PermutationGenerator(this.DataStore).Generate(1)[0];
			int length = System.Math.Min(maximumLength, permutation.Length - 1);
			int cutoff = RandomSingleton.Random.Next(0, length);
			
			int[] attributes = new int[cutoff + 1];
			for (int i = 0; i <= cutoff; i++)
				attributes[i] = permutation[i];

			ReductCrisp reduct = new ReductCrisp(this.DataStore, attributes, weights, 0);
			reduct.Id = this.GetNextReductId().ToString();						
			reduct.Reduce(attributes);
			return reduct;
		}
		
		public override IReduct CreateReduct(Permutation permutation)
		{
			throw new NotImplementedException("CreteReduct() method was not implemented.");
		}

		protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
		{
			ReductWeights r = new ReductWeights(this.DataStore, fieldIds, this.weightGenerator.Weights, epsilon);
			r.Id = id;
			return r;
		}

	}

	public class ReductEnsembleBoostingFactory : IReductFactory
	{
		public virtual string FactoryKey
		{
			get { return ReductEnsembleBoostingGenerator.FactoryKey; }
		}

		public virtual IReductGenerator GetReductGenerator(Args args)
		{
			ReductEnsembleBoostingGenerator rGen = new ReductEnsembleBoostingGenerator();
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
