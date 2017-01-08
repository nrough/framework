using Infovision.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    [Serializable]
    public class DataStoreDiscretizer
    {
        #region Properties

        public IDiscretization Discretizer { get; set; }
        public IEnumerable<int> Fields2Discretize { get; set; }
        
        #endregion

        #region Constructors

        public DataStoreDiscretizer(IDiscretization discretizer)
        {
            if (discretizer == null)
                throw new ArgumentNullException("discretizer", "discretizer == null");
            this.Discretizer = discretizer;
        }

        #endregion

        #region Methods        

        public virtual long[] GetCuts(DataStore data, int fieldId, double[] weights = null)
        {
            if (this.Discretizer == null)
                throw new InvalidOperationException("this.Discretizer == null");

            if (data == null)
                throw new ArgumentNullException("data", "data == null");

            if (fieldId < 1)
                throw new ArgumentOutOfRangeException("fieldId", "fieldId < 1");

            DataFieldInfo localFieldInfoTrain = data.DataStoreInfo.GetFieldInfo(fieldId);            
            long[] labels = data.DataStoreInfo.DecisionFieldId > 0
                ? data.GetColumnInternal(data.DataStoreInfo.DecisionFieldId)
                : null;
            
            if (localFieldInfoTrain.CanDiscretize())
                this.Discretizer.Compute(data.GetColumnInternal(fieldId), labels, weights);

            return this.Discretizer.Cuts;
        }

        public void Discretize(DataStore dataToDiscretize, double[] weights = null)
        {
            DataFieldInfo localFieldInfoTrain;
            IEnumerable<int> localFields = Fields2Discretize != null
                                         ? Fields2Discretize
                                         : dataToDiscretize.DataStoreInfo.GetFieldIds(FieldTypes.Standard);

            long[] labels = dataToDiscretize.GetColumnInternal(dataToDiscretize.DataStoreInfo.DecisionFieldId);

            foreach (int fieldId in localFields)
            {
                localFieldInfoTrain = dataToDiscretize.DataStoreInfo.GetFieldInfo(fieldId);
                if (localFieldInfoTrain.CanDiscretize())
                {
                    localFieldInfoTrain.Cuts = GetCuts(dataToDiscretize, fieldId, weights);
                    long[] newValues = this.Discretizer.Apply(dataToDiscretize.GetColumnInternal(fieldId));
                    localFieldInfoTrain.FieldValueType = typeof(long);
                    localFieldInfoTrain.IsNumeric = false;
                    localFieldInfoTrain.IsOrdered = true;

                    dataToDiscretize.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x));
                }
            }
        }
        
        public static void Discretize(DataStore dataToDiscretize, DataStore discretizedData)
        {
            DataFieldInfo localFieldInfoTrain, localFieldInfoTest;
            IEnumerable<int> localFields = dataToDiscretize.DataStoreInfo.GetFieldIds(FieldTypes.Standard);

            foreach (int fieldId in localFields)
            {
                localFieldInfoTest = dataToDiscretize.DataStoreInfo.GetFieldInfo(fieldId);
                if (localFieldInfoTest.CanDiscretize() )
                {
                    localFieldInfoTrain = discretizedData.DataStoreInfo.GetFieldInfo(fieldId);

                    if (localFieldInfoTrain.Cuts == null)
                        throw new InvalidOperationException("localFieldInfoTrain.Cuts == null");
                    
                    long[] newValues = DiscretizeBase.Apply(
                        dataToDiscretize.GetColumnInternal(fieldId), 
                        localFieldInfoTrain.Cuts);
                                        
                    localFieldInfoTest.FieldValueType = typeof(long);
                    localFieldInfoTest.Cuts = localFieldInfoTrain.Cuts;
                    localFieldInfoTest.IsNumeric = false;
                    localFieldInfoTest.IsOrdered = true;

                    dataToDiscretize.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x), localFieldInfoTrain);
                }
            }
        }

        public virtual bool CanDiscretize(DataFieldInfo field)
        {
            if (field.CanDiscretize())
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
