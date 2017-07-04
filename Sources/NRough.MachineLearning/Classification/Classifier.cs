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
using NRough.Core;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification
{
    public interface IClassifier
    {
        ClassificationResult Classify(IClassificationModel model, DataStore testData, double[] weights = null);
    }

    public class Classifier : IClassifier
    {
        public static long UnclassifiedOutput = -1;

        private static volatile Classifier instance = null;
        private static object syncRoot = new object();

        public static Classifier Default
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new Classifier();
                        }
                    }
                }

                return instance;
            }
        }

        public ClassificationResult Classify(IClassificationModel model, DataStore testData, double[] weights = null)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            ClassificationResult result = new ClassificationResult(
                testData, testData.DataStoreInfo.GetDecisionValues());            
            model.SetClassificationResultParameters(result);                    

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism
            };

            if (weights == null)
            {
                double w = 1.0 / testData.NumberOfRecords;
                //for(int objectIndex = 0; objectIndex<testData.NumberOfRecords; objectIndex++)
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = model.Compute(record);

                    if (prediction == Classifier.UnclassifiedOutput && model.DefaultOutput != null)
                        prediction = (long) model.DefaultOutput;

                    result.AddResult(objectIndex, prediction, record[testData.DataStoreInfo.DecisionFieldId], w);
                });
            }
            else
            {
                Parallel.For(0, testData.NumberOfRecords, options, objectIndex =>
                {
                    DataRecordInternal record = testData.GetRecordByIndex(objectIndex, false);
                    var prediction = model.Compute(record);

                    if (prediction == Classifier.UnclassifiedOutput && model.DefaultOutput != null)
                        prediction = (long)model.DefaultOutput;

                    result.AddResult(objectIndex, prediction, record[testData.DataStoreInfo.DecisionFieldId], (double)weights[objectIndex]);
                });
            }
            s.Stop();

            result.ClassificationTime = s.ElapsedMilliseconds;

            return result;
        }
    }
}