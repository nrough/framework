using Infovision.Data;
using System;
using System.Collections.Generic;

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
                        return discretizer;
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
            this.fieldDiscretizer = new Dictionary<int, IDiscretizer>(dataToDiscretize.DataStoreInfo.NumberOfFields);
            foreach (int fieldId in localFields)
            {
                localFieldInfoTrain = dataToDiscretize.DataStoreInfo.GetFieldInfo(fieldId);
                if (localFieldInfoTrain.CanDiscretize())
                {
                    IDiscretizer disc = SelectDiscretizer(dataToDiscretize, fieldId, weights);
                    this.fieldDiscretizer.Add(fieldId, disc);

                    if (disc != null && disc.Cuts != null)
                    {
                        long[] newValues = disc.Apply(dataToDiscretize.GetColumnInternal(fieldId));

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
