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
using NRough.Data;
using NRough.Data.Benchmark;
using NUnit.Framework;

namespace NRough.Tests.Data
{
    [TestFixture]
    public class DataSamplerTest
    {
        [Test]
        public void GetDataTest()
        {
            var data = Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);
            DataSampler sampler = new DataSampler(data);
            DataStore subData = null;
            for (int i = 0; i < 10; i++)
            {
                subData = sampler.GetData(i).Item1;
                foreach (var field in subData.DataStoreInfo.Attributes)
                {
                    Assert.AreNotEqual(
                        field.Histogram,
                        data.DataStoreInfo.GetFieldInfo(field.Id).Histogram);
                }
            }
        }
    }
}