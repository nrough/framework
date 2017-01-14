using Infovision.Core;
using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infovision.MachineLearning.Discretization
{
    [Serializable]
    public class DataStoreDiscretizer
    {
        #region TODO
                
        //example of some discretization automation
        //http://library.bayesia.com/display/FAQ/Choosing+a+Discretization+Algorithm
        //Supervised
        //1. Decision Tree Discretization (Single attribute)
        //2. MDL - Fayyad & Irani or Kononenko - OK
        //Unsupervised
        //2. Density Approximation - 
        //3. K-Means
        //4. Normalized Equal Distances
        //5. Equal distances
        //6. Equal frequencies - not recommended
        
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

        public bool NoFuckinIdeaHowToCallIt { get; set; } = false;
        public bool BinaryCuts { get; set; } = false;

        #endregion

        #region Constructors

        public DataStoreDiscretizer()
        {
            this.DiscretizerCollection = new List<IDiscretizer>(
                new IDiscretizer[] {
                    new DiscretizeFayyad(),
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
            DataFieldInfo localFieldInfoTrain;
            IEnumerable<int> localFields = Fields2Discretize != null
                                         ? Fields2Discretize
                                         : dataToDiscretize.DataStoreInfo.GetFieldIds(FieldTypes.Standard);

            long[] labels = dataToDiscretize.GetColumnInternal(dataToDiscretize.DataStoreInfo.DecisionFieldId);
            this.fieldDiscretizer = new Dictionary<int, IDiscretizer>(
                dataToDiscretize.DataStoreInfo.Fields.Count(f => f.CanDiscretize()));

            foreach (int fieldId in localFields)
            {
                localFieldInfoTrain = dataToDiscretize.DataStoreInfo.GetFieldInfo(fieldId);
                if (localFieldInfoTrain.CanDiscretize())
                {
                    IDiscretizer disc = SelectDiscretizer(dataToDiscretize, fieldId, weights);
                    this.fieldDiscretizer.Add(fieldId, disc);

                    if (disc != null && disc.Cuts != null)
                    {
                        long[] continuousValues = dataToDiscretize.GetColumnInternal(fieldId);
                        if (NoFuckinIdeaHowToCallIt)
                        {                            
                            for (int i = 1; i <= disc.Cuts.Length; i++)
                            {                                
                                long[] localCuts = BinaryCuts == false ? disc.Cuts.SubArray(0, i) : disc.Cuts.SubArray(i-1, 1);
                                if(localCuts.Length > 1)
                                    Array.Sort(localCuts);

                                long[] newValues = DiscretizeBase.Apply(continuousValues, localCuts);

                                DataFieldInfo newFieldInfo = dataToDiscretize.DataStoreInfo.GetFieldInfo(
                                    dataToDiscretize.AddColumn<long>(newValues));

                                newFieldInfo.IsNumeric = false;
                                newFieldInfo.IsOrdered = true;
                                newFieldInfo.Cuts = localCuts;
                                newFieldInfo.FieldValueType = typeof(long);
                                newFieldInfo.Name = String.Format("{0}-{1}{2}", localFieldInfoTrain.Name, "New", i);
                                newFieldInfo.Alias = String.Format("{0}-{1}{2}", localFieldInfoTrain.Alias, "New", i);
                                newFieldInfo.DerivedFrom = fieldId;
                            }
                        }
                        else
                        {
                            long[] newValues = disc.Apply(continuousValues);

                            if (UpdateDataColumns)
                            {
                                localFieldInfoTrain.Cuts = disc.Cuts;
                                localFieldInfoTrain.FieldValueType = typeof(long);
                                localFieldInfoTrain.IsNumeric = false;
                                localFieldInfoTrain.IsOrdered = true;

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
                                newFieldInfo.Name = String.Format("{0}-{1}", localFieldInfoTrain.Name, "New");
                                newFieldInfo.Alias = String.Format("{0}-{1}", localFieldInfoTrain.Alias, "New");
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
                                         ? dataToDiscretize.DataStoreInfo.GetFieldIds(FieldTypes.Standard)
                                         : fieldsToDiscretize;

            foreach (int fieldId in localFields)
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

                    foreach(var derivedField in derivedFields)
                    {                       
                        long[] newValues = DiscretizeBase.Apply(continuousValues, localFieldInfoTrain.Cuts);

                        //TODO Set the same field Id
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

        #endregion
    }
}
