namespace NRough.Data
{
    public interface IFilter
    {
        bool Enabled { get; set; }
        void Compute(DataStore data);
        DataStore Apply(DataStore data);        
    }
}
