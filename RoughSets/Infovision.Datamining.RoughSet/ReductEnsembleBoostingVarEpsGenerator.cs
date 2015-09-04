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

		
		public ReductEnsembleBoostingVarEpsGenerator()
			: base()
		{
		}

		public ReductEnsembleBoostingVarEpsGenerator(DataStore data)
			: base(data)		
		{						
		}

		public override void Generate()
		{
			this.ReductPool = this.CreateReductStore();
			this.ReductPool.AllowDuplicates = true;

			this.Models = new ReductStoreCollection();			

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
				
				//Normalize object weights
				for (int i = 0; i < this.DataStore.NumberOfRecords; i++)
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
		
		public virtual IReduct GetNextReduct(double[] weights, int minimumLength, int maximumLength)
		{
			Permutation permutation = new PermutationGenerator(this.DataStore).Generate(1)[0];            
			return this.CreateReduct(permutation.ToArray(), this.Epsilon, weights);
		}

		public override IReduct CreateReduct(int[] permutation, double epsilon, double[] weights)
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

		public virtual bool CheckIsReduct(IReduct reduct)
		{
			double partitionQuality = this.GetPartitionQuality(reduct);
			double tinyDouble = 0.0001 / this.DataStore.NumberOfRecords;
			if (partitionQuality >= (((1.0 - this.Epsilon) * this.GetDataSetQuality(reduct)) - tinyDouble))
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

		protected virtual double GetPartitionQuality(IReduct reduct)
		{
			return this.InformationMeasure.Calc(reduct);
		}
		
		protected virtual double GetDataSetQuality(IReduct reduct)
		{
			ReductWeights allAttributesReduct = new ReductWeights(this.DataStore, this.DataStore.DataStoreInfo.GetFieldIds(FieldTypes.Standard), reduct.Weights, reduct.Epsilon);
			return this.GetPartitionQuality(allAttributesReduct);
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
