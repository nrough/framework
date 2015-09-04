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
		public int MinReductLength { get; set; }
		public double Threshold { get; set; }
		public IdentificationType IdentyficationType { get; set;} 
		public VoteType VoteType {get; set; }
		public int NumberOfReductsInWeakClassifier { get; set; }
		public int MaxIterations { get; set; }
		
		public ReductEnsembleBoostingGenerator()
			: base()
		{
			this.MinReductLength = 1;
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

			this.MinReductLength = 1;
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

			if (args.Exist(ReductGeneratorParamHelper.MinReductLength))
				this.MaxReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MinReductLength);

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
            double error2 = -1.0;

			/*
			IReductStore localReductStore = this.CreateReductStore();
			weightGenerator.Generate();
			for (int i = 0; i < this.NumberOfReductsInWeakClassifier; i++)
			{
				IReduct reduct = this.GetNextReduct(weightGenerator.Weights, this.MinReductLength, this.MaxReductLength);
				localReductStore.AddReduct(reduct);

				this.ReductPool.AddReduct(reduct);
				Console.WriteLine("{0}", reduct);
			}
			*/

			do
			{
				iter++;
				
				IReductStore localReductStore = this.CreateReductStore();
				weightGenerator.Generate();
				for (int i = 0; i < this.NumberOfReductsInWeakClassifier; i++)
				{
					IReduct reduct = this.GetNextReduct(weightGenerator.Weights, this.MinReductLength, this.MaxReductLength);
					localReductStore.AddReduct(reduct);
					Console.WriteLine("{0}", reduct);
					this.ReductPool.AddReduct(reduct);
				}
				

				RoughClassifier classifier = new RoughClassifier();				
				classifier.ReductStore = localReductStore;

				IReductStoreCollection rsCollection = new ReductStoreCollection();
				rsCollection.AddStore(localReductStore);
				classifier.ReductStoreCollection = rsCollection;
				
				classifier.Classify(this.DataStore);

                //Should we use changed weights as error indicator or 1/n weights
                ClassificationResult result = classifier.Vote(this.DataStore, this.IdentyficationType, this.VoteType, weightGenerator.Weights);
                error = result.WeightUnclassified + result.WeightMisclassified;

                ClassificationResult result2 = classifier.Vote(this.DataStore, this.IdentyficationType, this.VoteType, null);				
                error2 = result2.WeightUnclassified + result2.WeightMisclassified;

                Console.WriteLine("Iteration {0}: error_modified_w: {1} error_standard_w: {2}", iter, error, error2);

				if (error >= this.Threshold)
				{
                    Console.WriteLine("ERROR {0} EXCEEDS THRESHOLD {1}", error, this.Threshold);
					//break;
                    continue;
				}
												
				double alpha = error != 0.0 ? System.Math.Log((1.0 - error) / error) : 10000;
				localReductStore.Weight = alpha;
				this.models.AddStore(localReductStore);

				double sum = 0.0;
				for (int i = 0; i < this.DataStore.NumberOfRecords; i++ )
				{

					
					if (this.DataStore.GetDecisionValue(i) == result.GetResult(this.DataStore.ObjectIndex2ObjectId(i)))
					{						
						weightGenerator.Weights[i] *= System.Math.Exp(-alpha);
					}
					else
					{						
						weightGenerator.Weights[i] *= System.Math.Exp(alpha);
					}
					
					
					/*
					//Standard AdaBoost
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
				
			} while(iter < this.MaxIterations);
						
			// Normalize w for models confidence
			foreach(IReductStore rs in this.models)
				rs.Weight /= alphaSum;
		}

		public virtual IReductStoreCollection GetReductGroups(int numberOfEnsembles = Int32.MaxValue)
		{
			return this.models;
		}

		private IReduct GetNextReduct(double[] weights, int minimumLength, int maximumLength)
		{            			
			Permutation permutation = new PermutationGenerator(this.DataStore).Generate(1)[0];
			int length = System.Math.Min(maximumLength, permutation.Length - 1);
			int minLen = System.Math.Max(minimumLength - 1, 0);
			int cutoff = RandomSingleton.Random.Next(minLen, length);
			
			
			int[] attributes = new int[cutoff + 1];
			for (int i = 0; i <= cutoff; i++)
				attributes[i] = permutation[i];

			ReductCrisp reduct = new ReductCrisp(this.DataStore, attributes, weights, 0);
			reduct.Id = this.GetNextReductId().ToString();						
			reduct.Reduce(attributes, this.MinReductLength);
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
