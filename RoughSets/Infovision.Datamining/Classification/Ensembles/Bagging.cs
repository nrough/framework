using NRough.Data;
using NRough.MachineLearning.Classification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.Ensembles
{
    public class Bagging<TModel> : EnsembleBase
        where TModel : ILearner, IPredictionModel, ICloneable
    {
        private TModel prototype;

        public Bagging(TModel classifierPrototype)
            : base()
        {
            prototype = classifierPrototype;
        }

        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            if (this.Iterations < 0) this.Iterations = DefaultIterations;
            int iterPassed = 0;
            long[] decisionValues = data.DataStoreInfo.GetDecisionValues().ToArray();
            double alphaSum = 0.0;

            do
            {
                DataSampler baggingDataSampler = new DataSampler(data);
                var localDataBag = baggingDataSampler.GetData(iterPassed);

                TModel weakClassifier = (TModel)prototype.Clone();
                var result = weakClassifier.Learn(localDataBag.Item1, attributes);
                double alpha = CalcModelConfidence(decisionValues.Length, result.Error);
                AddClassfier(weakClassifier, alpha);
                alphaSum += alpha;
                iterPassed++;

            } while (iterPassed < this.Iterations);

            if (alphaSum != 0.0)
                foreach (var weakClassifier in weakClassifiers)
                    weakClassifier.Weight /= alphaSum;

            return Classifier.Default.Classify(this, data);
        }
    }
}
