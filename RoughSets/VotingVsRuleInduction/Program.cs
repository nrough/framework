using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Configuration;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Benchmark;
using Infovision.Datamining.Experimenter;
using Infovision.Datamining.Experimenter.Parms;
using Infovision.Datamining.Roughset;
using Infovision.Math;
using Infovision.Utils;

namespace VotingVsRuleInduction
{
    class Program
    {
        private static ILog log;
        private Dictionary<int, PermutationCollection> permutationCollections = new Dictionary<int, PermutationCollection>();

        static void Main(string[] args)
        {
            int numberOfTests = Int32.Parse(args[0]);
            string[] datasets = new string[args.Length - 1];
            Array.Copy(args, 1, datasets, 0, args.Length - 1);

            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();
            Program program = new Program();

            NameValueCollection properties = new NameValueCollection();
            properties["showDateTime"] = "true";
            properties["showLogName"] = "true";
            properties["level"] = "All";
            properties["configType"] = "FILE";
            properties["configFile"] = "~/NLog.config";
            
            Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter(properties);
            log = Common.Logging.LogManager.GetLogger(program.GetType());

            ReductLengthComparer reductLengthComparer = new ReductLengthComparer();
            ReductMeasureLength reductMeasureLength = new ReductMeasureLength();

            foreach (var kvp in BenchmarkDataHelper.GetDataFiles("Data", datasets))
            {                
                using (StreamWriter outputFile = new StreamWriter(Path.Combine("results", kvp.Key + ".result"), false)) 
                {
                    Console.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}{9}|{10}",
                        "Test",
                        ReductGeneratorParamHelper.TrainData,
                        ReductGeneratorParamHelper.TestData,
                        ReductGeneratorParamHelper.FactoryKey,
                        ReductGeneratorParamHelper.NumberOfReducts,
                        ReductGeneratorParamHelper.Epsilon,
                        ReductGeneratorParamHelper.IdentificationType,
                        ReductGeneratorParamHelper.VoteType,
                        ClassificationResult.ResultHeader(),
                        "ReductCalculationTime",
                        "ClassificationTime"
                        );

                    outputFile.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}{9}|{10}",
                        "Test",
                        ReductGeneratorParamHelper.TrainData,
                        ReductGeneratorParamHelper.TestData,
                        ReductGeneratorParamHelper.FactoryKey,
                        ReductGeneratorParamHelper.NumberOfReducts,
                        ReductGeneratorParamHelper.Epsilon,
                        ReductGeneratorParamHelper.IdentificationType,
                        ReductGeneratorParamHelper.VoteType,
                        ClassificationResult.ResultHeader(),
                        "ReductCalculationTime",
                        "ClassificationTime"
                        );

                    for (int t = 0; t < numberOfTests; t++)
                    {
                        ParameterCollection parms = program.Parameters(kvp.Value);
                        program.permutationCollections = new Dictionary<int, PermutationCollection>();

                        string lastTrainName = null;                        
                        string lastFactoryKey = null;
                        decimal lastEpsilon = Decimal.MinusOne;
                        int lastNumberOfReducts = -1;

                        IReductGenerator reductGenerator = null;
                        IReductStoreCollection origReductStoreCollection = null;
                        IReductStoreCollection filteredReductStoreCollection = null;
                        bool emptyReductResult = false;                        

                        foreach (var parmVector in parms.Values())
                        {
                            bool regeneratedReducts = false;
                            var setup = program.ConvertParameterVector(parmVector);

                            if (lastTrainName != ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TrainData)).Name
                                || lastFactoryKey != (string)setup.GetParameter(ReductGeneratorParamHelper.FactoryKey))
                            {
                                emptyReductResult = false;
                            }
                            
