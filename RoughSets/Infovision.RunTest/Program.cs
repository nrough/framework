using System;
using System.IO;
using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Test;

namespace Infovision.RunTest
{
    public class Program
    {   
        public ParameterList RoughMeasureWeightsTestParmList(string trainFileName,
                                        string testFileName,
                                        int numberOfReducts,
                                        int numberOfPermutations,
                                        int numberOfTests,
                                        int numberOfFolds)
        {
            DataStore dataStoreTraining = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTraining.DataStoreInfo);

            ITestParameter parmDataStoreTraining = new ParameterObjectReference<DataStore>("DataStoreTraining", dataStoreTraining);
            ITestParameter parmDataStoreTest = new ParameterObjectReference<DataStore>("DataStoreTest", dataStoreTest);
            ITestParameter parmNumberOfReducts = new ParameterValueList<int>("NumberOfReducts", numberOfReducts);
            ITestParameter parmNumberOfPermutations = new ParameterValue<int>("NumberOfPermutations", numberOfPermutations);
            ITestParameter parmNFold = new ParameterValueList<int>("NumberOfFolds", numberOfFolds);
            ITestParameter parmFoldNumber = new ParameterNumericRange<int>("FoldNumber", 0, 0, 1);

            if (numberOfFolds > 1)
            {
                parmFoldNumber = new ParameterNumericRange<int>("FoldNumber", 1, numberOfFolds, 1);
            }

            ITestParameter parmTestNumber = new ParameterNumericRange<int>("NumberOfTests", 1, numberOfTests, 1);
            ITestParameter parmEpsilon = new ParameterNumericRange<int>("ApproximationDegree", 0, 99, 1);

            ITestParameter parmReductType = new ParameterValueList<string>("ReductType",
                                                                            new string[] { 
                                                                                "ApproximateReductRelativeWeights",
                                                                                "ApproximateReductRelative",
                                                                                "ApproximateReductMajorityWeights",            
                                                                                "ApproximateReductMajority" });

            ITestParameter parmReductMeasure = new ParameterValueList<string>("ReductMeasure",
                                                                              new string[] { "ReductMeasureLength" });

            ITestParameter parmIdentification = new ParameterValueList<IdentificationType>("IdentificationType", new IdentificationType[] { IdentificationType.Confidence,
                                                                                                                                            IdentificationType.Coverage,
                                                                                                                                            IdentificationType.WeightConfidence,
                                                                                                                                            IdentificationType.WeightCoverage});
            ITestParameter parmVote = new ParameterValueList<VoteType>("VoteType", new VoteType[] { VoteType.Confidence,
                                                                                                    VoteType.Support,
                                                                                                    VoteType.Ratio,
                                                                                                    VoteType.Coverage,                                                                                                                                                                                                        
                                                                                                    VoteType.Strength,
                                                                                                    VoteType.WeightConfidence,
                                                                                                    VoteType.WeightSupport, 
                                                                                                    VoteType.WeightRatio,
                                                                                                    VoteType.WeightCoverage,                                                                                                    
                                                                                                    VoteType.MajorDecision,
                                                                                                    VoteType.WeightStrength});
 
            ParameterList parameterList = new ParameterList(new ITestParameter[] { parmDataStoreTraining,//0
                                                                                   parmDataStoreTest,//1
                                                                                   parmNumberOfReducts,//2
                                                                                   parmNumberOfPermutations,//3
                                                                                   parmNFold,//4
                                                                                   parmFoldNumber,//5
                                                                                   parmTestNumber,//6
                                                                                   parmReductType,//7
                                                                                   parmEpsilon,//8
                                                                                   parmReductMeasure,//9
                                                                                   parmIdentification,//10
                                                                                   parmVote});//11
            
            
            /*
            ParameterList parameterList = new ParameterList(new ITestParameter[] { parmDataStoreTraining,//0
                                                                                   parmDataStoreTest,//1
                                                                                   parmNumberOfReducts,//2
                                                                                   parmNumberOfPermutations,//3
                                                                                   parmNFold,//4

                                                                                   parmTestNumber,//5
                                                                                   parmFoldNumber,//6
                                                                                   
                                                                                   parmReductType,//7
                                                                                   parmEpsilon,//8
                                                                                   parmReductMeasure,//9
                                                                                   parmIdentification,//10
                                                                                   parmVote});//11
            */

