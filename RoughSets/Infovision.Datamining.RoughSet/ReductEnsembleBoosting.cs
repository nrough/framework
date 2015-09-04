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
		private ReductStoreCollection models;
		private int iterPassed;
		private int numberOfWeightResets;

		public int MaxReductLength { get; set; }
		public int MinReductLength { get; set; }
		public double Threshold { get; set; }
		public IdentificationType IdentyficationType { get; set;} 
		public VoteType VoteType {get; set; }
		public int NumberOfReductsInWeakClassifier { get; set; }
		public int MaxIterations { get; set; }
		public int IterationsPassed { get { return this.iterPassed; } }
		public int MaxNumberOfWeightResets { get; set; }
		public int NumberOfWeightResets { get { return this.numberOfWeightResets; } }
		public bool CheckEnsembleErrorDuringTraining { get; set; }
		public WeightGenerator WeightGenerator { get; set; }
				
		public ReductEnsembleBoostingGenerator()
			: base()
		{
			this.MinReductLength = 1;
			this.MaxReductLength = Int32.MaxValue;
			this.Threshold = 0.5;
			this.IdentyficationType = IdentificationType.WeightConfidence;
			this.VoteType = VoteType.WeightCoverage;
			this.NumberOfReductsInWeakClassifier = 1;
			this.MaxIterations = 100;
			this.MaxNumberOfWeightResets = Int32.MaxValue;
			this.CheckEnsembleErrorDuringTraining = true;
		}

		public ReductEnsembleBoostingGenerator(DataStore data)
			: this()		
		{
			this.WeightGenerator = new WeightBoostingGenerator(data);
		}

		public override void SetDefaultParameters()
		{
			base.SetDefaultParameters();

			this.MinReductLength = 1;
			this.MaxReductLength = Int32.MaxValue;
			this.Threshold = 0.5;
			this.IdentyficationType = IdentificationType.WeightConfidence;
			this.VoteType = VoteType.WeightCoverage;
			this.NumberOfReductsInWeakClassifier = 1;
			this.MaxIterations = 100;
			this.MaxNumberOfWeightResets = Int32.MaxValue;
			this.CheckEnsembleErrorDuringTraining = true;
		}

		public override void InitFromArgs(Args args)
		{
			base.InitFromArgs(args);

			if (this.DataStore != null)
			{
				//this.MaxReductLength = this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);
				this.MaxReductLength = (int) System.Math.Floor(System.Math.Log((double)this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) + 1.0, 2.0));

				this.WeightGenerator = new WeightBoostingGenerator(this.DataStore);
				this.WeightGenerator.Generate();

				IReduct emptyReduct = this.GetNextReduct(this.WeightGenerator.Weights, 0, 0);
				double M = new InformationMeasureWeights().Calc(emptyReduct);
				this.Threshold = 1.0 - M;
			}

			if (args.Exist(ReductGeneratorParamHelper.MinReductLength))
				this.MinReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MinReductLength);

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

			if (args.Exist(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining))
				this.CheckEnsembleErrorDuringTraining = (bool)args.GetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining);

			if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
				this.WeightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);
		}

		public override void Generate()
		{									
			this.ReductPool = this.CreateReductStore();
			this.ReductPool.AllowDuplicates = true;
			
			this.models = new ReductStoreCollection();

			double alphaSum = 0.0;
			iterPassed = 0;
			numberOfWeightResets = 0;
			double error = -1.0;
			int K = this.DataStore.DataStoreInfo.NumberOfDecisionValues;
			this.WeightGenerator.Generate();

			do
			{								
				IReductStore localReductStore = this.CreateReductStore();				
				for (int i = 0; i < this.NumberOfReductsInWeakClassifier; i++)
				{
					IReduct reduct = this.GetNextReduct(this.WeightGenerator.Weights, this.MinReductLength, this.MaxReductLength);
					localReductStore.AddReduct(reduct);					
					this.ReductPool.AddReduct(reduct);
				}				

				RoughClassifier classifier = new RoughClassifier();
				classifier.ReductStore = localReductStore;

				IReductStoreCollection rsCollection = new ReductStoreCollection();
				rsCollection.AddStore(localReductStore);
				classifier.ReductStoreCollection = rsCollection;
				
				classifier.Classify(this.DataStore);

				ClassificationResult result = classifier.Vote(this.DataStore, this.IdentyficationType, this.VoteType, this.WeightGenerator.Weights);
				error = result.WeightUnclassified + result.WeightMisclassified;

				if (error >= this.Threshold)
				{
					numberOfWeightResets++;

					if (numberOfWeightResets > this.MaxNumberOfWeightResets)
						break;

					this.WeightGenerator.Reset();
					continue;
				}

				double alpha = error != 0.0 
					? System.Math.Log((1.0 - error) / error) + System.Math.Log(K - 1)
					: (Double.MaxValue / 2.0);

				this.AddModel(localReductStore, alpha);

				double sum = 0.0;
				for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
				{
					//TODO Consider changing weights only for objects where weak classifier is not correct
					if (this.DataStore.GetDecisionValue(i) == result.GetResult(this.DataStore.ObjectIndex2ObjectId(i)))
						this.WeightGenerator.Weights[i] *= System.Math.Exp(-alpha); //*= error / (1.0 - error)
					else
						this.WeightGenerator.Weights[i] *= System.Math.Exp(alpha);  //*= (1.0 - error) / error) 

					sum += this.WeightGenerator.Weights[i];
				}

				//Normalize object weights
				for (int i = 0; i < this.DataStore.NumberOfRecords; i++ )
					this.WeightGenerator.Weights[i] /= sum;

				alphaSum += localReductStore.Weight;

				iterPassed++;

				if (error == 0.0)
					break;

				if (this.CheckEnsembleErrorDuringTraining)
				{
					if (this.models.Count > 1)
					{
						RoughClassifier classifierEnsemble = new RoughClassifier();
						classifierEnsemble.ReductStoreCollection = this.models;
						classifierEnsemble.Classify(this.DataStore);
						ClassificationResult resultEnsemble = classifierEnsemble.Vote(this.DataStore, this.IdentyficationType, this.VoteType, null);

						if (resultEnsemble.WeightMisclassified + resultEnsemble.WeightUnclassified == 0.0)
						{
							bool modelHasConverged = true;
							foreach (IReductStore model in this.models)
							{
								model.IsActive = false;

								RoughClassifier localClassifierEnsemble = new RoughClassifier();
								localClassifierEnsemble.ReductStoreCollection = this.models;
								localClassifierEnsemble.Classify(this.DataStore);
								ClassificationResult localResultEnsemble = localClassifierEnsemble.Vote(this.DataStore, this.IdentyficationType, this.VoteType, null);

								model.IsActive = true;

								if (resultEnsemble.WeightMisclassified + resultEnsemble.WeightUnclassified != 0.0)
								{
									modelHasConverged = false;
									break;
								}
							}

							if (modelHasConverged == true)
								break;
						}
					}
				}
			} while(iterPassed < this.MaxIterations);
						
			// Normalize weights for models confidence
			foreach(IReductStore rs in this.models)
				rs.Weight /= alphaSum;
		}		

		protected virtual void AddModel(IReductStore model, double modelWeight)
		{
			model.Weight = modelWeight;
			this.models.AddStore(model);
		}

		public virtual IReductStoreCollection GetReductGroups(int numberOfEnsembles = Int32.MaxValue)
		{
			return this.models;
		}

		protected virtual IReduct GetNextReduct(double[] weights, int minimumLength, int maximumLength)
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

			if (reduct.Attributes.Count < minimumLength)
				throw new InvalidProgramException("Reduct length is less than minimum length");

			return reduct;
		}
		
		public override IReduct CreateReduct(Permutation permutation)
		{
			throw new NotImplementedException("CreteReduct() method was not implemented.");
		}

		protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
		{
			ReductWeights r = new ReductWeights(this.DataStore, fieldIds, this.WeightGenerator.Weights, epsilon);
			r.Id = id;
			return r;
		}

	}

	public class ReductEnsembleBoostingFactory : IReductFactory
	{
		public virtual string FactoryKey
		{
			get { return ReductFactoryKeyHelper.ReductEnsembleBoosting; }
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
