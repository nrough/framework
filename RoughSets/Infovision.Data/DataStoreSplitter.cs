using System;
using System.Linq;
using System.Threading.Tasks;
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

        private int activeFold = -1;
        protected bool SplitCalculated { get; set; }
        
        public DataStoreSplitter(DataStore dataStore, int nfold)
        {
            this.dataStore = dataStore;
            this.nfold = nfold;
            
            this.InitSplit();
        }

        public int NFold
        {
            get { return this.nfold; }
            set 
            {
                if (value <= 1)
                    throw new InvalidOperationException("Number of folds must be greater that one.");

                if (value != this.nfold)
                {
                    this.SplitCalculated = false;
                    this.nfold = value;
                }
            }
        }

        public int ActiveFold
        {
            get { return activeFold; }
            set 
            {
                if (value < 0 || value > this.nfold - 1)
                    throw new IndexOutOfRangeException(String.Format("ActiveFold must have value from {0} to {1}", 0, this.nfold - 1));
                
                this.activeFold = value; 
            }
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

        protected virtual int RandomSplit(int index)
        {
            return (index % nfold);
        }

        protected virtual void GenerateSplit()
        {
            if (this.dataStore.DataStoreInfo.DecisionFieldId > 0)
            {
                foreach (long decisionValue in this.dataStore.DataStoreInfo.GetDecisionValues())
                {
                    int[] objectsTmp = this.DataStore.GetObjectIndexes(decisionValue).ToArray();
                    objectsTmp.Shuffle();
                    Parallel.For(0, objectsTmp.Length, i =>
                    {
                        folds[objectsTmp[i]] = RandomSplit(i);
                    });
                }

                for (int i = 0; i < this.dataStore.DataStoreInfo.NumberOfRecords; i++)
                {
                    foldSize[folds[i]]++;
                }
            }
            else
            {
                for (int i = 0; i < this.dataStore.DataStoreInfo.NumberOfRecords; i++)
                {
                    folds[i] = RandomSplit(i);
                    foldSize[folds[i]]++;
                }

                folds.Shuffle();
            }

            this.SplitCalculated = true;
        }        

        public virtual void Split(ref DataStore dataStore1, ref DataStore dataStore2)
        {
            if (ActiveFold < 0)
                throw new InvalidOperationException("Active folde was not set. Set ActiveFold before calling Split method.");

            if(!this.SplitCalculated)
                this.GenerateSplit();

            DataStoreInfo dataStoreInfo1 = new DataStoreInfo(dataStore.DataStoreInfo.NumberOfFields);
            dataStoreInfo1.InitFromDataStoreInfo(dataStore.DataStoreInfo, true, true);
            dataStoreInfo1.NumberOfRecords = dataStore.DataStoreInfo.NumberOfRecords - foldSize[this.ActiveFold];

            dataStore1 = new DataStore(dataStoreInfo1);
            dataStore1.Name = dataStore.Name + "-" + this.ActiveFold.ToString();

            DataStoreInfo dataStoreInfo2 = new DataStoreInfo(dataStore.DataStoreInfo.NumberOfFields);
            dataStoreInfo2.InitFromDataStoreInfo(dataStore.DataStoreInfo, true, true);
            dataStoreInfo2.NumberOfRecords = foldSize[this.ActiveFold];
           
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

        public virtual void GetTrainingData(ref DataStore dataStore1)
        {
            if (ActiveFold < 0)
                throw new InvalidOperationException("Active folde was not set. Set ActiveFold before calling Split method.");

            if (!this.SplitCalculated)
                this.GenerateSplit();

            DataStoreInfo dataStoreInfo1 = new DataStoreInfo(dataStore.DataStoreInfo.NumberOfFields);
            dataStoreInfo1.InitFromDataStoreInfo(dataStore.DataStoreInfo, true, true);
            dataStoreInfo1.NumberOfRecords = dataStore.DataStoreInfo.NumberOfRecords - foldSize[this.ActiveFold];

            dataStore1 = new DataStore(dataStoreInfo1);
            dataStore1.Name = dataStore.Name + "-" + this.ActiveFold.ToString();

            for (int i = 0; i < folds.Length; i++)
            {
                if (folds[i] != this.ActiveFold)
                {
                    dataStore1.Insert(dataStore.GetRecordByIndex(i));
                }
            }
        }

        public virtual void GetTestData(ref DataStore dataStore2)
        {
            if (ActiveFold < 0)
                throw new InvalidOperationException("Active folde was not set. Set ActiveFold before calling Split method.");

            if (!this.SplitCalculated)
                this.GenerateSplit();

            DataStoreInfo dataStoreInfo2 = new DataStoreInfo(dataStore.DataStoreInfo.NumberOfFields);
            dataStoreInfo2.InitFromDataStoreInfo(dataStore.DataStoreInfo, true, true);
            dataStoreInfo2.NumberOfRecords = foldSize[this.ActiveFold];

            dataStore2 = new DataStore(dataStoreInfo2);
            dataStore2.Name = dataStore.Name + "-" + this.ActiveFold.ToString(); ;

            for (int i = 0; i < folds.Length; i++)
            {
                if (folds[i] == this.ActiveFold)
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
            this.ActiveFold = 0;
        }

        public double SplitRatio
        {
            get { return this.splitRatio; }
        }

        protected override int RandomSplit(int index)
        {
            return (this.splitRatio > RandomSingleton.Random.NextDouble()) ? 1 : 0;
        }        
    }
}
