namespace Raccoon.Data
{
    public interface IFilter
    {
        void Compute(DataStore data);
        DataStore Apply(DataStore data);
    }
}