            /*
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.WeightConfidence, "ApproximateReductMajority" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.WeightConfidence, "ApproximateReductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.WeightCoverage, "ApproximateReductMajority" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.WeightCoverage, "ApproximateReductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.WeightSupport, "ApproximateReductMajority" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.WeightSupport, "ApproximateReductRelative" }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.WeightSupport, "ApproximateReductMajority" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.WeightSupport, "ApproximateReductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.WeightConfidence, "ApproximateReductMajority" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.WeightConfidence, "ApproximateReductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.WeightCoverage, "ApproximateReductMajority" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.WeightCoverage, "ApproximateReductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.WeightRatio, "ApproximateReductMajority" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.WeightRatio, "ApproximateReductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.WeightStrenght, "ApproximateReductMajority" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.WeightStrenght, "ApproximateReductRelative" }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightConfidence, IdentificationType.Confidence }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightConfidence, IdentificationType.Coverage }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightConfidence, IdentificationType.Support }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightCoverage, IdentificationType.Confidence }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightCoverage, IdentificationType.Coverage }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightCoverage, IdentificationType.Support }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightRatio, IdentificationType.Confidence }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightRatio, IdentificationType.Coverage }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightRatio, IdentificationType.Support }));            

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightStrenght, IdentificationType.Confidence}));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightStrenght, IdentificationType.Coverage }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightStrenght, IdentificationType.Support }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightSupport, IdentificationType.Confidence }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightSupport, IdentificationType.Coverage }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightSupport, IdentificationType.Support }));  

 
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Confidence, IdentificationType.WeightConfidence }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Confidence, IdentificationType.WeightCoverage }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Confidence, IdentificationType.WeightSupport }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Coverage, IdentificationType.WeightConfidence }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Coverage, IdentificationType.WeightCoverage }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Coverage, IdentificationType.WeightSupport }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Ratio, IdentificationType.WeightConfidence }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Ratio, IdentificationType.WeightCoverage }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Ratio, IdentificationType.WeightSupport }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Strenght, IdentificationType.WeightConfidence }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Strenght, IdentificationType.WeightCoverage }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Strenght, IdentificationType.WeightSupport }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Support, IdentificationType.WeightConfidence }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.Support, IdentificationType.WeightCoverage }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "IdentificationType" }, new object[] { VoteType.WeightSupport, IdentificationType.WeightSupport }));
            */
            
            
            return parameterList;
        }
        
        
        public ParameterList RoughMeasureTestParmList(string trainFileName,
                                        string testFileName, 
                                        int numberOfReducts,
                                        int numberOfPermutations,
                                        int numberOfTests,
                                        int numberOfFolds)
        {
            DataStore dataStoreTraining = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTraining.DataStoreInfo);

            ITestParameter parmDataStoreTraining = new ParameterObjectReference<DataStore>("DataStoreTraining", dataStoreTraining);
            ITestParameter parmDataStoreTest = new ParameterObjectReference<DataStore>("DataStoreTest", dataStoreTest);
            ITestParameter parmNumberOfReducts = new ParameterValueList<int>("NumberOfReducts", numberOfReducts);
            ITestParameter parmNumberOfPermutations = new ParameterValue<int>("NumberOfPermutations", numberOfPermutations);
            ITestParameter parmNFold = new ParameterValueList<int>("NumberOfFolds", numberOfFolds);
            ITestParameter parmFoldNumber = new ParameterNumericRange<int>("FoldNumber", 0, 0, 1);
            
            if (numberOfFolds > 1)
            {
                parmFoldNumber = new ParameterNumericRange<int>("FoldNumber", 1, numberOfFolds, 1);
            }

            ITestParameter parmTestNumber = new ParameterNumericRange<int>("NumberOfTests", 1, numberOfTests, 1);
            ITestParameter parmEpsilon = new ParameterNumericRange<int>("ApproximationDegree", 0, 99, 1);
            
            ITestParameter parmReductType = new ParameterValueList<string>("ReductType", 
                                                                            new string[] { "ApproximateReductPositive", 
                                                                                           "ApproximateReductMajority", 
                                                                                           "ApproximateReductRelative", 
                                                                                           "GammaBireduct", 
                                                                                           "Bireduct", 
                                                                                           "BireductRelative" }
                                                                          );

            ITestParameter parmReductMeasure = new ParameterValueList<string>("ReductMeasure", 
                                                                              new string[] { "ReductMeasureLength", 
                                                                                             "ReductMeasureNumberOfPartitions", 
                                                                                             "BireductMeasureMajority", 
                                                                                             "BireductMeasureRelative"});

            ITestParameter parmIdentification = new ParameterValueList<IdentificationType>("IdentificationType", Utils.EnumHelper.GetValues<IdentificationType>());
            ITestParameter parmVote = new ParameterValueList<VoteType>("VoteType", Utils.EnumHelper.GetValues<VoteType>());
            
