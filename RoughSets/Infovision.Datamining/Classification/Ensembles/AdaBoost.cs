using NRough.Core;
using NRough.Data;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Roughset;
using NRough.Math;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.Ensembles
{
    public class AdaBoost<T> : EnsembleBase
        where T : ILearner, IPredictionModel, ICloneable
    {
        protected static double DefaultThreshold = 0.5;

        public double Threshold { get; set; }
        public WeightBoosingtMethod WeightBoosingtMethod { get; set; }

        private T prototype;

        public AdaBoost(T classifierPrototype)
            : base()
        {
            this.Threshold = -1.0;
            this.Iterations = -1;
            this.Size = -1;

            prototype = classifierPrototype;
            weakClassifiers = new List<WeakClassifierInfo>();
            WeightBoosingtMethod = WeightBoostingMethods.UpdateWeightsAdaBoost_All;
            CalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            if (this.Iterations < 0) this.Iterations = DefaultIterations;
            if (this.Threshold < 0) this.Threshold = DefaultThreshold;

            double alphaSum = 0.0;
            int iterPassed = 0;
            double error = -1.0;

            double[] origWeights = data.Weights.ToArray();
            long[] decisionValues = data.DataStoreInfo.GetDecisionValues().ToArray();

            do
            {
                T weakClassifier = (T) prototype.Clone();
                weakClassifier.Learn(data, attributes);

                var result = Classifier.Default.Classify(weakClassifier, data);
                double alpha = this.CalcModelConfidence(decisionValues.Length, error);

                if (result.Error >= this.Threshold)
                {
                    if (iterPassed == 0)
                    {
                        iterPassed++;
                        var constClassifier = new ConstDecision();
                        if (DefaultOutput != null)
                            constClassifier.Output = (long)DefaultOutput;
                        constClassifier.Learn(data, attributes);
                        weakClassifiers.Add(
                            new WeakClassifierInfo(constClassifier, 1.0));
                    }
                    break;
                }

                weakClassifiers.Add(new WeakClassifierInfo(weakClassifier, alpha));
                data.Weights.NormalizeSum();

                alphaSum += alpha;
                iterPassed++;

                if (error == 0.0)
                    break;

            } while (iterPassed < this.Iterations);

            if (alphaSum != 0.0)
                foreach (var weakClassifier in weakClassifiers)
                    weakClassifier.Weight /= alphaSum;

            return Classifier.Default.Classify(this, data);
        }     
    }

    public delegate double[] WeightBoosingtMethod(
        double error, int numberOfOuputs,
        long[] predictedOutput, long[] actualOutputs, 
        double[] currentWeights);

    public static class WeightBoostingMethods
    {
        public static void Validate(
            double error, int numberOfOuputs,
            long[] predictedOutput, long[] actualOutputs,
            double[] currentWeights)
        {
            if (predictedOutput == null) throw new ArgumentNullException();
            if (actualOutputs == null) throw new ArgumentNullException();
            if (currentWeights == null) throw new ArgumentNullException();

            if (predictedOutput.Length != actualOutputs.Length) throw new ArgumentException();
            if (currentWeights.Length != predictedOutput.Length) throw new ArgumentException();
            if (numberOfOuputs < 0) throw new ArgumentOutOfRangeException();
        }

        public static double[] UpdateWeightsAdaBoostM1(
            double error, int numberOfOuputs,
            long[] predictedOutputs, long[] actualOutputs,
            double[] currentWeights)
        {
            Validate(error, numberOfOuputs, 
                predictedOutputs, actualOutputs, 
                currentWeights);

            double alpha = System.Math.Log((1.0 - error) / (error + 0.000000000001))
                        + System.Math.Log(numberOfOuputs - 1.0);

            double[] result = currentWeights.ToArray();
            for (int i = 0; i < currentWeights.Length; i++)
            {
                result[i] = actualOutputs[i] == predictedOutputs[i] ? 1.0
                    : currentWeights[i] * System.Math.Exp(alpha);
            }
            return result;
        }

        public static double[] UpdateWeightsAdaBoost_All(
            double error, int numberOfOuputs,
            long[] predictedOutputs, long[] actualOutputs,
            double[] currentWeights)
        {
            Validate(error, numberOfOuputs,
                predictedOutputs, actualOutputs,
                currentWeights);

            double alpha = System.Math.Log((1.0 - error) / (error + 0.000000000001))
                        + System.Math.Log(numberOfOuputs - 1.0);

            double[] result = currentWeights.ToArray();
            for (int i = 0; i < currentWeights.Length; i++)
            {
                result[i] = actualOutputs[i] == predictedOutputs[i]
                              ? currentWeights[i] * System.Math.Exp(-alpha)
                              : currentWeights[i] * System.Math.Exp(alpha);
            }
            return result;
        }

        public static double[] UpdateWeightsAdaBoost_OnlyCorrect(
            double error, int numberOfOuputs,
            long[] predictedOutputs, long[] actualOutputs,
            double[] currentWeights)
        {
            Validate(error, numberOfOuputs,
                predictedOutputs, actualOutputs,
                currentWeights);

            double alpha = System.Math.Log((1.0 - error) / (error + 0.000000000001))
                        + System.Math.Log(numberOfOuputs - 1.0);

            double[] result = currentWeights.ToArray();
            for (int i = 0; i < currentWeights.Length; i++)
            {
                if(actualOutputs[i] == predictedOutputs[i])
                    result[i] = currentWeights[i] * System.Math.Exp(-alpha);
            }
            return result;
        }

        public static double[] UpdateWeightsAdaBoost_OnlyNotCorrect(
            double error, int numberOfOuputs,
            long[] predictedOutputs, long[] actualOutputs,
            double[] currentWeights)
        {
            Validate(error, numberOfOuputs,
                predictedOutputs, actualOutputs,
                currentWeights);

            double alpha = System.Math.Log((1.0 - error) / (error + 0.000000000001))
                        + System.Math.Log(numberOfOuputs - 1.0);

            double[] result = currentWeights.ToArray();
            for (int i = 0; i < currentWeights.Length; i++)
            {
                if (actualOutputs[i] != predictedOutputs[i])
                    result[i] = currentWeights[i] * System.Math.Exp(alpha);
            }
            return result;
        }
    }
}
