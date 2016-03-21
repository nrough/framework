using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Benchmark
{
    public class BenchmarkDataHelper
    {
        private static string GetFilePath(string path, string filename)
        {
            if (String.IsNullOrEmpty(path))
                return filename;

            return Path.Combine(path, filename);
        }

        public static IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles(
            string dataPath = "Data", 
            params string[] names)
        {
            int cvFolds = 5;

            Dictionary<string, BenchmarkData> dataFiles = new Dictionary<string, BenchmarkData>(25);

            BenchmarkData benchmark = new BenchmarkData("golf",
                GetFilePath(dataPath, "playgolf.train"), GetFilePath(dataPath, "playgolf.train"));

            benchmark.AddFieldInfo(1, new DataFieldInfo(1, typeof(string)) { Alias = "O" });
            benchmark.AddFieldInfo(2, new DataFieldInfo(2, typeof(string)) { Alias = "T" });
            benchmark.AddFieldInfo(3, new DataFieldInfo(3, typeof(string)) { Alias = "H" });
            benchmark.AddFieldInfo(4, new DataFieldInfo(4, typeof(string)) { Alias = "W" });

            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("testGMDR", GetFilePath(dataPath, "testGMDR.trn"), GetFilePath(dataPath, "testGMDR.trn"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("dna", GetFilePath(dataPath, "dna_modified.trn"), GetFilePath(dataPath, "dna_modified.tst"));
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

            benchmark = new BenchmarkData("letter", GetFilePath(dataPath, "letter.trn"), GetFilePath(dataPath, "letter.tst"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("pen", GetFilePath(dataPath, "pendigits.trn"), GetFilePath(dataPath, "pendigits.tst"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("opt", GetFilePath(dataPath, "optdigits.trn"), GetFilePath(dataPath, "optdigits.tst"));
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("semeion", GetFilePath(dataPath, "semeion.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("chess", GetFilePath(dataPath, "chess.dta"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("nursery", GetFilePath(dataPath, "nursery.2.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("breast", GetFilePath(dataPath, "breast-cancer-wisconsin.2.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("soybean-small", GetFilePath(dataPath, "soybean-small.2.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("connect", GetFilePath(dataPath, "connect-4.dta"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("soybean-large", GetFilePath(dataPath, "soybean-large.data"), GetFilePath(dataPath, "soybean-large.test"))
            {
                FileFormat = FileFormat.Csv,
                DecisionFieldId = 1
            };

            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("house", GetFilePath(dataPath, "house-votes-84.2.data"), cvFolds)
            {
                FileFormat = FileFormat.Rses1_1
            };
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("audiology", GetFilePath(dataPath, "audiology.standardized.2.data"), GetFilePath(dataPath, "audiology.standardized.2.test"))
            {
                FileFormat = FileFormat.Rses1
            };
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("promoters", GetFilePath(dataPath, "promoters.2.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("mashroom", GetFilePath(dataPath, "agaricus-lepiota.2.data"), cvFolds);
            dataFiles.Add(benchmark.Name, benchmark);


            benchmark = new BenchmarkData("german", GetFilePath(dataPath, "german.data"), cvFolds)
            {
                DiscretizeUsingEntropy = false,
                DiscretizeUsingEqualFreq = true,
                FileFormat = FileFormat.Csv
            };

            int[] numericFields = new int[] { 2, 5, 8, 11, 13, 16, 18 };
            for (int i = 0; i < numericFields.Length; i++)
                benchmark.AddFieldInfo(numericFields[i], new DataFieldInfo(numericFields[i], typeof(int)) { IsNumeric = true });


            dataFiles.Add(benchmark.Name, benchmark);

            benchmark = new BenchmarkData("sat", GetFilePath(dataPath, "sat.trn"), GetFilePath(dataPath, "sat.tst"))
            {
                DiscretizeUsingEntropy = false,
                DiscretizeUsingEqualFreq = true
            };
            for (int i = 1; i <= 36; i++)
                benchmark.AddFieldInfo(i, new DataFieldInfo(i, typeof(int)) { IsNumeric = true });
            dataFiles.Add(benchmark.Name, benchmark);

            if (names != null && names.Length > 0)
            {
                Dictionary<string, BenchmarkData> result = new Dictionary<string, BenchmarkData>(names.Length);
                for(int i=0; i<names.Length; i++)
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
