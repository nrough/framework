﻿using Raccoon.Core;
using Raccoon.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raccoon.MachineLearning.Discretization
{
    [Serializable]
    public class DataStoreDiscretizer
    {
        #region TODO                                
        #endregion

        #region Members

        private Dictionary<int, IDiscretizer> fieldDiscretizer;

        #endregion

        #region Properties

        public IList<IDiscretizer> DiscretizerCollection { get; set; }
        public IEnumerable<int> Fields2Discretize { get; set; }
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

        public DataStoreDiscretizer()
        {
            this.DiscretizerCollection = new List<IDiscretizer>(
                new IDiscretizer[] {
                    new DiscretizeSupervisedBase() { NumberOfBuckets = 10 },
                    new DiscretizeEntropy(),
                    new DiscretizeEqualWidth()
                });
            this.fieldDiscretizer = new Dictionary<int, IDiscretizer>();
        }

        public DataStoreDiscretizer(IDiscretizer discretizer)
        {
            if (discretizer == null)
                throw new ArgumentNullException("discretizer", "discretizer == null");
            this.DiscretizerCollection = new List<IDiscretizer>(new IDiscretizer[] { discretizer });
            this.fieldDiscretizer = new Dictionary<int, IDiscretizer>();
        }

        public DataStoreDiscretizer(IEnumerable<IDiscretizer> discretizerCollection)
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

            DataFieldInfo localFieldInfoTrain = data.DataStoreInfo.GetFieldInfo(fieldId);
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
            DataFieldInfo fieldInfo;
            IEnumerable<int> localFields = Fields2Discretize != null
                    ? Fields2Discretize
                    : dataToDiscretize.DataStoreInfo.GetFields(FieldGroup.Standard)
                        .Where(f => f.CanDiscretize()).Select(fld => fld.Id);

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
                                long[] localCuts = UseBinaryCuts == false ? disc.Cuts.SubArray(0, i) : disc.Cuts.SubArray(i-1, 1);
                                if(localCuts.Length > 1)
                                    Array.Sort(localCuts);

                                long[] newValues = DiscretizeBase.Apply(continuousValues, localCuts);

                                DataFieldInfo newFieldInfo = dataToDiscretize.DataStoreInfo.GetFieldInfo(
                                    dataToDiscretize.AddColumn<long>(newValues));

                                newFieldInfo.IsNumeric = false;
                                newFieldInfo.IsOrdered = true;
                                newFieldInfo.Cuts = localCuts;
                                newFieldInfo.FieldValueType = typeof(long);
                                newFieldInfo.Name = String.Format("{0}-{1}", fieldInfo.Name, i);
                                newFieldInfo.Alias = String.Format("{0}-{1}", fieldInfo.Alias, i);
                                newFieldInfo.DerivedFrom = fieldId;
                            }

                            if (RemoveColumnAfterDiscretization)
                            {
                                //TODO add column to delete list and delete at the end
                                dataToDiscretize.RemoveColumn(fieldId);
                            }
                        }
                        else
                        {
                            long[] newValues = disc.Apply(continuousValues);

                            if (UpdateDataColumns)
                            {
                                fieldInfo.Cuts = disc.Cuts;
                                fieldInfo.FieldValueType = typeof(long);
                                fieldInfo.IsNumeric = false;
                                fieldInfo.IsOrdered = true;

                                dataToDiscretize.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x));
                            }
                            else
                            {
                                DataFieldInfo newFieldInfo = dataToDiscretize.DataStoreInfo.GetFieldInfo(
                                    dataToDiscretize.AddColumn<long>(newValues));

                                newFieldInfo.IsNumeric = false;
                                newFieldInfo.IsOrdered = true;
                                newFieldInfo.Cuts = disc.Cuts;
                                newFieldInfo.FieldValueType = typeof(long);
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
            DataFieldInfo localFieldInfoTrain, localFieldInfoTest;

            IEnumerable<int> localFields = fieldsToDiscretize == null
                                         ? dataToDiscretize.DataStoreInfo.GetFieldIds(FieldGroup.Standard)
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

                        localFieldInfoTest.FieldValueType = typeof(long);
                        localFieldInfoTest.Cuts = localFieldInfoTrain.Cuts;
                        localFieldInfoTest.IsNumeric = false;
                        localFieldInfoTest.IsOrdered = true;
                        dataToDiscretize.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x), localFieldInfoTrain);
                    }
                    
                    IEnumerable<DataFieldInfo> derivedFields = discretizedData.DataStoreInfo
                        .GetFields(f => f.DerivedFrom == fieldId && f.Cuts != null);

                    if (derivedFields != null)
                    {
                        foreach (var derivedField in derivedFields)
                        {
                            long[] newValues = DiscretizeBase.Apply(continuousValues, derivedField.Cuts);

                            DataFieldInfo newFieldInfo = dataToDiscretize.DataStoreInfo.GetFieldInfo(
                                        dataToDiscretize.AddColumn<long>(newValues, derivedField));

                            newFieldInfo.IsNumeric = false;
                            newFieldInfo.IsOrdered = true;
                            newFieldInfo.Cuts = derivedField.Cuts;
                            newFieldInfo.FieldValueType = typeof(long);
                            newFieldInfo.Name = derivedField.Name;
                            newFieldInfo.Alias = derivedField.Alias;
                            newFieldInfo.DerivedFrom = fieldId;
                        }
                    }
                }
            }
        }        

        #endregion
    }
}
