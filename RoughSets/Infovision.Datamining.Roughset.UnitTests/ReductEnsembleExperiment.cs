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

        //public double GroupAccuracy { get; set; }
        //public double GroupBalancedAccuracy { get; set; }
        //public double GroupCoverage { get; set; }
        //public double GroupConfidence { get; set; }

        public int ClusterId { get; set; }
        public string TestType { get; set; }
    }

    public static class ReductEnsembleExperimentExtensions
    {
        public static void SaveToFile(this List<ReductEnsembleExperimentResult> results, string fileName)
        {
            string separator = ";";
            StringBuilder sb = new StringBuilder();

            //header            
            sb.Append("Id").Append(separator)
                .Append("NumberOfClusters").Append(separator)
                .Append("ClusterId").Append(separator)
                .Append("TestType").Append(separator)
                .Append("MinEpsilon").Append(separator)
                .Append("MaxEpsilon").Append(separator)
                .Append("NumberOfPermuations").Append(separator)
                .Append("NumberOfReducts").Append(separator)
                .Append("Distance").Append(separator)
                .Append("Linkage").Append(separator)
                .Append("Dataset").Append(separator)
                .Append("WeightGenerator").Append(separator)
                .Append("DiscernibiltyVector").Append(separator)
                //.Append("PermutationCollection").Append(separator)
                .Append("Accuracy").Append(separator)
                .Append("BalancedAccuracy").Append(separator)
                .Append("Coverage").Append(separator)
                .Append("Confidence").Append(separator)
                .Append("NumberOfClassified").Append(separator)
                .Append("NumberOfMisclassified").Append(separator)
                .Append("NumberOfUnclassifed").Append(separator)
                //.Append("GroupAccuracy").Append(separator)
                //.Append("GroupBalancedAccuracy").Append(separator)
                //.Append("GroupCoverage").Append(separator)
                //.Append("GroupConfidence").Append(separator)
                
                ;
            sb.Append(Environment.NewLine);

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
                    //.Append(String.Format("{0}.{1}", result.Distance.Method.DeclaringType.Name, result.Distance.Method.Name)).Append(separator)
                    .Append(String.Format("{0}", result.Distance.Method.Name)).Append(separator)
                    //.Append(String.Format("{0}.{1}", result.Linkage.Method.DeclaringType.Name, result.Linkage.Method.Name)).Append(separator)
                    .Append(String.Format("{0}", result.Linkage.Method.Name)).Append(separator)
                    .Append(result.Dataset.Name).Append(separator)
                    .Append(result.WeightGenerator.GetType().Name).Append(separator)
                    //.Append(String.Format("{0}.{1}", result.DiscernibiltyVector.Method.DeclaringType.Name, result.DiscernibiltyVector.Method.Name)).Append(separator)
                    .Append(String.Format("{0}", result.DiscernibiltyVector.Method.Name)).Append(separator)
                    //.Append(result.PermutationCollection).Append(separator)
                    .Append(result.Accuracy).Append(separator)
                    .Append(result.BalancedAccuracy).Append(separator)
                    .Append(result.Coverage).Append(separator)
                    .Append(result.Confidence).Append(separator)
                    .Append(result.NumberOfClassified).Append(separator)
                    .Append(result.NumberOfMisclassified).Append(separator)
                    .Append(result.NumberOfUnclassifed).Append(separator)
                    //.Append(result.GroupAccuracy).Append(separator)
                    //.Append(result.GroupBalancedAccuracy).Append(separator)
                    //.Append(result.GroupCoverage).Append(separator)
                    //.Append(result.GroupConfidence).Append(separator)
                    ;
                
                sb.Append(Environment.NewLine);
            }

            File.WriteAllText(fileName, sb.ToString());
        }
    }
    
    [TestFixture]
    public class ReductEnsembleExperiment
    {
        [Test]
        public void RunExperiment()
        {
            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);            
            RandomSingleton.Seed = seed;

            int numberOfPermutations = 20;
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore testData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);
            int minEpsilon = 0;
            int maxEpsilon = 25;

            WeightGenerator weightGenerator = new WeightGeneratorMajority(data);
            data.DataStoreInfo.RecordWeights = weightGenerator.Weights;

            List<ReductEnsembleExperimentResult> experimentResults = new List<ReductEnsembleExperimentResult>();
            
            for (int testNo = 1; testNo <= 100; testNo++)
            {
                Console.WriteLine("Test {0}", testNo);

                PermutationGenerator permGenerator = new PermutationGenerator(data);
                PermutationCollection permList = permGenerator.Generate(numberOfPermutations);
                
                int[] epsilons = new int[numberOfPermutations];
                for (int i = 0; i < numberOfPermutations; i++)
                    epsilons[i] = RandomSingleton.Random.Next(minEpsilon, maxEpsilon);

                for (int clusterNo = 1; clusterNo <= 7; clusterNo++)
                {                    
                    Args args = new Args();

                    args.AddParameter("_TestData", testData);
                    args.AddParameter("_Seed", seed);

                    args.AddParameter("DataStore", data);
                    args.AddParameter("PermutationEpsilon", epsilons);
                    args.AddParameter("Distance", (Func<double[], double[], double>)Similarity.Manhattan);
                    args.AddParameter("Linkage", (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Complete);
                    args.AddParameter("NumberOfClusters", clusterNo);
                    args.AddParameter("FactoryKey", "ReductEnsemble");
                    args.AddParameter("PermutationCollection", permList);
                    args.AddParameter("WeightGenerator", weightGenerator);
                    args.AddParameter("ReconWeights", (Func<IReduct, double[], double[]>)ReductEnsembleReconWeightsHelper.GetCorrectReconWeights);

                    //TODO Generate should be called once
                    //TODO Add additional method for getting hierarchical cluster results for different number of clusters
                    IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
                    reductGenerator.Generate();
                    IReductStoreCollection reductStoreCollection = reductGenerator.ReductStoreCollection;                                        

                    ParameterCollection clusterCollection = new ParameterCollection(clusterNo, 0);
                    int counter = 0;
                    foreach (IReductStore reductStore in reductStoreCollection)
                    {
                        ParameterObjectReferenceCollection<IReduct> valueCollection
                                = new ParameterObjectReferenceCollection<IReduct>(String.Format("{0}", counter), reductStore.ToArray<IReduct>());
                        clusterCollection.Add(valueCollection);
                        counter++;
                    }

                    int ensembleId = 0;
                    foreach (object[] ensemble in clusterCollection.Values())
                    {
                        ReductStore tmpReductStore = new ReductStore();
                        for (int i = 0; i < ensemble.Length; i++)
                        {
                            tmpReductStore.DoAddReduct((IReduct)ensemble[i]);
                        }

                        RoughClassifier rc = new RoughClassifier();
                        rc.ReductStore = tmpReductStore;
                        rc.Classify(testData);
                        ClassificationResult classificationResult = rc.Vote(testData,
                                                                            IdentificationType.WeightConfidence,
                                                                            VoteType.WeightConfidence);

                        ReductEnsembleExperimentResult result = new ReductEnsembleExperimentResult
                        {
                            Id = String.Format("{0}", testNo),                            
                            NumberOfClusters = clusterNo,
                            ClusterId = ensembleId,
                            TestType = "Ensemble",
                            MinEpsilon = minEpsilon,
                            MaxEpsilon = maxEpsilon,
                            NumberOfPermuations = numberOfPermutations,
                            NumberOfReducts = tmpReductStore.Count,
                            Distance = (Func<double[], double[], double>)args["Distance"],
                            Linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args["Linkage"],
                            Dataset = data,
                            WeightGenerator = (WeightGenerator)args["WeightGenerator"],
                            DiscernibiltyVector = (Func<IReduct, double[], double[]>)args["ReconWeights"],
                            PermutationCollection = (PermutationCollection)args["PermutationCollection"],
                            
                            Accuracy = classificationResult.Accuracy,
                            BalancedAccuracy = classificationResult.BalancedAccuracy,
                            Coverage = classificationResult.Coverage,
                            Confidence = classificationResult.Confidence,
                            NumberOfClassified = classificationResult.NumberOfClassified,
                            NumberOfMisclassified = classificationResult.NumberOfMisclassified,
                            NumberOfUnclassifed = classificationResult.NumberOfUnclassifed                   
                        };

                        experimentResults.Add(result);
                                                
                        //random reduct ensemble of the same size

                        ReductStore randomReductStore = new ReductStore();
                        for (int i = 0; i < ensemble.Length; i++)
                        {
                            int randomIdx = RandomSingleton.Random.Next(reductGenerator.ReductPool.Count());
                            randomReductStore.DoAddReduct(reductGenerator.ReductPool.GetReduct(randomIdx));
                        }

                        RoughClassifier rc2 = new RoughClassifier();
                        rc2.ReductStore = randomReductStore;
                        rc2.Classify(testData);
                        ClassificationResult classificationResult2 = rc2.Vote(testData, IdentificationType.WeightConfidence, VoteType.WeightConfidence);

                        ReductEnsembleExperimentResult result2 = new ReductEnsembleExperimentResult
                        {
                            Id = String.Format("{0}", testNo),
                            NumberOfClusters = clusterNo,
                            ClusterId = ensembleId,
                            TestType = "Group",
                            MinEpsilon = minEpsilon,
                            MaxEpsilon = maxEpsilon,
                            NumberOfPermuations = numberOfPermutations,
                            NumberOfReducts = randomReductStore.Count,
                            Distance = (Func<double[], double[], double>)args["Distance"],
                            Linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args["Linkage"],
                            Dataset = data,
                            WeightGenerator = (WeightGenerator)args["WeightGenerator"],
                            DiscernibiltyVector = (Func<IReduct, double[], double[]>)args["ReconWeights"],
                            PermutationCollection = (PermutationCollection)args["PermutationCollection"],

                            Accuracy = classificationResult2.Accuracy,
                            BalancedAccuracy = classificationResult2.BalancedAccuracy,
                            Coverage = classificationResult2.Coverage,
                            Confidence = classificationResult2.Confidence,
                            NumberOfClassified = classificationResult2.NumberOfClassified,
                            NumberOfMisclassified = classificationResult2.NumberOfMisclassified,
                            NumberOfUnclassifed = classificationResult2.NumberOfUnclassifed
                        };

                        experimentResults.Add(result2);

                        ensembleId++;

                    } //for each ensemble


                } //clusterNo                
            } //test No           

            experimentResults.SaveToFile(@"f:\results.csv");
        }        
    }
}
