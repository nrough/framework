namespace Raccoon.Data
{
    public interface IFilter
    {
        DataStore Apply(DataStore data);
    }
}
