//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NRough.Data;
using NRough.MachineLearning.Clustering.Hierarchical;
using NRough.Math;
using NRough.Core;
using NUnit.Framework;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Roughsets;
using NRough.Core.Random;

namespace NRough.Tests.MachineLearning.Roughsets
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

            var data = Data.Benchmark.Factory.DnaModifiedTrain();
            //DataStore data = DataStore.Load(@"Data\dna_modified.trn", DataFormat.RSES1);

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