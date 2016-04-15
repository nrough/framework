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
            Run();
        }

        public static void Run()
        {
            int folds = 5;

            DataStore train = null, test = null;
            DataStore data = DataStore.Load(@"Data\dermatology.data", FileFormat.Csv);
            DataStoreSplitter splitter = new DataStoreSplitter(data, folds);
            for (int f = 0; f < folds; f++)
            {
                splitter.ActiveFold = f;
                splitter.Split(ref train, ref test);

                DataFieldInfo ageAttributeTrain = train.DataStoreInfo.GetFieldInfo(35); //a34                
                DataFieldInfo ageAttributeTest = test.DataStoreInfo.GetFieldInfo(35); //a34

                //TODO add m columns to DS
                //Foreach column discretize

                foreach (object val in ageAttributeTest.Values())
                {
                    int value = (int)val;
                    BinaryDiscretization<int> discretizer = new BinaryDiscretization<int>(value);
                    
                    int[] ageColumnTrain = train.GetColumn<int>(35);
                    int[] newValuesTrain = discretizer.Discretize(ageColumnTrain);

                    int[] ageColumnTest = test.GetColumn<int>(35);
                    int[] newValuesTest = discretizer.Discretize(ageColumnTest);
                                        
                    train.UpdateColumn(35, Array.ConvertAll(newValuesTrain, x => (object)x));                    

                    ageAttributeTrain.IsNumeric = false;
                    ageAttributeTrain.Cuts = discretizer.Cuts;
                    ageAttributeTrain.FieldValueType = typeof(int);

                    test.UpdateColumn(35, Array.ConvertAll(newValuesTrain, x => (object)x), ageAttributeTrain);

                    ageAttributeTest.IsNumeric = false;
                    ageAttributeTest.Cuts = discretizer.Cuts;
                    ageAttributeTest.FieldValueType = typeof(int);
                }                
            }

            data.WriteToCSVFileExt(@"Results\dermatology.csv", ",");

        }
    }
}
