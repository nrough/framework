using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Filters.Unsupervised.Attribute
{
    public class DataStoreDiscretizer
    {
        public virtual bool DiscretizeUsingEntropy { get; set; }
        public virtual bool DiscretizeUsingEqualFreq { get; set; }
        public virtual bool DiscretizeUsingEqualWidth { get; set; }
        public virtual int NumberOfBins {get; set;}        

        public DataStoreDiscretizer()
        {
            this.DiscretizeUsingEntropy = true;
            this.DiscretizeUsingEqualFreq = false;
            this.DiscretizeUsingEqualWidth = false;
        }

        public static DataStoreDiscretizer Construct(DiscretizationType discretizationType)
        {
            switch (discretizationType)
            {
                case DiscretizationType.Entropy :
                    return new DataStoreDiscretizer()
                    {
                        DiscretizeUsingEntropy = true,
                        DiscretizeUsingEqualFreq = false,
                        DiscretizeUsingEqualWidth = false
                    };

                case DiscretizationType.EqualFrequency:
                    return new DataStoreDiscretizer()
                    {
                        DiscretizeUsingEntropy = false,
                        DiscretizeUsingEqualFreq = true,
                        DiscretizeUsingEqualWidth = false
                    };

                case DiscretizationType.EqualWidth:
                    return new DataStoreDiscretizer()
                    {
                        DiscretizeUsingEntropy = false,
                        DiscretizeUsingEqualFreq = false,
                        DiscretizeUsingEqualWidth = true
                    };
            }

            return null;
        }
        
        public virtual void Discretize(ref DataStore trainingData, ref DataStore testData)
        {
            DataFieldInfo localFieldInfoTrain, localFieldInfoTest;
            foreach (int fieldId in trainingData.DataStoreInfo.GetFieldIds(FieldTypes.Standard))
            {
                localFieldInfoTrain = trainingData.DataStoreInfo.GetFieldInfo(fieldId);
                TypeCode trainFieldTypeCode = Type.GetTypeCode(localFieldInfoTrain.FieldValueType);

                if (localFieldInfoTrain.CanDiscretize())
                {
                    localFieldInfoTrain.IsNumeric = false;

                    int[] newValues = new int[trainingData.NumberOfRecords];
                    switch (trainFieldTypeCode)
                    {
                        case TypeCode.Decimal:
                            
                            Discretization<decimal> discretizeDecimal = new Discretization<decimal>();
                            discretizeDecimal.UseEntropy = this.DiscretizeUsingEntropy;
                            discretizeDecimal.UseEqualFrequency = this.DiscretizeUsingEqualFreq;

                            decimal[] oldValuesDecimal = trainingData.GetColumn<decimal>(fieldId);
                            discretizeDecimal.Compute(oldValuesDecimal);
                            localFieldInfoTrain.Cuts = Array.ConvertAll(discretizeDecimal.Cuts, x => (IComparable)x);
                            for (int j = 0; j < trainingData.NumberOfRecords; j++)
                                newValues[j] = discretizeDecimal.Search(oldValuesDecimal[j]);
                            break;

                        case TypeCode.Int32:
                            Discretization<int> discretizeInt = new Discretization<int>();
                            discretizeInt.UseEntropy = this.DiscretizeUsingEntropy;
                            discretizeInt.UseEqualFrequency = this.DiscretizeUsingEqualFreq;
                            int[] oldValuesInt = trainingData.GetColumn<int>(fieldId);
                            discretizeInt.Compute(oldValuesInt);
                            localFieldInfoTrain.Cuts = Array.ConvertAll(discretizeInt.Cuts, x => (IComparable)x);
                            for (int j = 0; j < trainingData.NumberOfRecords; j++)
                                newValues[j] = discretizeInt.Search(oldValuesInt[j]);
                            break;

                        case TypeCode.Double:
                            Discretization<double> discretizeDouble = new Discretization<double>();
                            discretizeDouble.UseEntropy = this.DiscretizeUsingEntropy;
                            discretizeDouble.UseEqualFrequency = this.DiscretizeUsingEqualFreq;
                            double[] oldValuesDouble = trainingData.GetColumn<double>(fieldId);
                            discretizeDouble.Compute(oldValuesDouble);
                            localFieldInfoTrain.Cuts = Array.ConvertAll(discretizeDouble.Cuts, x => (IComparable)x);
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
                            Discretization<int> discretizeInt = new Discretization<int>();
                            discretizeInt.Cuts = Array.ConvertAll(localFieldInfoTrain.Cuts, x => (int)x);
                            int[] oldValuesInt = testData.GetColumn<int>(fieldId);
                            for (int j = 0; j < testData.NumberOfRecords; j++)
                                newValues[j] = discretizeInt.Search(oldValuesInt[j]);
                            break;

                        case TypeCode.Decimal:
                            Discretization<decimal> discretizeDecimal = new Discretization<decimal>();
                            discretizeDecimal.Cuts = Array.ConvertAll(localFieldInfoTrain.Cuts, x => (decimal)x);
                            decimal[] oldValuesDecimal = testData.GetColumn<decimal>(fieldId);
                            for (int j = 0; j < testData.NumberOfRecords; j++)
                                newValues[j] = discretizeDecimal.Search(oldValuesDecimal[j]);
                            break;

                        case TypeCode.Double:
                            Discretization<double> discretizeDouble = new Discretization<double>();
                            discretizeDouble.Cuts = Array.ConvertAll(localFieldInfoTrain.Cuts, x => (double)x);
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
