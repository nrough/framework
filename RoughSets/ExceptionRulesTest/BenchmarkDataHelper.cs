using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace ExceptionRulesTest
{
    public class BenchmarkDataHelper
    {
        public static IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles(params string[] names)
        {
            Dictionary<string, BenchmarkData> dataFiles = new Dictionary<string, BenchmarkData>();

            BenchmarkData benchmark = new BenchmarkData("golf", @"Data\playgolf.train", @"Data\playgolf.train");

            benchmark.AddFieldAlias(1, "O");
            benchmark.AddFieldAlias(2, "T");
            benchmark.AddFieldAlias(3, "H");
            benchmark.AddFieldAlias(4, "W");

            dataFiles.Add("golf", benchmark);
            
            dataFiles.Add("dna", new BenchmarkData("dna", @"Data\dna_modified.trn", @"Data\dna_modified.tst"));
            dataFiles.Add("zoo", new BenchmarkData("zoo", @"Data\zoo.dta", 5));
            dataFiles.Add("monks-1", new BenchmarkData("monks-1", @"Data\monks-1.train", @"Data\monks-1.test"));
            dataFiles.Add("monks-2", new BenchmarkData("monks-2", @"Data\monks-2.train", @"Data\monks-2.test"));
            dataFiles.Add("monks-3", new BenchmarkData("monks-3", @"Data\monks-3.train", @"Data\monks-3.test"));            
            dataFiles.Add("spect", new BenchmarkData("spect", @"Data\SPECT.train", @"Data\SPECT.test"));
            dataFiles.Add("letter", new BenchmarkData("letter", @"Data\letter.trn", @"Data\letter.tst"));                                    
            dataFiles.Add("pen", new BenchmarkData("pen", @"Data\pendigits.trn", @"Data\pendigits.tst"));
            dataFiles.Add("opt", new BenchmarkData("opt", @"Data\optdigits.trn", @"Data\optdigits.tst"));
            dataFiles.Add("semeion", new BenchmarkData("semeion", @"Data\semeion.data", 5));

            if (names != null && names.Length > 0)
                return dataFiles.Where(item => names.Contains(item.Key));

            return dataFiles;
        }

        public static List<KeyValuePair<string, BenchmarkData>> GetDataFilesList(params string[] names)
        {
            List<KeyValuePair<string, BenchmarkData>> result = new List<KeyValuePair<string, BenchmarkData>>();
            foreach (var kvp in BenchmarkDataHelper.GetDataFiles(names))
            {
                result.Add(kvp);
            }
            return result;
        }
    }
}
