using Infovision.Data;
using Infovision.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining
{
    public delegate void PostLearingMethod(IModel model);

    public class CrossValidation<T>
        where T : IModel, IPredictionModel, ILearner, ICloneable, new()
    {
        private T modelPrototype;        
        private static int DefaultFolds = 10;

        public bool RunInParallel { get; set; }
        public Dictionary<int, int[]> Attributes { get; set; }

        public PostLearingMethod PostLearningMethod { get; set; }

        public CrossValidation(T model)
        {
            this.RunInParallel = true;
            this.modelPrototype = model;            
        }        

        public ClassificationResult Run(DataStore data, int[] attributes, IDataStoreSplitter dataSplitter)
        {            
            return this.CV(data, attributes, dataSplitter);
        }

        public ClassificationResult Run(DataStore data, int[] attributes, int folds)
        {                        
            return this.CV(data, attributes, new DataStoreSplitter(data, folds));
        }

        public ClassificationResult Run(DataStore data, int[] attributes)
        {
            return this.Run(data, attributes, CrossValidation<T>.DefaultFolds);
        }

        public ClassificationResult Run(DataStore data, DataStoreSplitter splitter)
        {
            return this.Run(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray(), splitter);
        }

        public ClassificationResult Run(DataStore data, int folds)
        {
            return this.Run(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray(), folds);
        }

        public ClassificationResult Run(DataStore data)
        {
            return this.Run(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray(), CrossValidation<T>.DefaultFolds);
        }

        private ClassificationResult CV(DataStore data, int[] attributes, IDataStoreSplitter dataSplitter)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (attributes == null) throw new ArgumentNullException("attributes");
            if (dataSplitter == null) throw new ArgumentNullException("dataSplitter");

            ClassificationResult result = new ClassificationResult(data, data.DataStoreInfo.GetDecisionValues());
            modelPrototype.SetClassificationResultParameters(result);
            
            if (this.RunInParallel)
            {
                var options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
                };

                Parallel.For(0, dataSplitter.NFold, options, f =>
                {
                    DataStore trainDS = null, testDS = null;
                    dataSplitter.Split(ref trainDS, ref testDS, f);

                    int[] localAttributes = attributes;
                    if (this.Attributes != null)
                        if (!this.Attributes.TryGetValue(f, out localAttributes))
                            localAttributes = attributes;

                    T model = (T)this.modelPrototype.Clone();
                    model.Learn(trainDS, localAttributes);

                    if (this.PostLearningMethod != null)
                        this.PostLearningMethod(model);

                    result.AddLocalResult(Classifier.DefaultClassifer.Classify(model, testDS));
                });
            }
            else
            {
                DataStore trainDS = null, testDS = null;
                for (int f = 0; f < dataSplitter.NFold; f++)
                {
                    dataSplitter.Split(ref trainDS, ref testDS, f);
                    
                    int[] localAttributes = attributes;
                    if (this.Attributes != null)
                        if (!this.Attributes.TryGetValue(f, out localAttributes))
                            localAttributes = attributes;

                    T model = (T)this.modelPrototype.Clone();
                    model.Learn(trainDS, localAttributes);

                    if (this.PostLearningMethod != null)
                        this.PostLearningMethod(model);

                    result.AddLocalResult(Classifier.DefaultClassifer.Classify(model, testDS));
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
