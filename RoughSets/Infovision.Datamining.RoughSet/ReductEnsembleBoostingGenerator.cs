﻿using System;
using System.Collections.Concurrent;
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

		public double Threshold { get; set; }
		public RuleQualityFunction IdentyficationType { get; set;} 
		public RuleQualityFunction VoteType {get; set; }
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
			this.Threshold = 0.5;
			this.IdentyficationType = RuleQuality.ConfidenceW;
			this.VoteType = RuleQuality.CoverageW;
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

			this.Threshold = 0.5;
			this.IdentyficationType = RuleQuality.ConfidenceW;
			this.VoteType = RuleQuality.CoverageW;
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
				if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
				{
					this.WeightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);
				}
				else
				{
					this.WeightGenerator = new WeightBoostingGenerator(this.DataStore);
					this.WeightGenerator.Generate();
				}


				int numOfAttr = this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);											
				decimal m0 = new InformationMeasureWeights().Calc(new ReductWeights(this.DataStore, new int[]{} , this.WeightGenerator.Weights, this.Epsilon));
				this.Threshold = (double)(Decimal.One - m0);
			}

			if (args.Exist(ReductGeneratorParamHelper.WeightGenerator))
				this.WeightGenerator = (WeightGenerator)args.GetParameter(ReductGeneratorParamHelper.WeightGenerator);

			if (args.Exist(ReductGeneratorParamHelper.Threshold))
				this.Threshold = (double)args.GetParameter(ReductGeneratorParamHelper.Threshold);

			if (args.Exist(ReductGeneratorParamHelper.IdentificationType))
				this.IdentyficationType = (RuleQualityFunction)args.GetParameter(ReductGeneratorParamHelper.IdentificationType);

			if (args.Exist(ReductGeneratorParamHelper.VoteType))
				this.VoteType = (RuleQualityFunction)args.GetParameter(ReductGeneratorParamHelper.VoteType);

			if (args.Exist(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier))
				this.NumberOfReductsInWeakClassifier = (int)args.GetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier);

			if (args.Exist(ReductGeneratorParamHelper.MaxIterations))
				this.MaxIterations = (int)args.GetParameter(ReductGeneratorParamHelper.MaxIterations);

			if (args.Exist(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining))
				this.CheckEnsembleErrorDuringTraining = (bool)args.GetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining);			

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
			decimal[] weights = this.WeightGenerator.Weights;

			long[] decisionValues = this.DataStore.DataStoreInfo.GetDecisionValues().ToArray();
			object tmpLock = new object();                

			do
			{
				IReductStoreCollection reductStoreCollection = this.CreateModel(weights, this.NumberOfReductsInWeakClassifier);
				/*
				IReductStore localReductStore = this.CreateReductStore(this.NumberOfReductsInWeakClassifier);				
				for (int i = 0; i < this.NumberOfReductsInWeakClassifier; i++)
				{
					IReduct reduct = this.GetNextReduct(weights);
					localReductStore.AddReduct(reduct);					
					this.ReductPool.AddReduct(reduct);
				}
				IReductStoreCollection reductStoreCollection = new ReductStoreCollection(1);
				reductStoreCollection.AddStore(localReductStore);
				*/

				RoughClassifier classifier = new RoughClassifier(reductStoreCollection, this.IdentyficationType, this.VoteType, decisionValues);
				ClassificationResult result = classifier.Classify(this.DataStore);
				error = result.WeightUnclassified + result.WeightMisclassified;
				double alpha = this.CalcModelConfidence(K, error);

				if (error >= this.Threshold)
				{
					this.NumberOfWeightResets++;

					if (this.NumberOfWeightResets > this.MaxNumberOfWeightResets)
					{
						if (iterPassed == 0)
						{
							this.AddModel(reductStoreCollection.First(), alpha);
							iterPassed = 1;
						}
						
						break;
					}

					this.WeightGenerator.Reset();
					weights = this.WeightGenerator.Weights;
					continue;
				}


				this.AddModel(reductStoreCollection.First(), alpha);
				double sum = 0.0d;
				var rangePrtitioner = Partitioner.Create(0, weights.Length);
				Parallel.ForEach(
					rangePrtitioner,
					() => 0.0,
					(range, loopState, initialValue) =>
					{
						double partialSum = initialValue;
						for (int i = range.Item1; i < range.Item2; i++)
						{
							weights[i] = (decimal)this.UpdateWeights((double)weights[i],
																		 K,
																		 this.DataStore.GetDecisionValue(i),
																		 result.GetResult(this.DataStore.ObjectIndex2ObjectId(i)),
																		 error);
							partialSum += (double)weights[i];
						}
						return partialSum;
					},                    
					(localPartialSum) =>
					{                        
						lock (tmpLock)
						{
							sum += localPartialSum;
						}
					});
									

				result = null;
				//Normalize object weights
				Parallel.For(0, this.DataStore.NumberOfRecords, i =>
				{
					weights[i] /= (decimal)sum;
				});
				
				alphaSum += alpha;
				
				iterPassed++;

				if (this.CheckEnsembleErrorDuringTraining)
				{
					if (this.Models.Count > 1)
					{
						// Normalize weights for models confidence
						Parallel.ForEach(this.Models.Where( r => r.IsActive), rs =>
						{
							rs.Weight /= (decimal)alphaSum;
						});

						RoughClassifier classifierEnsemble = new RoughClassifier(
							this.Models, this.IdentyficationType, this.VoteType, decisionValues);
						ClassificationResult resultEnsemble = classifierEnsemble.Classify(this.DataStore);
						
						// De-normalize weights for models confidence
						Parallel.ForEach(this.Models.Where(r => r.IsActive), rs =>
						{
							rs.Weight *= (decimal)alphaSum;
						});

						if (resultEnsemble.WeightMisclassified + resultEnsemble.WeightUnclassified <= 0.0001)
						{
							bool modelHasConverged = true;
							foreach (IReductStore model in this.Models)
							{
								model.IsActive = false;

								// Normalize weights for models confidence
								Parallel.ForEach(this.Models.Where(r => r.IsActive), rs =>                                
								{                                    
									rs.Weight /= ((decimal)alphaSum - model.Weight);
								});

								RoughClassifier localClassifierEnsemble = new RoughClassifier(
									this.Models, this.IdentyficationType, this.VoteType, decisionValues);
								ClassificationResult localResultEnsemble = localClassifierEnsemble.Classify(this.DataStore);

								// De-normalize weights for models confidence
								Parallel.ForEach(this.Models.Where(r => r.IsActive), rs =>                                
								{                                    
									rs.Weight *= ((decimal)alphaSum - model.Weight);
								});

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
			if (alphaSum != 0.0)
			{
				Parallel.ForEach(this.Models, rs =>				
				{
					rs.Weight /= (decimal)alphaSum;
				});
			}
		}		

		protected virtual void AddModel(IReductStore model, double modelWeight)
		{
			model.Weight = (decimal)modelWeight;
			this.Models.AddStore(model);
		}

		public virtual IReductStoreCollection GetReductGroups(int numberOfEnsembles = Int32.MaxValue)
		{
			return this.Models;
		}

		public virtual IReduct GetNextReduct(decimal[] weights)
		{
			Permutation permutation = this.InnerParameters != null 
									? ReductFactory.GetPermutationGenerator(this.InnerParameters).Generate(1)[0]
									: new PermutationGenerator(this.DataStore).Generate(1)[0];

			return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
		}

		public virtual IReductStoreCollection CreateModel(decimal[] weights, int size = 0)
		{
			if (this.InnerParameters == null)
				throw new InvalidOperationException("Parameters for internal model are not privided. Please use InnerParameters key to provide setup for internal model creation.");

			decimal[] weightsCopy = new decimal[weights.Length];
			Array.Copy(weights, weightsCopy, weights.Length);

			WeightGenerator localWeightGen = null;
			if (!this.InnerParameters.SetProperty(ReductGeneratorParamHelper.WeightGenerator, ref localWeightGen))
				localWeightGen = new WeightGenerator(this.DataStore);
			localWeightGen.Weights = weights;
			
			this.InnerParameters.SetParameter(ReductGeneratorParamHelper.WeightGenerator, localWeightGen);

			if (size != 0)
			{
				this.InnerParameters.RemoveParameter(ReductGeneratorParamHelper.PermutationCollection);
				this.InnerParameters.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, size);
				this.InnerParameters.SetParameter(ReductGeneratorParamHelper.NumberOfPermutations, size);                
			}

			IReductGenerator generator = ReductFactory.GetReductGenerator(this.InnerParameters);
			generator.Generate();

			return generator.GetReductStoreCollection();
		}

		public override IReduct CreateReduct(int[] permutation, decimal epsilon, decimal[] weights)
		{
			decimal[] weightsCopy = new decimal[weights.Length];
			Array.Copy(weights, weightsCopy, weights.Length);

			int[] attr = new int[permutation.Length];
			Array.Copy(permutation, attr, permutation.Length);

			if (this.InnerParameters != null)
			{
				WeightGenerator localWeightGen;
				if (this.InnerParameters.Exist(ReductGeneratorParamHelper.WeightGenerator))
					localWeightGen = (WeightGenerator)this.InnerParameters.GetParameter(ReductGeneratorParamHelper.WeightGenerator);
				else
					localWeightGen = new WeightGenerator(this.DataStore);
				localWeightGen.Weights = weights;
				this.InnerParameters.SetParameter(ReductGeneratorParamHelper.WeightGenerator, localWeightGen);

				decimal localEpsilon = epsilon;
				this.InnerParameters.SetProperty(ReductGeneratorParamHelper.Epsilon, ref localEpsilon);
				
				IReductGenerator generator = ReductFactory.GetReductGenerator(this.InnerParameters);                
				IReduct reduct = generator.CreateReduct(permutation, localEpsilon, weights);
				reduct.Id = this.GetNextReductId().ToString();
				return reduct;
			}
			else
			{
				ReductGeneralizedMajorityDecision reduct = new ReductGeneralizedMajorityDecision(this.DataStore, permutation, weights, 0);
				reduct.Id = this.GetNextReductId().ToString();
				reduct.Reduce(permutation);
				return reduct;
			}
		}

		protected override IReduct CreateReductObject(int[] fieldIds, decimal epsilon, string id)
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
