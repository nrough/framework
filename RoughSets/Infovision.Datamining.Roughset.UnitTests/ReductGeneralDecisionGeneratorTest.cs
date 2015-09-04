using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Math;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class ReductGeneralDecisionGeneratorTest
    {
        public static IEnumerable<Dictionary<string, object>> GetGenerateTestArgs()
        {
            List<Dictionary<string, object>> argsList = new List<Dictionary<string, object>>();

            Random randSeed = new Random();
            int seed = randSeed.Next(Int32.MaxValue);            

            RandomSingleton.Seed = seed;

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 20;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorConstant weightGenerator = new WeightGeneratorConstant(data);
            weightGenerator.Value = 1.0;

            //TODO Epsilon generation according to some distribution
            int[] epsilons = new int[numberOfPermutations];
            for (int i = 0; i < numberOfPermutations; i++)
            {
                epsilons[i] = RandomSingleton.Random.Next(36);
            }

            //Func<IReduct, double[], double[]> reconWeights = ReductEnsembleGenerator.GetDefaultReconWeights;
            Func<IReduct, double[], double[]> reconWeights = (r, w) =>
            {
                double[] result = new double[w.Length];
                Array.Copy(w, result, w.Length);
                foreach (EquivalenceClass e in r.EquivalenceClassMap)
                    foreach (int i in e.GetObjectIndexes(e.MajorDecision))
                        result[i] *= -1;

                return result;
            };

            Dictionary<string, object> argSet;

            argSet = new Dictionary<string, object>();
            argSet.Add("DataStore", data);
            argSet.Add("NumberOfThreads", 1);
            argSet.Add("PermutationEpsilon", epsilons);
            argSet.Add("Distance", (Func<double[], double[], double>)Similarity.Manhattan);
            argSet.Add("Linkage", (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Single);
            argSet.Add("NumberOfClusters", 3);
            argSet.Add("FactoryKey", "ReductGeneralizedDecision");
            argSet.Add("PermutationCollection", permList);
            argSet.Add("DendrogramBitmapFile", @"euclidean");
            argSet.Add("ReductWeightFileName", @"euclidean");
            argSet.Add("WeightGenerator", weightGenerator);
            argSet.Add("ReconWeights", reconWeights);
            argsList.Add(argSet);

            return argsList;
        }

        [Test, TestCaseSource("GetGenerateTestArgs")]
        public void GenerateTest(Dictionary<string, object> args)
        {            
            Args parms = new Args();
            foreach (KeyValuePair<string, object> kvp in args)
            {
                parms.AddParameter(kvp.Key, kvp.Value);
            }

            ReductGeneralDecisionGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductGeneralDecisionGenerator;
            reductGenerator.Generate();

            Console.WriteLine();
            Console.WriteLine("Reduct Pool");
            Console.WriteLine("======================================");

            IReductStore reductPool = reductGenerator.ReductPool;
            foreach (IReduct reduct in reductPool)
            {
                Console.WriteLine(reduct);
            }
        }
    }
}
