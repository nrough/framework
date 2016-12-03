using System;
using System.Collections.Generic;
using Infovision.Data;

namespace Infovision.MachineLearning.Filters.Unsupervised.Attribute
{
    public class DataStoreDiscretizer
    {
        public virtual bool DiscretizeUsingEntropy { get; set; }
        public virtual bool DiscretizeUsingEqualFreq { get; set; }
        public virtual bool DiscretizeUsingEqualWidth { get; set; }
        public virtual int NumberOfBins { get; set; }
        public virtual IEnumerable<int> Fields2Discretize { get; set; }
        public virtual DiscretizationType DiscretizationType { get; set; }

        public DataStoreDiscretizer()
        {
            this.DiscretizationType = DiscretizationType.Unsupervised_Entropy;
            this.DiscretizeUsingEntropy = true;
            this.DiscretizeUsingEqualFreq = false;
            this.DiscretizeUsingEqualWidth = false;
        }

        public static DataStoreDiscretizer Construct(DiscretizationType discretizationType, IEnumerable<int> fields2Discretize = null)
        {
            switch (discretizationType)
            {
                case DiscretizationType.Unsupervised_Entropy:
                    return new DataStoreDiscretizer()
                    {
                        DiscretizeUsingEntropy = true,
                        DiscretizeUsingEqualFreq = false,
                        DiscretizeUsingEqualWidth = false,
                        Fields2Discretize = fields2Discretize,
                        DiscretizationType = discretizationType
                    };

                case DiscretizationType.Unsupervised_EqualFrequency:
                    return new DataStoreDiscretizer()
                    {
                        DiscretizeUsingEntropy = false,
                        DiscretizeUsingEqualFreq = true,
                        DiscretizeUsingEqualWidth = false,
                        Fields2Discretize = fields2Discretize,
                        DiscretizationType = discretizationType
                    };

                case DiscretizationType.Unsupervised_EqualWidth:
                    return new DataStoreDiscretizer()
                    {
                        DiscretizeUsingEntropy = false,
                        DiscretizeUsingEqualFreq = false,
                        DiscretizeUsingEqualWidth = true,
                        Fields2Discretize = fields2Discretize,
                        DiscretizationType = discretizationType
                    };
            }

            return null;
        }

        public virtual double[] GetCuts(DataStore data, int fieldId, double[] weights = null)
        {
            DataFieldInfo localFieldInfoTrain = data.DataStoreInfo.GetFieldInfo(fieldId);
            double[] cuts = null;

            if (localFieldInfoTrain.CanDiscretize())
            {
                TypeCode trainFieldTypeCode = Type.GetTypeCode(localFieldInfoTrain.FieldValueType);
                switch (trainFieldTypeCode)
                {
                    case TypeCode.Int32:
                        Discretization<int> discretizeInt = new Discretization<int>();
                        discretizeInt.UseEntropy = this.DiscretizeUsingEntropy;
                        discretizeInt.UseEqualFrequency = this.DiscretizeUsingEqualFreq;
                        if (this.NumberOfBins > 1)
                        {
                            discretizeInt.NumberOfBuckets = this.NumberOfBins;
                        }
                        int[] oldValuesInt = data.GetColumn<int>(fieldId);
                        discretizeInt.Compute(oldValuesInt);

                        if ((discretizeInt.Cuts == null || discretizeInt.Cuts.Length == 0)
                                && this.DiscretizeUsingEntropy == true)
                        {
                            discretizeInt = new Discretization<int>();
                            discretizeInt.UseEqualFrequency = true;
                            discretizeInt.Compute(oldValuesInt, weights);
                        }

                        cuts = discretizeInt.Cuts;
                        break;

                    case TypeCode.Double:
                        Discretization<double> discretizeDouble = new Discretization<double>();
                        discretizeDouble.UseEntropy = this.DiscretizeUsingEntropy;
                        discretizeDouble.UseEqualFrequency = this.DiscretizeUsingEqualFreq;
                        if (this.NumberOfBins > 1)
                        {
                            discretizeDouble.NumberOfBuckets = this.NumberOfBins;
                        }
                        double[] oldValuesDouble = data.GetColumn<double>(fieldId);
                        discretizeDouble.Compute(oldValuesDouble);

                        if ((discretizeDouble.Cuts == null || discretizeDouble.Cuts.Length == 0)
                                && this.DiscretizeUsingEntropy == true)
                        {
                            discretizeDouble = new Discretization<double>();
                            discretizeDouble.UseEqualFrequency = true;
                            discretizeDouble.Compute(oldValuesDouble, weights);
                        }

                        cuts = discretizeDouble.Cuts;
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
                        case TypeCode.Int32:
                            Discretization<int> discretizeInt = new Discretization<int>();
                            discretizeInt.UseEntropy = this.DiscretizeUsingEntropy;
                            discretizeInt.UseEqualFrequency = this.DiscretizeUsingEqualFreq;
                            if (this.NumberOfBins > 1)
                            {
                                discretizeInt.NumberOfBuckets = this.NumberOfBins;
                            }
                            int[] oldValuesInt = trainingData.GetColumn<int>(fieldId);
                            discretizeInt.Compute(oldValuesInt, weights);

                            if ((discretizeInt.Cuts == null || discretizeInt.Cuts.Length == 0)
                                && this.DiscretizeUsingEntropy == true)
                            {
                                discretizeInt = new Discretization<int>();
                                discretizeInt.UseEqualFrequency = true;
                                discretizeInt.Compute(oldValuesInt, weights);
                            }

                            localFieldInfoTrain.Cuts = discretizeInt.Cuts;
                            for (int j = 0; j < trainingData.NumberOfRecords; j++)
                                newValues[j] = discretizeInt.Search(oldValuesInt[j]);
                            break;

                        case TypeCode.Double:
                            Discretization<double> discretizeDouble = new Discretization<double>();
                            discretizeDouble.UseEntropy = this.DiscretizeUsingEntropy;
                            discretizeDouble.UseEqualFrequency = this.DiscretizeUsingEqualFreq;
                            if (this.NumberOfBins > 1)
                            {
                                discretizeDouble.NumberOfBuckets = this.NumberOfBins;
                            }
                            double[] oldValuesDouble = trainingData.GetColumn<double>(fieldId);
                            discretizeDouble.Compute(oldValuesDouble, weights);

                            if ((discretizeDouble.Cuts == null || discretizeDouble.Cuts.Length == 0)
                                && this.DiscretizeUsingEntropy == true)
                            {
                                discretizeDouble = new Discretization<double>();
                                discretizeDouble.UseEqualFrequency = true;
                                discretizeDouble.Compute(oldValuesDouble, weights);
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
                            Discretization<int> discretizeInt = new Discretization<int>();
                            discretizeInt.Cuts = localFieldInfoTrain.Cuts;
                            int[] oldValuesInt = testData.GetColumn<int>(fieldId);
                            for (int j = 0; j < testData.NumberOfRecords; j++)
                                newValues[j] = discretizeInt.Search(oldValuesInt[j]);
                            break;

                        case TypeCode.Double:
                            Discretization<double> discretizeDouble = new Discretization<double>();
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
            if (field.CanDiscretize())
            {
                if (this.NumberOfBins < field.NumberOfValues)
                    return true;
            }

            return false;
        }
    }
}