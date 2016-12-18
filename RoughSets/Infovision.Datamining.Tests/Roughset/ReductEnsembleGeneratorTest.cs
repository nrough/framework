﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Infovision.Data;
using Infovision.MachineLearning.Clustering.Hierarchical;
using Infovision.Math;
using Infovision.Core;
using NUnit.Framework;
using Infovision.MachineLearning.Weighting;
using Infovision.MachineLearning.Permutations;

namespace Infovision.MachineLearning.Roughset.UnitTests
{
    [TestFixture]
    internal class ReductEnsembleGeneratorTest
    {
        public ReductEnsembleGeneratorTest()
        {
            Random randSeed = new Random();
            int seed = Guid.NewGuid().GetHashCode();
            //Console.WriteLine("class ReductEnsembleGeneratorTest Seed: {0}", seed);
            RandomSingleton.Seed = seed;
        }

        public static IEnumerable<Dictionary<string, object>> GetGenerateTestArgs()
        {
            List<Dictionary<string, object>> argsList = new List<Dictionary<string, object>>();

            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

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

            Func<IReduct, double[], RuleQualityFunction, double[]> reconWeights = ReductEnsembleReconWeightsHelper.GetDefaultReconWeights;
            Dictionary<string, object> argSet;

            argSet = new Dictionary<string, object>();
            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.NumberOfThreads, 1);
            argSet.Add(ReductGeneratorParamHelper.PermutationEpsilon, epsilons);
            argSet.Add(ReductGeneratorParamHelper.Distance, (Func<double[], double[], double>)Similarity.Manhattan);
            argSet.Add(ReductGeneratorParamHelper.Linkage, (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Single);
            argSet.Add(ReductGeneratorParamHelper.NumberOfClusters, 3);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsemble);
            argSet.Add(ReductGeneratorParamHelper.PermutationCollection, permList);
            argSet.Add(ReductGeneratorParamHelper.DendrogramBitmapFile, @"euclidean");
            argSet.Add("ReductWeightFileName", @"euclidean");
            argSet.Add(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            argSet.Add(ReductGeneratorParamHelper.ReconWeights, reconWeights);
            argsList.Add(argSet);

            return argsList;
        }

        [Test, TestCaseSource("GetGenerateTestArgs")]
        public void GenerateTest(Dictionary<string, object> args)
        {
            Func<double[], double[], double> distance = (Func<double[], double[], double>)args[ReductGeneratorParamHelper.Distance];
            //Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);

            Args parms = new Args();
            foreach (KeyValuePair<string, object> kvp in args)
            {
                parms.SetParameter(kvp.Key, kvp.Value);
            }

            ReductEnsembleGenerator reductGenerator = ReductFactory.GetReductGenerator(parms) as ReductEnsembleGenerator;
            reductGenerator.Run();
            IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection((int)args[ReductGeneratorParamHelper.NumberOfClusters]);

            DataStore data = (DataStore)parms.GetParameter(ReductGeneratorParamHelper.TrainData);
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