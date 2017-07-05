// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

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
        where TModel : ILearner, IClassificationModel, ICloneable
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