            /*
            ParameterList parameterList = new ParameterList(new ITestParameter[] { parmDataStoreTraining,//0
                                                                                   parmDataStoreTest,//1
                                                                                   parmNumberOfReducts,//2
                                                                                   parmNumberOfPermutations,//3
                                                                                   parmNFold,//4
                                                                                   parmFoldNumber,//5
                                                                                   parmTestNumber,//6
                                                                                   parmReductType,//7
                                                                                   parmEpsilon,//8
                                                                                   parmReductMeasure,//9
                                                                                   parmIdentification,//10
                                                                                   parmVote});//11
            */

            ParameterList parameterList = new ParameterList(new ITestParameter[] { parmDataStoreTraining,//0
                                                                                   parmDataStoreTest,//1
                                                                                   parmNumberOfReducts,//2
                                                                                   parmNumberOfPermutations,//3
                                                                                   parmNFold,//4
                                                                                   
                                                                                   parmTestNumber,//5
                                                                                   parmFoldNumber,//6
                                                                                                                                                                      
                                                                                   parmReductType,//7
                                                                                   parmEpsilon,//8
                                                                                   parmReductMeasure,//9
                                                                                   parmIdentification,//10
                                                                                   parmVote});//11

            //parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure" }, new Object[] { "Bireduct" }));
            //parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure" }, new Object[] { "BireductRelative" }));
            //parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure" }, new Object[] { "GammaBireduct" }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "BireductMeasureMajority", "ApproximateReductPositive" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "BireductMeasureMajority", "ApproximateReductMajority" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "BireductMeasureMajority", "ApproximateReductRelative" }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "BireductMeasureRelative", "ApproximateReductPositive" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "BireductMeasureRelative", "ApproximateReductMajority" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "BireductMeasureRelative", "ApproximateReductRelative" }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "ReductMeasureLength", "Bireduct" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "ReductMeasureLength", "BireductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "ReductMeasureLength", "GammaBireduct" }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "ReductMeasureNumberOfPartitions", "Bireduct" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "ReductMeasureNumberOfPartitions", "BireductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", "ReductType" }, new object[] { "ReductMeasureNumberOfPartitions", "GammaBireduct" }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.Confidence, "Bireduct" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.Confidence, "BireductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.Confidence, "GammaBireduct" }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.Confidence, "Bireduct" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.Confidence, "BireductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "IdentificationType", "ReductType" }, new object[] { IdentificationType.Confidence, "GammaBireduct" }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.Support, "Bireduct" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.Support, "BireductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.Support, "GammaBireduct" }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.Confidence, "Bireduct" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.Confidence, "BireductRelative" }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "VoteType", "ReductType" }, new object[] { VoteType.Confidence, "GammaBireduct" }));

