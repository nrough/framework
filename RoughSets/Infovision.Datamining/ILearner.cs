using Infovision.Data;

namespace Infovision.Datamining
{
    public interface ILearner
    {
        double Learn(DataStore data, int[] attributes);
    }
}
