using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NRough.Data;
using NRough.MachineLearning.Clustering.Hierarchical;
using NRough.MachineLearning.Experimenter.Parms;
using NRough.Math;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Roughsets;
using NRough.Core.Random;
using NRough.Benchmark;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    public class ReductEnsembleExperiment
    {
        public ReductEnsembleExperiment()
        {                                    
            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();
        }

        public IEnumerable<KeyValuePair<string, BenchmarkData>> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles("Data");
        }

        [Test, Ignore("For verification!")] 
        public void RunExperiment()
        {
            int numberOfPermutations = 10;

            var data = Data.Benchmark.Factory.DnaModified();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);
            DataStore testData = DataStore.Load(@"Data\dna_modified.tst", DataFormat.RSES1, data.DataStoreInfo);
            string resultFileName = @"dna_modified_103.csv";
            int minEpsilon = 0;
            int maxEpsilon = 25;

            WeightGenerator weightGenerator = new WeightGeneratorMajority(data);

            RuleQualityMethod identificationType = RuleQualityMethods.ConfidenceW;
            RuleQualityMethod voteType = RuleQualityMethods.ConfidenceW;

            PermutationGenerator permGenerator = new PermutationGenerator(data);

            List<ReductEnsembleExperimentResult> experimentResults = new List<ReductEnsembleExperimentResult>();
            experimentResults.SaveToFile(resultFileName, false, true);

            //for (int testNo = 1; testNo <= 100; testNo++)
            for (int testNo = 1; testNo <= 1; testNo++)
            {
                //Console.WriteLine("Test {0}", testNo);

                PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

                double[] epsilons = new double[numberOfPermutations];
                for (int i = 0; i < numberOfPermutations; i++)
                    epsilons[i] = (double)(RandomSingleton.Random.Next(minEpsilon, maxEpsilon) / 100.0);

                Args args = new Args();
                args.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsemble);
                args.SetParameter(ReductFactoryOptions.DecisionTable, data);
                args.SetParameter(ReductFactoryOptions.PermutationEpsilon, epsilons);
                args.SetParameter(ReductFactoryOptions.Distance, (Func<double[], double[], double>)Distance.Manhattan);
                args.SetParameter(ReductFactoryOptions.Linkage, (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Mean);
                args.SetParameter(ReductFactoryOptions.PermutationCollection, permList);
                args.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
                args.SetParameter(ReductFactoryOptions.ReconWeights, (Func<IReduct, double[], RuleQualityMethod, double[]>)ReductToVectorConversionMethods.GetCorrectBinary);

                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Run();

                int poolSize = reductGenerator.ReductPool.Count();
                for (int clusterNo = 1; clusterNo <= poolSize; clusterNo += 2)
                {
                    IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection(clusterNo);

                    ParameterCollection clusterCollection = new ParameterCollection(clusterNo, 0);
                    int counter = 0;
                    foreach (IReductStore reductStore in reductStoreCollection)
                    {
                        ParameterObjectReferenceCollection<IReduct> valueCollection
                                = new ParameterObjectReferenceCollection<IReduct>(String.Format("{0}", counter++), reductStore.ToArray<IReduct>());
                        clusterCollection.Add(valueCollection);
                    }

                    int ensembleId = 0;
                    foreach (object[] ensemble in clusterCollection.Values())
                    {
                        ReductStore reductEnsemble = new ReductStore(ensemble.Length);
                        for (int i = 0; i < ensemble.Length; i++)
                        {
                            reductEnsemble.DoAddReduct((IReduct)ensemble[i]);
                        }

                        RoughClassifier rc = new RoughClassifier(
                            reductStoreCollection,
                            identificationType,
                            voteType,
                            data.DataStoreInfo.GetDecisionValues());
                        ClassificationResult classificationResult = rc.Classify(testData, weightGenerator.Weights);

                        experimentResults.Add(new ReductEnsembleExperimentResult
                        {
                            Id = String.Format("{0}", testNo),
                            NumberOfClusters = clusterNo,
                            ClusterId = ensembleId,
                            TestType = "Ensemble",
                            MinEpsilon = minEpsilon,
                            MaxEpsilon = maxEpsilon,
                            NumberOfPermuations = numberOfPermutations,
                            NumberOfReducts = reductEnsemble.Count,
                            Distance = (Func<double[], double[], double>)args[ReductFactoryOptions.Distance],
                            Linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args[ReductFactoryOptions.Linkage],
                            Dataset = data,
                            WeightGenerator = (WeightGenerator)args[ReductFactoryOptions.WeightGenerator],
                            DiscernibiltyVector = (Func<IReduct, double[], double[]>)args[ReductFactoryOptions.ReconWeights],
                            PermutationCollection = (PermutationCollection)args[ReductFactoryOptions.PermutationCollection],

                            Accuracy = classificationResult.Accuracy,
                            BalancedAccuracy = classificationResult.BalancedAccuracy,
                            Coverage = classificationResult.Coverage,
                            Confidence = classificationResult.Confidence,
                            NumberOfClassified = classificationResult.Classified,
                            NumberOfMisclassified = classificationResult.Misclassified,
                            NumberOfUnclassifed = classificationResult.Unclassified,

                            IdentificationType = identificationType.Method.Name,
                            VoteType = voteType.Method.Name
                        });

                        ReductStore randomReductGroup = new ReductStore(ensemble.Length);
                        int[] randomReductIndices = RandomExtensions.RandomVectorNoRepetition(ensemble.Length, 0, reductGenerator.ReductPool.Count() - 1);
                        for (int i = 0; i < ensemble.Length; i++)
                        {
                            randomReductGroup.DoAddReduct(reductGenerator.ReductPool.GetReduct(randomReductIndices[i]));
                        }

                        IReductStoreCollection localReductStoreCollection = new ReductStoreCollection(1);
                        localReductStoreCollection.AddStore(randomReductGroup);

                        RoughClassifier rc2 = new RoughClassifier(
                            localReductStoreCollection,
                            identificationType,
                            voteType,
                            data.DataStoreInfo.GetDecisionValues());
                        ClassificationResult classificationResult2 = rc2.Classify(testData, null);

                        experimentResults.Add(new ReductEnsembleExperimentResult
                        {
                            Id = String.Format("{0}", testNo),
                            NumberOfClusters = clusterNo,
                            ClusterId = ensembleId,
                            TestType = "Random group",
                            MinEpsilon = minEpsilon,
                            MaxEpsilon = maxEpsilon,
                            NumberOfPermuations = numberOfPermutations,
                            NumberOfReducts = randomReductGroup.Count,
                            Distance = (Func<double[], double[], double>)args[ReductFactoryOptions.Distance],
                            Linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args[ReductFactoryOptions.Linkage],
                            Dataset = data,
                            WeightGenerator = (WeightGenerator)args[ReductFactoryOptions.WeightGenerator],
                            DiscernibiltyVector = (Func<IReduct, double[], double[]>)args[ReductFactoryOptions.ReconWeights],
                            PermutationCollection = (PermutationCollection)args[ReductFactoryOptions.PermutationCollection],

                            Accuracy = classificationResult2.Accuracy,
                            BalancedAccuracy = classificationResult2.BalancedAccuracy,
                            Coverage = classificationResult2.Coverage,
                            Confidence = classificationResult2.Confidence,
                            NumberOfClassified = classificationResult2.Classified,
                            NumberOfMisclassified = classificationResult2.Misclassified,
                            NumberOfUnclassifed = classificationResult2.Unclassified,

                            IdentificationType = identificationType.Method.Name,
                            VoteType = voteType.Method.Name
                        });

                        ensembleId++;
                    } //for each ensemble
                } //clusterNo

                experimentResults.SaveToFile(resultFileName, true, false);
                experimentResults = new List<ReductEnsembleExperimentResult>();
            } //testData No
        }
    }

    public class ReductEnsembleExperimentResult
    {
        public string Id { get; set; }
        public int NumberOfClusters { get; set; }
        public int MinEpsilon { get; set; }
        public int MaxEpsilon { get; set; }
        public int NumberOfPermuations { get; set; }
        public int NumberOfReducts { get; set; }
        public Func<double[], double[], double> Distance { get; set; }
        public Func<int[], int[], DistanceMatrix, double[][], double> Linkage { get; set; }
        public DataStore Dataset { get; set; }
        public WeightGenerator WeightGenerator { get; set; }
        public Func<IReduct, double[], double[]> DiscernibiltyVector { get; set; }
        public PermutationCollection PermutationCollection { get; set; }

        public double Accuracy { get; set; }
        public double BalancedAccuracy { get; set; }
        public double Coverage { get; set; }
        public double Confidence { get; set; }
        public int NumberOfClassified { get; set; }
        public int NumberOfMisclassified { get; set; }
        public int NumberOfUnclassifed { get; set; }

        public int ClusterId { get; set; }
        public string TestType { get; set; }

        public string IdentificationType { get; set; }
        public string VoteType { get; set; }
    }

    public static class ReductEnsembleExperimentExtensions
    {
        public static void SaveToFile(this List<ReductEnsembleExperimentResult> results, string fileName, bool append, bool addHeader)
        {
            string separator = ";";
            StringBuilder sb = new StringBuilder();

            //header
            if (addHeader)
            {
                sb.Append("Id").Append(separator)
                    .Append(ReductFactoryOptions.NumberOfClusters).Append(separator)
                    .Append("ClusterId").Append(separator)
                    .Append("TestType").Append(separator)
                    .Append("MinEpsilon").Append(separator)
                    .Append("MaxEpsilon").Append(separator)
                    .Append("NumberOfPermuations").Append(separator)
                    .Append(ReductFactoryOptions.NumberOfReducts).Append(separator)
                    .Append(ReductFactoryOptions.Distance).Append(separator)
                    .Append(ReductFactoryOptions.Linkage).Append(separator)
                    .Append("Dataset").Append(separator)
                    .Append(ReductFactoryOptions.WeightGenerator).Append(separator)
                    .Append("DiscernibiltyVector").Append(separator)
                    .Append("Accuracy").Append(separator)
                    .Append("BalancedAccuracy").Append(separator)
                    .Append("Coverage").Append(separator)
                    .Append("Confidence").Append(separator)
                    .Append("Classified").Append(separator)
                    .Append("Misclassified").Append(separator)
                    .Append("Unclassified").Append(separator)
                    .Append(ReductFactoryOptions.IdentificationType).Append(separator)
                    .Append(ReductFactoryOptions.VoteType).Append(separator);

                sb.Append(Environment.NewLine);
            }

            foreach (ReductEnsembleExperimentResult result in results)
            {
                sb.Append(result.Id).Append(separator)
                    .Append(result.NumberOfClusters).Append(separator)
                    .Append(result.ClusterId).Append(separator)
                    .Append(result.TestType).Append(separator)
                    .Append(result.MinEpsilon).Append(separator)
                    .Append(result.MaxEpsilon).Append(separator)
                    .Append(result.NumberOfPermuations).Append(separator)
                    .Append(result.NumberOfReducts).Append(separator)
                    .Append(String.Format("{0}", result.Distance.Method.Name)).Append(separator)
                    .Append(String.Format("{0}", result.Linkage.Method.Name)).Append(separator)
                    .Append(result.Dataset.Name).Append(separator)
                    .Append(result.WeightGenerator.GetType().Name).Append(separator)
                    .Append(String.Format("{0}", result.DiscernibiltyVector.Method.Name)).Append(separator)
                    .Append(result.Accuracy).Append(separator)
                    .Append(result.BalancedAccuracy).Append(separator)
                    .Append(result.Coverage).Append(separator)
                    .Append(result.Confidence).Append(separator)
                    .Append(result.NumberOfClassified).Append(separator)
                    .Append(result.NumberOfMisclassified).Append(separator)
                    .Append(result.NumberOfUnclassifed).Append(separator)
                    .Append(result.IdentificationType).Append(separator)
                    .Append(result.VoteType).Append(separator);

                sb.Append(Environment.NewLine);
            }

            if (append)
                File.AppendAllText(fileName, sb.ToString());
            else
                File.WriteAllText(fileName, sb.ToString());
        }
    }
}