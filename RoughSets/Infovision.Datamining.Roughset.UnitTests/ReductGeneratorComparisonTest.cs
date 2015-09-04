using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Utils;
using Infovision.Math;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Datamining.Experimenter;
using Infovision.Datamining.Experimenter.Parms;



namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    class ReductGeneratorComparisonTest
    {
        private int numberOfPermutations;
        private DataStore data;
        private DataStore testData;
        private int minEpsilon, maxEpsilon;
        private Random rand;
        private PermutationGenerator permGenerator;
        private PermutationCollection permList;
        private WeightGenerator weightGenerator;
        private int[] epsilons;        
        
        public ReductGeneratorComparisonTest()
        {
            numberOfPermutations = 20;
            data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            testData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);
            minEpsilon = 5;
            maxEpsilon = 25;
            
            rand = new Random();
            permGenerator = new PermutationGenerator(data);
            permList = permGenerator.Generate(numberOfPermutations);
            //weightGenerator = new WeightGeneratorConstant(data);
            weightGenerator = new WeightGeneratorRelative(data);

            data.DataStoreInfo.RecordWeights = weightGenerator.Weights;
            
            epsilons = new int[numberOfPermutations];
            for (int i = 0; i < numberOfPermutations; i++)
                epsilons[i] = rand.Next(minEpsilon, maxEpsilon);
        }

        
        public IEnumerable<Dictionary<string, object>> GetGenerateTestArgs()
        {                        
            List<Dictionary<string, object>> argsList = new List<Dictionary<string, object>>();                                                            
            Dictionary<string, object> argSet;

            argSet = new Dictionary<string, object>();
            argSet.Add("DataStore", data);            
            argSet.Add("PermutationEpsilon", epsilons);
            argSet.Add("Distance", (Func<double[], double[], double>)Similarity.Manhattan);
            argSet.Add("Linkage", (Func<int[], int[], DistanceMatrix, double>)ClusteringLinkage.Min);
            argSet.Add("NumberOfClusters", 5);
            argSet.Add("FactoryKey", "ReductEnsemble");
            argSet.Add("PermutationCollection", permList);
            argSet.Add("WeightGenerator", weightGenerator);
            argSet.Add("ReconWeights", (Func<IReduct, double[], double[]>) ReductEnsembleReconWeightsHelper.GetCorrectReconWeights);
            argsList.Add(argSet);

            return argsList;
        }

        [Test, TestCaseSource("GetGenerateTestArgs")]
        public void GenerateTest(Dictionary<string, object> args)
        {
            //TODO Add Classification result of training set
            //TODO Add reduct statistics info
            //TODO Add Number of recognized and unrecognized objects                        
            
            Console.WriteLine("Generator: {0}", (string)args["FactoryKey"]);
            Func<double[], double[], double> distance = (Func<double[], double[], double>)args["Distance"];
            Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);  
                     
            Func<IReduct, double[], double[]> recognition = (Func<IReduct, double[], double[]>) args["ReconWeights"];
            Console.WriteLine("{0}.{1}", recognition.Method.DeclaringType.Name, recognition.Method.Name); 

            Args parms = new Args();
            foreach (KeyValuePair<string, object> kvp in args)           
                parms.AddParameter(kvp.Key, kvp.Value);           
            
            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Generate();
            IReductStoreCollection reductStoreCollection = reductGenerator.ReductStoreCollection;
            
            ReductStore reductPool = reductGenerator.ReductPool as ReductStore;
            if (reductPool != null)
            {
                reductPool.SaveRecognitionToFile(data, recognition, @"F:\Reducts.csv");
            }

            Console.WriteLine("------------------------ Reduct Groups ------------------------");
            
            foreach (IReductStore reductStore in reductStoreCollection)
            {
                RoughClassifier rc = new RoughClassifier();
                rc.ReductStore = reductStore;
                rc.Classify(testData);
                ClassificationResult classificationResult = rc.Vote(testData, IdentificationType.Confidence, VoteType.MajorDecision);

                PrintResult(reductStore, classificationResult);
            }

            Console.WriteLine("------------------------ Ensembles ------------------------");

            int numberOfClusters = (int)args["NumberOfClusters"];
            ParameterCollection clusterCollection = new ParameterCollection(numberOfClusters, 0);
            int counter = 0;
            foreach (IReductStore reductStore in reductStoreCollection)
            {
                ParameterObjectReferenceCollection<IReduct> valueCollection = new ParameterObjectReferenceCollection<IReduct>(String.Format("{0}", counter), reductStore.ToArray<IReduct>());
                clusterCollection.Add(valueCollection);
                counter++;
            }

            foreach (object[] ensemble in clusterCollection.Values())
            {
                ReductStore tmpReductStore = new ReductStore();
                for (int i = 0; i < numberOfClusters; i++)
                {
                    tmpReductStore.DoAddReduct((IReduct)ensemble[i]);
                }

                RoughClassifier rc = new RoughClassifier();
                rc.ReductStore = tmpReductStore;
                rc.Classify(testData);
                ClassificationResult classificationResult = rc.Vote(testData, IdentificationType.WeightConfidence, VoteType.WeightConfidence);

                PrintResult(tmpReductStore, classificationResult);    
            }

        }

        private void PrintResult(IReductStore reductStore, ClassificationResult classificationResult)
        {
            Console.WriteLine(reductStore);            
            Console.WriteLine(classificationResult.ToString2());
        }
    }
}
