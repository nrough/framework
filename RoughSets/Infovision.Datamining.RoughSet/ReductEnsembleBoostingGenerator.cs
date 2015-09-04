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
		public static string FactoryKey = "ReductEnsembleBoosting";
		private double[] alpha;
		private int numberOfWeakClassifiers;
		private IdentificationType identyficationType;
		private VoteType voteType;
		private double threshold;
        private int maximumReductLength;
        private WeightBoostingGenerator weightGenerator;

        public WeightBoostingGenerator WeightGenerator { get { return this.weightGenerator; } }

		public ReductEnsembleBoostingGenerator()
			: base()
		{
			this.numberOfWeakClassifiers = 10; //TODO should be calculated of passed as parameter
			this.identyficationType = IdentificationType.WeightConfidence;
			this.voteType = VoteType.WeightCoverage;
            this.threshold = 0.5; //TODO how to estimate threshold? Check Accord UnitTests for AdaBoost

			alpha = new double[this.numberOfWeakClassifiers];
		}        

		public override void Generate()
		{
            //TODO To be set as parameter
            this.maximumReductLength = this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) / 2;

            double alphaSum = 0.0;
			this.ReductPool = this.CreateReductStore();
			weightGenerator = new WeightBoostingGenerator(this.DataStore);
			int m = 10;
			int iter = 0;

			do
			{
				IReductStore localReductStore = this.CreateReductStore();
				weightGenerator.Generate();
				for (int i = 0; i < m; m++)
				{
                    IReduct reduct = this.GetNextReduct(weightGenerator.Weights, this.maximumReductLength);
					localReductStore.AddReduct(reduct); //TODO reduct should be added with a weight, default weight = 1.0
				}

				//TODO Add reducts to global store

				RoughClassifier classifier = new RoughClassifier();
				classifier.ReductStore = localReductStore;
				classifier.Classify(this.DataStore);
				
				ClassificationResult result = classifier.Vote(this.DataStore, this.identyficationType, this.voteType);                                
				double error = result.UnclassifiedSumOfWeigths + result.MisclassifiedSumOfWeights;

				if(error >= threshold)
					break;
				
                //AdaBoost
                double alpha = 0.5 * System.Math.Log((1.0 - error) / error);
                
                //SAME
                //In SAME there is no 0.5
                //int K = this.DataStore.DataStoreInfo.GetDecisionValues().Count;
                //double alpha = System.Math.Log((1.0 - error) / error) + System.Math.Log(K - 1);                

				double sum = 0.0;				
				for (int i = 0; i < this.DataStore.NumberOfRecords; i++ )
				{					                    					
					//TODO This is only for binary decision encoded as +1 / -1
					weightGenerator.Weights[i] *= System.Math.Exp(
							-alpha
							* this.DataStore.GetDecisionValue(i)
							* result.GetResult(this.DataStore.ObjectIndex2ObjectId(i))
							);

					sum += weightGenerator.Weights[i];
				}

				for (int i = 0; i < this.DataStore.NumberOfRecords; i++ )
					weightGenerator.Weights[i] /= sum;

				//TODO classifier.Add(alpha, model) - Classifier must be ready for weak classifier weighting

				alphaSum += alpha;
				iter++;

				//convergence.NewValue = error;

			} while (true);
			//while(convergence.HasConverged)

			/*
			// Normalize weights for confidence calculation
			for (int i = 0; i < classifier.Models.Count; i++)
				classifier.Models[i].Weight /= alphaSum;

			return ComputeError(inputs, outputs);
			*/

			//TODO
			//add generalized decision 
			//calculate m reducts from permutations based on majority weighting scheme
			//adjust weights 
			//calculate next m reducts
			//continue up to convergence point
			//create ensembles from most diverse reducts calculated in each iteration

			//from 
			//from http://read.pudn.com/downloads127/sourcecode/windows/csharp/539127/Classification.NET/Code/Classification.NET/Classifiers/AdaBoost.cs__.htm


			/*public override void Train(LabeledDataSet<double> trainingSet) 
			{ 
				// Initialise weights 
				if (_weights == null) 
				{ 
					_weights = new double[trainingSet.Count]; 
					for (int i = 0; i < trainingSet.Count; i++) 
						_weights[i] = 1.0 / trainingSet.Count; 
				} 
 
				// Perform the learning 
				for (int t = _h.Count; t < _numberOfRounds; t++) 
				{ 
					// Create the weak learner and train it 
					WeakLearner h = _factory(); 
					h.Weights = _weights; 
					h.Train(trainingSet); 
 
					// Compute the classifications and training error 
					int[] hClassification = new int[trainingSet.Count]; 
					double epsilon = 0.0; 
					for (int i = 0; i < trainingSet.Count; i++) 
					{ 
						hClassification[i] = h.Classify(trainingSet.Data[i]); 
						epsilon += hClassification[i] != trainingSet.Labels[i] ? _weights[i] : 0.0; 
					} 
 
					// Check stopping condition 
					if (epsilon >= 0.5) 
						break; 
 
					// Calculate alpha 
					double alpha = 0.5 * Math.Log((1 - epsilon) / epsilon); 
 
					// Update the weights 
					double weightsSum = 0.0; 
					for (int i = 0; i < trainingSet.Count; i++) 
					{ 
						_weights[i] *= Math.Exp(-alpha * trainingSet.Labels[i] * hClassification[i]); 
						weightsSum += _weights[i]; 
					} 
					// Normalise 
					for (int i = 0; i < trainingSet.Count; i++) 
						_weights[i] /= weightsSum; 
 
					// Store the weak learner and alpha value 
					_h.Add(h); 
					_alpha.Add(alpha); 
 
					// Break if perfectly classifying data 
					if (epsilon == 0.0) 
						break; 
				} 
			} 
			*/

			/*
			public override int Classify(IDataPoint<double> dataPoint) 
			{ 
				double classification = 0.0; 
 
				// Call the weak learner classify methods and combine results 
				for (int t = 0; t < _h.Count; t++) 
					classification += _alpha[t] * _h[t].Classify(dataPoint); 
 
				// Return the thresholded classification 
				return classification > 0.0 ? +1 : -1; 
			}
			*/

			/*
			private double run(double[][] inputs, int[] outputs, double[] sampleWeights)
			{
				double error = 0;
				double weightSum = 0;

				int[] actualOutputs = new int[outputs.Length];

				do
				{
					// Create and train a classifier
					TModel model = Creation(sampleWeights);

					if (model == null)
						break;

					// Determine its current accuracy
					for (int i = 0; i < actualOutputs.Length; i++)
						actualOutputs[i] = model.Compute(inputs[i]);

					error = 0;
					for (int i = 0; i < actualOutputs.Length; i++)
						if (actualOutputs[i] != outputs[i]) error += sampleWeights[i];

					if (error >= threshold)
						break;


					// AdaBoost
					double w = 0.5 * System.Math.Log((1.0 - error) / error);

					double sum = 0;
					for (int i = 0; i < sampleWeights.Length; i++)
						sum += sampleWeights[i] *= System.Math.Exp(-w * outputs[i] * actualOutputs[i]);

					// Update sample weights
					for (int i = 0; i < sampleWeights.Length; i++)
						sampleWeights[i] /= sum;

					classifier.Add(w, model);

					weightSum += w;

					convergence.NewValue = error;

				} while (!convergence.HasConverged);


				// Normalize weights for confidence calculation
				for (int i = 0; i < classifier.Models.Count; i++)
					classifier.Models[i].Weight /= weightSum;

				return ComputeError(inputs, outputs);
			}
			*/
		}

		private IReduct GetNextReduct(double[] weights, int maximumLength)
		{            
			int idx = this.GetNextReductId();
			Permutation permutation = new PermutationGenerator(this.DataStore).Generate(1)[0];
            int length = System.Math.Min(maximumLength, permutation.Length - 1);
			int cutoff = RandomSingleton.Random.Next(0, length);
			
			int[] attributes = new int[cutoff + 1];
			for (int i = 0; i <= cutoff; i++)
				attributes[i] = permutation[i];

			ReductCrisp reduct = new ReductCrisp(this.DataStore, attributes, weights, 0);
			reduct.Id = idx.ToString();
			foreach (EquivalenceClass eq in reduct.EquivalenceClassMap)
				eq.RemoveObjectsWithMinorDecisions();
			for (int i = attributes.Length - 1; i >= 0; i--)
				reduct.TryRemoveAttribute(attributes[i]);

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
