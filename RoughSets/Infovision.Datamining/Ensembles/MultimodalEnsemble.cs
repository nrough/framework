using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raccoon.Data;
using Raccoon.MachineLearning.Classification;

namespace Raccoon.MachineLearning.Ensembles
{
    public class MultimodalEnsemble : EnsembleBase
    {
        public override ClassificationResult Learn(DataStore data, int[] attributes)
        {
            double alphaSum = 0.0;
            foreach (var weakClassifier in weakClassifiers)
                alphaSum += weakClassifier.Weight;

            if (alphaSum != 0.0)
                foreach (var weakClassifier in weakClassifiers)
                    weakClassifier.Weight /= alphaSum;

            return Classifier.Default.Classify(this, data);
        }
    }
}
