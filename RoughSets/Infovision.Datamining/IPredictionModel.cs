using Infovision.Data;

namespace Infovision.Datamining
{
    public interface IPredictionModel : IModel
    {
        long Compute(DataRecordInternal record);
        long? DefaultOutput { get; set; }
        void SetClassificationResultParameters(ClassificationResult result);        
    }
}
