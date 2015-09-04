using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
	class ReductEnsembleBoostingVarEps : ReductEnsembleBoostingGenerator
	{
		private ReductStoreCollection models;
		private int iterPassed;
		private int numberOfWeightResets;

		public ReductEnsembleBoostingVarEps()
			: base()
		{
		}

		public ReductEnsembleBoostingVarEps(DataStore data)
			: base(data)		
		{						
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
			double prevError = 0.5 * this.Threshold;
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
								
				if(iterPassed > 0)                
					prevError = error;

				error = result.WeightUnclassified + result.WeightMisclassified;

				if (error >= this.Threshold)
				{
					numberOfWeightResets++;

					if (numberOfWeightResets > this.MaxNumberOfWeightResets)
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

				//TODO Set this.Epsilon
				double c = 0.2;
				this.Epsilon *= (prevError - error > 0) 
							  ? 1.0 - c 
							  : 1.0 + c;
				

				//Normalize object weights
				for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
					this.WeightGenerator.Weights[i] /= sum;

				alphaSum += alpha;

				iterPassed++;

				if (this.CheckEnsembleErrorDuringTraining)
				{
					if (this.models.Count > 1)
					{
						// Normalize weights for models confidence
						foreach (IReductStore rs in this.models)
							if (rs.IsActive)
								rs.Weight /= alphaSum;

						RoughClassifier classifierEnsemble = new RoughClassifier();
						classifierEnsemble.ReductStoreCollection = this.models;
						classifierEnsemble.Classify(this.DataStore);
						ClassificationResult resultEnsemble = classifierEnsemble.Vote(this.DataStore, this.IdentyficationType, this.VoteType, null);

						// De-normalize weights for models confidence
						foreach (IReductStore rs in this.models)
							if (rs.IsActive)
								rs.Weight *= alphaSum;

						if (resultEnsemble.WeightMisclassified + resultEnsemble.WeightUnclassified <= 0.0001)
						{
							bool modelHasConverged = true;
							foreach (IReductStore model in this.models)
							{
								model.IsActive = false;

								// Normalize weights for models confidence
								foreach (IReductStore rs in this.models)
									if (rs.IsActive)
										rs.Weight /= (alphaSum - model.Weight);

								RoughClassifier localClassifierEnsemble = new RoughClassifier();
								localClassifierEnsemble.ReductStoreCollection = this.models;
								localClassifierEnsemble.Classify(this.DataStore);
								ClassificationResult localResultEnsemble = localClassifierEnsemble.Vote(this.DataStore, this.IdentyficationType, this.VoteType, null);

								// De-normalize weights for models confidence
								foreach (IReductStore rs in this.models)
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
			foreach (IReductStore rs in this.models)
				rs.Weight /= alphaSum;
		}
		
		public virtual IReduct GetNextReduct(double[] weights, int minimumLength, int maximumLength)
		{
			Permutation permutation = new PermutationGenerator(this.DataStore).Generate(1)[0];            
			return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
		}

		public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights)
		{
			ReductWeights reduct = new ReductWeights(this.DataStore, new int[]{}, weights, this.Epsilon);
			reduct.Id = this.GetNextReductId().ToString();
			//TODO Reach
			//TODO Reduce
			return reduct;
		}

		public override void SetDefaultParameters()
		{
			base.SetDefaultParameters();
			this.Epsilon = 0.5 * this.Threshold;
		}

		public override void InitFromArgs(Args args)
		{
			base.InitFromArgs(args);
			this.Epsilon = 0.5 * this.Threshold;
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
			ReductEnsembleBoostingVarEps rGen = new ReductEnsembleBoostingVarEps();
			rGen.InitFromArgs(args);
			return rGen;
		}

		public virtual IPermutationGenerator GetPermutationGenerator(Args args)
		{
			throw new NotImplementedException("GetPermutationGenerator(Args args) method is not implemented");
		}
	}
}
