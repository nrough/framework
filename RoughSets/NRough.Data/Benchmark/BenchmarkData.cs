using System.Collections.Generic;
using System.Linq;
using NRough.Data;
using NRough.Doc;

namespace NRough.Data.Benchmark
{
    [AssemblyTreeVisible(false)]
    public class BenchmarkData
    {
        public string Name { get; set; }
        public string TestFile { get; set; }
        public string TrainFile { get; set; }
        public string DataFile { get; set; }
        public string NamesFile { get; set; }
        public bool CrossValidationActive { get; set; }
        public int CrossValidationFolds { get; set; }
        public DataFormat FileFormat { get; set; }
        public int DecisionFieldId { get; set; }       
        private Dictionary<int, AttributeInfo> fieldMetadata;

        protected BenchmarkData()
        {
            this.CrossValidationActive = false;
            this.CrossValidationFolds = 1;
            this.FileFormat = DataFormat.RSES1;
            this.DecisionFieldId = -1;

            this.fieldMetadata = new Dictionary<int, AttributeInfo>();
        }

        public BenchmarkData(string name, string dataFile, int folds)
            : this()
        {
            this.Name = name;
            this.DataFile = dataFile;
            this.TrainFile = dataFile;
            this.CrossValidationActive = true;
            this.CrossValidationFolds = folds;
        }

        public BenchmarkData(string name, string trainFile, string testFile)
            : this()
        {
            this.Name = name;
            this.TrainFile = trainFile;
            this.TestFile = testFile;
        }

        public void AddFieldInfo(int fieldId, AttributeInfo fieldInfo)
        {
            this.fieldMetadata.Add(fieldId, fieldInfo);
        }

        public AttributeInfo GetFieldinfo(int fieldId)
        {
            return this.fieldMetadata[fieldId];
        }

        public string GetFieldAlias(int fieldId)
        {
            if (this.fieldMetadata.ContainsKey(fieldId))
                return fieldMetadata[fieldId].Alias;
            else
                return fieldId.ToString();
        }        

        public IEnumerable<AttributeInfo> GetNumericFields()
        {
            return this.fieldMetadata.Values.Where(f => f.IsNumeric && f.CanDiscretize());
        }
    }
}