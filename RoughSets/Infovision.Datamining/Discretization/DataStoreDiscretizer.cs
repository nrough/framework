﻿using Infovision.Data;
using System;
using System.Collections.Generic;

namespace Infovision.MachineLearning.Discretization
{
    [Serializable]
    public class DataStoreDiscretizer
    {
        #region TODO
                
        //http://library.bayesia.com/display/FAQ/Choosing+a+Discretization+Algorithm
        //Supervised
        //1. MDL - Fayyad & Irani or Kononenko - OK
        //Unsupervised
        //2. Density Approximation - 
        //3. K-Means
        //4. Normalized Equal Distances
        //5. Equal distances
        //6. Equal frequences - not recomended

        //TODO Create a method returning a report which field was discretized with what method and what are the cuts        
        #endregion

        #region Members

        private Dictionary<int, IDiscretization> fieldCuts;

        #endregion

        #region Properties

        public IList<IDiscretization> DiscretizerCollection { get; set; }
        public IEnumerable<int> Fields2Discretize { get; set; }
        public Dictionary<int, IDiscretization> FieldCuts
        {
            get { return this.fieldCuts; }
            private set { this.fieldCuts = value; }
        }

        #endregion

        #region Constructors

        public DataStoreDiscretizer()
        {
            this.DiscretizerCollection = new List<IDiscretization>(
                new IDiscretization[] {
                    new DiscretizeFayyad(),
                    new DiscretizeEntropy(),
                    new DiscretizeEqualWidth()
                });
            this.fieldCuts = new Dictionary<int, IDiscretization>();
        }

        public DataStoreDiscretizer(IDiscretization discretizer)
        {
            if (discretizer == null)
                throw new ArgumentNullException("discretizer", "discretizer == null");
            this.DiscretizerCollection = new List<IDiscretization>(new IDiscretization[] { discretizer });
            this.fieldCuts = new Dictionary<int, IDiscretization>();
        }

        public DataStoreDiscretizer(IEnumerable<IDiscretization> discretizerCollection)
        {
            if (discretizerCollection == null)
                throw new ArgumentNullException("discretizerCollection", "discretizerCollection == null");
            this.DiscretizerCollection = new List<IDiscretization>(discretizerCollection);
            this.fieldCuts = new Dictionary<int, IDiscretization>();
        }

        #endregion

        #region Methods

        public virtual IDiscretization SelectDiscretizer(DataStore data, int fieldId, double[] weights = null)
        {
            if (this.DiscretizerCollection == null)
                throw new InvalidOperationException("this.DiscretizerCollection == null");

            if (this.DiscretizerCollection.Count == 0)
                throw new InvalidOperationException("this.DiscretizerCollection.Count == 0");

            if (data == null)
                throw new ArgumentNullException("data", "data == null");

            if (fieldId < 1)
                throw new ArgumentOutOfRangeException("fieldId", "fieldId < 1");

            DataFieldInfo localFieldInfoTrain = data.DataStoreInfo.GetFieldInfo(fieldId);
            long[] labels = data.DataStoreInfo.DecisionFieldId > 0
                ? data.GetColumnInternal(data.DataStoreInfo.DecisionFieldId)
                : null;

            if (localFieldInfoTrain.CanDiscretize())
            {
                foreach (IDiscretization discretizer in this.DiscretizerCollection)
                {
                    discretizer.Compute(data.GetColumnInternal(fieldId), labels, weights);
                    if (discretizer.Cuts != null)
                        return discretizer;
                }
            }

            return null;
        }

        public virtual long[] GetCuts(DataStore data, int fieldId, double[] weights = null)
        {
            IDiscretization discretizer = SelectDiscretizer(data, fieldId, weights);
            if (discretizer != null)
                return discretizer.Cuts;
            return null;
        }

        public void Discretize(DataStore dataToDiscretize, double[] weights = null)
        {
            DataFieldInfo localFieldInfoTrain;
            IEnumerable<int> localFields = Fields2Discretize != null
                                         ? Fields2Discretize
                                         : dataToDiscretize.DataStoreInfo.GetFieldIds(FieldTypes.Standard);

            long[] labels = dataToDiscretize.GetColumnInternal(dataToDiscretize.DataStoreInfo.DecisionFieldId);
            this.fieldCuts = new Dictionary<int, IDiscretization>(dataToDiscretize.DataStoreInfo.NumberOfFields);
            foreach (int fieldId in localFields)
            {
                localFieldInfoTrain = dataToDiscretize.DataStoreInfo.GetFieldInfo(fieldId);
                if (localFieldInfoTrain.CanDiscretize())
                {
                    IDiscretization disc = SelectDiscretizer(dataToDiscretize, fieldId, weights);
                    this.fieldCuts.Add(fieldId, disc);

                    if (disc != null && disc.Cuts != null)
                    {                                        
                        localFieldInfoTrain.Cuts = disc.Cuts;
                        long[] newValues = disc.Apply(dataToDiscretize.GetColumnInternal(fieldId));

                        localFieldInfoTrain.FieldValueType = typeof(long);
                        localFieldInfoTrain.IsNumeric = false;
                        localFieldInfoTrain.IsOrdered = true;

                        dataToDiscretize.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x));
                    }
                }
            }
        }        
        
        public static void Discretize(DataStore dataToDiscretize, DataStore discretizedData, IEnumerable<int> fieldsToDiscretize = null)
        {
            DataFieldInfo localFieldInfoTrain, localFieldInfoTest;

            IEnumerable<int> localFields = fieldsToDiscretize == null
                                         ? dataToDiscretize.DataStoreInfo.GetFieldIds(FieldTypes.Standard)
                                         : fieldsToDiscretize;

            foreach (int fieldId in localFields)
            {
                localFieldInfoTest = dataToDiscretize.DataStoreInfo.GetFieldInfo(fieldId);
                if (localFieldInfoTest.CanDiscretize() )
                {
                    localFieldInfoTrain = discretizedData.DataStoreInfo.GetFieldInfo(fieldId);

                    if (localFieldInfoTrain.Cuts != null)
                    {
                        long[] newValues = DiscretizeBase.Apply(
                            dataToDiscretize.GetColumnInternal(fieldId), localFieldInfoTrain.Cuts);

                        localFieldInfoTest.FieldValueType = typeof(long);
                        localFieldInfoTest.Cuts = localFieldInfoTrain.Cuts;
                        localFieldInfoTest.IsNumeric = false;
                        localFieldInfoTest.IsOrdered = true;
                        dataToDiscretize.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x), localFieldInfoTrain);
                    }
                }
            }
        }        

        #endregion
    }
}
