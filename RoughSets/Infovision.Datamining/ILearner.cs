using Infovision.Data;

namespace Infovision.Datamining
{
    public interface ILearner
    {
        ClassificationResult Learn(DataStore data, int[] attributes);
    }
}
