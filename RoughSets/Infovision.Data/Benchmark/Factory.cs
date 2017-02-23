using NRough.Data.Properties;
using NRough.Data.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Benchmark
{
    public class Factory
    {        
        private static readonly Factory instance = new Factory();
        private static readonly string location = @"Examples\Data";

        public static Factory Instance
        {
            get
            {
                return instance;
            }
        }

        private Factory()
        {
        }

        public static DataStore Dna()
        {
            DataStore res;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dna.train"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                res = dataReader.Read();
            }

            foreach (var fieldInfo in res.DataStoreInfo.SelectAttributes(x => x.IsNumeric))
                fieldInfo.IsNumeric = false;
            return res;
            
            //DataStore res = DataStore.Load(Path.Combine(location, "dna.train"), DataFormat.RSES1);
            //foreach (var fieldInfo in res.DataStoreInfo.SelectAttributes(x => x.IsNumeric))
            //    fieldInfo.IsNumeric = false;
            //return res;            
        }

        public static DataStore DnaTest()
        {
            DataStore train = Dna();            
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dna.test"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }            

            //DataStore train = Dna();
            //return DataStore.Load(Path.Combine(location, "dna.test"), DataFormat.RSES1, train.DataStoreInfo);
        }

        public static DataStore DnaModified()
        {
            DataStore res;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dna_modified.trn"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                res = dataReader.Read();
            }

            foreach (var fieldInfo in res.DataStoreInfo.SelectAttributes(x => x.IsNumeric))
                fieldInfo.IsNumeric = false;
            return res;
            
            //DataStore res = DataStore.Load(Path.Combine(location, "dna_modified.trn"), DataFormat.RSES1);
            //foreach (var fieldInfo in res.DataStoreInfo.SelectAttributes(x => x.IsNumeric))
            //    fieldInfo.IsNumeric = false;
            //return res; ;
        }

        public static DataStore DnaModifiedTest()
        {
            DataStore train = Dna();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dna_modified.tst"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }            

            //DataStore train = DnaModified();
            //return DataStore.Load(Path.Combine(location, "dna_modified.tst"), DataFormat.RSES1, train.DataStoreInfo);
        }

        public static DataStore German()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.german.data"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "german.data"), DataFormat.CSV);
        }

        public static DataStore Mashroom()
        {            
            //var names1 = typeof(Factory).Assembly.GetManifestResourceNames();
            //var names2 = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.agaricus-lepiota.2.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }            
        }

        public static DataStore Audiology()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.audiology.standardized.2.data"))
            { 
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }            
        }

        public static DataStore Breast()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.breast-cancer-wisconsin.2.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "breast-cancer-wisconsin.2.data"), DataFormat.RSES1);
        }

        public static DataStore Chess()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.chess.dta"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "chess.dta"), DataFormat.RSES1);
        }

        public static DataStore Dermatology()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dermatology.data"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "dermatology.data"), DataFormat.CSV);
        }

        public static DataStore Connect4()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dermatology.data"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                var data = dataReader.Read();
                data.SetDecisionFieldId(43);
                return data;
            }            

            //var res = DataStore.Load(Path.Combine(location, "connect-4.data"), DataFormat.CSV);
            //res.SetDecisionFieldId(43);
            //return res;
        }

        public static DataStore House()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.house-votes-84.2.data"))
            {
                IDataReader dataReader = new DataRSES11FileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "house-votes-84.2.data"), DataFormat.RSES1_1);
        }

        public static DataStore Hypothyrois()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.hypothyroid.data"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "hypothyroid.data"), DataFormat.CSV);
        }

        public static DataStore Letter()
        {           
            throw new NotImplementedException();
        }

        public static DataStore LetterTest()
        {
            throw new NotImplementedException();
        }

        public static DataStore Lymphography()
        {
            throw new NotImplementedException();
        }

        public static DataStore Monks1()
        {
            throw new NotImplementedException();
        }

        public static DataStore Monks1Test()
        {
            throw new NotImplementedException();
        }

        public static DataStore Monks2()
        {
            throw new NotImplementedException();
        }

        public static DataStore Monks2Test()
        {
            throw new NotImplementedException();
        }

        public static DataStore Monks3()
        {
            throw new NotImplementedException();
        }

        public static DataStore Monks3Test()
        {
            throw new NotImplementedException();
        }

        public static DataStore Nursery()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.nursery.2.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "nursery.2.data"), DataFormat.RSES1);
        }

        public static DataStore Opt()
        {
            throw new NotImplementedException();
        }

        public static DataStore OptTest()
        {
            throw new NotImplementedException();
        }

        public static DataStore Pen()
        {
            throw new NotImplementedException();
        }

        public static DataStore PenTest()
        {
            throw new NotImplementedException();
        }

        public static DataStore Golf()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.playgolf.train"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "playgolf.train"), DataFormat.RSES1);
        }

        public static DataStore Tenis()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.playgolf2.train"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "playgolf2.train"), DataFormat.RSES1);
        }

        public static DataStore Promoters()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.promoters.2.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "promoters.2.data"), DataFormat.RSES1);
        }

        public static DataStore Sat()
        {
            throw new NotImplementedException();
        }

        public static DataStore SatTest()
        {
            throw new NotImplementedException();
        }

        public static DataStore Semeion()
        {
            DataStore res;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.semeion.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                res = dataReader.Read();
            }

            //var res = DataStore.Load(Path.Combine(location, "semeion.data"), DataFormat.RSES1);
            foreach (var fieldInfo in res.DataStoreInfo.SelectAttributes(x => x.IsNumeric))
                fieldInfo.IsNumeric = false;
            return res;
        }

        public static DataStore SoybeanLarge()
        {
            throw new NotImplementedException();
        }

        public static DataStore SoybeanSmall()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.soybean-small.2.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "soybean-small.2.data"), DataFormat.RSES1); 
        }

        public static DataStore Spect()
        {
            throw new NotImplementedException();
        }

        public static DataStore Vehicle()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.vehicle.tab"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }

            //return DataStore.Load(Path.Combine(location, "vehicle.tab"), DataFormat.RSES1);
        }

        public static DataStore Vowel()
        {
            throw new NotImplementedException();
        }

        public static DataStore Zoo()
        {
            var res =  DataStore.Load(Path.Combine(location, "zoo.dta"), DataFormat.RSES1);
            foreach (var fieldInfo in res.DataStoreInfo.SelectAttributes(x => x.IsNumeric))
                fieldInfo.IsNumeric = false;
            return res;
        }
    }
}
