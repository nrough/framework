using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.Data;
using NRough.MachineLearning.Classification;

namespace NRough.MachineLearning.Classification.Ensembles
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
