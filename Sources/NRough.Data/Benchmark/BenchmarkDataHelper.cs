// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.IO;
using NRough.Data;
using NRough.Doc;

namespace NRough.Data.Benchmark
{
    [AssemblyTreeVisible(false)]
    public class BenchmarkDataHelper
    {
        private static string GetFilePath(string path, string filename)
        {
            if (String.IsNullOrEmpty(path))
                return filename;

            return Path.Combine(path, filename);
        }


        //TODO Add encoding of numeric and symbolic attributes
        public static IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles(
            string dataPath = "Data",
            params string[] names)
        {
            int cvFolds = 5;

            Dictionary<string, BenchmarkData> dataFiles = new Dictionary<string, BenchmarkData>(25);

            BenchmarkData benchmark = new BenchmarkData("golf",
                GetFilePath(dataPath, "playgolf.train"), GetFilePath(dataPath, "playgolf.train"));

            benchmark.AddFieldInfo(1, new AttributeInfo(1, typeof(string)) { Name = "Outlook", Alias = "O" });
            benchmark.AddFieldInfo(2, new AttributeInfo(2, typeof(string)) { Name = "Temperature", Alias = "T" });
            benchmark.AddFieldInfo(3, new AttributeInfo(3, typeof(string)) { Name = "Humidity", Alias = "H" });
            benchmark.AddFieldInfo(4, new AttributeInfo(4, typeof(string)) { Name = "Wind", Alias = "W" });
            benchmark.AddFieldInfo(5, new AttributeInfo(4, typeof(string)) { Name = "Play", Alias = "d" });

            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("testGMDR", GetFilePath(dataPath, "testGMDR.trn"), GetFilePath(dataPath, "testGMDR.trn"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("dna-modified", GetFilePath(dataPath, "dna_modified.trn"), GetFilePath(dataPath, "dna_modified.tst"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("dna", GetFilePath(dataPath, "dna.train"), GetFilePath(dataPath, "dna.test"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("zoo", GetFilePath(dataPath, "zoo.dta"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("monks-1", GetFilePath(dataPath, "monks-1.train"), GetFilePath(dataPath, "monks-1.test"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("monks-2", GetFilePath(dataPath, "monks-2.train"), GetFilePath(dataPath, "monks-2.test"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("monks-3", GetFilePath(dataPath, "monks-3.train"), GetFilePath(dataPath, "monks-3.test"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("spect", GetFilePath(dataPath, "SPECT.train"), GetFilePath(dataPath, "SPECT.test"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("letter", GetFilePath(dataPath, "letter.disc.trn"), GetFilePath(dataPath, "letter.disc.tst"));
            dataFiles.Add(benchmark.Name, benchmark);

            //benchmark = new BenchmarkData("pen", GetFilePath(dataPath, "pendigits.trn"), GetFilePath(dataPath, "pendigits.tst"));
            benchmark = new BenchmarkData("pen", GetFilePath(dataPath, "pendigits.disc.trn"), GetFilePath(dataPath, "pendigits.disc.tst"));            
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("opt", GetFilePath(dataPath, "optdigits.trn"), GetFilePath(dataPath, "optdigits.tst"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("semeion", GetFilePath(dataPath, "semeion.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("chess", GetFilePath(dataPath, "chess.dta"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("dermatology", GetFilePath(dataPath, "dermatology_modified.data"), cvFolds);
            benchmark.DataFormat = DataFormat.CSV;
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("lymphography", GetFilePath(dataPath, "lymphography.all"), cvFolds);
            benchmark.DataFormat = DataFormat.CSV;
            dataFiles.Add(benchmark.Name, benchmark);
            
          
            benchmark = new BenchmarkData("nursery", GetFilePath(dataPath, "nursery.2.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("breast", GetFilePath(dataPath, "breast-cancer-wisconsin.2.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("soybean-small", GetFilePath(dataPath, "soybean-small.2.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("connect", GetFilePath(dataPath, "connect-4.dta"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("soybean-large", 
                GetFilePath(dataPath, "soybean-large.data"), 
                GetFilePath(dataPath, "soybean-large.test"))
            {
                DataFormat = DataFormat.RSES1
            };

            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("house", GetFilePath(dataPath, "house-votes-84.2.data"), cvFolds)
            {
                DataFormat = DataFormat.RSES1_1
            };
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("audiology", GetFilePath(dataPath, "audiology.standardized.2.data"), GetFilePath(dataPath, "audiology.standardized.2.test"))
            {
                DataFormat = DataFormat.RSES1
            };
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("promoters", GetFilePath(dataPath, "promoters.2.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("mushroom", GetFilePath(dataPath, "agaricus-lepiota.2.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("german", GetFilePath(dataPath, "german.data"), cvFolds)
            {                
                DataFormat = DataFormat.CSV
            };

            int[] numericFields = new int[] { 2, 5, 8, 11, 13, 16, 18 };
            for (int i = 0; i < numericFields.Length; i++)
                benchmark.AddFieldInfo(numericFields[i], new AttributeInfo(numericFields[i], typeof(int)) { IsNumeric = true });

            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("sat", GetFilePath(dataPath, "sat.disc.trn"), GetFilePath(dataPath, "sat.disc.tst"));                        
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("vowel", GetFilePath(dataPath, "vowel.disc.trn"), GetFilePath(dataPath, "vowel.disc.tst"));
            benchmark.DataFormat = DataFormat.CSV;

            dataFiles.Add(benchmark.Name, benchmark);

            if (names != null && names.Length > 0)
            {
                Dictionary<string, BenchmarkData> result = new Dictionary<string, BenchmarkData>(names.Length);
                for (int i = 0; i < names.Length; i++)
                {
                    if (dataFiles.ContainsKey(names[i]))
                    {
                        result.Add(names[i], dataFiles[names[i]]);
                    }
                }

                return result;
            }

            return dataFiles;
        }

        public static List<KeyValuePair<string, BenchmarkData>> GetDataFilesList(string path, params string[] names)
        {
            List<KeyValuePair<string, BenchmarkData>> result = new List<KeyValuePair<string, BenchmarkData>>();
            foreach (var kvp in BenchmarkDataHelper.GetDataFiles(path, names))
            {
                result.Add(kvp);
            }
            return result;
        }
    }
}