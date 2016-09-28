using System;
using System.Collections.Generic;
using System.Text;
using Infovision.Data;
using Infovision.Utils;
using NUnit.Framework;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class BireductTest
    {
        [Test]
        public void CalcBiReductPositive()
        {
            this.CheckBireductUnique(ReductFactoryKeyHelper.GammaBireduct);
        }

        [Test]
        public void CalcBiReductMajority()
        {
            this.CheckBireductUnique(ReductFactoryKeyHelper.Bireduct);
        }

        [Test]
        public void CalcBiReductRelative()
        {
            this.CheckBireductUnique(ReductFactoryKeyHelper.BireductRelative);
        }

        private void CheckBireductUnique(string reductGeneratorKey)
        {
            string trainFileName = @"Data\dna.train";
            string testFileName = @"Data\dna.train";

            var dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            var dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            var dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            Args parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, ReductGeneratorParamHelper.TrainData, ReductGeneratorParamHelper.NumberOfPermutations },
                                  new Object[] { reductGeneratorKey, dataStoreTrain, 100 });

            IReductGenerator bireductGenerator = ReductFactory.GetReductGenerator(parms);
            bireductGenerator.Run();
            IReductStore reductStore = bireductGenerator.ReductPool;

            foreach (IReduct reduct in reductStore)
            {
                foreach (EquivalenceClass reductStat in reduct.EquivalenceClasses)
                {
                    Assert.AreEqual(1, reductStat.NumberOfDecisions);
                }
            }
        }

        [Test]
        public void RelativeMeasureTest()
        {
            string trainFileName = @"Data\dna.train";
            string testFileName = @"Data\dna.train";

            var dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            var dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            var dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            Dictionary<int, double> elementWeights = new Dictionary<int, double>(dataStoreTrain.NumberOfRecords);
            double sumWeights = 0;

            int j = dataStoreTrain.DataStoreInfo.NumberOfFields - 1;

            int decisionIndex = dataStoreTrain.DataStoreInfo.DecisionFieldIndex;
            for (int objectIdx = 0; objectIdx < dataStoreTrain.NumberOfRecords; objectIdx++)
            {
                long decisionValue = dataStoreTrain.GetFieldIndexValue(objectIdx, decisionIndex);
                double p = 1.0 / (dataStoreTrain.DataStoreInfo.NumberOfObjectsWithDecision(decisionValue) * dataStoreTrain.DataStoreInfo.NumberOfDecisionValues);

                elementWeights[objectIdx] = p;
                sumWeights += p;
            }

            InformationMeasureRelative roughMeasure = new InformationMeasureRelative();
            Reduct reduct = new Reduct(dataStoreTrain, dataStoreTrainInfo.GetFieldIds(FieldTypes.Standard), 0);

            double r = roughMeasure.Calc(reduct);
            double u = sumWeights;

            Assert.AreEqual(r, u, 0.0000001);

            WeightGeneratorRelative weightGenerator = new WeightGeneratorRelative(dataStoreTrain);
            ReductWeights reductWeights = new ReductWeights(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0, weightGenerator.Weights);
            InformationMeasureWeights weightMeasure = new InformationMeasureWeights();
            double w = weightMeasure.Calc(reductWeights);

            Assert.AreEqual(r, w, 0.0000001);
        }

        [Test]
        public void MajorityMeasureTest()
        {
            string trainFileName = @"Data\dna.train";
            string testFileName = @"Data\dna.train";

            var dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            var dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            var dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            Dictionary<int, double> elementWeights = new Dictionary<int, double>(dataStoreTrain.NumberOfRecords);
            double sumWeights = 0;

            int j = dataStoreTrain.DataStoreInfo.NumberOfFields - 1;
            int decisionIndex = dataStoreTrain.DataStoreInfo.DecisionFieldIndex;
            for (int objectIdx = 0; objectIdx < dataStoreTrain.NumberOfRecords; objectIdx++)
            {
                long decisionValue = dataStoreTrain.GetFieldIndexValue(objectIdx, decisionIndex);
                double p = 1.0 / dataStoreTrain.NumberOfRecords;

                elementWeights[objectIdx] = p;
                sumWeights += p;
            }

            InformationMeasureMajority roughMeasure = new InformationMeasureMajority();
            Reduct reduct = new Reduct(dataStoreTrain, dataStoreTrainInfo.GetFieldIds(FieldTypes.Standard), 0);

            double r = roughMeasure.Calc(reduct);
            double u = sumWeights;

            Assert.AreEqual(r, u, 0.0000001);

            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(dataStoreTrain);
            ReductWeights reductWeights = new ReductWeights(dataStoreTrain, dataStoreTrain.DataStoreInfo.GetFieldIds(FieldTypes.Standard), 0, weightGenerator.Weights);
            InformationMeasureWeights weightMeasure = new InformationMeasureWeights();
            double w = weightMeasure.Calc(reductWeights);

            Assert.AreEqual(r, w, 0.0000001);
        }

        [Test]
        public void BireductMajorityClassifierTest()
        {
            this.Classify(ReductFactoryKeyHelper.Bireduct);
        }

        [Test]
        public void BireductPositiveClassifierTest()
        {
            this.Classify(ReductFactoryKeyHelper.GammaBireduct);
        }

        [Test]
        public void BireductRelativeClassifierTest()
        {
            this.Classify(ReductFactoryKeyHelper.BireductRelative);
        }

        //Ad 1
        [Test]
        public void PrintDecisionRulesTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);

            localDataStore.DataStoreInfo.GetFieldInfo(1).Alias = "O";
            localDataStore.DataStoreInfo.GetFieldInfo(2).Alias = "T";
            localDataStore.DataStoreInfo.GetFieldInfo(3).Alias = "H";
            localDataStore.DataStoreInfo.GetFieldInfo(4).Alias = "W";
            localDataStore.DataStoreInfo.DecisionInfo.Alias = "d";

            Args args = new Args();
            args.SetParameter(ReductGeneratorParamHelper.TrainData, localDataStore);
            args.SetParameter(ReductGeneratorParamHelper.Epsilon, 0.0);
            args.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ApproximateReductMajority);
            args.SetParameter(ReductGeneratorParamHelper.PermutationCollection, ReductFactory.GetPermutationGenerator(args).Generate(10));

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
            reductGenerator.Run();

            RoughClassifier classifier = new RoughClassifier(
                reductGenerator.GetReductStoreCollection(),
                RuleQuality.Confidence,
                RuleQuality.SingleVote,
                localDataStore.DataStoreInfo.GetDecisionValues());

            //Console.Write(classifier.PrintDecisionRules(localDataStore.DataStoreInfo));
        }

        //Ad 2
        [Test]
        public void TestBireductGolf_2()
        {
            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            localDataStore.DataStoreInfo.GetFieldInfo(1).Alias = "O";
            localDataStore.DataStoreInfo.GetFieldInfo(2).Alias = "T";
            localDataStore.DataStoreInfo.GetFieldInfo(3).Alias = "H";
            localDataStore.DataStoreInfo.GetFieldInfo(4).Alias = "W";
            localDataStore.DataStoreInfo.DecisionInfo.Alias = "d";

            /*
            \scriptsize{O 8 W 1 4 7 2 14 10 12 9 T 6 3 13 5 11 H} & \scriptsize{(\{H\},$\{u_i: i \in \{1,2,5,7..11,13..14\}\}$)}\\ \hline
            \scriptsize{H 13 T 8 W 6 11 3 14 10 O 5 7 9 2 1 4 12} & \scriptsize{(\{O\},\{1..3 6..8 12..14\})}\\ \hline
            \scriptsize{3 8 T 1 W 11 9 O 14 12 6 4 7 H 10 13 2 5} & \scriptsize{(\{O H\},\{1..3 6..9 11..14\})}\\ \hline
            \scriptsize{2 13 5 14 11 7 12 4 3 1 9 6 8 10 H O W T} & \scriptsize{(\{O T W\},\{1..14\})}\\ \hline
            \scriptsize{9 4 12 14 1 8 7 3 10 13 6 11 2 5 W T H O} & \scriptsize{(\{O H W\},\{1..14\})}\\ \hline
            \scriptsize{11 O 2 H 1 10 5 7 9 8 3 13 T 6 14 12 4 W} & \scriptsize{(\{T\},\{1..2 4..5 7 9..12\})}\\ \hline
            \scriptsize{T 2 5 H 10 11 W 14 1 12 7 9 13 6 4 8 3 O} & \scriptsize{(\{O\},\{1..5,7..8,10,12..13\})}\\ \hline
            \scriptsize{W 6 H O 5 8 4 7 3 2 10 9 12 11 13 14 1 T} & \scriptsize{(\{T\},\{3 6 8 13..14\})}\\ \hline
            \scriptsize{O 2 3 13 1 H 4 T W 6 12 14 5 8 9 10 11 7} & \scriptsize{(\{W\},\{2..6 9..10 13..14\})}\\ \hline
            \scriptsize{O H 14 1 10 7 4 3 12 13 5 W 9 T 11 8 2 6} & \scriptsize{(\{T W\},\{1..2 4..5 7 9..10 14\})}\\ \hline
            \scriptsize{6 5 10 9 H O 12 T 8 W 4 2 13 3 7 1 14 11} & \scriptsize{(\{T W\},\{2..6 9..13\})}\\ \hline
            \scriptsize{11 14 9 13 3 7 8 2 5 1 12 W 6 4 10 H O T} & \scriptsize{(\{O H\},\{1..3 5 7..14\})}\\ \hline
            \scriptsize{13 8 6 H 7 W 9 T 5 3 4 12 O 2 10 14 11 1} & \scriptsize{(\{O T\},\{1..4 6..10 12..13\})}\\ \hline
            \scriptsize{9 H 2 4 6 13 14 7 T 11 10 O W 3 5 1 8 12} & \scriptsize{(\{O W\},\{2..7 9..10 12..14\})}\\ \hline
            \scriptsize{W 5 3 O 12 4 T H 7 2 13 11 10 6 8 1 14 9} & \scriptsize{(\{\},\{3..5 7 9..13\})}\\ \hline
            */

            PermutationCollection permutations = new PermutationCollection();
            permutations.Add(new Permutation(new int[] { -1, 7, -4, 0, 3, 6, 1, 13, 9, 11, 8, -2, 5, 2, 12, 4, 10, -3 }));
            permutations.Add(new Permutation(new int[] { -3, 12, -2, 7, -4, 5, 10, 2, 13, 9, -1, 4, 6, 8, 1, 0, 3, 11 }));
            permutations.Add(new Permutation(new int[] { 2, 7, -2, 0, -4, 10, 8, -1, 13, 11, 5, 3, 6, -3, 9, 12, 1, 4 }));
            permutations.Add(new Permutation(new int[] { 1, 12, 4, 13, 10, 6, 11, 3, 2, 0, 8, 4, 7, 9, -3, -1, -4, -2 }));
            permutations.Add(new Permutation(new int[] { 8, 3, 11, 13, 0, 7, 6, 2, 9, 12, 5, 10, 1, 4, -4, -2, -3, -1 }));
            permutations.Add(new Permutation(new int[] { 10, -1, 1, -3, 0, 9, 4, 6, 8, 7, 2, 12, -2, 5, 13, 11, 3, -4 }));
            permutations.Add(new Permutation(new int[] { -2, 1, 4, -3, 9, 10, -4, 13, 0, 11, 6, 8, 12, 5, 3, 7, 2, -1 }));
            permutations.Add(new Permutation(new int[] { -4, 5, -3, -1, 4, 8, 3, 6, 2, 1, 9, 8, 11, 10, 12, 13, 0, -2 }));
            permutations.Add(new Permutation(new int[] { -1, 1, 2, 12, 0, -3, 3, -2, -4, 5, 11, 13, 4, 7, 8, 9, 10, 6 }));
            permutations.Add(new Permutation(new int[] { -1, -3, 13, 0, 9, 6, 3, 2, 11, 12, 4, -4, 8, -2, 10, 7, 1, 5 }));
            permutations.Add(new Permutation(new int[] { 5, 4, 9, 8, -3, -1, 11, -2, 7, -4, 3, 1, 12, 2, 6, 0, 13, 10 }));
            permutations.Add(new Permutation(new int[] { 10, 13, 8, 12, 2, 6, 7, 1, 4, 0, 11, -4, 5, 3, 9, -3, -1, -2 }));
            permutations.Add(new Permutation(new int[] { 12, 7, 5, -3, 6, -4, 8, -2, 4, 2, 3, 11, -1, 1, 9, 13, 10, 0 }));
            permutations.Add(new Permutation(new int[] { 8, -3, 1, 3, 5, 12, 13, 6, -2, 10, 9, -1, -4, 2, 4, 0, 7, 11 }));
            permutations.Add(new Permutation(new int[] { -4, 4, 2, -1, 11, 3, -2, -3, 6, 1, 12, 10, 9, 5, 7, 0, 13, 8 }));
            Args parms;

            parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, ReductGeneratorParamHelper.TrainData, ReductGeneratorParamHelper.PermutationCollection },
                             new object[] { ReductFactoryKeyHelper.Bireduct, localDataStore, permutations });

            BireductGenerator bireductGenerator = (BireductGenerator)ReductFactory.GetReductGenerator(parms);

            parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, ReductGeneratorParamHelper.TrainData, ReductGeneratorParamHelper.PermutationCollection },
                             new object[] { ReductFactoryKeyHelper.GammaBireduct, localDataStore, permutations });

            BireductGammaGenerator gammaGenerator = (BireductGammaGenerator)ReductFactory.GetReductGenerator(parms);

            foreach (Permutation perm in permutations)
            {
                IReduct r1 = bireductGenerator.CreateReduct(perm.ToArray(), 0.0, null);
                IReduct r2 = gammaGenerator.CreateReduct(perm.ToArray(), 0.0, null);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < perm.Length; i++)
                {
                    if (perm[i] < 0)
                    {
                        sb.Append(14 + (-perm[i])).Append(String.Format("({0})", localDataStore.DataStoreInfo.GetFieldInfo(-perm[i]).Alias)).Append(' ');
                    }
                    else
                    {
                        sb.Append(perm[i]).Append(' ');
                    }
                }

                //Console.WriteLine("{0} & {1} & {2}", sb.ToString(), r1.ToString(), r2.ToString());
            }
        }

        //Ad 3
        [Test]
        public void CheckIsBireduct()
        {
            /*
            \begin{table}[t]
            \normalsize
            \centering
            \begin{tabular}{|c|c|}
            \hline
            \textbf{\small{Decision Bireduct}} & \textbf{\small{$\gamma$-Decision Bireduct}} \\ \hline
            % O
            \begin{tabular}{c}
            (\{O\},\{1..5,7..8,10,12..13\})\\
            (\{O\},\{1..3,6..8,12..14\})\\
            (\{O\},\{3,6..7,9,11..14\})\\
            \end{tabular} & (\{O\},\{3,7,12..13\})\\ \hline
            % O,H
            (\{O,H\},\{1..3,6..9,11..14\}) & (\{O,H\},\{1..3,7..9,11..13\}) \\ \hline
            % O,H,W
            (\{O,H,W\},\{1..14\}) & (\{O,H,W\},\{1..14\})\\ \hline
            % O,T,W
            (\{O,T,W\},\{1..14\}) & (\{O,T,W\},\{1..14\})\\ \hline
            % T,W
            \begin{tabular}{c}
            (\{T,W\},\{1..2,4..5,7,9..10,14\})\\
            (\{T,W\},\{2..6,9..13\}) \\
            \end{tabular} & (\{T W\},\{2,5,9\})\\ \hline
            % T,H
            (\{T,H\},\{1..2,6,8,10..11,13..14\}) & (\{T,H\},\{10..11,13\})\\ \hline
            % O,W
            (\{O,W\},\{2..7,9..10,12..14\}) & (\{O,W\},\{3..7,10,12..14\})\\ \hline
            % O,T
            (\{O,T\},\{1..4,6..10,12..13\}) & (\{O,T\},\{1..3,7,9,12..13\})\\ \hline
            % H,W
            (\{H,W\},\{1,5..6,8..10,12..13\}) & (\{H,W\},\{5,9..10,13\})\\ \hline
            % W
            (\{W\},\{2..6,9..10,13..14\}) & (\{W\}, $\emptyset$) \\ \hline
            \end{tabular}
            \vspace{\baselineskip}
            \caption{Examples of decisionInternalValue bireducts and $\gamma$-decisionInternalValue bireducts. \label{play_golf_table_bireducts}}
            \end{table}
             */

            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            localDataStore.DataStoreInfo.GetFieldInfo(1).Alias = "O";
            localDataStore.DataStoreInfo.GetFieldInfo(2).Alias = "T";
            localDataStore.DataStoreInfo.GetFieldInfo(3).Alias = "H";
            localDataStore.DataStoreInfo.GetFieldInfo(4).Alias = "W";
            localDataStore.DataStoreInfo.DecisionInfo.Alias = "d";

            int[][] attributesBireducts = new int[][]
            {
                new int[] { 1 },
                new int[] { 1 },
                new int[] { 1 },
                new int[] { 1, 3 },
                new int[] { 1, 3, 4 },
                new int[] { 1, 2, 4 },
                new int[] { 2, 4 },
                new int[] { 2, 4 },
                new int[] { 2, 3 },
                new int[] { 1, 4 },
                new int[] { 1, 2 },
                new int[] { 3, 4 },
                new int[] { 4 }
            };

            int[][] attributesGamma = new int[][]
            {
                new int[] { 1 },
                new int[] { 1, 3 },
                new int[] { 1, 3, 4 },
                new int[] { 1, 2, 4 },
                new int[] { 2, 4 },
                new int[] { 2, 3 },
                new int[] { 1, 4 },
                new int[] { 1, 2 },
                new int[] { 3, 4 },
                new int[] { 4 }
            };

            int[][] objectsBireducts = new int[][]
            {
                new int[] { 0,1,2,3,4,6,7,9,11,12 },
                new int[] { 0,1,2,5,6,7,11,12,13 },
                new int[] { 2,5,6,8,10,11,12,13 },
                new int[] { 0,1,2,5,6,7,8,10,11,12,13 },
                new int[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13 },
                new int[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13 },
                new int[] { 0,1,3,4,6,8,9,13 },
                new int[] { 1,2,3,4,5,8,9,10,11,12 },
                new int[] { 0,1,5,7,9,10,12,13 },
                new int[] { 1,2,3,4,5,6,8,9,11,12,13 },
                new int[] { 0,1,2,3,5,6,7,8,9,11,12 },
                new int[] { 0,4,5,7,8,9,11,12 },
                new int[] { 1,2,3,4,5,8,9,12,13 }
            };

            int[][] objectsGamma = new int[][]
            {
                new int[] { 2,6,11,12 },
                new int[] { 0,1,2,6,7,8,10,11,12 },
                new int[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13 },
                new int[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13 },
                new int[] { 1,4,8 },
                new int[] { 9,10,12 },
                new int[] { 2,3,4,5,6,9,11,12,13 },
                new int[] { 0,1,2,6,8,11,12 },
                new int[] { 4,8,9,12 },
                new int[] { }
            };

            for (int i = 0; i < attributesBireducts.Length; i++)
            {
                Bireduct bireduct = new Bireduct(localDataStore, attributesBireducts[i], objectsBireducts[i], 0);
                EquivalenceClassCollection.CheckRegionPositive(bireduct.Attributes, localDataStore, bireduct.ObjectSet);

                for (int k = 1; k <= 4; k++)
                    Assert.IsFalse(bireduct.TryRemoveAttribute(k));

                for (int k = 0; k < 14; k++)
                    Assert.IsFalse(bireduct.TryAddObject(k));
            }

            for (int i = 0; i < attributesBireducts.Length; i++)
            {
                BireductGamma bireductGamma = new BireductGamma(localDataStore, attributesBireducts[i], objectsBireducts[i], 0);
                EquivalenceClassCollection.CheckRegionPositive(bireductGamma.Attributes, localDataStore, bireductGamma.ObjectSet);

                for (int k = 1; k <= 4; k++)
                    Assert.IsFalse(bireductGamma.TryRemoveAttribute(k));

                for (int k = 0; k < 14; k++)
                    Assert.IsFalse(bireductGamma.TryAddObject(k));
            }
        }

        //Ad 4
        [Test]
        public void PrintDecisionRulesForBireduct()
        {
            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            localDataStore.DataStoreInfo.GetFieldInfo(1).Alias = "O";
            localDataStore.DataStoreInfo.GetFieldInfo(2).Alias = "T";
            localDataStore.DataStoreInfo.GetFieldInfo(3).Alias = "H";
            localDataStore.DataStoreInfo.GetFieldInfo(4).Alias = "W";
            localDataStore.DataStoreInfo.DecisionInfo.Alias = "d";

            /*
            \scriptsize{O 8 W 1 4 7 2 14 10 12 9 T 6 3 13 5 11 H} & \scriptsize{(\{H\},$\{u_i: i \in \{1,2,5,7..11,13..14\}\}$)}\\ \hline
            \scriptsize{H 13 T 8 W 6 11 3 14 10 O 5 7 9 2 1 4 12} & \scriptsize{(\{O\},\{1..3 6..8 12..14\})}\\ \hline
            \scriptsize{3 8 T 1 W 11 9 O 14 12 6 4 7 H 10 13 2 5} & \scriptsize{(\{O H\},\{1..3 6..9 11..14\})}\\ \hline
            \scriptsize{2 13 5 14 11 7 12 4 3 1 9 6 8 10 H O W T} & \scriptsize{(\{O T W\},\{1..14\})}\\ \hline
            \scriptsize{9 4 12 14 1 8 7 3 10 13 6 11 2 5 W T H O} & \scriptsize{(\{O H W\},\{1..14\})}\\ \hline
            \scriptsize{11 O 2 H 1 10 5 7 9 8 3 13 T 6 14 12 4 W} & \scriptsize{(\{T\},\{1..2 4..5 7 9..12\})}\\ \hline
            \scriptsize{T 2 5 H 10 11 W 14 1 12 7 9 13 6 4 8 3 O} & \scriptsize{(\{O\},\{1..5,7..8,10,12..13\})}\\ \hline
            \scriptsize{W 6 H O 5 8 4 7 3 2 10 9 12 11 13 14 1 T} & \scriptsize{(\{T\},\{3 6 8 13..14\})}\\ \hline
            \scriptsize{O 2 3 13 1 H 4 T W 6 12 14 5 8 9 10 11 7} & \scriptsize{(\{W\},\{2..6 9..10 13..14\})}\\ \hline
            \scriptsize{O H 14 1 10 7 4 3 12 13 5 W 9 T 11 8 2 6} & \scriptsize{(\{T W\},\{1..2 4..5 7 9..10 14\})}\\ \hline
            \scriptsize{6 5 10 9 H O 12 T 8 W 4 2 13 3 7 1 14 11} & \scriptsize{(\{T W\},\{2..6 9..13\})}\\ \hline
            \scriptsize{11 14 9 13 3 7 8 2 5 1 12 W 6 4 10 H O T} & \scriptsize{(\{O H\},\{1..3 5 7..14\})}\\ \hline
            \scriptsize{13 8 6 H 7 W 9 T 5 3 4 12 O 2 10 14 11 1} & \scriptsize{(\{O T\},\{1..4 6..10 12..13\})}\\ \hline
            \scriptsize{9 H 2 4 6 13 14 7 T 11 10 O W 3 5 1 8 12} & \scriptsize{(\{O W\},\{2..7 9..10 12..14\})}\\ \hline
            \scriptsize{W 5 3 O 12 4 T H 7 2 13 11 10 6 8 1 14 9} & \scriptsize{(\{\},\{3..5 7 9..13\})}\\ \hline
            */

            PermutationCollection permutations = new PermutationCollection();
            permutations.Add(new Permutation(new int[] { -1, 7, -4, 0, 3, 6, 1, 13, 9, 11, 8, -2, 5, 2, 12, 4, 10, -3 }));
            permutations.Add(new Permutation(new int[] { -3, 12, -2, 7, -4, 5, 10, 2, 13, 9, -1, 4, 6, 8, 1, 0, 3, 11 }));
            permutations.Add(new Permutation(new int[] { 2, 7, -2, 0, -4, 10, 8, -1, 13, 11, 5, 3, 6, -3, 9, 12, 1, 4 }));
            permutations.Add(new Permutation(new int[] { 1, 12, 4, 13, 10, 6, 11, 3, 2, 0, 8, 4, 7, 9, -3, -1, -4, -2 }));
            permutations.Add(new Permutation(new int[] { 8, 3, 11, 13, 0, 7, 6, 2, 9, 12, 5, 10, 1, 4, -4, -2, -3, -1 }));
            permutations.Add(new Permutation(new int[] { 10, -1, 1, -3, 0, 9, 4, 6, 8, 7, 2, 12, -2, 5, 13, 11, 3, -4 }));
            permutations.Add(new Permutation(new int[] { -2, 1, 4, -3, 9, 10, -4, 13, 0, 11, 6, 8, 12, 5, 3, 7, 2, -1 }));
            permutations.Add(new Permutation(new int[] { -4, 5, -3, -1, 4, 8, 3, 6, 2, 1, 9, 8, 11, 10, 12, 13, 0, -2 }));
            permutations.Add(new Permutation(new int[] { -1, 1, 2, 12, 0, -3, 3, -2, -4, 5, 11, 13, 4, 7, 8, 9, 10, 6 }));
            permutations.Add(new Permutation(new int[] { -1, -3, 13, 0, 9, 6, 3, 2, 11, 12, 4, -4, 8, -2, 10, 7, 1, 5 }));
            permutations.Add(new Permutation(new int[] { 5, 4, 9, 8, -3, -1, 11, -2, 7, -4, 3, 1, 12, 2, 6, 0, 13, 10 }));
            permutations.Add(new Permutation(new int[] { 10, 13, 8, 12, 2, 6, 7, 1, 4, 0, 11, -4, 5, 3, 9, -3, -1, -2 }));
            permutations.Add(new Permutation(new int[] { 12, 7, 5, -3, 6, -4, 8, -2, 4, 2, 3, 11, -1, 1, 9, 13, 10, 0 }));
            permutations.Add(new Permutation(new int[] { 8, -3, 1, 3, 5, 12, 13, 6, -2, 10, 9, -1, -4, 2, 4, 0, 7, 11 }));
            permutations.Add(new Permutation(new int[] { -4, 4, 2, -1, 11, 3, -2, -3, 6, 1, 12, 10, 9, 5, 7, 0, 13, 8 }));

            Args parms;

            parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, ReductGeneratorParamHelper.TrainData, ReductGeneratorParamHelper.PermutationCollection },
                             new object[] { ReductFactoryKeyHelper.Bireduct, localDataStore, permutations });

            BireductGenerator bireductGenerator = (BireductGenerator)ReductFactory.GetReductGenerator(parms);

            parms = new Args(new string[] { ReductGeneratorParamHelper.FactoryKey, ReductGeneratorParamHelper.TrainData, ReductGeneratorParamHelper.PermutationCollection },
                             new object[] { ReductFactoryKeyHelper.GammaBireduct, localDataStore, permutations });

            BireductGammaGenerator gammaGenerator = (BireductGammaGenerator)ReductFactory.GetReductGenerator(parms);

            foreach (Permutation perm in permutations)
            {
                IReduct r1 = bireductGenerator.CreateReduct(perm.ToArray(), 0.0, null);
                IReduct r2 = gammaGenerator.CreateReduct(perm.ToArray(), 0.0, null);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < perm.Length; i++)
                {
                    if (perm[i] < 0)
                    {
                        sb.Append(14 + (-perm[i])).Append(String.Format("({0})", localDataStore.DataStoreInfo.GetFieldInfo(-perm[i]).Alias)).Append(' ');
                    }
                    else
                    {
                        sb.Append(perm[i]).Append(' ');
                    }
                }

                /*
                Console.WriteLine("{0} & {1} & {2}", sb.ToString(), r1.ToString(), r2.ToString());

                Console.WriteLine("Bireduct rules");
                foreach (EquivalenceClass eq in r1.EquivalenceClasses)
                {
                    Console.WriteLine(String.Format("{0} => {1}={2}",
                                    eq.Instance.ToString2(localDataStore.DataStoreInfo),
                                    localDataStore.DataStoreInfo.GetDecisionFieldInfo().Name,
                                    localDataStore.DataStoreInfo.GetDecisionFieldInfo().Internal2External(eq.MajorDecision)));
                }

                Console.WriteLine();

                Console.WriteLine("Gamma Bireduct rules");
                foreach (EquivalenceClass eq in r2.EquivalenceClasses)
                {
                    Console.WriteLine(String.Format("{0} => {1}={2}",
                                    eq.Instance.ToString2(localDataStore.DataStoreInfo),
                                    localDataStore.DataStoreInfo.GetDecisionFieldInfo().Name,
                                    localDataStore.DataStoreInfo.GetDecisionFieldInfo().Internal2External(eq.MajorDecision)));
                }

                Console.WriteLine();
                */
            }
        }

        private void Classify(string reductGeneratorKey)
        {
            string trainFileName = @"Data\dna.train";
            string testFileName = @"Data\dna.train";

            var dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            var dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            var dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;

            Args args = new Args(4);
            args.SetParameter<DataStore>(ReductGeneratorParamHelper.TrainData, dataStoreTrain);
            args.SetParameter<double>(ReductGeneratorParamHelper.Epsilon, 0.5);
            args.SetParameter<string>(ReductGeneratorParamHelper.FactoryKey, reductGeneratorKey);
            args.SetParameter<PermutationCollection>(ReductGeneratorParamHelper.PermutationCollection, ReductFactory.GetPermutationGenerator(args).Generate(10));

            IReductGenerator reductGenerator = ReductFactory.GetReductGenerator(args);
            reductGenerator.Run();

            RoughClassifier classifier = new RoughClassifier(
                reductGenerator.GetReductStoreCollection(),
                RuleQuality.Confidence,
                RuleQuality.SingleVote,
                dataStoreTrain.DataStoreInfo.GetDecisionValues());
            ClassificationResult classificationResult = classifier.Classify(dataStoreTest, null);

            Assert.AreEqual(dataStoreTest.NumberOfRecords, classificationResult.Classified
                                                            + classificationResult.Misclassified
                                                            + classificationResult.Unclassified);
        }
    }
}