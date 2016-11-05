using Infovision.Data;

namespace Infovision.Datamining
{
    public interface IPredictionModel
    {
        long Compute(DataRecordInternal record);
        long? DefaultOutput { get; set; }

        void SetClassificationResultParameters(ClassificationResult result);
        double Epsilon { get; set; }      
    }
}
