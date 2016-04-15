using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining.Filters.Unsupervised.Attribute;

namespace DermoReducts
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateNewDataSet();
        }

        public static void CreateNewDataSet()
        {
            DataStore data = DataStore.Load(@"Data\dermatology.data", FileFormat.Csv);
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

            data.WriteToCSVFileExt(@"Results\dermatology-1.csv", ",", true);

            data.RemoveColumn(34);
            //data.SwitchColumns(data.DataStoreInfo.MaxFieldId, 35);

            data.WriteToCSVFileExt(@"Results\dermatology-2.csv", ",", true);

        }

        public static void HandleMissingData()
        {
            DataStore data = DataStore.Load(@"Results\dermatology.csv", FileFormat.Csv);
        }
    }
}
