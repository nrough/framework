using NRough.Data;
using NRough.MachineLearning.Classification;

namespace NRough.MachineLearning
{
    public interface ILearner
    {
        ClassificationResult Learn(DataStore data, int[] attributes);        
    }
}
