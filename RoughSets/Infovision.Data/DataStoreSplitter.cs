using System;
using Infovision.Utils;

namespace Infovision.Data
{
    public interface IDataStoreSplitter
    {
        void Split(ref DataStore dataStore1, ref DataStore dataStore2);
    }

    [Serializable]
    public class DataStoreSplitter : IDataStoreSplitter
    {
        private DataStore dataStore;
        
        private int[] folds;
        private int[] foldSize;
        private int nfold;

        private int activeFold = 1;
        private bool splitCalculated = false;
        
        public DataStoreSplitter(DataStore dataStore, int nfold)
        {
            this.dataStore = dataStore;
            this.nfold = nfold;
            
            this.InitSplit();
        }

        public int NFold
        {
            get { return this.nfold; }
        }

        public int ActiveFold
        {
            get { return activeFold; }
            set { this.activeFold = value; }
        }

        public DataStore DataStore
        {
            get { return this.dataStore; }
        }

        private void InitSplit()
        {
            folds = new int[this.dataStore.DataStoreInfo.NumberOfRecords];
            foldSize = new int[this.nfold];
        }

        protected virtual int RandomSplit()
        {
            return (RandomSingleton.Random.Next() % nfold) + 1;
        }

        private void GenerateSplit()
        {
            for (int i = 0; i < this.dataStore.DataStoreInfo.NumberOfRecords; i++)
            {
                folds[i] = RandomSplit();
                foldSize[folds[i] - 1]++;
            }

            splitCalculated = true;
        }

        public virtual void Split(ref DataStore dataStore1, ref DataStore dataStore2)
        {
            if(!splitCalculated)
                this.GenerateSplit();

            DataStoreInfo dataStoreInfo1 = new DataStoreInfo();
            dataStoreInfo1.InitFromDataStoreInfo(dataStore.DataStoreInfo);
            dataStoreInfo1.NumberOfRecords = dataStore.DataStoreInfo.NumberOfRecords - foldSize[this.ActiveFold - 1];

            dataStore1 = new DataStore(dataStoreInfo1);
            dataStore1.Name = dataStore.Name + "-" + this.ActiveFold.ToString();
            
            DataStoreInfo dataStoreInfo2 = new DataStoreInfo();
            dataStoreInfo2.InitFromDataStoreInfo(dataStore.DataStoreInfo);
            dataStoreInfo2.NumberOfRecords = foldSize[this.ActiveFold - 1];
           
            dataStore2 = new DataStore(dataStoreInfo2);
            dataStore2.Name = dataStore.Name + "-" + this.ActiveFold.ToString(); ;
            
            for (int i = 0; i < folds.Length; i++)
            {
                if (folds[i] != this.ActiveFold)
                {
                    dataStore1.Insert(dataStore.GetRecordByIndex(i));
                }
                else
                {
                    dataStore2.Insert(dataStore.GetRecordByIndex(i));
                }
            }
        }

    }

    public class DataStoreSplitterRatio : DataStoreSplitter
    {
        private double splitRatio = 0.5;

        public DataStoreSplitterRatio(DataStore dataStore, double splitRatio)
            : base (dataStore, 2)
        {
            if (splitRatio > 1 || splitRatio < 0)
                throw new ArgumentOutOfRangeException("splitRatio", "Value must be between 0 and 1");

            this.splitRatio = splitRatio;
        }

        public Double SplitRatio
        {
            get { return this.splitRatio; }
        }

        protected override Int32 RandomSplit()
        {
            return (this.splitRatio > RandomSingleton.Random.NextDouble()) ? 2 : 1;
        }
    }
}
