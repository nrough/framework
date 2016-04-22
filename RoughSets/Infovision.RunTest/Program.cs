using System;
using System.IO;
using Infovision.Data;
using Infovision.Datamining.Roughset;
using Infovision.Test;

namespace Infovision.RunTest
{
    public class Program
    {   
        public ParameterCollection RoughMeasureWeightsTestParmList(string trainFileName,
                                        string testFileName,
                                        int numberOfReducts,
                                        int numberOfPermutations,
                                        int numberOfTests,
                                        int numberOfFolds)
        {
            DataStore dataStoreTraining = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTraining.DataStoreInfo);

            ITestParameter parmDataStoreTraining = new ParameterObjectReference<DataStore>(ReductGeneratorParamHelper.TrainData, dataStoreTraining);
            ITestParameter parmDataStoreTest = new ParameterObjectReference<DataStore>("DataStoreTest", dataStoreTest);
            ITestParameter parmNumberOfReducts = new ParameterValueList<int>(ReductGeneratorParamHelper.NumberOfReducts, numberOfReducts);
            ITestParameter parmNumberOfPermutations = new ParameterValue<int>(ReductGeneratorParamHelper.NumberOfPermutations, numberOfPermutations);
            ITestParameter parmNFold = new ParameterValueList<int>("NumberOfFolds", numberOfFolds);
            ITestParameter parmFoldNumber = new ParameterNumericRange<int>("FoldNumber", 0, 0, 1);

            if (numberOfFolds > 1)
            {
                parmFoldNumber = new ParameterNumericRange<int>("FoldNumber", 1, numberOfFolds, 1);
            }

            ITestParameter parmTestNumber = new ParameterNumericRange<int>("NumberOfTests", 1, numberOfTests, 1);
            ITestParameter parmEpsilon = new ParameterNumericRange<int>(ReductGeneratorParamHelper.Epsilon, 0, 99, 1);

            ITestParameter parmReductType = new ParameterValueList<string>(ReductGeneratorParamHelper.FactoryKey,
                                                                            new string[] { 
                                                                                ReductFactoryKeyHelper.ApproximateReductRelativeWeights,
                                                                                ReductFactoryKeyHelper.ApproximateReductRelative,
                                                                                ReductFactoryKeyHelper.ApproximateReductMajorityWeights,            
                                                                                ReductFactoryKeyHelper.ApproximateReductMajority });

            ITestParameter parmReductMeasure = new ParameterValueList<string>("ReductMeasure",
                                                                              new string[] { "ReductMeasureLength" });

            ITestParameter parmIdentification = new ParameterValueList<RuleQualityFunction>(ReductGeneratorParamHelper.IdentificationType, new RuleQualityFunction[] { RuleQuality_DEL.Confidence,
                                                                                                                                            RuleQuality_DEL.Coverage,
                                                                                                                                            RuleQuality_DEL.ConfidenceW,
                                                                                                                                            RuleQuality_DEL.CoverageW});

            ITestParameter parmVote = new ParameterValueList<RuleQualityFunction>(ReductGeneratorParamHelper.VoteType, new RuleQualityFunction[] { RuleQuality_DEL.Confidence,
                                                                                                    RuleQuality_DEL.Support,
                                                                                                    RuleQuality_DEL.Ratio,
                                                                                                    RuleQuality_DEL.Coverage,                                                                                                                                                                                                        
                                                                                                    RuleQuality_DEL.Strength,
                                                                                                    RuleQuality_DEL.ConfidenceW,                                                                                                    
                                                                                                    RuleQuality_DEL.RatioW,
                                                                                                    RuleQuality_DEL.CoverageW,
                                                                                                    RuleQuality_DEL.SingleVote,
                                                                                                    RuleQuality_DEL.StrengthW});
 