                            if(lastTrainName != ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TrainData)).Name                                
                                || lastFactoryKey != (string)setup.GetParameter(ReductGeneratorParamHelper.FactoryKey)
                                || lastEpsilon != (decimal)setup.GetParameter(ReductGeneratorParamHelper.Epsilon))
                            {
                                if (emptyReductResult == false)
                                {
                                    reductGenerator = ReductFactory.GetReductGenerator(setup);
                                    reductGenerator.Run();
                                    origReductStoreCollection = reductGenerator.GetReductStoreCollection();

                                    regeneratedReducts = true;
                                }
                            }

                            if (lastTrainName != ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TrainData)).Name
                                || lastFactoryKey != (string)setup.GetParameter(ReductGeneratorParamHelper.FactoryKey)
                                || lastEpsilon != (decimal)setup.GetParameter(ReductGeneratorParamHelper.Epsilon)
                                || lastNumberOfReducts != (int)setup.GetParameter(ReductGeneratorParamHelper.NumberOfReducts)
                                || regeneratedReducts)
                            {
                                filteredReductStoreCollection = origReductStoreCollection.Filter(
                                        (int)setup.GetParameter(ReductGeneratorParamHelper.NumberOfReducts),
                                        reductLengthComparer);
                            }

                            RoughClassifier classifier = new RoughClassifier(
                                filteredReductStoreCollection,
                                (RuleQualityFunction)setup.GetParameter(ReductGeneratorParamHelper.IdentificationType),
                                (RuleQualityFunction)setup.GetParameter(ReductGeneratorParamHelper.VoteType),
                                ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TrainData)).DataStoreInfo.GetDecisionValues());
                            ClassificationResult result = classifier.Classify((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TestData));
                            result.QualityRatio = filteredReductStoreCollection.GetWeightedAvgMeasure(reductMeasureLength, true);

                            if (DoubleEpsilonComparer.Instance.Equals(result.QualityRatio, 0.0d))
                            {
                                emptyReductResult = true;
                            }

                            Console.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}{9}|{10}",
                                t,
                                ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TrainData)).Name,
                                ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TestData)).Name,
                                setup.GetParameter(ReductGeneratorParamHelper.FactoryKey),
                                setup.GetParameter(ReductGeneratorParamHelper.NumberOfReducts),                                
                                setup.GetParameter(ReductGeneratorParamHelper.Epsilon),
                                ((RuleQualityFunction)setup.GetParameter(ReductGeneratorParamHelper.IdentificationType)).Method.Name,
                                ((RuleQualityFunction)setup.GetParameter(ReductGeneratorParamHelper.VoteType)).Method.Name,
                                result,
                                regeneratedReducts ? reductGenerator.ReductGenerationTime : 0,
                                classifier.ClassificationTime
                                );

                            outputFile.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}{9}|{10}",
                                t,
                                ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TrainData)).Name,
                                ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TestData)).Name,
                                setup.GetParameter(ReductGeneratorParamHelper.FactoryKey),
                                setup.GetParameter(ReductGeneratorParamHelper.NumberOfReducts),                                
                                setup.GetParameter(ReductGeneratorParamHelper.Epsilon),
                                ((RuleQualityFunction)setup.GetParameter(ReductGeneratorParamHelper.IdentificationType)).Method.Name,
                                ((RuleQualityFunction)setup.GetParameter(ReductGeneratorParamHelper.VoteType)).Method.Name,
                                result,
                                regeneratedReducts ? reductGenerator.ReductGenerationTime : 0,
                                classifier.ClassificationTime
                                );

                            lastTrainName = ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TrainData)).Name;                            
                            lastFactoryKey = (string)setup.GetParameter(ReductGeneratorParamHelper.FactoryKey);
                            lastEpsilon = (decimal)setup.GetParameter(ReductGeneratorParamHelper.Epsilon);
                            lastNumberOfReducts = (int)setup.GetParameter(ReductGeneratorParamHelper.NumberOfReducts);
                        }
                    }
                }
            }

            Console.ReadKey();
        }

        public Args ConvertParameterVector(object[] parameterVector)
        {
            Tuple<DataStore, DataStore> data = (Tuple<DataStore, DataStore>)parameterVector[0];            
            int numberOfReducts = (int)parameterVector[3];

            PermutationCollection permuationCollection = null;
            if (permutationCollections.ContainsKey(numberOfReducts))
            {
                permuationCollection = permutationCollections[numberOfReducts];
            }
            else
            {                
                permuationCollection = new PermutationGenerator(data.Item1).Generate(numberOfReducts * 10);
                permutationCollections[numberOfReducts] = permuationCollection;
            }            

            Args conf = new Args();
            conf.SetParameter(ReductGeneratorParamHelper.TrainData, ((Tuple<DataStore, DataStore>)parameterVector[0]).Item1);
            conf.SetParameter(ReductGeneratorParamHelper.FactoryKey, parameterVector[1]);
            conf.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permuationCollection);
            conf.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, parameterVector[3]);
            conf.SetParameter(ReductGeneratorParamHelper.Epsilon, parameterVector[2]);

            conf.SetParameter(ReductGeneratorParamHelper.TestData, ((Tuple<DataStore, DataStore>)parameterVector[0]).Item2);
            conf.SetParameter(ReductGeneratorParamHelper.IdentificationType, parameterVector[4]);
            conf.SetParameter(ReductGeneratorParamHelper.VoteType, parameterVector[5]);

            return conf;

        }

        public ParameterCollection Parameters(BenchmarkData benchmark)
        {
            DataStore t1 = null, t2 = null;
            Tuple<DataStore, DataStore>[] dataTuple = new Tuple<DataStore, DataStore>[benchmark.CrossValidationFolds];

            if (benchmark.CrossValidationActive)
            {                
                DataStore data = DataStore.Load(benchmark.DataFile, benchmark.FileFormat);
                DataStoreSplitter splitter = new DataStoreSplitter(data, benchmark.CrossValidationFolds);
                
                for (int i = 0; i < benchmark.CrossValidationFolds; i++)
                {                    
                    splitter.ActiveFold = i;
                    splitter.Split(ref t1, ref t2);
                    dataTuple[i] = new Tuple<DataStore, DataStore>(t1, t2);
                }
            }
            else
            {                
                t1 = DataStore.Load(benchmark.TrainFile, benchmark.FileFormat);
                t2 = DataStore.Load(benchmark.TestFile, benchmark.FileFormat, t1.DataStoreInfo);
                dataTuple[0] = new Tuple<DataStore, DataStore>(t1, t2);
            }            

            IParameter parmDataTuple = new ParameterObjectReferenceCollection<Tuple<DataStore, DataStore>>("Data", dataTuple);            

            IParameter parmReductType = new ParameterValueCollection<string>(
                ReductGeneratorParamHelper.FactoryKey, new string[] { 
                    ReductFactoryKeyHelper.ApproximateReductRelativeWeights, 
                    ReductFactoryKeyHelper.ApproximateReductMajorityWeights
                });
            
            IParameter parmEpsilon = new ParameterNumericRange<decimal>(ReductGeneratorParamHelper.Epsilon, 
                0.0m, 0.99m, 0.01m);
                //0.15m, 0.15m, 0.01m);

            IParameter parmNumberOfReducts = new ParameterValueCollection<int>(ReductGeneratorParamHelper.NumberOfReducts, 
                new int[] { 20, 10, 2, 1 });
                //new int[] { 2, 1 });
            
            IParameter parmIdentification = new ParameterValueCollection<RuleQualityFunction>(
                ReductGeneratorParamHelper.IdentificationType, new RuleQualityFunction[] {
                    RuleQuality.ConfidenceW,                     
                    RuleQuality.CoverageW
                });

            IParameter parmVote = new ParameterValueCollection<RuleQualityFunction>(
                ReductGeneratorParamHelper.VoteType, new RuleQualityFunction[] {
                    RuleQuality.ConfidenceW,
                    RuleQuality.CoverageW,
                    RuleQuality.RatioW,
                    RuleQuality.SupportW,
                    RuleQuality.StrengthW,
                    RuleQuality.SingleVote,
                    RuleQuality.ConfidenceRelativeW
                });

            ParameterCollection parameterList = new ParameterCollection(
                new IParameter[] { 
                    parmDataTuple,
                    parmReductType,
                    parmEpsilon,
                    parmNumberOfReducts,
                    parmIdentification,
                    parmVote
                });

            return parameterList;
        }
    }
}