            return parameterList;
        }

        public ParameterList RelativeTestParmList(string trainFileName,
                                        string testFileName,
                                        int numberOfReducts,
                                        int numberOfPermutations,
                                        int numberOfTests,
                                        int numberOfFolds)
        {
            DataStore dataStoreTraining = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTraining.DataStoreInfo);

            ITestParameter parmDataStoreTraining = new ParameterObjectReference<DataStore>("DataStoreTraining", dataStoreTraining);
            ITestParameter parmDataStoreTest = new ParameterObjectReference<DataStore>("DataStoreTest", dataStoreTest);
            ITestParameter parmNumberOfReducts = new ParameterValueList<Int32>("NumberOfReducts", numberOfReducts);
            ITestParameter parmNumberOfPermutations = new ParameterValue<Int32>("NumberOfPermutations", numberOfPermutations);
            ITestParameter parmNFold = new ParameterValueList<Int32>("NumberOfFolds", numberOfFolds);
            ITestParameter parmFoldNumber = new ParameterNumericRange<Int32>("FoldNumber", 0, 0, 1);

            if (numberOfFolds > 1)
            {
                parmFoldNumber = new ParameterNumericRange<Int32>("FoldNumber", 1, numberOfFolds, 1);
            }

            ITestParameter parmTestNumber = new ParameterNumericRange<Int32>("NumberOfTests", 1, numberOfTests, 1);
            ITestParameter parmEpsilon = new ParameterNumericRange<Int32>("ApproximationDegree", 0, 99, 1);

            ITestParameter parmReductType = new ParameterValueList<String>("ReductType",
                                                                            new string[] { "ApproximateReductMajorityWeights",
                                                                                           "ApproximateReductRelativeWeights",
                                                                                           "ApproximateReductRelative", 
                                                                                           "ApproximateReductMajority"
                                                                                           });

            ITestParameter parmReductMeasure = new ParameterValueList<String>("ReductMeasure",
                                                                              new string[] { "ReductMeasureLength" });

            ITestParameter parmIdentification = new ParameterValueList<IdentificationType>("IdentificationType",
                                                                                           new IdentificationType[] { IdentificationType.Confidence, 
                                                                                                                      IdentificationType.Coverage});

            ITestParameter parmVote = new ParameterValueList<VoteType>("VoteType", 
                                                                       new VoteType[] { VoteType.Confidence,
                                                                                        VoteType.ConfidenceRelative,
                                                                                        VoteType.Coverage });

            /*
            ParameterList parameterList = new ParameterList(new ITestParameter[] { parmDataStoreTraining,       // 0
                                                                                   parmDataStoreTest,           // 1
                                                                                   parmNumberOfReducts,         // 2
                                                                                   parmNumberOfPermutations,    // 3
                                                                                   parmNFold,                   // 4
                                                                                   parmFoldNumber,              // 5
                                                                                   parmTestNumber,              // 6
                                                                                   parmReductType,              // 7
                                                                                   parmEpsilon,                 // 8
                                                                                   parmReductMeasure,           // 9
                                                                                   parmIdentification,          //10
                                                                                   parmVote});                  //11
            */

            ParameterList parameterList = new ParameterList(new ITestParameter[] { parmDataStoreTraining,       // 0
                                                                                   parmDataStoreTest,           // 1
                                                                                   parmNumberOfReducts,         // 2
                                                                                   parmNumberOfPermutations,    // 3
                                                                                   parmNFold,                   // 4

                                                                                   parmTestNumber,              // 5
                                                                                   parmFoldNumber,              // 6                                                                                   
                                                                                   
                                                                                   parmReductType,              // 7
                                                                                   parmEpsilon,                 // 8
                                                                                   parmReductMeasure,           // 9
                                                                                   parmIdentification,          //10
                                                                                   parmVote});                  //11
            
            return parameterList;
        }

        /// <summary>
        /// Print data from store in internal encoding
        /// </summary>
        private void ShowInternalData(DataStore dataStore)
        {
            Console.WriteLine(String.Format("Dataset: {0}", dataStore.Name));
            Console.WriteLine(dataStore.ToStringInternal());
        }

        /// <summary>
        /// Print data from strore
        /// </summary>
        private void ShowData(DataStore dataStore)
        {
            Console.WriteLine(String.Format("Dataset: {0}", dataStore.Name));
            Console.WriteLine(dataStore.ToStringExternal());
        }

        private void ShowAttributeInfo(DataStoreInfo dataStoreInfo)
        {
            Console.Write(dataStoreInfo.ToStringInfo());            
        }

        /// <summary>
        /// Stop program execution and wait for user key press
        /// </summary>
        private void Pause()
        {
            Console.WriteLine(String.Format("Press any key to continue..."));
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            Program program = new Program();

            if (args.Length != 7)
            {
                Console.WriteLine("Error in parameter list");
                Console.WriteLine("Usage: <ExecutableFileName> <TrainFilename> <TestFilename> <NumberOfReducts> <NumberOfPermutations> <NumberOdTests> <NumberOfFolds> <ResultFilename>");
            }

            string trainFilename        = args[0];
            string testFilename         = args[1];
            int numberOfReducts         = Int32.Parse(args[2]);
            int numberOfPermutations    = Int32.Parse(args[3]);
            int numberOfTests           = Int32.Parse(args[4]);
            int numberOfFolds           = Int32.Parse(args[5]);
            string resultFilename       = args[6];
            
            string autoSaveFilename     = Path.ChangeExtension(resultFilename, "bin");
            //string permutationFilename = Path.ChangeExtension(resultFilename, "per");

            if (File.Exists(resultFilename))
            {
                Console.Write("Result file {0} already exists. Do you want to delete it (Y/N)? ", resultFilename);

                while (true)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    if (keyInfo.KeyChar == 'Y' || keyInfo.KeyChar == 'y')
                    {
                        break;
                    }
                    else if (keyInfo.KeyChar == 'N' || keyInfo.KeyChar == 'n')
                    {
                        Environment.Exit(0);
                        //throw new SystemException("Result file already exists. Delete it and start application again.");
                    }
                }

                Console.WriteLine();
            }

            try
            {
                if (File.Exists(autoSaveFilename))
                {
                    File.Delete(autoSaveFilename);
                }

                if (File.Exists(resultFilename))
                {
                    File.Delete(resultFilename);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }

            ParameterList parameterList = program.RoughMeasureWeightsTestParmList(trainFilename,
                                                                       testFilename,
                                                                       numberOfReducts,
                                                                       numberOfPermutations,
                                                                       numberOfTests,
                                                                       numberOfFolds);
            
            RoughMeasureTest test = new RoughMeasureTest(parameterList);
            //test.PermutationFilename = permutationFilename;

            //if (System.IO.File.Exists(permutationFilename))
            //{
            //    test.FixedPermutationList = PermutationList.LoadFromCsvFile(permutationFilename); 
            //}

            TestRunner testRunner = new TestRunner(resultFilename, test);
            testRunner.Run();
        }
    }
}
