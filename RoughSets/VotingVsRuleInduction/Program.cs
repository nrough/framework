using System;
using System.Collections.Generic;
using System.IO;
using Common.Logging;
using Common.Logging.Configuration;
using Raccoon.Data;
using Raccoon.MachineLearning;
using Raccoon.MachineLearning.Benchmark;
using Raccoon.MachineLearning.Experimenter.Parms;
using Raccoon.MachineLearning.Roughset;
using Raccoon.Math;
using Raccoon.Core;
using Raccoon.MachineLearning.Permutations;
using Raccoon.MachineLearning.Classification;

namespace VotingVsRuleInduction
{
    internal class Program
    {
        private static ILog log;
        private Dictionary<int, PermutationCollection> permutationCollections = new Dictionary<int, PermutationCollection>();

        private static void Main(string[] args)
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
                    Console.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                        "Test",
                        "Fold",
                        ReductGeneratorParamHelper.FactoryKey,
                        ReductGeneratorParamHelper.NumberOfReducts,
                        ReductGeneratorParamHelper.Epsilon,
                        ReductGeneratorParamHelper.IdentificationType,
                        ReductGeneratorParamHelper.VoteType,
                        ClassificationResult.TableHeader()
                        );

                    outputFile.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                        "Test",
                        "Fold",
                        ReductGeneratorParamHelper.FactoryKey,
                        ReductGeneratorParamHelper.NumberOfReducts,
                        ReductGeneratorParamHelper.Epsilon,
                        ReductGeneratorParamHelper.IdentificationType,
                        ReductGeneratorParamHelper.VoteType,
                        ClassificationResult.TableHeader()
                        );

                    for (int t = 0; t < numberOfTests; t++)
                    {
                        ParameterCollection parms = program.Parameters(kvp.Value);
                        program.permutationCollections = new Dictionary<int, PermutationCollection>();

                        string lastTrainName = null;
                        string lastFactoryKey = null;
                        double lastEpsilon = -1.0;;
                        int lastNumberOfReducts = -1;

                        IReductGenerator reductGenerator = null;
                        IReductStoreCollection origReductStoreCollection = null;
                        IReductStoreCollection filteredReductStoreCollection = null;
                        bool emptyReductResult = false;

                        foreach (var parmVector in parms.Values())
                        {
                            bool regeneratedReducts = false;
                            var setup = program.ConvertParameterVector(parmVector);
                            int f = (int)setup.GetParameter(ReductGeneratorParamHelper.CVActiveFold);

                            if (lastTrainName != ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TrainData)).Name
                                || lastFactoryKey != (string)setup.GetParameter(ReductGeneratorParamHelper.FactoryKey))
                            {
                                emptyReductResult = false;
                            }

                            if (lastTrainName != ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TrainData)).Name
                                || lastFactoryKey != (string)setup.GetParameter(ReductGeneratorParamHelper.FactoryKey)
                                || lastEpsilon != (double)setup.GetParameter(ReductGeneratorParamHelper.Epsilon))
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
                                || lastEpsilon != (double)setup.GetParameter(ReductGeneratorParamHelper.Epsilon)
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
                            result.AvgNumberOfAttributes = filteredReductStoreCollection.GetWeightedAvgMeasure(reductMeasureLength, true);

                            result.ModelCreationTime = reductGenerator.ReductGenerationTime;

                            if (DoubleEpsilonComparer.Instance.Equals(result.AvgNumberOfAttributes, 0.0d))
                            {
                                emptyReductResult = true;
                            }

                            Console.WriteLine("{0,2}|{1}|{2}|{3,2}|{4,4}|{5,11}|{6,19}|{7}",
                                t,
                                f,
                                setup.GetParameter(ReductGeneratorParamHelper.FactoryKey),
                                setup.GetParameter(ReductGeneratorParamHelper.NumberOfReducts),
                                setup.GetParameter(ReductGeneratorParamHelper.Epsilon),
                                ((RuleQualityFunction)setup.GetParameter(ReductGeneratorParamHelper.IdentificationType)).Method.Name,
                                ((RuleQualityFunction)setup.GetParameter(ReductGeneratorParamHelper.VoteType)).Method.Name,
                                result
                                );

                            outputFile.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                                t,
                                f,
                                setup.GetParameter(ReductGeneratorParamHelper.FactoryKey),
                                setup.GetParameter(ReductGeneratorParamHelper.NumberOfReducts),
                                setup.GetParameter(ReductGeneratorParamHelper.Epsilon),
                                ((RuleQualityFunction)setup.GetParameter(ReductGeneratorParamHelper.IdentificationType)).Method.Name,
                                ((RuleQualityFunction)setup.GetParameter(ReductGeneratorParamHelper.VoteType)).Method.Name,
                                result
                                );

                            lastTrainName = ((DataStore)setup.GetParameter(ReductGeneratorParamHelper.TrainData)).Name;
                            lastFactoryKey = (string)setup.GetParameter(ReductGeneratorParamHelper.FactoryKey);
                            lastEpsilon = (double)setup.GetParameter(ReductGeneratorParamHelper.Epsilon);
                            lastNumberOfReducts = (int)setup.GetParameter(ReductGeneratorParamHelper.NumberOfReducts);
                        }
                    }
                }
            }

            Console.ReadKey();
        }

        public Args ConvertParameterVector(object[] parameterVector)
        {
            Tuple<DataStore, DataStore, int> data = (Tuple<DataStore, DataStore, int>)parameterVector[0];
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
            conf.SetParameter(ReductGeneratorParamHelper.TrainData, data.Item1);
            conf.SetParameter(ReductGeneratorParamHelper.FactoryKey, parameterVector[1]);
            conf.SetParameter(ReductGeneratorParamHelper.PermutationCollection, permuationCollection);
            conf.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, parameterVector[3]);
            conf.SetParameter(ReductGeneratorParamHelper.Epsilon, parameterVector[2]);

            conf.SetParameter(ReductGeneratorParamHelper.TestData, data.Item2);
            conf.SetParameter(ReductGeneratorParamHelper.IdentificationType, parameterVector[4]);
            conf.SetParameter(ReductGeneratorParamHelper.VoteType, parameterVector[5]);
            conf.SetParameter(ReductGeneratorParamHelper.CVActiveFold, data.Item3);

            return conf;
        }

        public ParameterCollection Parameters(BenchmarkData benchmark)
        {
            DataStore t1 = null, t2 = null;
            Tuple<DataStore, DataStore, int>[] dataTuple = new Tuple<DataStore, DataStore, int>[benchmark.CrossValidationFolds];

            if (benchmark.CrossValidationActive)
            {
                DataStore data = DataStore.Load(benchmark.DataFile, benchmark.FileFormat);

                if (benchmark.DecisionFieldId > 0)
                    data.SetDecisionFieldId(benchmark.DecisionFieldId);

                DataStoreSplitter splitter = new DataStoreSplitter(data, benchmark.CrossValidationFolds);

                for (int i = 0; i < benchmark.CrossValidationFolds; i++)
                {                    
                    splitter.Split(ref t1, ref t2, i);
                    dataTuple[i] = new Tuple<DataStore, DataStore, int>(t1, t2, i);
                }
            }
            else
            {
                t1 = DataStore.Load(benchmark.TrainFile, benchmark.FileFormat);

                if (benchmark.DecisionFieldId > 0)
                    t1.SetDecisionFieldId(benchmark.DecisionFieldId);

                t2 = DataStore.Load(benchmark.TestFile, benchmark.FileFormat, t1.DataStoreInfo);
                dataTuple[0] = new Tuple<DataStore, DataStore, int>(t1, t2, 0);
            }

            IParameter parmDataTuple = new ParameterObjectReferenceCollection<Tuple<DataStore, DataStore, int>>("Data", dataTuple);

            IParameter parmReductType = new ParameterValueCollection<string>(
                ReductGeneratorParamHelper.FactoryKey, new string[] {
                    ReductFactoryKeyHelper.ApproximateReductRelativeWeights,
                    ReductFactoryKeyHelper.ApproximateReductMajorityWeights
                });

            IParameter parmEpsilon = new ParameterNumericRange<double>(ReductGeneratorParamHelper.Epsilon,
                0.0, 0.99, 0.01);

            IParameter parmNumberOfReducts = new ParameterValueCollection<int>(ReductGeneratorParamHelper.NumberOfReducts,
                new int[] { 20, 10, 2, 1 });

            IParameter parmIdentification = new ParameterValueCollection<RuleQualityFunction>(
                ReductGeneratorParamHelper.IdentificationType, new RuleQualityFunction[] {
                    RuleQuality.ConfidenceW,
                    RuleQuality.CoverageW,
                    RuleQuality.Confidence,
                    RuleQuality.Coverage,
                });

            IParameter parmVote = new ParameterValueCollection<RuleQualityFunction>(
                ReductGeneratorParamHelper.VoteType, new RuleQualityFunction[] {
                    RuleQuality.ConfidenceW,
                    RuleQuality.CoverageW,
                    RuleQuality.RatioW,
                    RuleQuality.SupportW,
                    RuleQuality.SingleVote,
                    RuleQuality.ConfidenceRelativeW,
                    RuleQuality.Confidence,
                    RuleQuality.Coverage,
                    RuleQuality.Ratio,
                    RuleQuality.Support,
                    RuleQuality.ConfidenceRelative
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