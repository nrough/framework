using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Filters.Supervised.Attribute
{    
    public class DataStoreDiscretizer
    {
    
        public virtual bool UseBetterEncoding { get; set; }
        public virtual bool UseKononenko { get; set; }
        public virtual IEnumerable<int> Fields2Discretize { get; set; }

        public DataStoreDiscretizer()
        {
            UseBetterEncoding = false;
            UseKononenko = true;
        }        
        
        public virtual void Discretize(ref DataStore trainingData, ref DataStore testData, double[] weights = null)
        {
            DataFieldInfo localFieldInfoTrain, localFieldInfoTest;
            IEnumerable<int> localFields = Fields2Discretize != null
                                         ? Fields2Discretize
                                         : trainingData.DataStoreInfo.GetFieldIds(FieldTypes.Standard);

            long[] labels = trainingData.GetColumnInternal(trainingData.DataStoreInfo.DecisionFieldId);

            foreach (int fieldId in localFields)
            {
                localFieldInfoTrain = trainingData.DataStoreInfo.GetFieldInfo(fieldId);                
                if (localFieldInfoTrain.CanDiscretize())
                {
                    TypeCode trainFieldTypeCode = Type.GetTypeCode(localFieldInfoTrain.FieldValueType);
                    localFieldInfoTrain.IsNumeric = false;

                    int[] newValues = new int[trainingData.NumberOfRecords];
                    
                    switch (trainFieldTypeCode)
                    {
                        /*
                        case TypeCode.Decimal:
                            
                            Discretization<decimal, long> discretizeDecimal = new Discretization<decimal, long>();
                            discretizeInt.UseKononenko = this.UseKononenko;
                            discretizeInt.UseBetterEncoding = this.UseBetterEncoding;

                            decimal[] oldValuesDecimal = trainingData.GetColumn<decimal>(fieldId);
                            discretizeDecimal.Compute(oldValuesDecimal);
                            localFieldInfoTrain.Cuts = discretizeDecimal.Cuts;
                            for (int j = 0; j < trainingData.NumberOfRecords; j++)
                                newValues[j] = discretizeDecimal.Search(oldValuesDecimal[j]);
                            break;
                        */

                        case TypeCode.Int32:
                            Discretization<int, long> discretizeInt = new Discretization<int, long>();
                            discretizeInt.UseKononenko = this.UseKononenko;
                            discretizeInt.UseBetterEncoding = this.UseBetterEncoding;
                            int[] oldValuesInt = trainingData.GetColumn<int>(fieldId);
                            discretizeInt.Compute(oldValuesInt, labels, weights);
                            localFieldInfoTrain.Cuts = discretizeInt.Cuts;
                            for (int j = 0; j < trainingData.NumberOfRecords; j++)
                                newValues[j] = discretizeInt.Search(oldValuesInt[j]);
                            break;

                        case TypeCode.Double:
                            Discretization<double, long> discretizeDouble = new Discretization<double, long>();
                            discretizeDouble.UseKononenko = this.UseKononenko;
                            discretizeDouble.UseBetterEncoding = this.UseBetterEncoding;
                            double[] oldValuesDouble = trainingData.GetColumn<double>(fieldId);
                            discretizeDouble.Compute(oldValuesDouble, labels, weights);
                            localFieldInfoTrain.Cuts = discretizeDouble.Cuts;
                            for (int j = 0; j < trainingData.NumberOfRecords; j++)
                                newValues[j] = discretizeDouble.Search(oldValuesDouble[j]);
                            break;

                        default:
                            throw new NotImplementedException(
                                String.Format("Type {0} is not implemented for discretization", 
                                Type.GetTypeCode(localFieldInfoTrain.FieldValueType)));
                    }

                    localFieldInfoTrain.FieldValueType = typeof(int);
                    trainingData.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x));

                    localFieldInfoTest = testData.DataStoreInfo.GetFieldInfo(fieldId);
                    //TypeCode testFieldTypeCode = Type.GetTypeCode(localFieldInfoTest.FieldValueType);
                    localFieldInfoTest.IsNumeric = false;                    

                    newValues = new int[testData.NumberOfRecords];
                    switch (trainFieldTypeCode)
                    {
                        case TypeCode.Int32:
                            Discretization<int, long> discretizeInt = new Discretization<int, long>();
                            discretizeInt.Cuts = localFieldInfoTrain.Cuts;
                            int[] oldValuesInt = testData.GetColumn<int>(fieldId);
                            for (int j = 0; j < testData.NumberOfRecords; j++)
                                newValues[j] = discretizeInt.Search(oldValuesInt[j]);
                            break;

                        /*
                        case TypeCode.Decimal:
                            Discretization<decimal, long>> discretizeDecimal = new Discretization<decimal, long>>();
                            discretizeDecimal.Cuts = localFieldInfoTrain.Cuts;
                            decimal[] oldValuesDecimal = testData.GetColumn<decimal>(fieldId);
                            for (int j = 0; j < testData.NumberOfRecords; j++)
                                newValues[j] = discretizeDecimal.Search(oldValuesDecimal[j]);
                            break;
                        */ 

                        case TypeCode.Double:
                            Discretization<double, long> discretizeDouble = new Discretization<double, long>();
                            discretizeDouble.Cuts = localFieldInfoTrain.Cuts;
                            double[] oldValuesDouble = testData.GetColumn<double>(fieldId);
                            for (int j = 0; j < testData.NumberOfRecords; j++)
                                newValues[j] = discretizeDouble.Search(oldValuesDouble[j]);
                            break;
                    }

                    localFieldInfoTest.FieldValueType = typeof(int);
                    localFieldInfoTest.Cuts = localFieldInfoTrain.Cuts;
                    testData.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x), localFieldInfoTrain);
                }
            }
        }
    }
}
