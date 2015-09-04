﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Experimenter.Parms;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

namespace ApproxReductBoostingCV
{
    class Program
    {
        static void Main(string[] args)
        {
            string dataFilename = args[0];            
            int numberOfTests = Int32.Parse(args[1]);
            int maxNumberOfIterations = Int32.Parse(args[2]);
            int startIteration = Int32.Parse(args[3]);
            int iterationStep = Int32.Parse(args[4]);

            if (startIteration < 1)
                throw new ArgumentOutOfRangeException();

            if (startIteration > maxNumberOfIterations)
                throw new ArgumentOutOfRangeException();

            DataStore data = DataStore.Load(dataFilename, FileFormat.Rses1);
            DataStoreSplitter splitter = new DataStoreSplitter(data, 5);            

            ParameterCollection parmList = new ParameterCollection(
                new IParameter[] {
                    new ParameterNumericRange<int>("NumberOfIterations", startIteration, maxNumberOfIterations, iterationStep),
                    new ParameterNumericRange<int>("NumberOfTests", 0, numberOfTests-1, 1),
                    ParameterValueCollection<string>.CreateFromElements<string>("ReductFactory", ReductFactoryKeyHelper.ReductEnsembleBoosting,
                                                                                                 ReductFactoryKeyHelper.ReductEnsembleBoostingWithAttributeDiversity),
                    ParameterValueCollection<WeightingSchema>.CreateFromElements<WeightingSchema>("WeightingSchama", WeightingSchema.Relative, 
                                                                                                                     WeightingSchema.Majority) ,
                    ParameterValueCollection<bool>.CreateFromElements<bool>("CheckEnsembleErrorDuringTraining", true, false),
                    ParameterValueCollection<UpdateWeightsDelegate>.CreateFromElements<UpdateWeightsDelegate>("UpdateWeights", ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All, 
                                                                                                                               ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_OnlyNotCorrect,
                                                                                                                               ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoostM1),
                    ParameterValueCollection<int>.CreateFromElements<int>("MinLenght", 0)
                }
            );

            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}",
                                     "METHOD",
                                     "IDENTYFICATION",
                                     "VOTETYPE",
                                     "MIN_LEN",
                                     "UPDATE_WEIGHTS",
                                     "WEIGHT_TYPE",
                                     "CHECK_ENSEBLE_ERROR",
                                     "TESTID",
                                     "MAXITER",
                                     "NOF_MODELS",
                                     "NOF_WRESET",
                                     "TRN_ERROR",
                                     "TST_ERROR",
                                     "AVG_REDUCT",
                                     "FOLD");

            int i = 0;
            foreach (object[] p in parmList.Values())
            {
                i++;

                int iter = (int)p[0];
                int t = (int)p[1];
                string factoryKey = (string)p[2];
                WeightingSchema weightingSchema = (WeightingSchema)p[3];
                bool checkEnsembleErrorDuringTraining = (bool)p[4];
                UpdateWeightsDelegate updateWeights = (UpdateWeightsDelegate)p[5];
                int minLen = (int)p[6];                
                
                for (int f = 1; f <= 5; f++)
                {
                    DataStore trnFold = null;
                    DataStore tstFold = null;
                    splitter.ActiveFold = f;
                    splitter.Split(ref trnFold, ref tstFold);

                    Args parms = new Args();
                    parms.AddParameter(ReductGeneratorParamHelper.DataStore, trnFold);
                    parms.AddParameter(ReductGeneratorParamHelper.NumberOfThreads, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, factoryKey);
                    parms.AddParameter(ReductGeneratorParamHelper.IdentificationType, IdentificationType.WeightConfidence);
                    parms.AddParameter(ReductGeneratorParamHelper.VoteType, VoteType.WeightConfidence);
                    parms.AddParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
                    parms.AddParameter(ReductGeneratorParamHelper.MaxIterations, iter);
                    parms.AddParameter(ReductGeneratorParamHelper.UpdateWeights, updateWeights);

                    WeightGenerator weightGenerator;
                    switch (weightingSchema)
                    {
                        case WeightingSchema.Majority:
                            weightGenerator = new WeightGeneratorMajority(trnFold);
                            break;

                        case WeightingSchema.Relative:
                            weightGenerator = new WeightGeneratorRelative(trnFold);
                            break;

                        default:
                            weightGenerator = new WeightBoostingGenerator(trnFold);
                            break;
                    }

                    parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                    parms.AddParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, checkEnsembleErrorDuringTraining);

                    if (minLen != 0)
                        parms.AddParameter(ReductGeneratorParamHelper.MinReductLength, minLen);

                    ReductEnsembleBoostingGenerator reductGenerator = (ReductEnsembleBoostingGenerator)ReductFactory.GetReductGenerator(parms);//as ReductEnsembleBoostingGenerator;
                    reductGenerator.Generate();

                    RoughClassifier classifierTrn = new RoughClassifier();
                    classifierTrn.ReductStoreCollection = reductGenerator.GetReductGroups();
                    classifierTrn.Classify(trnFold);
                    ClassificationResult resultTrn = classifierTrn.Vote(trnFold, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

                    RoughClassifier classifierTst = new RoughClassifier();
                    classifierTst.ReductStoreCollection = reductGenerator.GetReductGroups();
                    classifierTst.Classify(tstFold);
                    ClassificationResult resultTst = classifierTst.Vote(tstFold, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

                    string updWeightsMethodName = String.Empty;
                    switch (reductGenerator.UpdateWeights.Method.Name)
                    {
                        case "UpdateWeightsAdaBoost_All":
                            updWeightsMethodName = "All";
                            break;

                        case "UpdateWeightsAdaBoost_OnlyNotCorrect":
                            updWeightsMethodName = "NotCorrectOnly";
                            break;

                        case "UpdateWeightsAdaBoost_OnlyCorrect":
                            updWeightsMethodName = "CorrectOnly";
                            break;

                        case "UpdateWeightsAdaBoostM1":
                            updWeightsMethodName = "M1";
                            break;

                        default:
                            updWeightsMethodName = "Unknown";
                            break;
                    }

                    Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}",
                                        factoryKey,
                                        reductGenerator.IdentyficationType,
                                        reductGenerator.VoteType,
                                        reductGenerator.MinReductLength,
                                        updWeightsMethodName, //reductGenerator.UpdateWeights,
                                        weightingSchema,
                                        reductGenerator.CheckEnsembleErrorDuringTraining,
                                        t + 1,
                                        reductGenerator.MaxIterations,
                                        reductGenerator.IterationsPassed,
                                        reductGenerator.NumberOfWeightResets,
                                        resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                        resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                        reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()),
                                        splitter.ActiveFold);
                }                
            }
        }
    }
}
