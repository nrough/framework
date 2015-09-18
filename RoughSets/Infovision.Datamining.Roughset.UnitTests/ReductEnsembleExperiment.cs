using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Datamining.Experimenter.Parms;
using Infovision.Math;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ReductEnsembleExperiment
    {
        public ReductEnsembleExperiment()
        {
            Random randSeed = new Random();
            int seed = Guid.NewGuid().GetHashCode();
            Console.WriteLine("class ReductEnsembleExperiment Seed: {0}", seed);
            RandomSingleton.Seed = seed;
        }
        
        [Test, TestCaseSource("GetDataFiles")]
        public void RunExperimentIncremental(KeyValuePair<string, BenchmarkData> kvp)
        {
            Console.WriteLine("Data: {0}", kvp.Key);

            int numberOfPermutations = 500;
            DataStore data = DataStore.Load(kvp.Value.TrainFile, FileFormat.Rses1);
            data.Name = kvp.Key;

            int minEpsilon = 0;
            int maxEpsilon = 0;
            int reductSize = data.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) / 2;            

            PermutationGenerator permGenerator = new PermutationGenerator(data);

            for (int testNo = 1; testNo <= 1; testNo++)
            {
                PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

                decimal[] epsilons = new decimal[numberOfPermutations];
                for (int i = 0; i < numberOfPermutations; i++)
                    epsilons[i] = (decimal)(RandomSingleton.Random.Next(minEpsilon, maxEpsilon) / 100.0);

                Args args = new Args();
                args.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleStream);
                args.AddParameter(ReductGeneratorParamHelper.DataStore, data);
                args.AddParameter(ReductGeneratorParamHelper.PermutationEpsilon, epsilons);
                args.AddParameter(ReductGeneratorParamHelper.Distance, (Func<double[], double[], double>)Similarity.Hamming);
                args.AddParameter(ReductGeneratorParamHelper.Linkage, (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Mean);
                args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
                args.AddParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorRandom(data));
                //args.AddParameter(ReductGeneratorParamHelper.WeightGenerator, new WeightGeneratorMajority(data));
                args.AddParameter(ReductGeneratorParamHelper.ReconWeights, (Func<IReduct, decimal[], double[]>)ReductEnsembleReconWeightsHelper.GetCorrectBinary);
                args.AddParameter(ReductGeneratorParamHelper.ReductSize, reductSize);
                args.AddParameter(ReductGeneratorParamHelper.MinimumNumberOfInstances, 10);

                ReductEnsembleStreamGenerator reductGenerator = ReductFactory.GetReductGenerator(args) as ReductEnsembleStreamGenerator;
                reductGenerator.Generate();

            } //test No
        } 

        public Dictionary<string, BenchmarkData> GetDataFiles()
        {
            return BenchmarkDataHelper.GetDataFiles();
        }

        [Test]
        public void RunExperiment()
        {
            int numberOfPermutations = 10;
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore testData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);
            string resultFileName = @"F:\dna_modified_103.csv";
            int minEpsilon = 0;
            int maxEpsilon = 25;

            WeightGenerator weightGenerator = new WeightGeneratorMajority(data);            

            IdentificationType identificationType = IdentificationType.WeightConfidence;
            VoteType voteType = VoteType.WeightConfidence;

            PermutationGenerator permGenerator = new PermutationGenerator(data);

            List<ReductEnsembleExperimentResult> experimentResults = new List<ReductEnsembleExperimentResult>();
            experimentResults.SaveToFile(resultFileName, false, true);

            //for (int testNo = 1; testNo <= 100; testNo++)
            for (int testNo = 1; testNo <= 1; testNo++)
            {
                Console.WriteLine("Test {0}", testNo);

                PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

                decimal[] epsilons = new decimal[numberOfPermutations];
                for (int i = 0; i < numberOfPermutations; i++)
                    epsilons[i] = (decimal)(RandomSingleton.Random.Next(minEpsilon, maxEpsilon) / 100.0);

                Args args = new Args();
                args.AddParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsemble);
                args.AddParameter(ReductGeneratorParamHelper.DataStore, data);
                args.AddParameter(ReductGeneratorParamHelper.PermutationEpsilon, epsilons);
                args.AddParameter(ReductGeneratorParamHelper.Distance, (Func<double[], double[], double>)Similarity.Manhattan);
                args.AddParameter(ReductGeneratorParamHelper.Linkage, (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Mean);
                args.AddParameter(ReductGeneratorParamHelper.PermutationCollection, permList);
                args.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                args.AddParameter(ReductGeneratorParamHelper.ReconWeights, (Func<IReduct, decimal[], double[]>)ReductEnsembleReconWeightsHelper.GetCorrectBinary);

                IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                reductGenerator.Generate();

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
                        ReductStore reductEnsemble = new ReductStore();
                        for (int i = 0; i < ensemble.Length; i++)
                        {
                            reductEnsemble.DoAddReduct((IReduct)ensemble[i]);
                        }

                        RoughClassifier rc = new RoughClassifier();
                        rc.ReductStore = reductEnsemble;
                        rc.Classify(testData);
                        ClassificationResult classificationResult = rc.Vote(testData, identificationType, voteType, weightGenerator.Weights);

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
                            Distance = (Func<double[], double[], decimal>)args[ReductGeneratorParamHelper.Distance],
                            Linkage = (Func<int[], int[], DistanceMatrix, double[][], decimal>)args[ReductGeneratorParamHelper.Linkage],
                            Dataset = data,
                            WeightGenerator = (WeightGenerator)args[ReductGeneratorParamHelper.WeightGenerator],
                            DiscernibiltyVector = (Func<IReduct, double[], double[]>)args[ReductGeneratorParamHelper.ReconWeights],
                            PermutationCollection = (PermutationCollection)args[ReductGeneratorParamHelper.PermutationCollection],

                            Accuracy = classificationResult.Accuracy,
                            BalancedAccuracy = classificationResult.BalancedAccuracy,
                            Coverage = classificationResult.Coverage,
                            Confidence = classificationResult.Confidence,
                            NumberOfClassified = classificationResult.Classified,
                            NumberOfMisclassified = classificationResult.Misclassified,
                            NumberOfUnclassifed = classificationResult.Unclassified,

                            IdentificationType = identificationType,
                            VoteType = voteType
                        });

                        ReductStore randomReductGroup = new ReductStore();
                        int[] randomReductIndices = RandomExt.RandomVectorNoRepetition(ensemble.Length, 0, reductGenerator.ReductPool.Count() - 1);
                        for (int i = 0; i < ensemble.Length; i++)
                        {
                            randomReductGroup.DoAddReduct(reductGenerator.ReductPool.GetReduct(randomReductIndices[i]));
                        }

                        RoughClassifier rc2 = new RoughClassifier();
                        rc2.ReductStore = randomReductGroup;
                        rc2.Classify(testData);
                        ClassificationResult classificationResult2 = rc2.Vote(testData, identificationType, voteType, weightGenerator.Weights);

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
                            Distance = (Func<double[], double[], decimal>)args[ReductGeneratorParamHelper.Distance],
                            Linkage = (Func<int[], int[], DistanceMatrix, double[][], decimal>)args[ReductGeneratorParamHelper.Linkage],
                            Dataset = data,
                            WeightGenerator = (WeightGenerator)args[ReductGeneratorParamHelper.WeightGenerator],
                            DiscernibiltyVector = (Func<IReduct, double[], double[]>)args[ReductGeneratorParamHelper.ReconWeights],
                            PermutationCollection = (PermutationCollection)args[ReductGeneratorParamHelper.PermutationCollection],

                            Accuracy = classificationResult2.Accuracy,
                            BalancedAccuracy = classificationResult2.BalancedAccuracy,
                            Coverage = classificationResult2.Coverage,
                            Confidence = classificationResult2.Confidence,
                            NumberOfClassified = classificationResult2.Classified,
                            NumberOfMisclassified = classificationResult2.Misclassified,
                            NumberOfUnclassifed = classificationResult2.Unclassified,

                            IdentificationType = identificationType,
                            VoteType = voteType
                        });

                        ensembleId++;
                    } //for each ensemble
                } //clusterNo

                experimentResults.SaveToFile(resultFileName, true, false);
                experimentResults = new List<ReductEnsembleExperimentResult>();
            } //test No            
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
        public Func<double[], double[], decimal> Distance { get; set; }
        public Func<int[], int[], DistanceMatrix, double[][], decimal> Linkage { get; set; }
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

        public IdentificationType IdentificationType { get; set; }
        public VoteType VoteType { get; set; }
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
                    .Append(ReductGeneratorParamHelper.NumberOfClusters).Append(separator)
                    .Append("ClusterId").Append(separator)
                    .Append("TestType").Append(separator)
                    .Append("MinEpsilon").Append(separator)
                    .Append("MaxEpsilon").Append(separator)
                    .Append("NumberOfPermuations").Append(separator)
                    .Append(ReductGeneratorParamHelper.NumberOfReducts).Append(separator)
                    .Append(ReductGeneratorParamHelper.Distance).Append(separator)
                    .Append(ReductGeneratorParamHelper.Linkage).Append(separator)
                    .Append("Dataset").Append(separator)
                    .Append(ReductGeneratorParamHelper.WeightGenerator).Append(separator)
                    .Append("DiscernibiltyVector").Append(separator)
                    .Append("Accuracy").Append(separator)
                    .Append("BalancedAccuracy").Append(separator)
                    .Append("Coverage").Append(separator)
                    .Append("Confidence").Append(separator)
                    .Append("Classified").Append(separator)
                    .Append("Misclassified").Append(separator)
                    .Append("Unclassified").Append(separator)
                    .Append(ReductGeneratorParamHelper.IdentificationType).Append(separator)
                    .Append(ReductGeneratorParamHelper.VoteType).Append(separator);

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
