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
        //Refactor        
        //TODO Replace Args by Dictionary<string, object>
        //TODO Add parameter names as static variables             

        public static IEnumerable<Dictionary<string, object>> GetGenerateTestArgs()
        {
            List<Dictionary<string, object>> argsList = new List<Dictionary<string, object>>();

            Random rand = new Random();
            
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            
            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 5;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorConstant weightGenerator = new WeightGeneratorConstant(data);
            weightGenerator.Value = 1.0;            
            
            //TODO Epsilon generation according to some distribution
            int[] epsilons = new int[numberOfPermutations];
            for (int i = 0; i < numberOfPermutations; i++)
            {
                epsilons[i] = rand.Next(36);
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
                        
            
            Dictionary<string, object> argSet1 = new Dictionary<string, object>();
            argSet1.Add("DataStore", data);
            argSet1.Add("NumberOfThreads", 1);
            argSet1.Add("PermutationEpsilon", epsilons);            
            argSet1.Add("Distance", (Func<double[], double[], double>)Similarity.SquaredEuclidean);
            argSet1.Add("Linkage", (Func<int[], int[], DistanceMatrix, double>)ClusteringLinkage.Min);
            argSet1.Add("NumberOfClusters", 3);
            argSet1.Add("FactoryKey", "ReductEnsemble");                
            argSet1.Add("PermutationCollection", permList);
            argSet1.Add("DendrogramBitmapFile", @"f:\euclidean_reversed.bmp");
            argSet1.Add("ReductWeightFileName", @"f:\euclidean_reversed.csv");
            argSet1.Add("WeightGenerator", weightGenerator);
            argSet1.Add("ReconWeights", reconWeights);

            argsList.Add(argSet1);

            Dictionary<string, object> argSet2 = new Dictionary<string, object>(argSet1);
            argSet2["Distance"] = (Func<double[], double[], double>) Disimilarity.SquaredEuclidean;
            argSet2["DendrogramBitmapFile"] = @"f:\euclidean_standard.bmp";
            argSet1["ReductWeightFileName"] = @"f:\euclidean_standard.csv";

            argsList.Add(argSet2);

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
                                    
            //TODO Create generator based on parms, not generator name
            //TODO Store args inside generator
            //TODO Generate method without parameters
            ReductEnsembleGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleGenerator;
            IReductStoreCollection reductStoreCollection = reductGenerator.Generate(parms);

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
                Console.WriteLine("Reduct Group Alias {0}: {0}", k++);
                Console.WriteLine("======================");
                
                foreach (IReduct reduct in reductStore)
                {
                    Console.WriteLine("{0}", reduct);                    
                }
            }
            
            ReductEnsembleGenerator ensembleGenerator = reductGenerator as ReductEnsembleGenerator;
            if (ensembleGenerator != null)
            {
                Bitmap dendrogram = ensembleGenerator.Dendrogram.GetDendrogramAsBitmap(640, 480);
                dendrogram.Save((string) args["DendrogramBitmapFile"]);                
            }            
        }
     

    }
}
