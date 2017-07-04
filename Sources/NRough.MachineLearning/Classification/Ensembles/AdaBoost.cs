//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using NRough.Core;
using NRough.Data;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Roughsets;
using NRough.Math;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.Ensembles
{
    public class AdaBoost<TModel> : EnsembleBase
        where TModel : ILearner, IClassificationModel, ICloneable
    {
        protected static double DefaultThreshold = 0.5;

        public double Threshold { get; set; }
        public WeightBoostingMethod WeightBoosingtMethod { get; set; }

        private TModel prototype;

        public AdaBoost(TModel classifierPrototype)
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
                TModel weakClassifier = (TModel) prototype.Clone();
                weakClassifier.Learn(data, attributes);

                var result = Classifier.Default.Classify(weakClassifier, data);
                double alpha = this.CalcModelConfidence(decisionValues.Length, error);

                if (result.Error >= this.Threshold)
                {
                    if (iterPassed == 0)
                    {
                        iterPassed++;
                        var constClassifier = new ConstDecisionModel();
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

    public delegate double[] WeightBoostingMethod(
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
