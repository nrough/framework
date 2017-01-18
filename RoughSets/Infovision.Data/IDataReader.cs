namespace Raccoon.Data
{
    public interface IDataReader
    {
        string DataName { get; }
        int DecisionId { get; set; }
        bool HandleMissingData { get; set; }
        string MissingValue { get; set; }
        DataStoreInfo ReferenceDataStoreInfo { get; set; }

        DataStoreInfo Analyze();
        void Load(DataStore dataStore, DataStoreInfo dataStoreInfo);
    }
}