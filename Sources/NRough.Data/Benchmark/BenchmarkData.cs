//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
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
        public DataFormat DataFormat { get; set; }
        public int DecisionFieldId { get; set; }       
        private Dictionary<int, AttributeInfo> fieldMetadata;

        protected BenchmarkData()
        {
            this.CrossValidationActive = false;
            this.CrossValidationFolds = 1;
            this.DataFormat = DataFormat.RSES1;
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