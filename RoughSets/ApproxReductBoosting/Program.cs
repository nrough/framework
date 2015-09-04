using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Experimenter.Parms;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

namespace ApproxReductBoosting
{
    class Program
    {
        static void Main(string[] args)
        {
            string trainFilename = args[0];
            string testFilename = args[1];
            int numberOfTests = Int32.Parse(args[2]);
            int maxNumberOfIterations = Int32.Parse(args[3]);

            DataStore trnData = DataStore.Load(trainFilename, FileFormat.Rses1);
            DataStore tstData = DataStore.Load(testFilename, FileFormat.Rses1, trnData.DataStoreInfo);                        

            ParameterCollection parmList = new ParameterCollection(
                new IParameter[] {
                    new ParameterNumericRange<int>("NumberOfIterations", 1, maxNumberOfIterations, 1),
                    new ParameterNumericRange<int>("NumberOfTests", 0, numberOfTests, 1),
                    ParameterValueCollection<string>.CreateFromElements<string>("ReductFactory", ReductFactoryKeyHelper.ReductEnsembleBoosting,
                                                                                                 ReductFactoryKeyHelper.ReductEnsembleBoostingWithAttributeDiversity),
                    ParameterValueCollection<WeightingSchema>.CreateFromElements<WeightingSchema>("WeightingSchama", WeightingSchema.Majority, 
                                                                                                                     WeightingSchema.Relative) ,
                    ParameterValueCollection<bool>.CreateFromElements<bool>("CheckEnsembleErrorDuringTraining", true, false),
                    ParameterValueCollection<UpdateWeights>.CreateFromElements<UpdateWeights>("UpdateWeights", UpdateWeights.All, UpdateWeights.NotCorrectOnly),
                    ParameterValueCollection<int>.CreateFromElements<int>("MinLenght", 0, 1)
                }
            );

            Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
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
                                     "AVG_REDUCT");

            int i = 0;
            foreach (object[] p in parmList.Values())
            {                
                i++;

                int iter = (int)p[0];
                int t = (int)p[1];
                string factoryKey = (string)p[2];
                WeightingSchema weightingSchema = (WeightingSchema)p[3];
                bool checkEnsembleErrorDuringTraining = (bool)p[4];
                UpdateWeights updateWeights = (UpdateWeights)p[5];
                int minLen = (int)p[6];
                
                Args parms = new Args();
                parms.AddParameter(ReductGeneratorParamHelper.DataStore, trnData);
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
                        weightGenerator = new WeightGeneratorMajority(trnData);
                        break;

                    case WeightingSchema.Relative:
                        weightGenerator = new WeightGeneratorRelative(trnData);
                        break;

                    default:
                        weightGenerator = new WeightBoostingGenerator(trnData);
                        break;
                }

                parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
                parms.AddParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, checkEnsembleErrorDuringTraining);

                if(minLen != 0)
                    parms.AddParameter(ReductGeneratorParamHelper.MinReductLength, minLen);

                ReductEnsembleBoostingGenerator reductGenerator = (ReductEnsembleBoostingGenerator) ReductFactory.GetReductGenerator(parms) ;//as ReductEnsembleBoostingGenerator;
                reductGenerator.Generate();

                RoughClassifier classifierTrn = new RoughClassifier();
                classifierTrn.ReductStoreCollection = reductGenerator.GetReductGroups();
                classifierTrn.Classify(trnData);
                ClassificationResult resultTrn = classifierTrn.Vote(trnData, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

                RoughClassifier classifierTst = new RoughClassifier();
                classifierTst.ReductStoreCollection = reductGenerator.GetReductGroups();
                classifierTst.Classify(tstData);
                ClassificationResult resultTst = classifierTst.Vote(tstData, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

                Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                    factoryKey,
                                    reductGenerator.IdentyficationType,
                                    reductGenerator.VoteType,
                                    reductGenerator.MinReductLength,
                                    reductGenerator.UpdateWeights,
                                    weightingSchema,
                                    reductGenerator.CheckEnsembleErrorDuringTraining,
                                    t + 1,
                                    reductGenerator.MaxIterations,
                                    reductGenerator.IterationsPassed,
                                    reductGenerator.NumberOfWeightResets,
                                    resultTrn.WeightMisclassified + resultTrn.WeightUnclassified,
                                    resultTst.WeightMisclassified + resultTst.WeightUnclassified,
                                    reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
            }
        }
    }
}
