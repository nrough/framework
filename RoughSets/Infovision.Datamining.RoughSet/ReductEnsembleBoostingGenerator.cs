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

		public ReductEnsembleBoostingGenerator()
			: base()
		{
			this.numberOfWeakClassifiers = 10; //TODO should be calculated of passed as parameter
			this.identyficationType = IdentificationType.WeightConfidence;
			this.voteType = VoteType.WeightCoverage;


			alpha = new double[this.numberOfWeakClassifiers];
		}

		public override void Generate()
		{			
			this.ReductPool = this.CreateReductStore();
			WeightBoostingGenerator weightGenerator = new WeightBoostingGenerator(this.DataStore);
			int m = 10;
			int iter = 0;

			do
			{
				IReductStore localReductStore = this.CreateReductStore();
				weightGenerator.Generate();
				for (int i = 0; i < m; m++)
				{
					IReduct reduct = this.GetNextReduct(weightGenerator.Weights);
					localReductStore.AddReduct(reduct);
				}

				RoughClassifier classifier = new RoughClassifier();

				classifier.ReductStore = localReductStore;
				classifier.Classify(this.DataStore);
				ClassificationResult result = classifier.Vote(this.DataStore, this.identyficationType, this.voteType);                                
				double error = result.Error;
				alpha[iter] = System.Math.Log((1.0 - error) / error);

				for (int i = 0; i < this.DataStore.NumberOfRecords; i++ )
				{
					//TODO What is I( c_i != T^m (x_i))

					
					weightGenerator.Weights[i] = weightGenerator.Weights[i] * System.Math.Exp(alpha[iter] * )
				}

				iter++;

			} while (true);

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
		}

		private IReduct GetNextReduct(double[] weights)
		{            
			int idx = this.GetNextReductId();
			Permutation permutation = new PermutationGenerator(this.DataStore).Generate(1)[0];
			int cutoff = RandomSingleton.Random.Next(0, permutation.Length - 1);
			
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
