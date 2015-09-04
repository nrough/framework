using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Data
{
    public class BenchmarkData
    {
        public virtual string Alias { get; set; }
        public virtual string TestFile { get; set; }
        public virtual string TrainFile { get; set; }
        public virtual string DataFile { get; set; }
        public virtual bool CrossValidationActive { get; set; }
        public virtual int CrossValidationFolds { get; set; }        

        public BenchmarkData(string alias, string dataFile, int folds)
        {
            this.Alias = alias;
            this.DataFile = dataFile;
            this.TrainFile = dataFile;
            this.CrossValidationActive = true;
            this.CrossValidationFolds = folds;
        }

        public BenchmarkData(string alias, string trainFile, string testFile)
        {
            this.Alias = alias;
            this.TrainFile = trainFile;
            this.TestFile = testFile;
            this.CrossValidationActive = false;            
        }
    }
}
