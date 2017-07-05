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
        //private static readonly string location = @"Examples\Data";

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

        public static DataStore DnaTrain()
        {            
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dna.train"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }                                                           
        }

        public static DataStore DnaTest()
        {
            DataStore train = DnaTrain();            
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dna.test"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }                        
        }

        public static DataStore DnaModifiedTrain()
        {            
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dna_modified.trn"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }            
        }

        public static DataStore DnaModifiedTest()
        {
            DataStore train = DnaModifiedTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dna_modified.tst"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }                        
        }

        public static DataStore German()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.german.data"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                return dataReader.Read();
            }           
        }

        public static DataStore Mashroom()
        {                       
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.agaricus-lepiota.2.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
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
        }

        public static DataStore Chess()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.chess.dta"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }           
        }

        public static DataStore Dermatology()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.dermatology.data"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                return dataReader.Read();
            }
        }

        public static DataStore Connect4()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.connect-4.data"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                var data = dataReader.Read();
                data.SetDecisionFieldId(43);
                return data;
            }                        
        }

        public static DataStore House()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.house-votes-84.2.data"))
            {
                IDataReader dataReader = new DataRSES11FileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }            
        }

        public static DataStore Hypothyrois()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.hypothyroid.data"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                return dataReader.Read();
            }            
        }

        public static DataStore LetterTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.letter.trn"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore LetterTest()
        {
            DataStore train = LetterTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.letter.tst"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore LetterDiscTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.letter.disc.trn"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore LetterDiscTest()
        {
            DataStore train = LetterDiscTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.letter.disc.tst"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore Lymphography()
        {            
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.lymphography.all"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }            
        }

        public static DataStore Monks1Train()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.monks-1.train"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore Monks1Test()
        {
            DataStore train = Monks1Train();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.monks-1.test"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore Monks2Train()
        {            
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.monks-2.train"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore Monks2Test()
        {
            DataStore train = Monks2Train();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.monks-2.test"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore Monks3Train()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.monks-3.train"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore Monks3Test()
        {
            DataStore train = Monks3Train();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.monks-3.test"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore Nursery()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.nursery.2.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }
        }

        public static DataStore OptTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.optdigits.trn"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore OptTest()
        {
            DataStore train = OptTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.optdigits.tst"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore OptDiscTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.optdigits.disc.trn"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore OptDiscTest()
        {
            DataStore train = OptDiscTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.optdigits.disc.tst"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore PenTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.pendigits.trn"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore PenTest()
        {
            DataStore train = PenTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.pendigits.tst"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore PenDiscTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.pendigits.disc.trn"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore PenDiscTest()
        {
            DataStore train = PenDiscTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.pendigits.disc.tst"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }        

        public static DataStore Golf()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.playgolf.train"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }            
        }

        public static DataStore Tenis()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.playgolf2.train"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }            
        }

        public static DataStore Promoters()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.promoters.2.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }            
        }

        public static DataStore Sat()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.sat.dta"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }
        }

        public static DataStore SatTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.sat.trn"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }
        }

        public static DataStore SatTest()
        {
            DataStore train = SatTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.sat.tst"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore SatDiscTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.sat.disc.trn"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }
        }

        public static DataStore SatDiscTest()
        {
            DataStore train = SatDiscTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.sat.disc.tst"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore Semeion()
        {           
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.semeion.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }            
        }

        public static DataStore SoybeanLargeTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.soybean-large.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return dataReader.Read();
            }
        }

        public static DataStore SoybeanLargeTest()
        {
            DataStore train = SoybeanLargeTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.soybean-large.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore SoybeanSmall()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.soybean-small.2.data"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }            
        }

        public static DataStore SpectTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.SPECT.train"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore SpectTest()
        {
            DataStore train = SpectTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.SPECT.test"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore Vehicle()
        {
            DataStore result = null;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.vehicle.tab"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                result = dataReader.Read();
            }

            result.DataStoreInfo.DecisionInfo.IsNumeric = false;
            return result;                        
        }

        public static DataStore VowelTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.vowel.trn"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                return dataReader.Read();
            }
        }

        public static DataStore VowelTest()
        {
            DataStore train = VowelTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                 "NRough.Data.Benchmark.Data.vowel.tst"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore VowelDiscTrain()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.vowel.disc.trn"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                return dataReader.Read();
            }
        }

        public static DataStore VowelDiscTest()
        {
            DataStore train = VowelDiscTrain();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                 "NRough.Data.Benchmark.Data.vowel.disc.tst"))
            {
                IDataReader dataReader = new DataCSVFileReader(stream);
                dataReader.ReferenceDataStoreInfo = train.DataStoreInfo;
                return dataReader.Read();
            }
        }

        public static DataStore Zoo()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "NRough.Data.Benchmark.Data.zoo.dta"))
            {
                IDataReader dataReader = new DataRSESFileReader(stream);
                return SetAllAttributesToNominal(dataReader.Read());
            }
        }

        public static DataStore SetAllAttributesToNominal(DataStore data)
        {
            foreach (var fieldInfo in data.DataStoreInfo.SelectAttributes(x => x.IsNumeric))
                fieldInfo.IsNumeric = false;
            return data;
        }
    }
}
