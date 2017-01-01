using System;
using System.Collections.Generic;
using Infovision.Data;
using Infovision.MachineLearning.Filters.Unsupervised.Attribute;
using System.Linq;

namespace Infovision.MachineLearning.Filters.Supervised.Attribute
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

        public virtual double[] GetCuts(DataStore data, int fieldId, double[] weights = null)
        {
            DataFieldInfo localFieldInfoTrain = data.DataStoreInfo.GetFieldInfo(fieldId);
            long[] labels = data.GetColumnInternal(data.DataStoreInfo.DecisionFieldId);
            double[] cuts = null;

            if (localFieldInfoTrain.CanDiscretize())
            {
                TypeCode trainFieldTypeCode = Type.GetTypeCode(localFieldInfoTrain.FieldValueType);
                switch (trainFieldTypeCode)
                {
                    case TypeCode.Int32:
                        Discretization<int, long> discretizeInt = new Discretization<int, long>();
                        discretizeInt.UseKononenko = this.UseKononenko;
                        discretizeInt.UseBetterEncoding = this.UseBetterEncoding;
                        int[] oldValuesInt = data.GetColumn<int>(fieldId);
                        discretizeInt.Compute(oldValuesInt, labels, false, weights);
                        cuts = discretizeInt.Cuts;

                        if (cuts == null || cuts.Length == 0)
                        {
                            Discretization<int> u_discretizeInt = new Discretization<int>();
                            u_discretizeInt.UseEntropy = true;
                            u_discretizeInt.Compute(oldValuesInt, weights);
                            cuts = u_discretizeInt.Cuts;
                        }

                        if (cuts == null || cuts.Length == 0)
                        {
                            Discretization<int> u_discretizeInt = new Discretization<int>();
                            u_discretizeInt.UseEqualFrequency = true;
                            u_discretizeInt.Compute(oldValuesInt, weights);
                            cuts = u_discretizeInt.Cuts;
                        }
                        break;

                    case TypeCode.Double:
                        Discretization<double, long> discretizeDouble = new Discretization<double, long>();
                        discretizeDouble.UseKononenko = this.UseKononenko;
                        discretizeDouble.UseBetterEncoding = this.UseBetterEncoding;
                        double[] oldValuesDouble = data.GetColumn<double>(fieldId);
                        discretizeDouble.Compute(oldValuesDouble, labels, false, weights);
                        cuts = discretizeDouble.Cuts;

                        if (cuts == null || cuts.Length == 0)
                        {
                            Discretization<double> u_discretizeDouble = new Discretization<double>();
                            u_discretizeDouble.UseEntropy = true;
                            u_discretizeDouble.Compute(oldValuesDouble, weights);
                            cuts = u_discretizeDouble.Cuts;
                        }

                        if (cuts == null || cuts.Length == 0)
                        {
                            Discretization<double> u_discretizeDouble = new Discretization<double>();
                            u_discretizeDouble.UseEqualFrequency = true;
                            u_discretizeDouble.Compute(oldValuesDouble, weights);
                            cuts = u_discretizeDouble.Cuts;
                        }

                        break;

                    default:
                        throw new NotImplementedException(
                            String.Format("Type {0} is not implemented for discretization",
                            Type.GetTypeCode(localFieldInfoTrain.FieldValueType)));
                }
            }

            return cuts.ToArray();
        }        

        public void Discretize(DataStore trainingData, double[] weights = null)
        {
            DataFieldInfo localFieldInfoTrain;
            IEnumerable<int> localFields = Fields2Discretize != null
                                         ? Fields2Discretize
                                         : trainingData.DataStoreInfo.GetFieldIds(FieldTypes.Standard);

            long[] labels = trainingData.GetColumnInternal(trainingData.DataStoreInfo.DecisionFieldId);

            foreach (int fieldId in localFields)
            {
                localFieldInfoTrain = trainingData.DataStoreInfo.GetFieldInfo(fieldId);
                if (localFieldInfoTrain.CanDiscretize())
                {                                        
                    localFieldInfoTrain.Cuts = GetCuts(trainingData, fieldId, weights);                    
                    int[] newValues = new int[trainingData.NumberOfRecords];

                    switch (Type.GetTypeCode(localFieldInfoTrain.FieldValueType))
                    {
                        case TypeCode.Int32:
                            int[] oldValuesInt = trainingData.GetColumn<int>(fieldId);
                            for (int j = 0; j < trainingData.NumberOfRecords; j++)
                                newValues[j] = Discretization<int, long>.Search(oldValuesInt[j], localFieldInfoTrain.Cuts);
                            break;

                        case TypeCode.Double:
                            double[] oldValuesDouble = trainingData.GetColumn<double>(fieldId);
                            for (int j = 0; j < trainingData.NumberOfRecords; j++)
                                newValues[j] = Discretization<double, long>.Search(oldValuesDouble[j], localFieldInfoTrain.Cuts);
                            break;

                    }                    

                    localFieldInfoTrain.FieldValueType = typeof(int);
                    localFieldInfoTrain.IsNumeric = false;
                    trainingData.UpdateColumn(fieldId, Array.ConvertAll(newValues, x => (object)x));
                }
            }
        }

        public void Discretize(DataStore dataToDiscretize, DataStore discretizedData)
        {
            DataFieldInfo localFieldInfoTrain, localFieldInfoTest;            
            IEnumerable<int> localFields = Fields2Discretize != null
                                         ? Fields2Discretize
                                         : dataToDiscretize.DataStoreInfo.GetFieldIds(FieldTypes.Standard);            

            foreach (int fieldId in localFields)
            {
                localFieldInfoTest = dataToDiscretize.DataStoreInfo.GetFieldInfo(fieldId);
                if (localFieldInfoTest.CanDiscretize())
                {
                    localFieldInfoTrain = discretizedData.DataStoreInfo.GetFieldInfo(fieldId);

                    if (localFieldInfoTrain.Cuts == null)
                        throw new InvalidOperationException("localFieldInfoTrain.Cuts == null");
                                        
                    int[] newValues = new int[dataToDiscretize.NumberOfRecords];                                        
                    switch (Type.GetTypeCode(localFieldInfoTest.FieldValueType))
                    {
                        case TypeCode.Int32:                            
                            int[] oldValuesInt = dataToDiscretize.GetColumn<int>(fieldId);
                            for (int j = 0; j < dataToDiscretize.NumberOfRecords; j++)
                                newValues[j] = Discretization<int, long>.Search(oldValuesInt[j], localFieldInfoTrain.Cuts);
                            break;

                        case TypeCode.Double:                            
                            double[] oldValuesDouble = dataToDiscretize.GetColumn<double>(fieldId);
                            for (int j = 0; j < dataToDiscretize.NumberOfRecords; j++)
                                newValues[j] = Discretization<double, long>.Search(oldValuesDouble[j], localFieldInfoTrain.Cuts);
                            break;
                    }

                    localFieldInfoTest.FieldValueType = typeof(int);
                    localFieldInfoTest.Cuts = localFieldInfoTrain.Cuts;
                    localFieldInfoTest.IsNumeric = false;
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
    }
}