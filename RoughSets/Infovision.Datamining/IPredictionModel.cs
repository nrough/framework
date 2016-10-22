using Infovision.Data;

namespace Infovision.Datamining
{
    public interface IPredictionModel
    {
        long Compute(DataRecordInternal record);
        void SetClassificationResultParameters(ClassificationResult result);

        double Epsilon { get; set; }      
    }
}
