using System;
using System.Collections.Generic;
using System.Linq;
using NRough.Math;
using NRough.Core;
using NRough.Core.Random;
using NRough.Core.CollectionExtensions;

namespace NRough.Data
{
    public class DataSampler
    {
        private int bagSizePercent;
        private Dictionary<int, DataStore> bagCache;
        private Dictionary<int, DataStore> oobCache;

        public DataStore Data { get; set; }
        public double[] Weights { get; set; }
        public bool CacheResults { get; set; }

        public int BagSizePercent
        {
            get { return this.bagSizePercent; }
            set
            {
                if (value < 1 || value > 100)
                    throw new IndexOutOfRangeException("value < 1 || value > 100");
                this.bagSizePercent = value;
            }
        }

        public DataSampler()
        {
            this.BagSizePercent = 100;
            this.Weights = null;
            this.CacheResults = false;
        }

        public DataSampler(DataStore data)
            : this()
        {
            this.Data = data;
            this.Weights = (data.Weights != null) ? data.Weights : this.Weights;
        }

        public DataSampler(DataStore data, bool cacheDataChunks)
            : this(data)
        {
            this.CacheResults = cacheDataChunks;
            if (this.CacheResults)
            {
                bagCache = new Dictionary<int, DataStore>();
                oobCache = new Dictionary<int, DataStore>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iteration"></param>
        /// <returns>
        /// Returns a Tuple where first item is DataStore representing bagged data and the second item represents out of the bag instances (OOB)
        /// </returns>
        public Tuple<DataStore, DataStore> GetData(int iteration)
        {
            DataStore baggedData = null;
            DataStore oobData = null;

            if (this.CacheResults
                && bagCache.TryGetValue(iteration, out baggedData)
                && oobCache.TryGetValue(iteration, out oobData))
            {
                return new Tuple<DataStore, DataStore>(baggedData, oobData);
            }

            double[] localWeigths = null;
            if (this.Weights != null)
                localWeigths = this.Weights;
            else if (this.Data.Weights != null)
                localWeigths = this.Data.Weights;

            if (localWeigths == null)
            {
                localWeigths = new double[this.Data.NumberOfRecords];
                localWeigths.SetAll(1.0 / (double)this.Data.NumberOfRecords);
            }

            baggedData = this.ResampleWithWeights(localWeigths);
            baggedData.Shuffle();

            int bagsize = (int)(this.Data.NumberOfRecords * ((double)this.BagSizePercent / 100.0));
            bagsize = System.Math.Min(bagsize, this.Data.NumberOfRecords);

            baggedData = DataStore.Copy(baggedData, 0, bagsize);

            HashSet<long> oobSet = new HashSet<long>(this.Data.GetObjectIds());
            HashSet<long> bagSet = new HashSet<long>(baggedData.GetOrigObjectIds());
            oobSet.ExceptWith(bagSet);

            DataStoreInfo localDataStoreInfo = new DataStoreInfo(baggedData.DataStoreInfo.NumberOfFields);
            localDataStoreInfo.InitFromDataStoreInfo(baggedData.DataStoreInfo, true, true);
            localDataStoreInfo.NumberOfRecords = oobSet.Count;

            oobData = new DataStore(localDataStoreInfo);
            oobData.Name = baggedData.Name;

            int j = 0;
            foreach (long objectId in oobSet)
            {
                DataRecordInternal instance = this.Data.GetRecordByObjectId(objectId);
                oobData.Insert(instance);
                oobData.SetWeight(j++, this.Data.GetWeightByObjectId(objectId));
            }

            oobData.NormalizeWeights();
            oobData.CreateWeightHistogramsOnFields();

            if (this.CacheResults)
            {
                this.bagCache[iteration] = baggedData;
                this.oobCache[iteration] = oobData;
            }

            return new Tuple<DataStore, DataStore>(baggedData, oobData);
        }

        private DataStore ResampleWithWeights(double[] weights)
        {
            if (weights.Length != this.Data.NumberOfRecords)
                throw new ArgumentException("weights.Length != this.Data.NumberOfRecords", "weights");

            DataStoreInfo localDataStoreInfo = new DataStoreInfo(this.Data.DataStoreInfo.NumberOfFields);
            localDataStoreInfo.InitFromDataStoreInfo(this.Data.DataStoreInfo, true, true);
            localDataStoreInfo.NumberOfRecords = this.Data.NumberOfRecords;

            DataStore localDataStore = new DataStore(localDataStoreInfo);
            localDataStore.Name = this.Data.Name;

            double[] probabilities = new double[this.Data.NumberOfRecords];
            double sumProbs = 0;
            double sumOfWeights = (double)weights.Sum();
            for (int i = 0; i < this.Data.NumberOfRecords; i++)
            {
                sumProbs += RandomSingleton.Random.NextDouble();
                probabilities[i] = sumProbs;
            }

            Tools.Normalize(probabilities, sumProbs / sumOfWeights);
            probabilities[this.Data.NumberOfRecords - 1] = sumOfWeights;

            HashSet<long> inBagSet = new HashSet<long>();
            int k = 0; int l = 0;
            sumProbs = 0;
            while ((k < this.Data.NumberOfRecords && (l < this.Data.NumberOfRecords)))
            {
                sumProbs += (double)weights[l];
                while ((k < this.Data.NumberOfRecords) && (probabilities[k] <= sumProbs))
                {
                    localDataStore.InsertBag(this.Data.GetRecordByIndex(l, true));
                    localDataStore.SetWeight(k++, 1);                    
                }
                l++;
            }

            localDataStore.NormalizeWeights();
            localDataStore.CreateWeightHistogramsOnFields();

            return localDataStore;
        }        
    }
}