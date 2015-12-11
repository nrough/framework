using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Benchmark
{
    public class BenchmarkData
    {        
        public virtual string Name { get; set; }
        public virtual string TestFile { get; set; }
        public virtual string TrainFile { get; set; }
        public virtual string DataFile { get; set; }
        public virtual string NamesFile { get; set; }
        public virtual bool CrossValidationActive { get; set; }
        public virtual int CrossValidationFolds { get; set; }
        public virtual FileFormat FileFormat { get; set; }
        public virtual int DecisionFieldId { get; set; }
        
        public virtual bool DiscretizeUsingEntropy { get; set; }
        public virtual bool DiscretizeUsingEqualFreq { get; set; }
        public virtual bool DiscretizeUsingEqualWidth { get; set; }
           
        private Dictionary<int, DataFieldInfo> fieldMetadata;

        protected BenchmarkData()
        {
            this.CrossValidationActive = false;
            this.CrossValidationFolds = 1;
            this.FileFormat = FileFormat.Rses1;
            this.DecisionFieldId = -1;

            this.fieldMetadata = new Dictionary<int, DataFieldInfo>();
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

        public void AddFieldInfo(int fieldId, DataFieldInfo fieldInfo)
        {
            this.fieldMetadata.Add(fieldId, fieldInfo);
        }

        public DataFieldInfo GetFieldinfo(int fieldId)
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

        public bool CheckDiscretize()
        {
            return this.DiscretizeUsingEntropy || this.DiscretizeUsingEqualFreq || this.DiscretizeUsingEqualWidth;
        }

        public IEnumerable<DataFieldInfo> GetNumericFields()
        {
            return this.fieldMetadata.Values.Where(f => f.IsNumeric);
        }
    }
}
