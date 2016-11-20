using Infovision.Data;
using Infovision.Utils;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Infovision.Datamining
{
    public interface IClassifier
    {
        ClassificationResult Classify(IPredictionModel model, DataStore testData, double[] weights = null);
    }

    public class Classifier : IClassifier
    {
        public static long UnclassifiedOutput = -1;
        private static volatile Classifier instance = null;
        private static object syncRoot = new object();

        public static Classifier DefaultClassifer
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

        public ClassificationResult Classify(IPredictionModel model, DataStore testData, double[] weights = null)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            ClassificationResult result = new ClassificationResult(testData, testData.DataStoreInfo.GetDecisionValues());            
            model.SetClassificationResultParameters(result);                    

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = InfovisionConfiguration.MaxDegreeOfParallelism
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
                }
                );
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
                }
                );
            }
            s.Stop();

            result.ClassificationTime = s.ElapsedMilliseconds;

            return result;
        }
    }
}