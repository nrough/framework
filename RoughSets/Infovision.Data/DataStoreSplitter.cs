using System;
using System.Collections.Generic;
using System.Linq;
using Infovision.Core;

namespace Infovision.Data
{
    public delegate void PostSplitMethod(DataStore train, DataStore test);

    public interface IDataStoreSplitter
    {
        void Split(ref DataStore dataStore1, ref DataStore dataStore2, int fold);
        PostSplitMethod PostSplitMethod { get; set;}
        int NFold { get; }
    }

    [Serializable]
    public class DataStoreSplitter : IDataStoreSplitter
    {
        #region Members

        private DataStore dataStore;
        private int[] folds;
        private int[] foldSize;
        private int nfold;

        private Dictionary<int, DataStore> cacheTrainDS;
        private Dictionary<int, DataStore> cacheTestDS;

        private readonly object syncRoot = new object();

        #endregion

        #region Properties
                        
        public int NFold
        {
            get { return this.nfold; }
            set
            {
                if (value <= 1)
                    throw new InvalidOperationException("Number of folds must be greater that one.");

                if (value != this.nfold)
                {
                    lock (syncRoot)
                    {
                        if (value != this.nfold)
                        {
                            this.SplitCalculated = false;
                            this.nfold = value;

                            if (this.nfold > this.dataStore.NumberOfRecords)
                                this.nfold = this.dataStore.NumberOfRecords;
                        }
                    }
                }
            }
        }

        public bool UseDataStoreCache { get; private set; }
        public PostSplitMethod PostSplitMethod { get; set; }
        protected bool SplitCalculated { get; set; }
                        
        #endregion        

        #region Constructors

        public DataStoreSplitter(DataStore dataStore, int nfold, bool useCache = false)
        {
            this.dataStore = dataStore;
            this.NFold = nfold;
            this.UseDataStoreCache = useCache;

            this.InitSplit();
        }

        #endregion

        #region Methods
                
        public virtual void Split(ref DataStore dataStore1, ref DataStore dataStore2, int fold = 0)
        {            
            this.GetTrainingData(ref dataStore1, fold);
            this.GetTestData(ref dataStore2, fold);

            if (this.PostSplitMethod != null)            
                this.PostSplitMethod(dataStore1, dataStore2);
        }

        private void GetTrainingData(ref DataStore dataStore1, int fold)
        {           
            if (fold < 0)
                throw new ArgumentOutOfRangeException("fold");

            if (!this.SplitCalculated)
                this.GenerateSplit();

            if (this.UseDataStoreCache && this.cacheTestDS.ContainsKey(fold))
            {
                dataStore1 = this.cacheTrainDS[fold];
                return;
            }

            DataStoreInfo dataStoreInfo1 = new DataStoreInfo(dataStore.DataStoreInfo.NumberOfFields);
            dataStoreInfo1.InitFromDataStoreInfo(dataStore.DataStoreInfo, true, true);
            dataStoreInfo1.NumberOfRecords = dataStore.DataStoreInfo.NumberOfRecords - foldSize[fold];

            dataStore1 = new DataStore(dataStoreInfo1);
            dataStore1.Name = dataStore.Name + "-" + fold.ToString();
            dataStore1.Fold = fold;

            for (int i = 0; i < folds.Length; i++)
                if (folds[i] != fold)
                    dataStore1.Insert(dataStore.GetRecordByIndex(i));

            dataStore1.NormalizeWeights();
            dataStore1.CreateWeightHistogramsOnFields();

            if (this.UseDataStoreCache && !this.cacheTrainDS.ContainsKey(fold))
            {
                lock (syncRoot)
                {
                    if(!this.cacheTrainDS.ContainsKey(fold))
                        this.cacheTrainDS.Add(fold, dataStore1);
                }
            }
        }

        private void GetTestData(ref DataStore dataStore2, int fold)
        {            
            if (fold < 0)
                throw new ArgumentOutOfRangeException("fold");

            if (!this.SplitCalculated)
                this.GenerateSplit();

            if (this.UseDataStoreCache && this.cacheTestDS.ContainsKey(fold))
            {
                dataStore2 = this.cacheTestDS[fold];
                return;
            }

            DataStoreInfo dataStoreInfo2 = new DataStoreInfo(dataStore.DataStoreInfo.NumberOfFields);
            dataStoreInfo2.InitFromDataStoreInfo(dataStore.DataStoreInfo, true, true);
            dataStoreInfo2.NumberOfRecords = foldSize[fold];

            dataStore2 = new DataStore(dataStoreInfo2);
            dataStore2.Name = dataStore.Name + "-" + fold.ToString();
            dataStore2.Fold = fold;

            for (int i = 0; i < folds.Length; i++)
                if (folds[i] == fold)
                    dataStore2.Insert(dataStore.GetRecordByIndex(i));

            dataStore2.NormalizeWeights();
            dataStore2.CreateWeightHistogramsOnFields();

            if (this.UseDataStoreCache && !this.cacheTestDS.ContainsKey(fold))
            {
                lock (syncRoot)
                {
                    if (!this.cacheTestDS.ContainsKey(fold))
                        this.cacheTestDS.Add(fold, dataStore2);
                }
            }
        }

        private void InitSplit()
        {
            lock (syncRoot)
            {
                if (this.UseDataStoreCache)
                {
                    this.cacheTrainDS = new Dictionary<int, DataStore>(this.NFold);
                    this.cacheTestDS = new Dictionary<int, DataStore>(this.NFold);
                }

                folds = new int[this.dataStore.DataStoreInfo.NumberOfRecords];
                foldSize = new int[this.nfold];
            }
        }

        protected virtual int RandomSplit(int index)
        {
            return (index % nfold);
        }

        protected virtual void GenerateSplit()
        {
            lock (syncRoot)
            {
                if (!this.SplitCalculated)
                {
                    if (this.dataStore.DataStoreInfo.DecisionFieldId > 0)
                    {
                        foreach (long decisionValue in this.dataStore.DataStoreInfo.GetDecisionValues())
                        {
                            int[] objectsTmp = this.GetObjectIndexes(this.dataStore, decisionValue).ToArray();
                            objectsTmp.Shuffle();
                            for (int i = 0; i < objectsTmp.Length; i++)
                                folds[objectsTmp[i]] = RandomSplit(i);
                        }

                        for (int i = 0; i < this.dataStore.DataStoreInfo.NumberOfRecords; i++)
                            foldSize[folds[i]]++;
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
            }
        }

        protected IEnumerable<int> GetObjectIndexes(DataStore dataStore, long decisionValue)
        {
            List<int> result = new List<int>(dataStore.DataStoreInfo.NumberOfObjectsWithDecision(decisionValue));
            for (int objectIdx = 0; objectIdx < dataStore.NumberOfRecords; objectIdx++)
            {
                if (decisionValue == dataStore.GetDecisionValue(objectIdx))
                    result.Add(objectIdx);
            }
            return result;
        }

        #endregion
    }

    [Serializable]
    public class DataStoreSplitterRatio : DataStoreSplitter
    {        
        public double SplitRatio { get; private set; }

        public DataStoreSplitterRatio(DataStore dataStore, double splitRatio)
            : base(dataStore, 2)
        {
            if (splitRatio > 1 || splitRatio < 0)
                throw new ArgumentOutOfRangeException("splitRatio", "Value must be between 0 and 1");
            this.SplitRatio = splitRatio;            
        }        

        protected override int RandomSplit(int index)
        {
            return (this.SplitRatio > RandomSingleton.Random.NextDouble()) ? 1 : 0;
        }
    }
}