            ParameterCollection parameterList = new ParameterCollection(new ITestParameter[] { parmDataStoreTraining,//0
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
            ParameterCollection parameterList = new ParameterCollection(new ITestParameter[] { parmDataStoreTraining,//0
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
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2, ReductFactoryKeyHelper.ApproximateReductMajority }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2, ReductFactoryKeyHelper.ApproximateReductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)ReductQuality.CoverageW2, ReductFactoryKeyHelper.ApproximateReductMajority }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)ReductQuality.CoverageW2, ReductFactoryKeyHelper.ApproximateReductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)ReductQuality.Support2, ReductFactoryKeyHelper.ApproximateReductMajority }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)ReductQuality.Support2, ReductFactoryKeyHelper.ApproximateReductRelative }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Support2, ReductFactoryKeyHelper.ApproximateReductMajority }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Support2, ReductFactoryKeyHelper.ApproximateReductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2, ReductFactoryKeyHelper.ApproximateReductMajority }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2, ReductFactoryKeyHelper.ApproximateReductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.CoverageW2, ReductFactoryKeyHelper.ApproximateReductMajority }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.CoverageW2, ReductFactoryKeyHelper.ApproximateReductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.RatioW2, ReductFactoryKeyHelper.ApproximateReductMajority }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.RatioW2, ReductFactoryKeyHelper.ApproximateReductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.StrenghtW, ReductFactoryKeyHelper.ApproximateReductMajority }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (ReductQualityFunction)RuleQuality_DEL.StrenghtW, ReductFactoryKeyHelper.ApproximateReductRelative }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2, (ReductQualityFunction)RuleQuality_DEL.Confidence2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2, (ReductQualityFunction)RuleQuality_DEL.Coverage2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2, (ReductQualityFunction)RuleQuality_DEL.Support2 }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.CoverageW2, (ReductQualityFunction)RuleQuality_DEL.Confidence2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.CoverageW2, (ReductQualityFunction)RuleQuality_DEL.Coverage2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.CoverageW2, (ReductQualityFunction)RuleQuality_DEL.Support2 }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.RatioW2, (ReductQualityFunction)RuleQuality_DEL.Confidence2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.RatioW2, (ReductQualityFunction)RuleQuality_DEL.Coverage2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.RatioW2, (ReductQualityFunction)RuleQuality_DEL.Support2 }));            

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.WeightStrenght, (ReductQualityFunction)RuleQuality_DEL.Confidence2}));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.WeightStrenght, (ReductQualityFunction)RuleQuality_DEL.Coverage2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.WeightStrenght, (ReductQualityFunction)RuleQuality_DEL.Support2 }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Support2, (ReductQualityFunction)RuleQuality_DEL.Confidence2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Support2, (ReductQualityFunction)RuleQuality_DEL.Coverage2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Support2, (ReductQualityFunction)RuleQuality_DEL.Support2 }));  
 
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Confidence2, (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Confidence2, (ReductQualityFunction)RuleQuality_DEL.Coverage2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Confidence2, (ReductQualityFunction)RuleQuality_DEL.Support2 }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Coverage2, (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Coverage2, (ReductQualityFunction)RuleQuality_DEL.CoverageW2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Coverage2, (ReductQualityFunction)RuleQuality_DEL.Support2 }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Ratio2, (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Ratio2, (ReductQualityFunction)RuleQuality_DEL.CoverageW2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Ratio2, (ReductQualityFunction)RuleQuality_DEL.Support2 }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Strenght, (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Strenght, (ReductQualityFunction)RuleQuality_DEL.CoverageW2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Strenght, (ReductQualityFunction)RuleQuality_DEL.Support2 }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Support2, (ReductQualityFunction)RuleQuality_DEL.ConfidenceW2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Support2, (ReductQualityFunction)RuleQuality_DEL.CoverageW2 }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.IdentificationType }, new object[] { (ReductQualityFunction)RuleQuality_DEL.Support2, (ReductQualityFunction)RuleQuality_DEL.Support2 }));
            */

            return parameterList;
        }
        
        
        public ParameterCollection RoughMeasureTestParmList(string trainFileName,
                                        string testFileName, 
                                        int numberOfReducts,
                                        int numberOfPermutations,
                                        int numberOfTests,
                                        int numberOfFolds)
        {
            DataStore dataStoreTraining = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTraining.DataStoreInfo);

            ITestParameter parmDataStoreTraining = new ParameterObjectReference<DataStore>(ReductGeneratorParamHelper.TrainData, dataStoreTraining);
            ITestParameter parmDataStoreTest = new ParameterObjectReference<DataStore>("DataStoreTest", dataStoreTest);
            ITestParameter parmNumberOfReducts = new ParameterValueList<int>(ReductGeneratorParamHelper.NumberOfReducts, numberOfReducts);
            ITestParameter parmNumberOfPermutations = new ParameterValue<int>(ReductGeneratorParamHelper.NumberOfPermutations, numberOfPermutations);
            ITestParameter parmNFold = new ParameterValueList<int>("NumberOfFolds", numberOfFolds);
            ITestParameter parmFoldNumber = new ParameterNumericRange<int>("FoldNumber", 0, 0, 1);
            
            if (numberOfFolds > 1)
            {
                parmFoldNumber = new ParameterNumericRange<int>("FoldNumber", 1, numberOfFolds, 1);
            }

            ITestParameter parmTestNumber = new ParameterNumericRange<int>("NumberOfTests", 1, numberOfTests, 1);
            ITestParameter parmEpsilon = new ParameterNumericRange<int>(ReductGeneratorParamHelper.Epsilon, 0, 99, 1);

            ITestParameter parmReductType = new ParameterValueList<string>(ReductGeneratorParamHelper.FactoryKey, 
                                                                            new string[] { ReductFactoryKeyHelper.ApproximateReductPositive, 
                                                                                           ReductFactoryKeyHelper.ApproximateReductMajority, 
                                                                                           ReductFactoryKeyHelper.ApproximateReductRelative, 
                                                                                           ReductFactoryKeyHelper.GammaBireduct, 
                                                                                           ReductFactoryKeyHelper.Bireduct, 
                                                                                           ReductFactoryKeyHelper.BireductRelative }
                                                                          );

            ITestParameter parmReductMeasure = new ParameterValueList<string>("ReductMeasure", 
                                                                              new string[] { "ReductMeasureLength", 
                                                                                             "ReductMeasureNumberOfPartitions", 
                                                                                             "BireductMeasureMajority", 
                                                                                             "BireductMeasureRelative"});

            ITestParameter parmIdentification = new ParameterValueList<RuleQualityFunction>(
                ReductGeneratorParamHelper.IdentificationType,
                new RuleQualityFunction[] { RuleQuality_DEL.Confidence, RuleQuality_DEL.Coverage, RuleQuality_DEL.Support, RuleQuality_DEL.Ratio, RuleQuality_DEL.ConfidenceRelative, RuleQuality_DEL.Strength,
                                            RuleQuality_DEL.ConfidenceW, RuleQuality_DEL.CoverageW, RuleQuality_DEL.RatioW, RuleQuality_DEL.StrengthW } );
            ITestParameter parmVote = new ParameterValueList<RuleQualityFunction>(
                ReductGeneratorParamHelper.VoteType, 
                new RuleQualityFunction[] { RuleQuality_DEL.Confidence, RuleQuality_DEL.Coverage, RuleQuality_DEL.Support, RuleQuality_DEL.Ratio, RuleQuality_DEL.ConfidenceRelative, RuleQuality_DEL.Strength,
                                            RuleQuality_DEL.ConfidenceW, RuleQuality_DEL.CoverageW, RuleQuality_DEL.RatioW, RuleQuality_DEL.StrengthW, RuleQuality_DEL.SingleVote } );
            
            /*
            ParameterCollection parameterList = new ParameterCollection(new ITestParameter[] { parmDataStoreTraining,//0
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

            ParameterCollection parameterList = new ParameterCollection(new ITestParameter[] { parmDataStoreTraining,//0
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

            //parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure" }, new Object[] { ReductFactoryKeyHelper.Bireduct }));
            //parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure" }, new Object[] { ReductFactoryKeyHelper.BireductRelative }));
            //parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure" }, new Object[] { ReductFactoryKeyHelper.GammaBireduct }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "BireductMeasureMajority", ReductFactoryKeyHelper.ApproximateReductPositive }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "BireductMeasureMajority", ReductFactoryKeyHelper.ApproximateReductMajority }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "BireductMeasureMajority", ReductFactoryKeyHelper.ApproximateReductRelative }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "BireductMeasureRelative", ReductFactoryKeyHelper.ApproximateReductPositive }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "BireductMeasureRelative", ReductFactoryKeyHelper.ApproximateReductMajority }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "BireductMeasureRelative", ReductFactoryKeyHelper.ApproximateReductRelative }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "ReductMeasureLength", ReductFactoryKeyHelper.Bireduct }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "ReductMeasureLength", ReductFactoryKeyHelper.BireductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "ReductMeasureLength", ReductFactoryKeyHelper.GammaBireduct }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "ReductMeasureNumberOfPartitions", ReductFactoryKeyHelper.Bireduct }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "ReductMeasureNumberOfPartitions", ReductFactoryKeyHelper.BireductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { "ReductMeasure", ReductGeneratorParamHelper.FactoryKey }, new object[] { "ReductMeasureNumberOfPartitions", ReductFactoryKeyHelper.GammaBireduct }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Confidence, ReductFactoryKeyHelper.Bireduct }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Confidence, ReductFactoryKeyHelper.BireductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Confidence, ReductFactoryKeyHelper.GammaBireduct }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Confidence, ReductFactoryKeyHelper.Bireduct }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Confidence, ReductFactoryKeyHelper.BireductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.IdentificationType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Confidence, ReductFactoryKeyHelper.GammaBireduct }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Support, ReductFactoryKeyHelper.Bireduct }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Support, ReductFactoryKeyHelper.BireductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Support, ReductFactoryKeyHelper.GammaBireduct }));

            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Confidence, ReductFactoryKeyHelper.Bireduct }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Confidence, ReductFactoryKeyHelper.BireductRelative }));
            parameterList.AddExclusion(new ParameterExclusion(new string[] { ReductGeneratorParamHelper.VoteType, ReductGeneratorParamHelper.FactoryKey }, new object[] { (RuleQualityFunction)RuleQuality_DEL.Confidence, ReductFactoryKeyHelper.GammaBireduct }));

            return parameterList;
        }

        public ParameterCollection RelativeTestParmList(string trainFileName,
                                        string testFileName,
                                        int numberOfReducts,
                                        int numberOfPermutations,
                                        int numberOfTests,
                                        int numberOfFolds)
        {
            DataStore dataStoreTraining = DataStore.Load(trainFileName, FileFormat.Rses1);
            DataStore dataStoreTest = DataStore.Load(testFileName, FileFormat.Rses1, dataStoreTraining.DataStoreInfo);

            ITestParameter parmDataStoreTraining = new ParameterObjectReference<DataStore>(ReductGeneratorParamHelper.TrainData, dataStoreTraining);
            ITestParameter parmDataStoreTest = new ParameterObjectReference<DataStore>("DataStoreTest", dataStoreTest);
            ITestParameter parmNumberOfReducts = new ParameterValueList<int>(ReductGeneratorParamHelper.NumberOfReducts, numberOfReducts);
            ITestParameter parmNumberOfPermutations = new ParameterValue<int>(ReductGeneratorParamHelper.NumberOfPermutations, numberOfPermutations);
            ITestParameter parmNFold = new ParameterValueList<int>("NumberOfFolds", numberOfFolds);
            ITestParameter parmFoldNumber = new ParameterNumericRange<Int32>("FoldNumber", 0, 0, 1);

            if (numberOfFolds > 1)
            {
                parmFoldNumber = new ParameterNumericRange<Int32>("FoldNumber", 1, numberOfFolds, 1);
            }

            ITestParameter parmTestNumber = new ParameterNumericRange<int>("NumberOfTests", 1, numberOfTests, 1);
            ITestParameter parmEpsilon = new ParameterNumericRange<decimal>(ReductGeneratorParamHelper.Epsilon, Decimal.Zero, Decimal.One, 0.01m);

            ITestParameter parmReductType = new ParameterValueList<String>(ReductGeneratorParamHelper.FactoryKey,
                                                                            new string[] { ReductFactoryKeyHelper.ApproximateReductMajorityWeights,
                                                                                           ReductFactoryKeyHelper.ApproximateReductRelativeWeights,
                                                                                           ReductFactoryKeyHelper.ApproximateReductRelative, 
                                                                                           ReductFactoryKeyHelper.ApproximateReductMajority
                                                                                           });

            ITestParameter parmReductMeasure = new ParameterValueList<String>("ReductMeasure",
                                                                              new string[] { "ReductMeasureLength" });

            ITestParameter parmIdentification = new ParameterValueList<RuleQualityFunction>(ReductGeneratorParamHelper.IdentificationType,
                                                                                           new RuleQualityFunction[] { RuleQuality_DEL.Confidence, 
                                                                                                                      RuleQuality_DEL.Coverage});

            ITestParameter parmVote = new ParameterValueList<RuleQualityFunction>(ReductGeneratorParamHelper.VoteType,
                                                                       new RuleQualityFunction[] { RuleQuality_DEL.Confidence,
                                                                                        RuleQuality_DEL.ConfidenceRelative,
                                                                                        RuleQuality_DEL.Coverage });

            /*
            ParameterCollection parameterList = new ParameterCollection(new ITestParameter[] { parmDataStoreTraining,       // 0
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

            ParameterCollection parameterList = new ParameterCollection(new ITestParameter[] { parmDataStoreTraining,       // 0
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
            Console.WriteLine(dataStore.ToStringInternal(" "));
        }

        /// <summary>
        /// Print data from strore
        /// </summary>
        private void ShowData(DataStore dataStore)
        {
            Console.WriteLine(String.Format("Dataset: {0}", dataStore.Name));
            Console.WriteLine(dataStore.ToStringExternal(" "));
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

            ParameterCollection parameterList = program.RoughMeasureWeightsTestParmList(trainFilename,
                                                                       testFilename,
                                                                       numberOfReducts,
                                                                       numberOfPermutations,
                                                                       numberOfTests,
                                                                       numberOfFolds);
            
            RoughMeasureTest test = new RoughMeasureTest(parameterList);
            //test.PermutationFilename = permutationFilename;

            //if (System.IO.File.Exists(permutationFilename))
            //{
            //    test.FixedPermutationList = PermutationCollection.LoadFromCsvFile(permutationFilename); 
            //}

            TestRunner testRunner = new TestRunner(resultFilename, test);
            testRunner.Run();
        }
    }
}
