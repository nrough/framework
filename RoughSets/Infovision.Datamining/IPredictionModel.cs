using Raccoon.Data;
using Raccoon.MachineLearning.Classification;

namespace Raccoon.MachineLearning
{
    public interface IPredictionModel : IModel
    {
        long Compute(DataRecordInternal record);
        long? DefaultOutput { get; set; }
        void SetClassificationResultParameters(ClassificationResult result);        
    }
}
