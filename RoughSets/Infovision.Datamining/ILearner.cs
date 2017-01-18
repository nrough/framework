using Raccoon.Data;
using Raccoon.MachineLearning.Classification;

namespace Raccoon.MachineLearning
{
    public interface ILearner
    {
        ClassificationResult Learn(DataStore data, int[] attributes);        
    }
}
