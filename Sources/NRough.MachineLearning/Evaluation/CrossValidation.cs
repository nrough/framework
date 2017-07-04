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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRough.MachineLearning.Classification;
using NRough.Doc;

namespace NRough.MachineLearning.Evaluation
{
    [AssemblyTreeVisible(false)]
    public delegate void PostLearingMethod(IModel model, DataStore trainingData);

    [Serializable]
    public class CrossValidation
    {
        private static int DefaultFolds = 5;
        private static int DefaultRepeat = 1;

        public bool RunInParallel { get; set; }
        public PostLearingMethod PostLearningMethod { get; set; }
        public DataStore Data { get; set; }
        public IDataSplitter Splitter { get; set; }        
        public IList<IFilter> Filters { get; set; }
        public int Iterations { get; set; }

        public CrossValidation(DataStore data, IDataSplitter splitter, int repeat)
        {
            this.RunInParallel = true;
            this.Data = data;
            this.Splitter = splitter;
            this.Filters = new List<IFilter>();
            this.Iterations = repeat;
        }

        public CrossValidation(DataStore data, IDataSplitter splitter)
            : this(data, splitter, DefaultRepeat) { }

        public CrossValidation(DataStore data, int folds, int repeat)
            : this(data, new DataSplitter(data, folds, true), repeat) { }

        public CrossValidation(DataStore data, int folds)
            : this(data, new DataSplitter(data, folds, true), DefaultRepeat) { }

        public CrossValidation(DataStore data)
            : this(data, new DataSplitter(data, DefaultFolds, true), DefaultRepeat) { }
                
        public ClassificationResult Run<T>(T modelPrototype, int[] attributes)
            where T : IModel, IClassificationModel, ILearner, ICloneable, new()
        {            
            return this.CV<T>(modelPrototype, this.Data, attributes, this.Splitter);
        }

        public ClassificationResult Run<T>(T modelPrototype)
            where T : IModel, IClassificationModel, ILearner, ICloneable, new()
        {
            return this.Run<T>(modelPrototype, this.Data.GetStandardFields());
        }

        private DataStore ComputeFilters(DataStore data)
        {
            var res = data;            
            foreach (var filter in Filters.Where(f => f.Enabled))
            {                
                filter.Compute(res);
                res = filter.Apply(res);
            }
            return res;
        }

        private DataStore ApplyFilters(DataStore data)
        {
            var res = data;
            foreach (var filter in Filters.Where(f => f.Enabled))
                res = filter.Apply(res);
            return res;
        }

        private ClassificationResult RunFold<T>(T modelPrototype, 
            IDataSplitter dataSplitter, int fold, int[] attributes)
            where T : IModel, IClassificationModel, ILearner, ICloneable, new()
        {
            DataStore trainDs = null, testDs = null;
            dataSplitter.Split(out trainDs, out testDs, fold);
            
            DataStore filteredTrainDs = ComputeFilters(trainDs);

            HashSet<int> localAttributes = new HashSet<int>();
            foreach (var fieldId in attributes)
            {
                var fieldInfo = filteredTrainDs.DataStoreInfo.GetFieldInfo(fieldId);
                if (fieldInfo != null)
                    localAttributes.Add(fieldId);

                IEnumerable<int> derivedFieldIds = filteredTrainDs.DataStoreInfo
                        .SelectAttributes(f => f.DerivedFrom == fieldId)
                        .Select(g => g.Id);

                foreach (var derivedFieldId in derivedFieldIds)
                    localAttributes.Add(derivedFieldId);
            }            

            T model = (T) modelPrototype.Clone();
            ClassificationResult result = model.Learn(filteredTrainDs, localAttributes.ToArray());

            if (this.PostLearningMethod != null)
                this.PostLearningMethod(model, filteredTrainDs);

            return Classifier.Default.Classify(model, ApplyFilters(testDs));
        }

        private ClassificationResult CV<T>(T modelPrototype, DataStore data, 
            int[] attributes, IDataSplitter dataSplitter)
            where T : IModel, IClassificationModel, ILearner, ICloneable, new()
        {
            if (data == null) throw new ArgumentNullException("data");
            if (attributes == null) throw new ArgumentNullException("attributes");
            if (dataSplitter == null) throw new ArgumentNullException("dataSplitter");

            ClassificationResult result = new ClassificationResult(data, data.DataStoreInfo.GetDecisionValues());
            modelPrototype.SetClassificationResultParameters(result);
            result.Reset();

            if (this.RunInParallel)
            {
                var options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = ConfigManager.MaxDegreeOfParallelism
                };

                Parallel.For(0, dataSplitter.NFold, options, f =>
                {
                    result.MergeResult(this.RunFold<T>(modelPrototype, dataSplitter, f, attributes));
                });
            }
            else
            {                
                for (int f = 0; f < dataSplitter.NFold; f++)
                {
                    result.MergeResult(this.RunFold<T>(modelPrototype, dataSplitter, f, attributes));
                }
            }

            result.AverageIndicators(dataSplitter.NFold);
            
            return result;
        }
    }
}
