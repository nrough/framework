using Raccoon.Data;
using Raccoon.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raccoon.MachineLearning.Classification;
using Raccoon.MachineLearning.Discretization;

namespace Raccoon.MachineLearning
{
    public delegate void PostLearingMethod(IModel model);    

    public class CrossValidation
    {
        private static int DefaultFolds = 5;
        public bool RunInParallel { get; set; }
        public PostLearingMethod PostLearningMethod { get; set; }
        public DataStore Data { get; set; }
        public IDataStoreSplitter Splitter { get; set; }
        public bool Discretize { get; set; }
        public IFilter Filter { get; set; }

        public CrossValidation(DataStore data)
        {
            this.RunInParallel = true;
            this.Data = data;
            this.Splitter = new DataStoreSplitter(data, DefaultFolds, true);
        }

        public CrossValidation(DataStore data, int folds)
        {
            this.RunInParallel = true;
            this.Data = data;
            this.Splitter = new DataStoreSplitter(data, folds, true);
        }

        public CrossValidation(DataStore data, IDataStoreSplitter splitter)
        {
            this.RunInParallel = true;
            this.Data = data;
            this.Splitter = splitter;
        }

        public ClassificationResult Run<T>(T modelPrototype, int[] attributes)
            where T : IModel, IPredictionModel, ILearner, ICloneable, new()
        {            
            return this.CV<T>(modelPrototype, this.Data, attributes, this.Splitter);
        }

        public ClassificationResult Run<T>(T modelPrototype)
            where T : IModel, IPredictionModel, ILearner, ICloneable, new()
        {
            return this.Run<T>(modelPrototype, this.Data.GetStandardFields());
        }

        private ClassificationResult RunFold<T>(T modelPrototype, 
            IDataStoreSplitter dataSplitter, int fold, int[] attributes)
            where T : IModel, IPredictionModel, ILearner, ICloneable, new()
        {
            DataStore trainDS = null, testDS = null;
            dataSplitter.Split(ref trainDS, ref testDS, fold);

            if (Discretize)
            {
                var discretizer = new DataStoreDiscretizer();
                discretizer.Discretize(trainDS, trainDS.Weights);
            }

            DataStore filteredTrainDs = trainDS;
            if (Filter != null)
                filteredTrainDs = Filter.Apply(trainDS);

            HashSet<int> localAttributes = new HashSet<int>();
            foreach (var fieldId in attributes)
            {
                var fieldInfo = filteredTrainDs.DataStoreInfo.GetFieldInfo(fieldId);
                if (fieldInfo != null)
                    localAttributes.Add(fieldId);

                IEnumerable<int> derivedFieldIds = filteredTrainDs.DataStoreInfo
                        .GetFields(f => f.DerivedFrom == fieldId)
                        .Select(g => g.Id);

                foreach (var derivedFieldId in derivedFieldIds)
                    localAttributes.Add(derivedFieldId);
            }            

            T model = (T) modelPrototype.Clone();
            ClassificationResult result = model.Learn(filteredTrainDs, localAttributes.ToArray());

            if (this.PostLearningMethod != null)
                this.PostLearningMethod(model);

            if (Discretize)
            {
                if (testDS.DataStoreInfo.GetFields(FieldGroup.Standard).Any(f => f.CanDiscretize()))
                    DataStoreDiscretizer.Discretize(testDS, result.TestData);
            }

            DataStore filteredTestDs = trainDS;
            if (Filter != null)
                filteredTestDs = Filter.Apply(trainDS);

            return Classifier.DefaultClassifer.Classify(model, filteredTestDs);
        }

        private ClassificationResult CV<T>(T modelPrototype, DataStore data, 
            int[] attributes, IDataStoreSplitter dataSplitter)
            where T : IModel, IPredictionModel, ILearner, ICloneable, new()
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
                    MaxDegreeOfParallelism = RaccoonConfiguration.MaxDegreeOfParallelism
                };

                Parallel.For(0, dataSplitter.NFold, options, f =>
                {
                    result.AddLocalResult(this.RunFold<T>(modelPrototype, dataSplitter, f, attributes));
                });
            }
            else
            {                
                for (int f = 0; f < dataSplitter.NFold; f++)
                {
                    result.AddLocalResult(this.RunFold<T>(modelPrototype, dataSplitter, f, attributes));
                }
            }

            result.AvgNumberOfAttributes /= dataSplitter.NFold;
            result.NumberOfRules /= dataSplitter.NFold;
            result.MaxTreeHeight /= dataSplitter.NFold;
            result.AvgTreeHeight /= dataSplitter.NFold;
            result.ClassificationTime /= dataSplitter.NFold;
            result.ModelCreationTime /= dataSplitter.NFold;
            result.ExceptionRuleHitCounter /= dataSplitter.NFold;
            result.StandardRuleHitCounter /= dataSplitter.NFold;
            result.ExceptionRuleLengthSum /= dataSplitter.NFold;
            result.StandardRuleLengthSum /= dataSplitter.NFold;

            return result;
        }
    }
}
