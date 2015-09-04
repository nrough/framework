using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{       
	public delegate double UpdateWeightsDelegate(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError);
	public delegate double CalcModelConfidenceDelegate(int numberOfOutputValues, double totalError);
	
	public class ReductEnsembleBoostingGenerator : ReductGenerator
	{								
		protected int iterPassed;		

		public int MaxReductLength { get; set; }
		public int MinReductLength { get; set; }
		public double Threshold { get; set; }
		public IdentificationType IdentyficationType { get; set;} 
		public VoteType VoteType {get; set; }
		public int NumberOfReductsInWeakClassifier { get; set; }
		public int MaxIterations { get; set; }
		public int IterationsPassed { get { return this.iterPassed; } }
		public int MaxNumberOfWeightResets { get; set; }
		public int NumberOfWeightResets { get; protected set; }
		public bool CheckEnsembleErrorDuringTraining { get; set; }
		public WeightGenerator WeightGenerator { get; set; }				
		public UpdateWeightsDelegate UpdateWeights { get; set; }		
		public CalcModelConfidenceDelegate CalcModelConfidence{ get; set; }
		
		protected ReductStoreCollection Models { get; set; }

		public DataSet testDataSet {get; set;}
				
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
			this.MaxNumberOfWeightResets = 0;
			this.CheckEnsembleErrorDuringTraining = false;
			this.UpdateWeights = ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All;
			this.CalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
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
			this.MaxNumberOfWeightResets = 0;
			this.CheckEnsembleErrorDuringTraining = false;
			this.UpdateWeights = ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All;
			this.CalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
		}

		public override void InitFromArgs(Args args)
		{
			base.InitFromArgs(args);

			if (this.DataStore != null)
			{				
				this.WeightGenerator = new WeightBoostingGenerator(this.DataStore);
				this.WeightGenerator.Generate();

				int numOfAttr = this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);				
				
				this.MaxReductLength = (int) System.Math.Floor(System.Math.Log((double)numOfAttr + 1.0, 2.0));
				this.MinReductLength = 1;				

				IReduct emptyReduct = this.GetNextReduct(this.WeightGenerator.Weights, 0, 0);

				if (emptyReduct.Attributes.Count != 0)
					throw new InvalidOperationException("Empty reduct must be of zero length");

				double m0 = new InformationMeasureWeights().Calc(emptyReduct);
				this.Threshold = 1.0 - m0;
			}

			if (args.Exist(ReductGeneratorParamHelper.MinReductLength))
				this.MinReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MinReductLength);

			if (args.Exist(ReductGeneratorParamHelper.MaxReductLength))
				this.MaxReductLength = (int)args.GetParameter(ReductGeneratorParamHelper.MaxReductLength);

			if (this.MaxReductLength > this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard))
				this.MaxReductLength = this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);

			if (this.MaxReductLength < this.MinReductLength)
				this.MaxReductLength = this.MinReductLength;


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

			if (args.Exist(ReductGeneratorParamHelper.UpdateWeights))
				this.UpdateWeights = (UpdateWeightsDelegate)args.GetParameter(ReductGeneratorParamHelper.UpdateWeights);

			if (args.Exist(ReductGeneratorParamHelper.CalcModelConfidence))
				this.CalcModelConfidence = (CalcModelConfidenceDelegate)args.GetParameter(ReductGeneratorParamHelper.CalcModelConfidence);
		}

		public override void Generate()
		{
			this.ReductPool = this.CreateReductStore(this.NumberOfReductsInWeakClassifier * this.MaxIterations);
			this.ReductPool.AllowDuplicates = true;
			
			this.Models = new ReductStoreCollection(this.MaxIterations);

			double alphaSum = 0.0;
			iterPassed = 0;
			this.NumberOfWeightResets = 0;
			double error = -1.0;
			int K = this.DataStore.DataStoreInfo.NumberOfDecisionValues;
			this.WeightGenerator.Generate();

			do
			{
				IReductStore localReductStore = this.CreateReductStore(this.NumberOfReductsInWeakClassifier);				
				for (int i = 0; i < this.NumberOfReductsInWeakClassifier; i++)
				{
					IReduct reduct = this.GetNextReduct(this.WeightGenerator.Weights, this.MinReductLength, this.MaxReductLength);
					localReductStore.AddReduct(reduct);					
					this.ReductPool.AddReduct(reduct);
				}				

				RoughClassifier classifier = new RoughClassifier();
				classifier.ReductStore = localReductStore;
				classifier.ReductStoreCollection = new ReductStoreCollection(1);
				classifier.ReductStoreCollection.AddStore(localReductStore);
				classifier.Classify(this.DataStore);
				ClassificationResult result = classifier.Vote(this.DataStore, this.IdentyficationType, this.VoteType, this.WeightGenerator.Weights);
				error = result.WeightUnclassified + result.WeightMisclassified;				

				//clear objects and memory
				classifier = null;
				//GC.Collect();
				
				if (error >= this.Threshold)
				{
					this.NumberOfWeightResets++;

					if (this.NumberOfWeightResets > this.MaxNumberOfWeightResets)
					{
						if (iterPassed == 0)
						{
							this.AddModel(localReductStore, this.CalcModelConfidence(K, error));
							iterPassed = 1;
						}
						
						break;
					}

					this.WeightGenerator.Reset();
					continue;
				}

				double alpha = this.CalcModelConfidence(K, error);
				this.AddModel(localReductStore, alpha);
				double sum = 0.0;
				for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
				{
					this.WeightGenerator.Weights[i] = this.UpdateWeights(this.WeightGenerator.Weights[i], 
																		 K, 
																		 this.DataStore.GetDecisionValue(i), 
																		 result.GetResult(this.DataStore.ObjectIndex2ObjectId(i)), 
																		 error);                    
					sum += this.WeightGenerator.Weights[i];
				}

				result = null;

				//Normalize object weights
				for (int i = 0; i < this.DataStore.NumberOfRecords; i++ )
					this.WeightGenerator.Weights[i] /= sum;
				
				alphaSum += alpha;
				
				iterPassed++;

				if (this.CheckEnsembleErrorDuringTraining)
				{
					if (this.Models.Count > 1)
					{
						// Normalize weights for models confidence
						foreach (IReductStore rs in this.Models)
							if (rs.IsActive)
								rs.Weight /= alphaSum;
						
						RoughClassifier classifierEnsemble = new RoughClassifier();
						classifierEnsemble.ReductStoreCollection = this.Models;
						classifierEnsemble.Classify(this.DataStore);
						ClassificationResult resultEnsemble = classifierEnsemble.Vote(this.DataStore, this.IdentyficationType, this.VoteType, null);

						// De-normalize weights for models confidence
						foreach (IReductStore rs in this.Models)
							if (rs.IsActive)
								rs.Weight *= alphaSum;

						if (resultEnsemble.WeightMisclassified + resultEnsemble.WeightUnclassified <= 0.0001)
						{
							bool modelHasConverged = true;
							foreach (IReductStore model in this.Models)
							{
								model.IsActive = false;

								// Normalize weights for models confidence
								foreach (IReductStore rs in this.Models)
									if (rs.IsActive)
										rs.Weight /= (alphaSum - model.Weight);

								RoughClassifier localClassifierEnsemble = new RoughClassifier();
								localClassifierEnsemble.ReductStoreCollection = this.Models;
								localClassifierEnsemble.Classify(this.DataStore);
								ClassificationResult localResultEnsemble = localClassifierEnsemble.Vote(this.DataStore, this.IdentyficationType, this.VoteType, null);

								// De-normalize weights for models confidence
								foreach (IReductStore rs in this.Models)
									if (rs.IsActive)
										rs.Weight *= (alphaSum - model.Weight);

								model.IsActive = true;

								if (resultEnsemble.WeightMisclassified + resultEnsemble.WeightUnclassified > 0.001)
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
				else
				{
					if (error == 0.0)
						break;
				}
			} while (iterPassed < this.MaxIterations);
						
			// Normalize weights for models confidence
			foreach (IReductStore rs in this.Models)
				rs.Weight /= alphaSum;
		}		

		protected virtual void AddModel(IReductStore model, double modelWeight)
		{
			model.Weight = modelWeight;
			this.Models.AddStore(model);
		}

		public virtual IReductStoreCollection GetReductGroups(int numberOfEnsembles = Int32.MaxValue)
		{
			return this.Models;
		}

		public virtual IReduct GetNextReduct(double[] weights, int minimumLength, int maximumLength)
		{
			if (minimumLength > maximumLength)
				throw new ArgumentOutOfRangeException();
			
			Permutation permutation = new PermutationGenerator(this.DataStore).Generate(1)[0];
			
			int maxLen = System.Math.Min(maximumLength, this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard));
			int minLen = System.Math.Max(minimumLength, 0);

			//int cutoff = RandomSingleton.Random.Next(minLen, maxLen + 1);
			int cutoff = maxLen;
			
			int[] attributes = new int[cutoff];
			for (int i = 0; i < cutoff; i++)
				attributes[i] = permutation[i];

			return this.CreateReduct(attributes, this.Epsilon, weights);
		}        

		public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights)
		{
			ReductCrisp reduct = new ReductCrisp(this.DataStore, permutation, weights, 0);
			reduct.Id = this.GetNextReductId().ToString();
			reduct.Reduce(permutation, this.MinReductLength);
			return reduct;
		}

		protected override IReduct CreateReductObject(int[] fieldIds, double epsilon, string id)
		{
			throw new NotImplementedException("ReductEnsembleBoostingGenerator.CreateReductObject(...) is not implemented");
		}

		#region Delegate implementations
		
		public static double UpdateWeightsAdaBoostM1(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError)
		{            
			if (actualOutput == predictedOutput)
				return 1.0;
			double alpha = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1(numberOfOutputValues, totalError);
			return currentWeight * System.Math.Exp(alpha); 
		}        

		public static double UpdateWeightsAdaBoost_All(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError)
		{
			double alpha = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1(numberOfOutputValues, totalError);
			if (actualOutput == predictedOutput)
				return currentWeight * System.Math.Exp(-alpha);
			return currentWeight * System.Math.Exp(alpha);
		}

		public static double UpdateWeightsAdaBoost_OnlyCorrect(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError)
		{
			double alpha = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1(numberOfOutputValues, totalError);
			if (actualOutput == predictedOutput)
				return currentWeight * System.Math.Exp(-alpha);
			return currentWeight;
		}

		public static double UpdateWeightsAdaBoost_OnlyNotCorrect(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError)
		{            
			if (actualOutput == predictedOutput)
				return currentWeight;
			double alpha = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1(numberOfOutputValues, totalError);
			return currentWeight * System.Math.Exp(alpha);
		}

		public static double UpdateWeightsDummy(double currentWeight, int numberOfOutputValues, long actualOutput, long predictedOutput, double totalError)
		{
			return currentWeight;
		}

		public static double ModelConfidenceAdaBoostM1(int numberOfOutputValues, double totalError)
		{			
			return System.Math.Log((1.0 - totalError) / (totalError + 0.0000001)) + System.Math.Log(numberOfOutputValues - 1);
		}

		public static double ModelConfidenceEqual(int numberOfOutputValues, double totalError)
		{
			return 1.0;
		}

		#endregion
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
