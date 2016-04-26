using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Experimenter.Parms;
using Infovision.Datamining.Filters.Unsupervised.Attribute;
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
            FileFormat fileFormat = args.Length >= 6 ? (FileFormat)Int32.Parse(args[5]) : FileFormat.Rses1;
            int decisionPosition = args.Length >= 7 ? Int32.Parse(args[6]) : -1;

            if (startIteration < 1)
                throw new ArgumentOutOfRangeException();

            if (startIteration > maxNumberOfIterations)
                throw new ArgumentOutOfRangeException();

            DataStore data = DataStore.Load(dataFilename, fileFormat);

            if (decisionPosition != -1)
                data.SetDecisionFieldId(decisionPosition);

            Console.WriteLine("Training dataset: {0} ({1})", dataFilename, fileFormat);
            Console.WriteLine("Number of records: {0}", data.DataStoreInfo.NumberOfRecords);
            Console.WriteLine("Number of attributes: {0}", data.DataStoreInfo.NumberOfFields);
            Console.WriteLine("Decision attribute position: {0}", data.DataStoreInfo.DecisionFieldId);
            Console.WriteLine("Missing Values: {0}", data.DataStoreInfo.HasMissingData);
            
            int cvfolds = 5;
            DataStoreSplitter splitter = new DataStoreSplitter(data, cvfolds);

            Console.WriteLine("Using Cross Validation with {0} splits", cvfolds);            

            int numOfAttr = data.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);

            ParameterCollection parmList = new ParameterCollection(
                new IParameter[] {
                    //new ParameterNumericRange<int>("NumberOfIterations", startIteration, maxNumberOfIterations, iterationStep),
                    //ParameterValueCollection<int>.CreateFromElements<int>("NumberOfIterations", 1, 2, 5, 10, 20, 50, 100),
                    ParameterValueCollection<int>.CreateFromElements("NumberOfIterations", 100, 50, 20),
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
                    //ParameterValueCollection<int>.CreateFromElements("MinLenght", (int) System.Math.Floor(System.Math.Log((decimal)numOfAttr + 1.0M, 2.0)))
                    //ParameterValueCollection<int>.CreateFromElements("MinLenght", 1)
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
                //int minLen = (int)p[6];
                int epsilon = (int)p[6];
                
                for (int f = 0; f < cvfolds; f++)
                {
                    DataStore trnFoldOrig = null;
                    DataStore tstFoldOrig = null;
                    
                    splitter.ActiveFold = f;
                    splitter.Split(ref trnFoldOrig, ref tstFoldOrig);

                    DataStore trnFoldReplaced = null;

                    Args parms = new Args();

                    WeightGenerator weightGenerator;
                    if (trnFoldOrig.DataStoreInfo.HasMissingData)
                    {
                        trnFoldReplaced = new ReplaceMissingValues().Compute(trnFoldOrig);
                        parms.SetParameter(ReductGeneratorParamHelper.TrainData, trnFoldReplaced);                        
                        switch (weightingSchema)
                        {
                            case WeightingSchema.Majority:
                                weightGenerator = new WeightGeneratorMajority(trnFoldReplaced);
                                break;

                            case WeightingSchema.Relative:
                                weightGenerator = new WeightGeneratorRelative(trnFoldReplaced);
                                break;

                            default:
                                weightGenerator = new WeightBoostingGenerator(trnFoldReplaced);
                                break;
                        }
                    }
                    else
                    {
                        parms.SetParameter(ReductGeneratorParamHelper.TrainData, trnFoldOrig);                        
                        switch (weightingSchema)
                        {
                            case WeightingSchema.Majority:
                                weightGenerator = new WeightGeneratorMajority(trnFoldOrig);
                                break;

                            case WeightingSchema.Relative:
                                weightGenerator = new WeightGeneratorRelative(trnFoldOrig);
                                break;

                            default:
                                weightGenerator = new WeightBoostingGenerator(trnFoldOrig);
                                break;
                        }
                    }                    
                    
                    parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, factoryKey);
                    parms.SetParameter(ReductGeneratorParamHelper.IdentificationType, (Func<long, IReduct, EquivalenceClass, decimal>)RuleQuality.ConfidenceW);
                    parms.SetParameter(ReductGeneratorParamHelper.VoteType, (Func<long, IReduct, EquivalenceClass, decimal>)RuleQuality.ConfidenceW);
                    parms.SetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
                    parms.SetParameter(ReductGeneratorParamHelper.MaxIterations, iter);
                    parms.SetParameter(ReductGeneratorParamHelper.UpdateWeights, updateWeights);
                    parms.SetParameter(ReductGeneratorParamHelper.Epsilon, (decimal)epsilon / 100.0M);
                    parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                    parms.SetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, checkEnsembleErrorDuringTraining);

                    ReductEnsembleBoostingGenerator reductGenerator = (ReductEnsembleBoostingGenerator)ReductFactory.GetReductGenerator(parms);
                    reductGenerator.Run();

                    RoughClassifier classifierTrn = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType, 
                        reductGenerator.VoteType,
                        trnFoldOrig.DataStoreInfo.GetDecisionValues());

                    ClassificationResult resultTrn = classifierTrn.Classify(
                        trnFoldOrig.DataStoreInfo.HasMissingData ? trnFoldReplaced : trnFoldOrig, 
                        null);

                    RoughClassifier classifierTst = new RoughClassifier(
                        reductGenerator.GetReductGroups(),
                        reductGenerator.IdentyficationType, 
                        reductGenerator.VoteType,
                        data.DataStoreInfo.GetDecisionValues());
                    ClassificationResult resultTst = classifierTst.Classify(
                        tstFoldOrig, 
                        null);

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
                    reductGenerator.ReductPool.GetMeanStdDev(new ReductMeasureLength(), out redLenMean, out redLenAveDev);

                    Console.WriteLine(CSVFileHelper.GetRecord(' ',
                                        data.Name,
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
                                        splitter.ActiveFold,
                                        reductGenerator.Epsilon));
                }                
            }
        }
    }
}
