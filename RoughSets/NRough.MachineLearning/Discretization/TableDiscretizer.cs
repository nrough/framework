using NRough.Core;
using NRough.Core.CollectionExtensions;
using NRough.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NRough.MachineLearning.Discretization
{
    [Serializable]
    public class TableDiscretizer
    {
        #region TODO                                
        #endregion

        #region Members

        private Dictionary<int, IDiscretizer> fieldDiscretizer;

        #endregion

        #region Properties

        public IList<IDiscretizer> DiscretizerCollection { get; set; }
        public IEnumerable<int> FieldsToDiscretize { get; set; }
        public Dictionary<int, IDiscretizer> FieldDiscretizer
        {
            get { return this.fieldDiscretizer; }
            private set { this.fieldDiscretizer = value; }
        }
        public bool UpdateDataColumns { get; set; } = true;

        public bool AddColumnsBasedOnCuts { get; set; } = false;
        public bool RemoveColumnAfterDiscretization { get; set; } = false;
        public bool UseBinaryCuts { get; set; } = false;

        #endregion

        #region Constructors

        public TableDiscretizer()
        {
            this.DiscretizerCollection = new List<IDiscretizer>(
                new IDiscretizer[] {
                    new DiscretizeSupervisedBase() { NumberOfBuckets = 10 },
                    new DiscretizeEntropy(),
                    new DiscretizeEqualWidth()
                });
            this.fieldDiscretizer = new Dictionary<int, IDiscretizer>();
        }

        public TableDiscretizer(IDiscretizer discretizer)
        {
            if (discretizer == null)
                throw new ArgumentNullException("discretizer", "discretizer == null");
            this.DiscretizerCollection = new List<IDiscretizer>(new IDiscretizer[] { discretizer });
            this.fieldDiscretizer = new Dictionary<int, IDiscretizer>();
        }

        public TableDiscretizer(IEnumerable<IDiscretizer> discretizerCollection)
        {
            if (discretizerCollection == null)
                throw new ArgumentNullException("discretizerCollection", "discretizerCollection == null");
            this.DiscretizerCollection = new List<IDiscretizer>(discretizerCollection);
            this.fieldDiscretizer = new Dictionary<int, IDiscretizer>();
        }

        #endregion

        #region Methods

        public virtual IDiscretizer SelectDiscretizer(DataStore data, int fieldId, double[] weights = null)
        {
            if (this.DiscretizerCollection == null)
                throw new InvalidOperationException("this.DiscretizerCollection == null");

            if (this.DiscretizerCollection.Count == 0)
                throw new InvalidOperationException("this.DiscretizerCollection.Count == 0");

            if (data == null)
                throw new ArgumentNullException("data", "data == null");

            if (fieldId < 1)
                throw new ArgumentOutOfRangeException("fieldId", "fieldId < 1");

            AttributeInfo localFieldInfoTrain = data.DataStoreInfo.GetFieldInfo(fieldId);
            long[] labels = data.DataStoreInfo.DecisionFieldId > 0
                ? data.GetColumnInternal(data.DataStoreInfo.DecisionFieldId)
                : null;

            if (localFieldInfoTrain.CanDiscretize())
            {
                foreach (IDiscretizer discretizer in this.DiscretizerCollection)
                {
                    discretizer.Compute(data.GetColumnInternal(fieldId), labels, weights);
                    if (discretizer.Cuts != null)
                        return (IDiscretizer) discretizer.Clone();
                }
            }

            return null;
        }

        public virtual long[] GetCuts(DataStore data, int fieldId, double[] weights = null)
        {
            IDiscretizer discretizer = SelectDiscretizer(data, fieldId, weights);
            if (discretizer != null)
                return discretizer.Cuts;
            return null;
        }

        public void Discretize(DataStore dataToDiscretize, double[] weights = null)
        {
            AttributeInfo fieldInfo;
            IEnumerable<int> localFields = FieldsToDiscretize != null
                    ? FieldsToDiscretize
                    : dataToDiscretize.DataStoreInfo.SelectAttributeIds(a => a.IsStandard && a.CanDiscretize());

            long[] labels = dataToDiscretize.GetColumnInternal(dataToDiscretize.DataStoreInfo.DecisionFieldId);
            this.fieldDiscretizer = new Dictionary<int, IDiscretizer>(localFields.Count());

            foreach (int fieldId in localFields.ToList())
            {
                fieldInfo = dataToDiscretize.DataStoreInfo.GetFieldInfo(fieldId);
                if (fieldInfo.CanDiscretize())
                {
                    IDiscretizer disc = SelectDiscretizer(dataToDiscretize, fieldId, weights);
                    this.fieldDiscretizer.Add(fieldId, disc);

                    if (disc != null && disc.Cuts != null)
                    {
                        long[] continuousValues = dataToDiscretize.GetColumnInternal(fieldId);
                        if (AddColumnsBasedOnCuts)
                        {                            
                            for (int i = 1; i <= disc.Cuts.Length; i++)
                            {                                
                                long[] localCuts = UseBinaryCuts == false 
                                    ? disc.Cuts.SubArray(0, i) : disc.Cuts.SubArray(i-1, 1);
                                if(localCuts.Length > 1)
                                    Array.Sort(localCuts);

                                long[] newValues = DiscretizeBase.Apply(continuousValues, localCuts);

                                AttributeInfo newFieldInfo = dataToDiscretize.DataStoreInfo.GetFieldInfo(
                                    dataToDiscretize.AddColumn<long>(newValues));

                                newFieldInfo.IsNumeric = false;
                                newFieldInfo.IsOrdered = true;
                                newFieldInfo.Cuts = localCuts;
                                newFieldInfo.DataType = typeof(long);
                                newFieldInfo.Name = String.Format("{0}-{1}", fieldInfo.Name, i);
                                newFieldInfo.Alias = String.Format("{0}-{1}", fieldInfo.Alias, i);
                                newFieldInfo.DerivedFrom = fieldId;
                            }

                            if (RemoveColumnAfterDiscretization)
                            {                                
                                dataToDiscretize.RemoveColumn(fieldId);
                            }
                        }
                        else
                        {
                            long[] newValues = disc.Apply(continuousValues);

                            if (UpdateDataColumns)
                            {
                                fieldInfo.Cuts = disc.Cuts;
                                fieldInfo.DataType = typeof(long);
                                fieldInfo.IsNumeric = false;
                                fieldInfo.IsOrdered = true;

                                dataToDiscretize.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x));
                            }
                            else
                            {
                                AttributeInfo newFieldInfo = dataToDiscretize.DataStoreInfo.GetFieldInfo(
                                    dataToDiscretize.AddColumn<long>(newValues));

                                newFieldInfo.IsNumeric = false;
                                newFieldInfo.IsOrdered = true;
                                newFieldInfo.Cuts = disc.Cuts;
                                newFieldInfo.DataType = typeof(long);
                                newFieldInfo.Name = String.Format("{0}-{1}", fieldInfo.Name, 1);
                                newFieldInfo.Alias = String.Format("{0}-{1}", fieldInfo.Alias, 1);
                                newFieldInfo.DerivedFrom = fieldId;
                            }
                        }
                    }
                }
            }
        }        
        
        public static void Discretize(DataStore dataToDiscretize, DataStore discretizedData, IEnumerable<int> fieldsToDiscretize = null)
        {
            AttributeInfo localFieldInfoTrain, localFieldInfoTest;

            IEnumerable<int> localFields = fieldsToDiscretize == null
                                         ? dataToDiscretize.DataStoreInfo.SelectAttributeIds(a => a.IsStandard)
                                         : fieldsToDiscretize;

            foreach (int fieldId in localFields.ToList())
            {
                localFieldInfoTest = dataToDiscretize.DataStoreInfo.GetFieldInfo(fieldId);
                if (localFieldInfoTest.CanDiscretize() )
                {
                    localFieldInfoTrain = discretizedData.DataStoreInfo.GetFieldInfo(fieldId);
                    long[] continuousValues = dataToDiscretize.GetColumnInternal(fieldId);

                    if (localFieldInfoTrain != null && localFieldInfoTrain.Cuts != null)
                    {
                        long[] newValues = DiscretizeBase.Apply(continuousValues, localFieldInfoTrain.Cuts);

                        localFieldInfoTest.DataType = typeof(long);
                        localFieldInfoTest.Cuts = localFieldInfoTrain.Cuts;
                        localFieldInfoTest.IsNumeric = false;
                        localFieldInfoTest.IsOrdered = true;
                        dataToDiscretize.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x), localFieldInfoTrain);
                    }

                    
                    IEnumerable<AttributeInfo> derivedFields = discretizedData.DataStoreInfo
                        .SelectAttributes(f => f.DerivedFrom == fieldId && f.Cuts != null);

                    if (derivedFields != null)
                    {
                        foreach (var derivedField in derivedFields)
                        {
                            long[] newValues = DiscretizeBase.Apply(continuousValues, derivedField.Cuts);

                            AttributeInfo newFieldInfo = dataToDiscretize.DataStoreInfo.GetFieldInfo(
                                        dataToDiscretize.AddColumn<long>(newValues, derivedField));

                            newFieldInfo.IsNumeric = false;
                            newFieldInfo.IsOrdered = true;
                            newFieldInfo.Cuts = derivedField.Cuts;
                            newFieldInfo.DataType = typeof(long);
                            newFieldInfo.Name = derivedField.Name;
                            newFieldInfo.Alias = derivedField.Alias;
                            newFieldInfo.DerivedFrom = fieldId;
                        }
                    }

                    if (localFieldInfoTrain == null)
                    {
                        dataToDiscretize.RemoveColumn(fieldId);
                    }
                }
            }
        }        

        #endregion
    }
}
