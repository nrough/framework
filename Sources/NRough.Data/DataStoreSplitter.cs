// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Linq;
using NRough.Core;
using NRough.Core.Random;
using NRough.Core.CollectionExtensions;

namespace NRough.Data
{
    public interface IDataSplitter
    {
        void Split(out DataStore dataStore1, out DataStore dataStore2, int fold);
        int NFold { get; }
    }

    [Serializable]
    public class DataSplitter : IDataSplitter
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
        protected bool SplitCalculated { get; set; }
                        
        #endregion        

        #region Constructors

        public DataSplitter(DataStore dataStore, int nfold, bool useCache = false)
        {
            this.dataStore = dataStore;
            this.NFold = nfold;
            this.UseDataStoreCache = useCache;

            this.InitSplit();
        }

        #endregion

        #region Methods
                
        public virtual void Split(out DataStore dataStore1, out DataStore dataStore2, int fold = 0)
        {
            if (fold < 0) throw new ArgumentOutOfRangeException("fold");
            if (fold >= this.NFold) throw new ArgumentOutOfRangeException("fold", "fold >= this.NFold");

            Guid guid = Guid.NewGuid();
            this.GetTrainingData(out dataStore1, fold, guid);            
            this.GetTestData(out dataStore2, fold, guid);            
        }

        private void GetTrainingData(out DataStore dataStore1, int fold, Guid guid)
        {
            dataStore1 = null;

            lock (syncRoot)
            {
                if (!this.SplitCalculated)
                    this.GenerateSplit();

                if (this.UseDataStoreCache && cacheTrainDS.TryGetValue(fold, out dataStore1))                                     
                    return;

                DataStoreInfo dataStoreInfo1 = new DataStoreInfo(dataStore.DataStoreInfo.NumberOfFields);
                dataStoreInfo1.InitFromDataStoreInfo(dataStore.DataStoreInfo, true, true);
                dataStoreInfo1.NumberOfRecords = dataStore.DataStoreInfo.NumberOfRecords - foldSize[fold];

                dataStore1 = new DataStore(dataStoreInfo1);
                dataStore1.Name = dataStore.Name + "-TRN-" + fold.ToString();
                dataStore1.Fold = fold;

                for (int i = 0; i < folds.Length; i++)
                    if (folds[i] != fold)
                        dataStore1.Insert(dataStore.GetRecordByIndex(i));

                dataStore1.NormalizeWeights();
                dataStore1.CreateWeightHistogramsOnFields();
                dataStore1.TableId = guid;
                dataStore1.DatasetType = DatasetType.Training;

                if (this.UseDataStoreCache)
                    this.cacheTrainDS.Add(fold, dataStore1);
            }
        }

        private void GetTestData(out DataStore dataStore2, int fold, Guid guid)
        {                        
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
            dataStore2.Name = dataStore.Name + "-TST-" + fold.ToString();
            dataStore2.Fold = fold;

            for (int i = 0; i < folds.Length; i++)
                if (folds[i] == fold)
                    dataStore2.Insert(dataStore.GetRecordByIndex(i));

            dataStore2.NormalizeWeights();
            dataStore2.CreateWeightHistogramsOnFields();
            dataStore2.TableId = guid;
            dataStore2.DatasetType = DatasetType.Testing;

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
    public class DataSplitterRatio : DataSplitter
    {        
        public double SplitRatio { get; private set; }

        public DataSplitterRatio(DataStore dataStore, double splitRatio)
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

        public override void Split(out DataStore dataStore1, out DataStore dataStore2, int fold = 0)
        {
            base.Split(out dataStore1, out dataStore2, 0);
        }
    }
}