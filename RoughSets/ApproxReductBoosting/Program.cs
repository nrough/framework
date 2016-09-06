using System;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Experimenter.Parms;
using Infovision.Datamining.Filters.Unsupervised.Attribute;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

namespace ApproxReductBoosting
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string trainFilename = args[0];
            string testFilename = args[1];
            int numberOfTests = Int32.Parse(args[2]);
            int maxNumberOfIterations = Int32.Parse(args[3]);
            int startIteration = Int32.Parse(args[4]);
            int iterationStep = Int32.Parse(args[5]);
            FileFormat fileFormat = args.Length >= 7 ? (FileFormat)Int32.Parse(args[6]) : FileFormat.Rses1;
            int decisionPosition = args.Length >= 8 ? Int32.Parse(args[7]) : -1;

            if (startIteration < 1)
                throw new ArgumentOutOfRangeException();

            if (startIteration > maxNumberOfIterations)
                throw new ArgumentOutOfRangeException();

            DataStore trnDataOrig = DataStore.Load(trainFilename, fileFormat);
            DataStore tstDataOrig = DataStore.Load(testFilename, fileFormat, trnDataOrig.DataStoreInfo);
            DataStore trnDataReplaced = null;

            if (decisionPosition != -1)
            {
                trnDataOrig.SetDecisionFieldId(decisionPosition);
                tstDataOrig.SetDecisionFieldId(decisionPosition);
            }

            Console.WriteLine("Training dataset: {0} ({1})", trainFilename, fileFormat);
            Console.WriteLine("Number of records: {0}", trnDataOrig.DataStoreInfo.NumberOfRecords);
            Console.WriteLine("Number of attributes: {0}", trnDataOrig.DataStoreInfo.NumberOfFields);
            Console.WriteLine("Decision key position: {0}", trnDataOrig.DataStoreInfo.DecisionFieldId);
            Console.WriteLine("Missing Values: {0}", trnDataOrig.DataStoreInfo.HasMissingData);

            Console.WriteLine("Test dataset: {0} ({1})", testFilename, fileFormat);
            Console.WriteLine("Number of records: {0}", tstDataOrig.DataStoreInfo.NumberOfRecords);
            Console.WriteLine("Number of attributes: {0}", tstDataOrig.DataStoreInfo.NumberOfFields);
            Console.WriteLine("Decision key position: {0}", tstDataOrig.DataStoreInfo.DecisionFieldId);
            Console.WriteLine("Missing Values: {0}", tstDataOrig.DataStoreInfo.HasMissingData);

            if (trnDataOrig.DataStoreInfo.HasMissingData)
            {
                trnDataReplaced = new ReplaceMissingValues().Compute(trnDataOrig);
                Console.WriteLine("Missing newInstance replacing...DONE");
            }

            int numOfAttr = trnDataOrig.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);

            ParameterCollection parmList = new ParameterCollection(
                new IParameter[] {
					//new ParameterNumericRange<int>("NumberOfIterations", startIteration, maxNumberOfIterations, iterationStep),
					//ParameterValueCollection<int>.CreateFromElements("NumberOfIterations", 1, 2, 5, 10, 20, 50, 100),
					ParameterValueCollection<int>.CreateFromElements("NumberOfIterations", 100, 50, 20, 10, 5, 2, 1),
					new ParameterNumericRange<int>("NumberOfTests", 0, numberOfTests-1, 1),
					ParameterValueCollection<string>.CreateFromElements("ReductFactory"
																				 //,ReductFactoryKeyHelper.ReductEnsembleBoosting
																				 //,ReductFactoryKeyHelper.ReductEnsembleBoostingWithAttributeDiversity
																				 ,ReductFactoryKeyHelper.ReductEnsembleBoostingVarEps
																				 ,ReductFactoryKeyHelper.ReductEnsembleBoostingVarEpsWithAttributeDiversity
																			   ),
					ParameterValueCollection<WeightingSchema>.CreateFromElements("WeightingSchama", WeightingSchema.Majority),
					ParameterValueCollection<bool>.CreateFromElements("CheckEnsembleErrorDuringTraining", false),
					ParameterValueCollection<UpdateWeightsDelegate>.CreateFromElements("SetWeights", ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All),
					new ParameterNumericRange<int>(ReductGeneratorParamHelper.Epsilon, 0, 50, 5)
				}
            );

            Console.WriteLine(CSVFileHelper.GetRecord(' ',
                                     "DATASET",
                                     "METHOD",
                                     "IDENTYFICATION",
                                     "VOTETYPE",
                                     "UPDATE_WEIGHTS",
                                     "WEIGHT_TYPE",
                                     "CHECK_ENSEBLE_ERROR",
                                     "TESTID",
                                     "MAXITER",
                                     "NOF_MODELS",
                                     "NOF_WRESET",
                                     "TRN_ERROR",
                                     "TST_ERROR",
                                     "AVG_REDUCT_LEN",
                                     "AVEDEV_REDUCT_LEN",
                                     "AVG_M(B)",
                                     "STDDEV_M(B)",
                                     "FOLD",
                                     "EPSILON"));

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
                int epsilon = (int)p[6];

                Args parms = new Args();
                if (trnDataOrig.DataStoreInfo.HasMissingData)
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, trnDataReplaced);
                else
                    parms.SetParameter(ReductGeneratorParamHelper.TrainData, trnDataOrig);

                parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, factoryKey);
                parms.SetParameter(ReductGeneratorParamHelper.IdentificationType, (Func<long, IReduct, EquivalenceClass, double>)RuleQuality.ConfidenceW);
                parms.SetParameter(ReductGeneratorParamHelper.VoteType, (Func<long, IReduct, EquivalenceClass, double>)RuleQuality.ConfidenceW);
                parms.SetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
                parms.SetParameter(ReductGeneratorParamHelper.MaxIterations, iter);
                parms.SetParameter(ReductGeneratorParamHelper.UpdateWeights, updateWeights);
                parms.SetParameter(ReductGeneratorParamHelper.Epsilon, (double)epsilon / 100.0);

                WeightGenerator weightGenerator;
                switch (weightingSchema)
                {
                    case WeightingSchema.Majority:
                        weightGenerator = new WeightGeneratorMajority(trnDataOrig);
                        break;

                    case WeightingSchema.Relative:
                        weightGenerator = new WeightGeneratorRelative(trnDataOrig);
                        break;

                    default:
                        weightGenerator = new WeightBoostingGenerator(trnDataOrig);
                        break;
                }

                parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                parms.SetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, checkEnsembleErrorDuringTraining);

                ReductEnsembleBoostingGenerator reductGenerator = (ReductEnsembleBoostingGenerator)ReductFactory.GetReductGenerator(parms);
                reductGenerator.Run();

                RoughClassifier classifierTrn = new RoughClassifier(
                    reductGenerator.GetReductGroups(),
                    reductGenerator.IdentyficationType,
                    reductGenerator.VoteType,
                    trnDataOrig.DataStoreInfo.GetDecisionValues());
                ClassificationResult resultTrn = classifierTrn.Classify(
                    trnDataOrig.DataStoreInfo.HasMissingData ? trnDataReplaced : trnDataOrig,
                    null);

                RoughClassifier classifierTst = new RoughClassifier(
                    reductGenerator.GetReductGroups(),
                    RuleQuality.ConfidenceW,
                    RuleQuality.ConfidenceW,
                    trnDataOrig.DataStoreInfo.GetDecisionValues());
                ClassificationResult resultTst = classifierTst.Classify(tstDataOrig, null);

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

                double measureMean = 0.0, measureStdDev = 0.0;
                reductGenerator.ReductPool.GetMeanStdDev(new InformationMeasureMajority(), out measureMean, out measureStdDev);

                double redLenMean = 0.0, redLenAveDev = 0.0;
                reductGenerator.ReductPool.GetMeanAveDev(new ReductMeasureLength(), out redLenMean, out redLenAveDev);

                Console.WriteLine(CSVFileHelper.GetRecord(' ',
                                    trnDataOrig.Name,
                                    factoryKey,
                                    reductGenerator.IdentyficationType,
                                    reductGenerator.VoteType,
                                    updWeightsMethodName,
                                    weightingSchema,
                                    reductGenerator.CheckEnsembleErrorDuringTraining,
                                    t + 1,
                                    reductGenerator.MaxIterations,
                                    reductGenerator.IterationsPassed,
                                    reductGenerator.NumberOfWeightResets,
                                    resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                    resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                    redLenMean,
                                    redLenAveDev,
                                    measureMean,
                                    measureStdDev,
                                    1,
                                    reductGenerator.Epsilon));
            }
        }
    }
}