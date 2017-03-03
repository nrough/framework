using System;
using System.Collections.Generic;
using System.IO;
using Common.Logging;
using Common.Logging.Configuration;
using NRough.Data;
using NRough.MachineLearning.Experimenter.Parms;
using NRough.MachineLearning.Roughsets;
using NRough.Math;
using NRough.Core;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Classification;
using NRough.Core.Random;
using NRough.Core.Comparers;
using NRough.Benchmark;
using NRough.MachineLearning.Roughsets.Reducts.Comparers;

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
                        ReductFactoryOptions.ReductType,
                        ReductFactoryOptions.NumberOfReducts,
                        ReductFactoryOptions.Epsilon,
                        ReductFactoryOptions.IdentificationType,
                        ReductFactoryOptions.VoteType,
                        ClassificationResult.TableHeader()
                        );

                    outputFile.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                        "Test",
                        "Fold",
                        ReductFactoryOptions.ReductType,
                        ReductFactoryOptions.NumberOfReducts,
                        ReductFactoryOptions.Epsilon,
                        ReductFactoryOptions.IdentificationType,
                        ReductFactoryOptions.VoteType,
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
                            int f = (int)setup.GetParameter(ReductFactoryOptions.CVActiveFold);

                            if (lastTrainName != ((DataStore)setup.GetParameter(ReductFactoryOptions.DecisionTable)).Name
                                || lastFactoryKey != (string)setup.GetParameter(ReductFactoryOptions.ReductType))
                            {
                                emptyReductResult = false;
                            }

                            if (lastTrainName != ((DataStore)setup.GetParameter(ReductFactoryOptions.DecisionTable)).Name
                                || lastFactoryKey != (string)setup.GetParameter(ReductFactoryOptions.ReductType)
                                || lastEpsilon != (double)setup.GetParameter(ReductFactoryOptions.Epsilon))
                            {
                                if (emptyReductResult == false)
                                {
                                    reductGenerator = ReductFactory.GetReductGenerator(setup);
                                    reductGenerator.Run();
                                    origReductStoreCollection = reductGenerator.GetReductStoreCollection();

                                    regeneratedReducts = true;
                                }
                            }

                            if (lastTrainName != ((DataStore)setup.GetParameter(ReductFactoryOptions.DecisionTable)).Name
                                || lastFactoryKey != (string)setup.GetParameter(ReductFactoryOptions.ReductType)
                                || lastEpsilon != (double)setup.GetParameter(ReductFactoryOptions.Epsilon)
                                || lastNumberOfReducts != (int)setup.GetParameter(ReductFactoryOptions.NumberOfReducts)
                                || regeneratedReducts)
                            {
                                filteredReductStoreCollection = origReductStoreCollection.Filter(
                                        (int)setup.GetParameter(ReductFactoryOptions.NumberOfReducts),
                                        reductLengthComparer);
                            }

                            RoughClassifier classifier = new RoughClassifier(
                                filteredReductStoreCollection,
                                (RuleQualityMethod)setup.GetParameter(ReductFactoryOptions.IdentificationType),
                                (RuleQualityMethod)setup.GetParameter(ReductFactoryOptions.VoteType),
                                ((DataStore)setup.GetParameter(ReductFactoryOptions.DecisionTable)).DataStoreInfo.GetDecisionValues());

                            ClassificationResult result = classifier.Classify((DataStore)setup.GetParameter(ReductFactoryOptions.TestData));
                            result.AvgNumberOfAttributes = filteredReductStoreCollection.GetWeightedAvgMeasure(reductMeasureLength, true);

                            result.ModelCreationTime = reductGenerator.ReductGenerationTime;

                            if (DoubleEpsilonComparer.Instance.Equals(result.AvgNumberOfAttributes, 0.0d))
                            {
                                emptyReductResult = true;
                            }

                            Console.WriteLine("{0,2}|{1}|{2}|{3,2}|{4,4}|{5,11}|{6,19}|{7}",
                                t,
                                f,
                                setup.GetParameter(ReductFactoryOptions.ReductType),
                                setup.GetParameter(ReductFactoryOptions.NumberOfReducts),
                                setup.GetParameter(ReductFactoryOptions.Epsilon),
                                ((RuleQualityMethod)setup.GetParameter(ReductFactoryOptions.IdentificationType)).Method.Name,
                                ((RuleQualityMethod)setup.GetParameter(ReductFactoryOptions.VoteType)).Method.Name,
                                result
                                );

                            outputFile.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                                t,
                                f,
                                setup.GetParameter(ReductFactoryOptions.ReductType),
                                setup.GetParameter(ReductFactoryOptions.NumberOfReducts),
                                setup.GetParameter(ReductFactoryOptions.Epsilon),
                                ((RuleQualityMethod)setup.GetParameter(ReductFactoryOptions.IdentificationType)).Method.Name,
                                ((RuleQualityMethod)setup.GetParameter(ReductFactoryOptions.VoteType)).Method.Name,
                                result
                                );

                            lastTrainName = ((DataStore)setup.GetParameter(ReductFactoryOptions.DecisionTable)).Name;
                            lastFactoryKey = (string)setup.GetParameter(ReductFactoryOptions.ReductType);
                            lastEpsilon = (double)setup.GetParameter(ReductFactoryOptions.Epsilon);
                            lastNumberOfReducts = (int)setup.GetParameter(ReductFactoryOptions.NumberOfReducts);
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
            conf.SetParameter(ReductFactoryOptions.DecisionTable, data.Item1);
            conf.SetParameter(ReductFactoryOptions.ReductType, parameterVector[1]);
            conf.SetParameter(ReductFactoryOptions.PermutationCollection, permuationCollection);
            conf.SetParameter(ReductFactoryOptions.NumberOfReducts, parameterVector[3]);
            conf.SetParameter(ReductFactoryOptions.Epsilon, parameterVector[2]);

            conf.SetParameter(ReductFactoryOptions.TestData, data.Item2);
            conf.SetParameter(ReductFactoryOptions.IdentificationType, parameterVector[4]);
            conf.SetParameter(ReductFactoryOptions.VoteType, parameterVector[5]);
            conf.SetParameter(ReductFactoryOptions.CVActiveFold, data.Item3);

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

                DataSplitter splitter = new DataSplitter(data, benchmark.CrossValidationFolds);

                for (int i = 0; i < benchmark.CrossValidationFolds; i++)
                {                    
                    splitter.Split(out t1, out t2, i);
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
                ReductFactoryOptions.ReductType, new string[] {
                    ReductTypes.ApproximateReductRelativeWeights,
                    ReductTypes.ApproximateReductMajorityWeights
                });

            IParameter parmEpsilon = new ParameterNumericRange<double>(ReductFactoryOptions.Epsilon,
                0.0, 0.99, 0.01);

            IParameter parmNumberOfReducts = new ParameterValueCollection<int>(ReductFactoryOptions.NumberOfReducts,
                new int[] { 20, 10, 2, 1 });

            IParameter parmIdentification = new ParameterValueCollection<RuleQualityMethod>(
                ReductFactoryOptions.IdentificationType, new RuleQualityMethod[] {
                    RuleQualityMethods.ConfidenceW,
                    RuleQualityMethods.CoverageW,
                    RuleQualityMethods.Confidence,
                    RuleQualityMethods.Coverage,
                });

            IParameter parmVote = new ParameterValueCollection<RuleQualityMethod>(
                ReductFactoryOptions.VoteType, new RuleQualityMethod[] {
                    RuleQualityMethods.ConfidenceW,
                    RuleQualityMethods.CoverageW,
                    RuleQualityMethods.RatioW,
                    RuleQualityMethods.SupportW,
                    RuleQualityMethods.SingleVote,
                    RuleQualityMethods.ConfidenceRelativeW,
                    RuleQualityMethods.Confidence,
                    RuleQualityMethods.Coverage,
                    RuleQualityMethods.Ratio,
                    RuleQualityMethods.Support,
                    RuleQualityMethods.ConfidenceRelative
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