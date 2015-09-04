using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;

namespace Infovision.Datamining.Roughset.UnitTests
{
    public class BenchmarkDataHelper
    {
        public static Dictionary<string, BenchmarkData> GetDataFiles()
        {
            Dictionary<string, BenchmarkData> dataFiles = new Dictionary<string, BenchmarkData>();

            //dataFiles.Add("golf", new BenchmarkData("golf", @"Data\playgolf.train", @"Data\playgolf.train"));

            
            //dataFiles.Add("opt", new BenchmarkData("opt", @"Data\optdigits.trn", @"Data\optdigits.tst"));
            //dataFiles.Add("dna", new BenchmarkData("dna", @"Data\dna_modified.trn", @"Data\dna_modified.tst"));
            dataFiles.Add("letter", new BenchmarkData("letter", @"Data\letter.trn", @"Data\letter.tst"));
            dataFiles.Add("monks-1", new BenchmarkData("monks-1", @"Data\monks-1.train", @"Data\monks-1.test"));
            dataFiles.Add("monks-2", new BenchmarkData("monks-2", @"Data\monks-2.train", @"Data\monks-2.test"));
            dataFiles.Add("monks-3", new BenchmarkData("monks-3", @"Data\monks-3.train", @"Data\monks-3.test"));
            dataFiles.Add("zoo", new BenchmarkData("zoo", @"Data\zoo.dta", 5));
            dataFiles.Add("spect", new BenchmarkData("spect", @"Data\SPECT.train", @"Data\SPECT.test"));
            dataFiles.Add("semeion", new BenchmarkData("semeion", @"Data\semeion.data", 5));
            dataFiles.Add("pen", new BenchmarkData("pen", @"Data\pendigits.trn", @"Data\pendigits.tst"));
            
            

            return dataFiles;
        }
    }
}
