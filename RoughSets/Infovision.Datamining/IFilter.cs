using Raccoon.Data;

namespace Raccoon.MachineLearning
{
    public interface IFilter
    {
        DataStore Apply(DataStore data);
    }
}
