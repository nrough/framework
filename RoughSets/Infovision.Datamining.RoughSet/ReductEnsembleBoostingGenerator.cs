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
		private WeightGenerator weightGenerator;
		
		public WeightGenerator WeightGenerator
		{
			get { return this.weightGenerator; }
			set { this.weightGenerator = value; }
		}

		public override void Generate()
		{
			this.ReductPool = new ReductStore();
			this.WeightGenerator = new WeightBoostingGenerator(this.DataStore);

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
