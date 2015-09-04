using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NUnit.Framework;
using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Utils;
using Infovision.Math;
using Infovision.Datamining.Clustering.Hierarchical;
using System.IO;

namespace Infovision.Datamining.Roughset.UnitTests
{                
    [TestFixture]
    class ReductEnsembleGeneratorTest
    {
        public ReductEnsembleGeneratorTest()
        {
            Random randSeed = new Random();
            int seed = Guid.NewGuid().GetHashCode();
            Console.WriteLine("Seed: {0}");
            RandomSingleton.Seed = seed;
        }
        
        public static IEnumerable<Dictionary<string, object>> GetGenerateTestArgs()
        {
            List<Dictionary<string, object>> argsList = new List<Dictionary<string, object>>();
            
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 20;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorConstant weightGenerator = new WeightGeneratorConstant(data);
            weightGenerator.Value = 1.0;            

            double[] epsilons = new double[numberOfPermutations];
            for (int i = 0; i < numberOfPermutations; i++)
            {
                epsilons[i] = (double)RandomSingleton.Random.Next(36) / 100.0;
            }

            //Func<IReduct, double[], double[]> reconWeights = ReductEnsembleGenerator.GetDefaultReconWeights;
            Func<IReduct, double[], double[]> reconWeights = (r, w) =>
                {
                    double[] result = new double[w.Length];
                    Array.Copy(w, result, w.Length);
                    foreach (EquivalenceClass e in r.EquivalenceClasses)
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
            argSet.Add("FactoryKey", "ReductEnsemble");                
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
            
            Func<double[], double[], double> distance = (Func<double[], double[], double>)args["Distance"];            
            Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);
                        
            Args parms = new Args();
            foreach (KeyValuePair<string, object> kvp in args)
            {
                parms.AddParameter(kvp.Key, kvp.Value);
            }                                 
                                                            
            ReductEnsembleGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleGenerator;
            reductGenerator.Generate();
            IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection((int)args["NumberOfClusters"]);

            DataStore data = (DataStore) parms.GetParameter("DataStore");
            ReductStore reductPool = reductGenerator.ReductPool as ReductStore;
            double[][] errorWeights = reductGenerator.GetWeightVectorsFromReducts(reductPool);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.NumberOfRecords; i++)
            {
                for (int j = 0; j < reductPool.Count; j++)
                {
                    sb.Append(errorWeights[j][i]).Append(" ");
                }
                sb.AppendLine();
            }
            File.WriteAllText((string)parms.GetParameter("ReductWeightFileName"), sb.ToString());

            
            Console.WriteLine("All reducts");            
            for(int j=0; j<reductGenerator.ReductPool.Count; j++)
            {                
                Console.WriteLine("{0}: {1}", j, reductGenerator.ReductPool.GetReduct(j));
            }

            Console.WriteLine("Reduct distances");
            Console.WriteLine(reductGenerator.Dendrogram.DistanceMatrix);

            Console.WriteLine("Dendrogram");
            Console.WriteLine(reductGenerator.Dendrogram);

            /*
            Console.WriteLine("Reduct groups");
            Dictionary<int, List<int>> membership = reductGenerator.Dendrogram.GetClusterMembershipAsDict((int)parms.GetParameter("NumberOfClusters"));
            foreach (KeyValuePair<int, List<int>> kvp in membership)
            {
                Console.WriteLine("Reduct Group Alias {0}", kvp.Key);
                Console.WriteLine("======================");

                foreach (int reductId in kvp.Value)
                {
                    Console.WriteLine(reductGenerator.ReductPool.GetReduct(reductId));
                }
            }
            */

             
            int k=1;
            foreach (IReductStore reductStore in reductStoreCollection)
            {
                Console.WriteLine("Reduct Group Alias {0}:", k++);
                Console.WriteLine("======================");
                
                foreach (IReduct reduct in reductStore)
                {
                    Console.WriteLine("{0}", reduct);                    
                }
            }
            
            /*
            ReductEnsembleGenerator ensembleGenerator = reductGenerator as ReductEnsembleGenerator;
            if (ensembleGenerator != null)
            {
                Bitmap dendrogramLinkCollection = ensembleGenerator.Dendrogram.GetDendrogramAsBitmap(640, (int) ensembleGenerator.Dendrogram.DendrogramLinkCollection.MaxHeight + 100);
                dendrogramLinkCollection.Save((string) args["DendrogramBitmapFile"]);                
            }
            */
        }
     

    }
}
