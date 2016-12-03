using Infovision.Data;
using Infovision.MachineLearning.Classification;

namespace Infovision.MachineLearning
{
    public interface IPredictionModel : IModel
    {
        long Compute(DataRecordInternal record);
        long? DefaultOutput { get; set; }
        void SetClassificationResultParameters(ClassificationResult result);        
    }
}
