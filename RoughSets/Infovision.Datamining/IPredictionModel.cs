using Infovision.Data;

namespace Infovision.Datamining
{
    public interface IPredictionModel
    {
        long Compute(DataRecordInternal record);
        int EnsembleSize { get; }
        double QualityRatio { get; }
        double Epsilon { get; set; }
    }
}
