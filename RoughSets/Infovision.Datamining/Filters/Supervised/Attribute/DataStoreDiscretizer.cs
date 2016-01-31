using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining.Filters.Unsupervised.Attribute;

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
                    /*
                    case TypeCode.Decimal:
                            
                        Discretization<decimal, long> discretizeDecimal = new Discretization<decimal, long>();
                        discretizeInt.UseKononenko = this.UseKononenko;
                        discretizeInt.UseBetterEncoding = this.UseBetterEncoding;

                        decimal[] oldValuesDecimal = data.GetColumn<decimal>(fieldId);
                        discretizeDecimal.Compute(oldValuesDecimal);
                        cuts = discretizeDecimal.Cuts;                        
                        break;
                    */

                    case TypeCode.Int32:
                        Discretization<int, long> discretizeInt = new Discretization<int, long>();
                        discretizeInt.UseKononenko = this.UseKononenko;
                        discretizeInt.UseBetterEncoding = this.UseBetterEncoding;
                        int[] oldValuesInt = data.GetColumn<int>(fieldId);
                        discretizeInt.Compute(oldValuesInt, labels, weights);
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
                        discretizeDouble.Compute(oldValuesDouble, labels, weights);
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

            return cuts;
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
                            
                            if (discretizeInt.Cuts == null || discretizeInt.Cuts.Length == 0)
                            {
                                Discretization<int> u_discretizeInt = new Discretization<int>();
                                u_discretizeInt.UseEntropy = true;                                                        
                                u_discretizeInt.Compute(oldValuesInt, weights);
                                discretizeInt.Cuts = u_discretizeInt.Cuts;
                            }

                            if (discretizeInt.Cuts == null || discretizeInt.Cuts.Length == 0)
                            {
                                Discretization<int> u_discretizeInt = new Discretization<int>();
                                u_discretizeInt.UseEqualFrequency = true;
                                u_discretizeInt.Compute(oldValuesInt, weights);
                                discretizeInt.Cuts = u_discretizeInt.Cuts;
                            }

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

                            if (discretizeDouble.Cuts == null || discretizeDouble.Cuts.Length == 0)
                            {
                                Discretization<double> u_discretizeDouble = new Discretization<double>();
                                u_discretizeDouble.UseEntropy = true;
                                u_discretizeDouble.Compute(oldValuesDouble, weights);
                                discretizeDouble.Cuts = u_discretizeDouble.Cuts;
                            }

                            if (discretizeDouble.Cuts == null || discretizeDouble.Cuts.Length == 0)
                            {
                                Discretization<double> u_discretizeDouble = new Discretization<double>();
                                u_discretizeDouble.UseEqualFrequency = true;
                                u_discretizeDouble.Compute(oldValuesDouble, weights);
                                discretizeDouble.Cuts = u_discretizeDouble.Cuts;
                            }

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

        public virtual bool CanDiscretize(DataFieldInfo field)
        {
            if (field.CanDiscretize() && field.Values().Count > 2)
            {
                return true;
            }

            return false;
        }
    }
}
