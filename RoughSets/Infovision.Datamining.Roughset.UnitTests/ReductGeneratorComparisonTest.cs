using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Infovision.Data;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Datamining.Experimenter.Parms;
using Infovision.Math;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    internal class ReductGeneratorComparisonTest
    {
        public ReductGeneratorComparisonTest()
        {
            Random randSeed = new Random();
            int seed = Guid.NewGuid().GetHashCode();
            //Console.WriteLine("class ReductGeneratorComparisonTest Seed: {0}", seed);
            RandomSingleton.Seed = seed;
        }

        public IEnumerable<Dictionary<string, object>> GetComparisonTestArgs()
        {
            int numberOfPermutations = 20;
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            DataStore testData = DataStore.Load(@"Data\dna_modified.tst", FileFormat.Rses1, data.DataStoreInfo);
            int minEpsilon = 5;
            int maxEpsilon = 25;

            PermutationGenerator permGenerator = new PermutationGenerator(data);
            PermutationCollection permList = permGenerator.Generate(numberOfPermutations);
            WeightGenerator weightGenerator = new WeightGeneratorConstant(data);
            double[] epsilons;

            epsilons = new double[numberOfPermutations];
            for (int i = 0; i < numberOfPermutations; i++)
                epsilons[i] = RandomSingleton.Random.Next(minEpsilon, maxEpsilon);

            List<Dictionary<string, object>> argsList = new List<Dictionary<string, object>>();
            Dictionary<string, object> argSet;

            argSet = new Dictionary<string, object>();

            argSet.Add("_TestData", testData);

            argSet.Add(ReductGeneratorParamHelper.TrainData, data);
            argSet.Add(ReductGeneratorParamHelper.PermutationEpsilon, epsilons);
            argSet.Add(ReductGeneratorParamHelper.Distance, (Func<double[], double[], double>)Similarity.Manhattan);
            argSet.Add(ReductGeneratorParamHelper.Linkage, (Func<int[], int[], DistanceMatrix, double[][], double>)ClusteringLinkage.Complete);
            argSet.Add(ReductGeneratorParamHelper.NumberOfClusters, 5);
            argSet.Add(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsemble);
            argSet.Add(ReductGeneratorParamHelper.PermutationCollection, permList);
            argSet.Add(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            argSet.Add(ReductGeneratorParamHelper.ReconWeights, (Func<IReduct, double[], RuleQualityFunction, double[]>)ReductEnsembleReconWeightsHelper.GetCorrectReconWeights);
            argSet.Add(ReductGeneratorParamHelper.DendrogramBitmapFile, @"reducts.bmp");
            argsList.Add(argSet);

            //for (int i = 0; i < numberOfPermutations; i++)
            //{
            //    argSet = new Dictionary<string, object>(argSet);
            //    argSet[ReductGeneratorParamHelper.NumberOfClusters] = 2 + i;
            //    argsList.Add(argSet);
            //}

            return argsList;
        }

        [Test]
        public void QuickTest()
        {
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            WeightGenerator weightGenerator = new WeightGeneratorConstant(data);
            //Console.WriteLine(weightGenerator.GetType().Name);
        }

        [Test, TestCaseSource("GetComparisonTestArgs")]
        public void ComparisonTest(Dictionary<string, object> args)
        {
            DataStore data = (DataStore)args[ReductGeneratorParamHelper.TrainData];
            DataStore testData = (DataStore)args["_TestData"];
            int numberOfClusters = (int)args[ReductGeneratorParamHelper.NumberOfClusters];

            //Console.WriteLine("Generator: {0}", (string)args[ReductGeneratorParamHelper.FactoryKey]);
            Func<double[], double[], double> distance = (Func<double[], double[], double>)args[ReductGeneratorParamHelper.Distance];
            //Console.WriteLine("{0}.{1}", distance.Method.DeclaringType.Name, distance.Method.Name);

            Func<int[], int[], DistanceMatrix, double[][], double> linkage = (Func<int[], int[], DistanceMatrix, double[][], double>)args[ReductGeneratorParamHelper.Linkage];
            //Console.WriteLine("{0}.{1}", linkage.Method.DeclaringType.Name, linkage.Method.Name);

            Func<IReduct, double[], RuleQualityFunction, double[]> recognition = (Func<IReduct, double[], RuleQualityFunction, double[]>)args[ReductGeneratorParamHelper.ReconWeights];
            //Console.WriteLine("{0}.{1}", recognition.Method.DeclaringType.Name, recognition.Method.Name);

            Args parms = new Args();
            foreach (KeyValuePair<string, object> kvp in args)
                if (kvp.Key.Substring(0, 1) != "_")
                    parms.SetParameter(kvp.Key, kvp.Value);

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(parms);
            reductGenerator.Run();
            IReductStoreCollection reductStoreCollection = reductGenerator.GetReductStoreCollection(numberOfClusters);

            numberOfClusters = System.Math.Min(numberOfClusters, reductGenerator.ReductPool.Count());

            Assert.AreEqual(numberOfClusters, reductStoreCollection.Count(), "Number of reduct stores");

            ReductStore reductPool = reductGenerator.ReductPool as ReductStore;
            if (reductPool != null)
            {
                reductPool.SaveErrorVectorsInRFormat(data, recognition, @"reducts_r.csv", RuleQuality.Confidence);
                reductPool.SaveErrorVectorsInWekaFormat(data, recognition, @"reducts_weka.csv", RuleQuality.Confidence);
            }

            //Console.WriteLine("------------------------ Reduct Pool ------------------------");
            //Console.WriteLine(reductGenerator.ReductPool);

            ReductEnsembleGenerator rEnsembleGen = reductGenerator as ReductEnsembleGenerator;
            if (rEnsembleGen != null)
            {
                //Console.WriteLine("------------------------ Distances ------------------------");
                //Console.WriteLine(rEnsembleGen.Dendrogram.DistanceMatrix);

                //Console.WriteLine("------------------------ Linkages ------------------------");
                //Console.WriteLine(rEnsembleGen.Dendrogram);
            }

            //Console.WriteLine("------------------------ Reduct Groups ------------------------");

            foreach (IReductStore reductStore in reductStoreCollection)
            {
                IReductStoreCollection localStoreCollection = new ReductStoreCollection(1);
                localStoreCollection.AddStore(reductStore);

                RoughClassifier rc = new RoughClassifier(
                    localStoreCollection,
                    RuleQuality.Confidence,
                    RuleQuality.SingleVote,
                    data.DataStoreInfo.GetDecisionValues());
                ClassificationResult classificationResult = rc.Classify(testData, null);

                PrintResult(reductStore, classificationResult);
            }

            //return;

            //Console.WriteLine("------------------------ Ensembles ------------------------");

            ParameterCollection clusterCollection = new ParameterCollection(numberOfClusters, 0);
            int counter = 0;
            foreach (IReductStore reductStore in reductStoreCollection)
            {
                ParameterObjectReferenceCollection<IReduct> valueCollection
                        = new ParameterObjectReferenceCollection<IReduct>(String.Format("{0}", counter), reductStore.ToArray<IReduct>());
                clusterCollection.Add(valueCollection);
                counter++;
            }

            foreach (object[] ensemble in clusterCollection.Values())
            {
                ReductStore tmpReductStore = new ReductStore(numberOfClusters);
                for (int i = 0; i < numberOfClusters; i++)
                {
                    tmpReductStore.DoAddReduct((IReduct)ensemble[i]);
                }

                IReductStoreCollection tmpReductStoreCollection = new ReductStoreCollection(1);
                reductStoreCollection.AddStore(tmpReductStore);

                RoughClassifier rc = new RoughClassifier(
                    tmpReductStoreCollection,
                    RuleQuality.ConfidenceW,
                    RuleQuality.ConfidenceW,
                    data.DataStoreInfo.GetDecisionValues());
                ClassificationResult classificationResult = rc.Classify(testData, null);

                PrintResult(tmpReductStore, classificationResult);
            }

            ReductEnsembleGenerator ensembleGenerator = reductGenerator as ReductEnsembleGenerator;
            if (ensembleGenerator != null)
            {
                DendrogramChart dc = new DendrogramChart(ensembleGenerator.Dendrogram, 640, (int)ensembleGenerator.Dendrogram.Root.Height + 100);
                Bitmap dendrogram = dc.GetAsBitmap();
                dendrogram.Save((string)args[ReductGeneratorParamHelper.DendrogramBitmapFile]);
            }
        }

        private void PrintResult(IReductStore reductStore, ClassificationResult classificationResult)
        {
            //Console.WriteLine(reductStore);
            //Console.WriteLine(classificationResult.ToString2());
        }
    }
}