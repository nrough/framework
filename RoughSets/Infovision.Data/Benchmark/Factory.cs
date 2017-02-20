using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoon.Data.Benchmark
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
            DataStore res = DataStore.Load(Path.Combine(location, "dna.train"), FileFormat.RSES1);
            foreach (var fieldInfo in res.DataStoreInfo.SelectAttributes(x => x.IsNumeric))
                fieldInfo.IsNumeric = false;
            return res;            
        }

        public static DataStore DnaTest()
        {
            DataStore train = Dna();
            return DataStore.Load(Path.Combine(location, "dna.test"), FileFormat.RSES1, train.DataStoreInfo);
        }

        public static DataStore DnaModified()
        {
            DataStore res = DataStore.Load(Path.Combine(location, "dna_modified.trn"), FileFormat.RSES1);
            foreach (var fieldInfo in res.DataStoreInfo.SelectAttributes(x => x.IsNumeric))
                fieldInfo.IsNumeric = false;
            return res; ;
        }

        public static DataStore DnaModifiedTest()
        {
            DataStore train = DnaModified();
            return DataStore.Load(Path.Combine(location, "dna_modified.tst"), FileFormat.RSES1, train.DataStoreInfo);
        }

        public static DataStore German()
        {
            return DataStore.Load(Path.Combine(location, "german.data"), FileFormat.CSV);
        }

        public static DataStore Mashroom()
        {
            return DataStore.Load(Path.Combine(location, "agaricus-lepiota.2.data"), FileFormat.RSES1);            
        }

        public static DataStore Audiology()
        {
            return DataStore.Load(Path.Combine(location, "audiology.standardized.2.data"), FileFormat.RSES1);
        }

        public static DataStore Breast()
        {            
            return DataStore.Load(Path.Combine(location, "breast-cancer-wisconsin.2.data"), FileFormat.RSES1);
        }

        public static DataStore Chess()
        {
            return DataStore.Load(Path.Combine(location, "chess.dta"), FileFormat.RSES1);
        }

        public static DataStore Dermatology()
        {            
            return DataStore.Load(Path.Combine(location, "dermatology.data"), FileFormat.CSV);
        }

        public static DataStore Connect4()
        {
            var res = DataStore.Load(Path.Combine(location, "connect-4.data"), FileFormat.CSV);
            res.SetDecisionFieldId(43);
            return res;
        }

        public static DataStore House()
        {
            return DataStore.Load(Path.Combine(location, "house-votes-84.2.data"), FileFormat.RSES1_1);
        }

        public static DataStore Hypothyrois()
        {            
            return DataStore.Load(Path.Combine(location, "hypothyroid.data"), FileFormat.CSV);
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
            return DataStore.Load(Path.Combine(location, "nursery.2.data"), FileFormat.RSES1);
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
            return DataStore.Load(Path.Combine(location, "playgolf.train"), FileFormat.RSES1);
        }

        public static DataStore Tenis()
        {
            return DataStore.Load(Path.Combine(location, "playgolf2.train"), FileFormat.RSES1);
        }

        public static DataStore Promoters()
        {
            return DataStore.Load(Path.Combine(location, "promoters.2.data"), FileFormat.RSES1);
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
            var res = DataStore.Load(Path.Combine(location, "semeion.data"), FileFormat.RSES1);
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
            return DataStore.Load(Path.Combine(location, "soybean-small.2.data"), FileFormat.RSES1); 
        }

        public static DataStore Spect()
        {
            throw new NotImplementedException();
        }

        public static DataStore Vehicle()
        {
            return DataStore.Load(Path.Combine(location, "vehicle.tab"), FileFormat.RSES1);
        }

        public static DataStore Vowel()
        {
            throw new NotImplementedException();
        }

        public static DataStore Zoo()
        {
            var res =  DataStore.Load(Path.Combine(location, "zoo.dta"), FileFormat.RSES1);
            foreach (var fieldInfo in res.DataStoreInfo.SelectAttributes(x => x.IsNumeric))
                fieldInfo.IsNumeric = false;
            return res;
        }
    }
}
