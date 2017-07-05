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
using NRough.Data;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning.Discretization;

namespace DermoReducts
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HandleMissingData(@"Data\dermatology.data", @"Results\dermatology-nomissing.csv", @"Results\dermatology-nomissing-weights.csv");
            DiscretizeAgeAttribute(@"Results\dermatology-nomissing.csv", @"Results\dermatology-disc-nomissing.csv");
        }

        public static void HandleMissingData(string filename, string outputfile, string weightsfile)
        {
            DataStore data = DataStore.Load(filename, DataFormat.CSV);
            double[] w = new double[data.NumberOfRecords];
            for (int i = 0; i < data.NumberOfRecords; i++)
                w[i] = 1.0;

            IReduct reduct = new ReductWeights(data, data.DataStoreInfo.SelectAttributeIds(a => a.IsStandard), 0.0, w);
            AttributeInfo ageAttribute = data.DataStoreInfo.GetFieldInfo(34); //a34

            Dictionary<long, int> countVals = new Dictionary<long, int>(ageAttribute.NumberOfValues);
            foreach (long val in ageAttribute.InternalValues())
                if (val != ageAttribute.MissingValueInternal)
                    countVals.Add(val, (int)ageAttribute.Histogram.GetBinValue(val));

            DataRecordInternal[] missingRecs = new DataRecordInternal[(int)ageAttribute.Histogram.GetBinValue(ageAttribute.MissingValueInternal)];

            for (int i = 0, j = 0; i < data.NumberOfRecords; i++)
            {
                DataRecordInternal record = data.GetRecordByIndex(i);
                if (record[34] == ageAttribute.MissingValueInternal)
                {
                    missingRecs[j++] = record;
                }
            }

            List<long> newRecIds = new List<long>();
            double tmp = 0.0;
            int k = 0;
            int origNumberOfRecords = data.NumberOfRecords;
            Array.Resize<double>(ref w, w.Length + (missingRecs.Length * ageAttribute.NumberOfValues));
            foreach (DataRecordInternal rec in missingRecs)
            {
                foreach (KeyValuePair<long, int> kvp in countVals)
                {
                    tmp = (double)kvp.Value / (double)(origNumberOfRecords - missingRecs.Length);
                    w[origNumberOfRecords + k] = tmp;
                    DataRecordInternal recBis = new DataRecordInternal(rec);
                    recBis[34] = kvp.Key;

                    newRecIds.Add(data.AddRow(recBis));
                    k++;
                }
            }

            foreach (DataRecordInternal rec in missingRecs)
            {
                w[rec.ObjectIdx] = 0;
            }

            data.DumpExt(outputfile, ",", false, true);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(weightsfile))
            {
                for (int objectIndex = 0; objectIndex < data.NumberOfRecords; objectIndex++)
                {
                    file.WriteLine(w[objectIndex].ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }

        public static void DiscretizeAgeAttribute(string filename, string outputfile)
        {
            DataStore data = DataStore.Load(filename, DataFormat.CSV);
            data.SetDecisionFieldId(35);

            AttributeInfo ageAttribute = data.DataStoreInfo.GetFieldInfo(34); //a34

            foreach (AttributeInfo f in data.DataStoreInfo.Attributes)
            {
                f.Alias = (f.Id == data.DataStoreInfo.DecisionFieldId) ? "d" : String.Format("a{0}", f.Id);
                f.Name = f.Alias;
            }

            foreach (long val in ageAttribute.InternalValues())
            {
                int value = (int)val;
                if (value != (int)ageAttribute.MissingValue)
                {
                    DecisionTableDiscretizer discretizer = new DecisionTableDiscretizer(new DiscretizeManual(value));
                    long[] cuts = discretizer.GetCuts(data, ageAttribute.Id, null);
                    long[] newValuesTrain = DiscretizeBase.Apply(data.GetColumn<long>(ageAttribute.Id), cuts);
                    
                    AttributeInfo newFieldInfo = data.DataStoreInfo.GetFieldInfo(data.AddColumn<long>(newValuesTrain));
                    newFieldInfo.IsNumeric = false;
                    newFieldInfo.Cuts = cuts;
                    newFieldInfo.DataType = typeof(long);
                    newFieldInfo.Alias = String.Format("{0}-{1}", "Age", value);
                }
            }

            data.RemoveColumn(34);
            data.DumpExt(outputfile, ",", true, true);
        }
    }
}