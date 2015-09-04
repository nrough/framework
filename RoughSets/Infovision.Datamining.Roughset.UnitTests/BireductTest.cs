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
        DataStore dataStoreTrain = null;
        DataStore dataStoreTest = null;

        DataStoreInfo dataStoreTrainInfo = null;

        public BireductTest()
        {
            string trainFileName = @"Data\playgolf.train";
            string testFileName = @"Data\playgolf.train";

            dataStoreTrain = DataStore.Load(trainFileName, FileFormat.Rses1);
            dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTrain.DataStoreInfo);

            dataStoreTrainInfo = dataStoreTrain.DataStoreInfo;         
        }

        [Test]
        public void CalcBiReductPositive()
        {
            this.CheckBireductUnique("GammaBireduct");
        }

        [Test]
        public void CalcBiReductMajority()
        {
            this.CheckBireductUnique("Bireduct");
        }

        [Test]
        public void CalcBiReductRelative()
        {
            this.CheckBireductUnique("BireductRelative");
        }

        private void CheckBireductUnique(string reductGeneratorKey)
        {
            Args parms = new Args(new string[] { "FactoryKey", "DataStore", "NumberOfPermutations" },
                                  new Object[] { reductGeneratorKey, dataStoreTrain, 100 });
            
            IReductGenerator bireductGenerator = ReductFactory.GetReductGenerator(parms);
            bireductGenerator.Generate();
            IReductStore reductStore = bireductGenerator.ReductPool;

            foreach (IReduct reduct in reductStore)
            {
                foreach (EquivalenceClass reductStat in reduct.EquivalenceClassMap)
                {
                    Assert.AreEqual(1, reductStat.NumberOfDecisions);
                }
            }
        }

        [Test]
        public void RelativeMeasureTest()
        {
            Dictionary<int, double> elementWeights = new Dictionary<int, double>(dataStoreTrain.NumberOfRecords);
            double sumWeights = 0;

            int j = dataStoreTrain.DataStoreInfo.NumberOfFields - 1;

            foreach (int objectIdx in dataStoreTrain.GetObjectIndexes())
            {
                double p = 1 / dataStoreTrain.DataStoreInfo.PriorDecisionProbability(dataStoreTrain.GetDecisionValue(objectIdx));
                
                elementWeights[objectIdx] = p;
                sumWeights += p;
            }

            InformationMeasureRelative roughMeasure = new InformationMeasureRelative();
            Reduct reduct = new Reduct(dataStoreTrain, dataStoreTrainInfo.GetFieldIds(FieldTypes.Standard), 0);

            double r = roughMeasure.Calc(reduct);
            double u = sumWeights / dataStoreTrainInfo.NumberOfRecords;

            //Assert.AreEqual(r, u);
            Assert.GreaterOrEqual(r, u - (0.00001 / dataStoreTrain.DataStoreInfo.NumberOfRecords));
            Assert.LessOrEqual(r, u + (0.00001 / dataStoreTrain.DataStoreInfo.NumberOfRecords));            

        }

        [Test]
        public void BireductMajorityClassifierTest()
        {
            this.Classify("Bireduct");
        }

        [Test]
        public void BireductPositiveClassifierTest()
        {
            this.Classify("GammaBireduct");
        }

        [Test]
        public void BireductRelativeClassifierTest()
        {
            this.Classify("BireductRelative");
        }

        //Ad 1
        [Test]
        public void PrintDecisionRulesTest()
        {
            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);

            localDataStore.DataStoreInfo.GetFieldInfo(1).NameAlias = "O";
            localDataStore.DataStoreInfo.GetFieldInfo(2).NameAlias = "T";
            localDataStore.DataStoreInfo.GetFieldInfo(3).NameAlias = "H";
            localDataStore.DataStoreInfo.GetFieldInfo(4).NameAlias = "W";
            localDataStore.DataStoreInfo.GetDecisionFieldInfo().NameAlias = "d";


            RoughClassifier roughClassifier = new RoughClassifier();
            
            RoughClassifier classifier = new RoughClassifier();
            classifier.Train(localDataStore, "ApproximateReductMajority", 0, 10);
            Console.Write(classifier.PrintDecisionRules(localDataStore.DataStoreInfo));
        }


        //Ad 2
        [Test]
        public void TestBireductGolf_2()
        {
            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            localDataStore.DataStoreInfo.GetFieldInfo(1).NameAlias = "O";
            localDataStore.DataStoreInfo.GetFieldInfo(2).NameAlias = "T";
            localDataStore.DataStoreInfo.GetFieldInfo(3).NameAlias = "H";
            localDataStore.DataStoreInfo.GetFieldInfo(4).NameAlias = "W";
            localDataStore.DataStoreInfo.GetDecisionFieldInfo().NameAlias = "d";
            
            
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
            permutations.Add(new Permutation(new int[] { -1, 8, -4, 1, 4, 7, 2, 14, 10, 12, 9, -2, 6, 3, 13, 5, 11, -3 }));
            permutations.Add(new Permutation(new int[] { -3, 13, -2, 8, -4, 6, 11, 3, 14, 10, -1, 5, 7, 9, 2, 1, 4, 12 }));
            permutations.Add(new Permutation(new int[] { 3, 8, -2, 1, -4, 11, 9, -1, 14, 12, 6, 4, 7, -3, 10, 13, 2, 5 }));
            permutations.Add(new Permutation(new int[] { 2, 13, 5, 14, 11, 7, 12, 4, 3, 1, 9, 6, 8, 10, -3, -1, -4, -2 }));
            permutations.Add(new Permutation(new int[] { 9, 4, 12, 14, 1, 8, 7, 3, 10, 13, 6, 11, 2, 5, -4, -2, -3, -1 }));
            permutations.Add(new Permutation(new int[] { 11, -1, 2, -3, 1, 10, 5, 7, 9, 8, 3, 13, -2, 6, 14, 12, 4, -4 }));
            permutations.Add(new Permutation(new int[] { -2, 2, 5, -3, 10, 11, -4, 14, 1, 12, 7, 9, 13, 6, 4, 8, 3, -1 }));
            permutations.Add(new Permutation(new int[] { -4, 6, -3, -1, 5, 8, 4, 7, 3, 2, 10, 9, 12, 11, 13, 14, 1, -2 }));
            permutations.Add(new Permutation(new int[] { -1, 2, 3, 13, 1, -3, 4, -2, -4, 6, 12, 14, 5, 8, 9, 10, 11, 7 }));
            permutations.Add(new Permutation(new int[] { -1, -3, 14, 1, 10, 7, 4, 3, 12, 13, 5, -4, 9, -2, 11, 8, 2, 6 }));
            permutations.Add(new Permutation(new int[] { 6, 5, 10, 9, -3, -1, 12, -2, 8, -4, 4, 2, 13, 3, 7, 1, 14, 11 }));
            permutations.Add(new Permutation(new int[] { 11, 14, 9, 13, 3, 7, 8, 2, 5, 1, 12, -4, 6, 4, 10, -3, -1, -2 }));
            permutations.Add(new Permutation(new int[] { 13, 8, 6, -3, 7, -4, 9, -2, 5, 3, 4, 12, -1, 2, 10, 14, 11, 1 }));
            permutations.Add(new Permutation(new int[] { 9, -3, 2, 4, 6, 13, 14, 7, -2, 11, 10, -1, -4, 3, 5, 1, 8, 12 }));
            permutations.Add(new Permutation(new int[] { -4, 5, 3, -1, 12, 4, -2, -3, 7, 2, 13, 11, 10, 6, 8, 1, 14, 9 }));

            Args parms;            

            parms = new Args(new string[] { "FactoryKey", "DataStore", "PermutationCollection" },
                             new object[] { "Bireduct", localDataStore, permutations });

            BireductGenerator bireductGenerator = (BireductGenerator) ReductFactory.GetReductGenerator(parms);

            parms = new Args(new string[] { "FactoryKey", "DataStore", "PermutationCollection" },
                             new object[] { "GammaBireduct", localDataStore, permutations });

            BireductGammaGenerator gammaGenerator = (BireductGammaGenerator)ReductFactory.GetReductGenerator(parms);

            foreach (Permutation perm in permutations)
            {
                IReduct r1 = bireductGenerator.CreateReduct(perm);
                IReduct r2 = gammaGenerator.CreateReduct(perm);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < perm.Length; i++ )
                {
                    if (perm[i] < 0)
                    {
                        sb.Append(14 + (-perm[i])).Append(String.Format("({0})", localDataStore.DataStoreInfo.GetFieldInfo(-perm[i]).NameAlias)).Append(' ');
                    }
                    else
                    {
                        sb.Append(perm[i]).Append(' ');
                    }
                }

                Console.WriteLine("{0} & {1} & {2}", sb.ToString(), r1.ToString(), r2.ToString());
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
            \caption{Examples of decision bireducts and $\gamma$-decision bireducts. \label{play_golf_table_bireducts}}
            \end{table}
             */
            
            DataStore localDataStore = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            localDataStore.DataStoreInfo.GetFieldInfo(1).NameAlias = "O";
            localDataStore.DataStoreInfo.GetFieldInfo(2).NameAlias = "T";
            localDataStore.DataStoreInfo.GetFieldInfo(3).NameAlias = "H";
            localDataStore.DataStoreInfo.GetFieldInfo(4).NameAlias = "W";
            localDataStore.DataStoreInfo.GetDecisionFieldInfo().NameAlias = "d";

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
                EquivalenceClassMap.CheckRegionPositive(bireduct.Attributes, localDataStore, bireduct.ObjectSet);
                
                for (int k = 1; k <= 4; k++)
                    Assert.IsFalse(bireduct.TryRemoveAttribute(k));

                for(int k=0; k<14; k++)
                    Assert.IsFalse(bireduct.TryAddObject(k));

            }


            for (int i = 0; i < attributesBireducts.Length; i++)
            {
                BireductGamma bireductGamma = new BireductGamma(localDataStore, attributesBireducts[i], objectsBireducts[i], 0);
                EquivalenceClassMap.CheckRegionPositive(bireductGamma.Attributes, localDataStore, bireductGamma.ObjectSet);

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
            localDataStore.DataStoreInfo.GetFieldInfo(1).NameAlias = "O";
            localDataStore.DataStoreInfo.GetFieldInfo(2).NameAlias = "T";
            localDataStore.DataStoreInfo.GetFieldInfo(3).NameAlias = "H";
            localDataStore.DataStoreInfo.GetFieldInfo(4).NameAlias = "W";
            localDataStore.DataStoreInfo.GetDecisionFieldInfo().NameAlias = "d";


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
            permutations.Add(new Permutation(new int[] { -1, 8, -4, 1, 4, 7, 2, 14, 10, 12, 9, -2, 6, 3, 13, 5, 11, -3 }));
            permutations.Add(new Permutation(new int[] { -3, 13, -2, 8, -4, 6, 11, 3, 14, 10, -1, 5, 7, 9, 2, 1, 4, 12 }));
            permutations.Add(new Permutation(new int[] { 3, 8, -2, 1, -4, 11, 9, -1, 14, 12, 6, 4, 7, -3, 10, 13, 2, 5 }));
            permutations.Add(new Permutation(new int[] { 2, 13, 5, 14, 11, 7, 12, 4, 3, 1, 9, 6, 8, 10, -3, -1, -4, -2 }));
            permutations.Add(new Permutation(new int[] { 9, 4, 12, 14, 1, 8, 7, 3, 10, 13, 6, 11, 2, 5, -4, -2, -3, -1 }));
            permutations.Add(new Permutation(new int[] { 11, -1, 2, -3, 1, 10, 5, 7, 9, 8, 3, 13, -2, 6, 14, 12, 4, -4 }));
            permutations.Add(new Permutation(new int[] { -2, 2, 5, -3, 10, 11, -4, 14, 1, 12, 7, 9, 13, 6, 4, 8, 3, -1 }));
            permutations.Add(new Permutation(new int[] { -4, 6, -3, -1, 5, 8, 4, 7, 3, 2, 10, 9, 12, 11, 13, 14, 1, -2 }));
            permutations.Add(new Permutation(new int[] { -1, 2, 3, 13, 1, -3, 4, -2, -4, 6, 12, 14, 5, 8, 9, 10, 11, 7 }));
            permutations.Add(new Permutation(new int[] { -1, -3, 14, 1, 10, 7, 4, 3, 12, 13, 5, -4, 9, -2, 11, 8, 2, 6 }));
            permutations.Add(new Permutation(new int[] { 6, 5, 10, 9, -3, -1, 12, -2, 8, -4, 4, 2, 13, 3, 7, 1, 14, 11 }));
            permutations.Add(new Permutation(new int[] { 11, 14, 9, 13, 3, 7, 8, 2, 5, 1, 12, -4, 6, 4, 10, -3, -1, -2 }));
            permutations.Add(new Permutation(new int[] { 13, 8, 6, -3, 7, -4, 9, -2, 5, 3, 4, 12, -1, 2, 10, 14, 11, 1 }));
            permutations.Add(new Permutation(new int[] { 9, -3, 2, 4, 6, 13, 14, 7, -2, 11, 10, -1, -4, 3, 5, 1, 8, 12 }));
            permutations.Add(new Permutation(new int[] { -4, 5, 3, -1, 12, 4, -2, -3, 7, 2, 13, 11, 10, 6, 8, 1, 14, 9 }));

            Args parms;

            parms = new Args(new string[] { "FactoryKey", "DataStore", "PermutationCollection" },
                             new object[] { "Bireduct", localDataStore, permutations });

            BireductGenerator bireductGenerator = (BireductGenerator)ReductFactory.GetReductGenerator(parms);

            parms = new Args(new string[] { "FactoryKey", "DataStore", "PermutationCollection" },
                             new object[] { "GammaBireduct", localDataStore, permutations });

            BireductGammaGenerator gammaGenerator = (BireductGammaGenerator)ReductFactory.GetReductGenerator(parms);

            foreach (Permutation perm in permutations)
            {
                IReduct r1 = bireductGenerator.CreateReduct(perm);
                IReduct r2 = gammaGenerator.CreateReduct(perm);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < perm.Length; i++)
                {
                    if (perm[i] < 0)
                    {
                        sb.Append(14 + (-perm[i])).Append(String.Format("({0})", localDataStore.DataStoreInfo.GetFieldInfo(-perm[i]).NameAlias)).Append(' ');
                    }
                    else
                    {
                        sb.Append(perm[i]).Append(' ');
                    }
                }

                Console.WriteLine("{0} & {1} & {2}", sb.ToString(), r1.ToString(), r2.ToString());

                Console.WriteLine("Bireduct rules");
                foreach (EquivalenceClass eq in r1.EquivalenceClassMap)
                {
                    Console.WriteLine(String.Format("{0} => {1}={2}",
                                    eq.Instance.ToString2(localDataStore.DataStoreInfo),
                                    localDataStore.DataStoreInfo.GetDecisionFieldInfo().NameAlias,
                                    localDataStore.DataStoreInfo.GetDecisionFieldInfo().Internal2External(eq.MajorDecision)));
                }

                Console.WriteLine();

                Console.WriteLine("Gamma Bireduct rules");
                foreach (EquivalenceClass eq in r2.EquivalenceClassMap)
                {
                    Console.WriteLine(String.Format("{0} => {1}={2}",
                                    eq.Instance.ToString2(localDataStore.DataStoreInfo),
                                    localDataStore.DataStoreInfo.GetDecisionFieldInfo().NameAlias,
                                    localDataStore.DataStoreInfo.GetDecisionFieldInfo().Internal2External(eq.MajorDecision)));
                }

                Console.WriteLine();
            }
        }

        private void Classify(string reductGeneratorKey)
        {
            RoughClassifier classifier = new RoughClassifier();
            classifier.Train(dataStoreTrain, reductGeneratorKey, 50, 10);
            classifier.Classify(dataStoreTest);
            
            ClassificationResult classificationResult = classifier.Vote(dataStoreTest,
                                                                        IdentificationType.Confidence,
                                                                        VoteType.MajorDecision);

            Assert.AreEqual(dataStoreTest.NumberOfRecords, classificationResult.NumberOfClassified
                                                            + classificationResult.NumberOfMisclassified
                                                            + classificationResult.NumberOfUnclassifed);
        }
    }
}
