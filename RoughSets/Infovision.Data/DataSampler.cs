using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Utils;
using Infovision.Statistics;

namespace Infovision.Data
{
	public class DataSampler
	{
		private int bagSizePercent;

		public DataStore Data { get; set; }
		public double[] Weights { get; set; }
		public int BagSizePercent 
		{ 
			get { return this.bagSizePercent; }
			set
			{
				if (value < 1 || value > 100)
					throw new IndexOutOfRangeException("Value of bag size should be in range <1, 100>");
				this.bagSizePercent = value;
			}
		}

		public DataSampler()
		{
			this.BagSizePercent = 100;
		}

		public DataSampler(DataStore data)
			: this()
		{
			this.Data = data;
		}

		public DataStore GetData(int iteration)
		{
			int bagsize = this.Data.NumberOfRecords * (this.BagSizePercent / 100);

			double[] localWeigths = null;
			if (this.Weights != null)
				localWeigths = this.Weights;
			else if(this.Data.Weights != null)
				localWeigths = Array.ConvertAll(this.Data.Weights, x => (double)x);
			if(localWeigths == null)
			{
				localWeigths = new double[this.Data.NumberOfRecords];
				localWeigths.SetAll((double)1/(double)this.Data.NumberOfRecords);
			}

			DataStore baggedData = this.ResampleWithWeights(localWeigths);
			baggedData.Shuffle();
			baggedData = DataStore.Copy(baggedData, 0, bagsize);

			return baggedData;
		}

		private DataStore ResampleWithWeights(double[] weights)
		{
			if(weights.Length != this.Data.NumberOfRecords)
				throw new ArgumentException("weights.Length != this.Data.NumberOfRecords", "weights");

			DataStoreInfo localDataStoreInfo = new DataStoreInfo(this.Data.DataStoreInfo.NumberOfFields);
			localDataStoreInfo.InitFromDataStoreInfo(this.Data.DataStoreInfo, true, true);
			localDataStoreInfo.NumberOfRecords = this.Data.NumberOfRecords;

			DataStore localDataStore = new DataStore(localDataStoreInfo);
			localDataStore.Name = this.Data.Name;

			double[] probabilities = new double[this.Data.NumberOfRecords];
			double sumProbs = 0;
			double sumOfWeights = (double) weights.Sum();
			for (int i = 0; i < this.Data.NumberOfRecords; i++)
			{
				sumProbs += RandomSingleton.Random.NextDouble();
				probabilities[i] = sumProbs;
			}

			Tools.Normalize(probabilities, sumProbs / sumOfWeights);
			probabilities[this.Data.NumberOfRecords - 1] = sumOfWeights;

			int k = 0; int l = 0;
			sumProbs = 0;
			while ((k < this.Data.NumberOfRecords && (l < this.Data.NumberOfRecords)))
			{
				sumProbs += (double) weights[l];
				while ((k < this.Data.NumberOfRecords) && (probabilities[k] <= sumProbs))
				{
					localDataStore.Insert(this.Data.GetRecordByIndex(l, false));
					localDataStore.SetWeight(k, 1);
					k++;
				}
				l++;
			}

			localDataStore.NormalizeWeights();
			localDataStore.CreateWeightHistogramsOnFields();

			return localDataStore;
		}
	}
}
