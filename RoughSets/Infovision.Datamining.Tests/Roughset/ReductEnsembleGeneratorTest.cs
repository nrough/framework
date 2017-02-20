using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Raccoon.Data;
using Raccoon.MachineLearning.Clustering.Hierarchical;
using Raccoon.Math;
using Raccoon.Core;
using NUnit.Framework;
using Raccoon.MachineLearning.Weighting;
using Raccoon.MachineLearning.Permutations;

namespace Raccoon.MachineLearning.Roughset.UnitTests
{
    [TestFixture]
    internal class ReductEnsembleGeneratorTest
    {
        public ReductEnsembleGeneratorTest()
        {                    
            RandomSingleton.Seed = Guid.NewGuid().GetHashCode();
        }

        public static IEnumerable<Dictionary<string, object>> GetGenerateTestArgs()
        {
            List<Dictionary<string, object>> argsList = new List<Dictionary<string, object>>();

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.RSES1);

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            int numberOfPermutations = 2;
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);

            WeightGeneratorConstant weightGenerator = new WeightGeneratorConstant(data);
            weightGenerator.Value = 1.0;

            double[] epsilons = new double[numberOfPermutations];
            for (int i = 0; i < numberOfPermutations; i++)
            {
                epsilons[i] = (double)(RandomSingleton.Random.Next(36) / 100.0);
            }

            Func<IReduct, double[], RuleQualityMethod, double[]> reconWeights = ReductToVectorConversionMethods.GetDefaultReconWeights;
            Dictionary<string, object> argSet;

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductFactoryOptions.DecisionTable, data);
            argSet.Add(ReductFactoryOptions.PermutationEpsilon, epsilons);
            argSet.Add(ReductFactoryOptions.Distance, (Func<double[], double[], double>)Distance.Manhattan);
            argSet.Add(ReductFactoryOptions.Linkage, (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Single);
            argSet.Add(ReductFactoryOptions.NumberOfClusters, 3);
            argSet.Add(ReductFactoryOptions.ReductType, ReductTypes.ReductEnsemble);
            argSet.Add(ReductFactoryOptions.PermutationCollection, permList);
            argSet.Add(ReductFactoryOptions.DendrogramBitmapFile, @"euclidean");
            argSet.Add("ReductWeightFileName", @"euclidean");
            argSet.Add(ReductFactoryOptions.WeightGenerator, weightGenerator);
            argSet.Add(ReductFactoryOptions.ReconWeights, reconWeights);
            argsList.Add(argSet);

            return argsList;
        }

        [Test, TestCaseSource("GetGenerateTestArgs")]
        public void GenerateTest(Dictionary<string, object> args)
        {
            Func<double[], double[], double> distance = (Func<double[], double[], double>)args[ReductFactoryOptions.Distance];
            //Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);

            Args parms = new Args();
            foreach (KeyValuePair<string, object> kvp in args)
            {
                parms.SetParameter(kvp.Key, kvp.Value);
            }

            ReductEnsembleGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleGenerator;
            reductGenerator.Run();
            IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection((int)args[ReductFactoryOptions.NumberOfClusters]);

            DataStore data = (DataStore)parms.GetParameter(ReductFactoryOptions.DecisionTable);
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

            //Console.WriteLine("All reducts");
            for (int j = 0; j < reductGenerator.ReductPool.Count; j++)
            {
                IReduct r = reductGenerator.ReductPool.GetReduct(j);
                //Console.WriteLine("{0}: {1}", j, r);
            }

            //Console.WriteLine("Reduct distances");
            DistanceMatrix d = reductGenerator.Dendrogram.DistanceMatrix;
            //Console.WriteLine(d);

            //Console.WriteLine("Dendrogram");
            HierarchicalClusteringBase hc = reductGenerator.Dendrogram;
            //Console.WriteLine(hc);

            /*
            Console.WriteLine("Reduct groups");
            Dictionary<int, List<int>> membership = reductGenerator.Dendrogram.GetClusterMembershipAsDict((int)args.GetParameter(ReductGeneratorParamHelper.NumberOfClusters));
            foreach (KeyValuePair<int, List<int>> kvp in membership)
            {
                Console.WriteLine("Reduct Group Name {0}", kvp.Key);
                Console.WriteLine("======================");

                foreach (int reductId in kvp.Value)
                {
                    Console.WriteLine(reductGenerator.ReductPool.GetReduct(reductId));
                }
            }
            */

            //int k=1;
            foreach (IReductStore reductStore in reductStoreCollection)
            {
                //Console.WriteLine("Reduct Group Name {0}:", k++);
                //Console.WriteLine("======================");

                foreach (IReduct reduct in reductStore)
                {
                    //Console.WriteLine("{0}", reduct);
                }
            }

            /*
            ReductEnsembleGenerator ensembleGenerator = reductGenerator as ReductEnsembleGenerator;
            if (ensembleGenerator != null)
            {
                Bitmap dendrogramLinkCollection = ensembleGenerator.Dendrogram.GetDendrogramAsBitmap(640, (int) ensembleGenerator.Dendrogram.DendrogramLinkCollection.MaxHeight + 100);
                dendrogramLinkCollection.Save((string) args[ReductGeneratorParamHelper.DendrogramBitmapFile]);
            }
            */
        }
    }
}