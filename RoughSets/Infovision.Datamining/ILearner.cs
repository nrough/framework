using Infovision.Data;
using Infovision.MachineLearning.Classification;

namespace Infovision.MachineLearning
{
    public interface ILearner
    {
        ClassificationResult Learn(DataStore data, int[] attributes);        
    }
}
