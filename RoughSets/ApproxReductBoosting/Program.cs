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
			int startIteration = Int32.Parse(args[4]);
			int iterationStep = Int32.Parse(args[5]);
			FileFormat fileFormat = args.Length >= 7 ? (FileFormat)Int32.Parse(args[6]) : FileFormat.Rses1;
			int decisionPosition = args.Length >= 8 ? Int32.Parse(args[7]) : -1;

			if (startIteration < 1)
				throw new ArgumentOutOfRangeException();

			if(startIteration > maxNumberOfIterations)
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
			Console.WriteLine("Decision attribute position: {0}", trnDataOrig.DataStoreInfo.DecisionFieldId);
			Console.WriteLine("Missing Values: {0}", trnDataOrig.DataStoreInfo.HasMissingData);

			Console.WriteLine("Test dataset: {0} ({1})", testFilename, fileFormat);
			Console.WriteLine("Number of records: {0}", tstDataOrig.DataStoreInfo.NumberOfRecords);
			Console.WriteLine("Number of attributes: {0}", tstDataOrig.DataStoreInfo.NumberOfFields);
			Console.WriteLine("Decision attribute position: {0}", tstDataOrig.DataStoreInfo.DecisionFieldId);
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
					ParameterValueCollection<UpdateWeightsDelegate>.CreateFromElements("UpdateWeights", ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All),					
					new ParameterNumericRange<int>("Epsilon", 0, 50, 5)
				}
			);

			Console.WriteLine(CSVFileHelper.GetRecord(' ',
									 "DATASET",
									 "METHOD",
									 "IDENTYFICATION",
									 "VOTETYPE",
									 "MIN_LEN",
									 "MAX_LEN",
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
					parms.AddParameter(ReductGeneratorParamHelper.DataStore, trnDataReplaced);
				else
					parms.AddParameter(ReductGeneratorParamHelper.DataStore, trnDataOrig);

				parms.AddParameter(ReductGeneratorParamHelper.FactoryKey, factoryKey);
				parms.AddParameter(ReductGeneratorParamHelper.IdentificationType, (Func<long, IReduct, EquivalenceClass, decimal>)RuleQuality.ConfidenceW);
                parms.AddParameter(ReductGeneratorParamHelper.VoteType, (Func<long, IReduct, EquivalenceClass, decimal>)RuleQuality.ConfidenceW);
				parms.AddParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, 1);
				parms.AddParameter(ReductGeneratorParamHelper.MaxIterations, iter);
				parms.AddParameter(ReductGeneratorParamHelper.UpdateWeights, updateWeights);
				parms.AddParameter(ReductGeneratorParamHelper.Epsilon, (decimal)epsilon / 100.0M);

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

				parms.AddParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
				parms.AddParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, checkEnsembleErrorDuringTraining);				

				ReductEnsembleBoostingGenerator reductGenerator = (ReductEnsembleBoostingGenerator) ReductFactory.GetReductGenerator(parms);
				reductGenerator.MaxReductLength = (int)System.Math.Floor(System.Math.Log((double)numOfAttr + 1.0, 2.0));
				reductGenerator.Generate();

				RoughClassifier classifierTrn = new RoughClassifier();
				classifierTrn.ReductStoreCollection = reductGenerator.GetReductGroups();
				classifierTrn.Classify(trnDataOrig.DataStoreInfo.HasMissingData ? trnDataReplaced : trnDataOrig, 
                                       reductGenerator.IdentyficationType, reductGenerator.VoteType);
				ClassificationResult resultTrn = classifierTrn.Vote(trnDataOrig.DataStoreInfo.HasMissingData ? trnDataReplaced : trnDataOrig,
																	reductGenerator.IdentyficationType, reductGenerator.VoteType,
																	null);

				RoughClassifier classifierTst = new RoughClassifier();
				classifierTst.ReductStoreCollection = reductGenerator.GetReductGroups();
                classifierTst.Classify(tstDataOrig, RuleQuality.ConfidenceW, RuleQuality.ConfidenceW);
				ClassificationResult resultTst = classifierTst.Vote(tstDataOrig, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

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
									reductGenerator.MinReductLength,
									reductGenerator.MaxReductLength,
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
