using System;
using System.Collections.Generic;
using Infovision.Data;
using Infovision.Datamining.Filters.Unsupervised.Attribute;
using Infovision.Datamining.Roughset;

namespace DermoReducts
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HandleMissingData(@"Data\dermatology.data", @"Results\dermatology-nomissing.csv", @"Results\dermatology-nomissing-weights.csv");
            DiscretizeAgeAttribute(@"Results\dermatology-nomissing.csv", @"Results\dermatology-disc-nomissing.csv");
        }

        public static void HandleMissingData(string filename, string outputfile, string weightsfile)
        {
            DataStore data = DataStore.Load(filename, FileFormat.Csv);
            decimal[] w = new decimal[data.NumberOfRecords];
            for (int i = 0; i < data.NumberOfRecords; i++)
                w[i] = Decimal.One;

            IReduct reduct = new ReductWeights(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard), Decimal.Zero, w);
            DataFieldInfo ageAttribute = data.DataStoreInfo.GetFieldInfo(34); //a34

            Dictionary<long, int> countVals = new Dictionary<long, int>(ageAttribute.NumberOfValues);
            foreach (long val in ageAttribute.InternalValues())
                if (val != ageAttribute.MissingValueInternal)
                    countVals.Add(val, (int)ageAttribute.Histogram.GetBinValue(val));

            DataRecordInternal[] missingRecs = new DataRecordInternal[(int)ageAttribute.Histogram.GetBinValue(ageAttribute.MissingValueInternal)];

            for (int i = 0, j = 0; i < data.NumberOfRecords; i++)
            {
                DataRecordInternal record = data.GetRecordByIndex(i);
                if (record[34] == ageAttribute.MissingValueInternal)
                {
                    missingRecs[j++] = record;
                }
            }

            List<long> newRecIds = new List<long>();
            decimal tmp = Decimal.Zero;
            int k = 0;
            int origNumberOfRecords = data.NumberOfRecords;
            Array.Resize<decimal>(ref w, w.Length + (missingRecs.Length * ageAttribute.NumberOfValues));
            foreach (DataRecordInternal rec in missingRecs)
            {
                foreach (KeyValuePair<long, int> kvp in countVals)
                {
                    tmp = (decimal)kvp.Value / (decimal)(origNumberOfRecords - missingRecs.Length);
                    w[origNumberOfRecords + k] = tmp;
                    DataRecordInternal recBis = new DataRecordInternal(rec);
                    recBis[34] = kvp.Key;

                    newRecIds.Add(data.AddRow(recBis));
                    k++;
                }
            }

            foreach (DataRecordInternal rec in missingRecs)
            {
                w[rec.ObjectIdx] = 0;
            }

            data.WriteToCSVFileExt(outputfile, ",", false, true);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(weightsfile))
            {
                for (int objectIndex = 0; objectIndex < data.NumberOfRecords; objectIndex++)
                {
                    file.WriteLine(w[objectIndex].ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }

        public static void DiscretizeAgeAttribute(string filename, string outputfile)
        {
            DataStore data = DataStore.Load(filename, FileFormat.Csv);
            data.SetDecisionFieldId(35);

            DataFieldInfo ageAttribute = data.DataStoreInfo.GetFieldInfo(34); //a34

            foreach (DataFieldInfo f in data.DataStoreInfo.Fields)
            {
                f.Alias = (f.Id == data.DataStoreInfo.DecisionFieldId) ? "d" : String.Format("a{0}", f.Id);
                f.Name = f.Alias;
            }

            foreach (object val in ageAttribute.Values())
            {
                int value = (int)val;
                if (value != (int)ageAttribute.MissingValue)
                {
                    BinaryDiscretization<int> discretizer = new BinaryDiscretization<int>(value);

                    string[] newValuesTrain = discretizer.Discretize(data, ageAttribute);

                    int newFieldId = data.AddColumn<string>(newValuesTrain);
                    DataFieldInfo newFieldInfo = data.DataStoreInfo.GetFieldInfo(newFieldId);

                    newFieldInfo.IsNumeric = false;
                    newFieldInfo.Cuts = discretizer.Cuts;
                    newFieldInfo.FieldValueType = typeof(string);
                    newFieldInfo.Alias = String.Format("{0}-{1}", "Age", value);
                }
            }

            data.RemoveColumn(34);
            data.WriteToCSVFileExt(outputfile, ",", true, true);
        }
    }
}