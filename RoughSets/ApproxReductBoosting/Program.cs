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
				
				DataStore trnDataReplaced = new ReplaceMissingValues().Compute(trnDataOrig);
				DataStore tstDataReplaced = new ReplaceMissingValues().Compute(tstDataOrig, trnDataOrig);

				trnDataOrig = trnDataReplaced;
				tstDataOrig = tstDataReplaced;

				Console.WriteLine("Missing values replacing...DONE");
			}            

			int numOfAttr = trnDataOrig.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard);
									
			ParameterCollection parmList = new ParameterCollection(
				new IParameter[] {
					//new ParameterNumericRange<int>("NumberOfIterations", startIteration, maxNumberOfIterations, iterationStep),
					ParameterValueCollection<int>.CreateFromElements<int>("NumberOfIterations", 1, 2, 5, 10, 20, 50, 100),
					new ParameterNumericRange<int>("NumberOfTests", 0, numberOfTests-1, 1),
					ParameterValueCollection<string>.CreateFromElements<string>("ReductFactory", ReductFactoryKeyHelper.ReductEnsembleBoosting,
																								 ReductFactoryKeyHelper.ReductEnsembleBoostingWithAttributeDiversity),
					ParameterValueCollection<WeightingSchema>.CreateFromElements<WeightingSchema>("WeightingSchama", WeightingSchema.Majority),
					ParameterValueCollection<bool>.CreateFromElements<bool>("CheckEnsembleErrorDuringTraining", false),
					ParameterValueCollection<UpdateWeightsDelegate>.CreateFromElements<UpdateWeightsDelegate>("UpdateWeights", ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All),
					//ParameterValueCollection<int>.CreateFromElements<int>("MinLenght", (int) System.Math.Floor(System.Math.Log((double)numOfAttr + 1.0, 2.0)))
					ParameterValueCollection<int>.CreateFromElements<int>("MinLenght", 1)
				}
			);

			Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}",
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
				UpdateWeightsDelegate updateWeights = (UpdateWeightsDelegate)p[5];
				int minLen = (int)p[6];
				
				Args parms = new Args();
				parms.AddParameter(ReductGeneratorParamHelper.DataStore, trnDataOrig);
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
				parms.AddParameter(ReductGeneratorParamHelper.MinReductLength, minLen);

				ReductEnsembleBoostingGenerator reductGenerator = (ReductEnsembleBoostingGenerator) ReductFactory.GetReductGenerator(parms);

				ReductCrisp reduct = (ReductCrisp) reductGenerator.GetNextReduct(weightGenerator.Weights, numOfAttr, numOfAttr);
				reductGenerator.MaxReductLength = reduct.Attributes.Count;

				reductGenerator.Generate();

				RoughClassifier classifierTrn = new RoughClassifier();
				classifierTrn.ReductStoreCollection = reductGenerator.GetReductGroups();
				classifierTrn.Classify(trnDataOrig);
				ClassificationResult resultTrn = classifierTrn.Vote(trnDataOrig, reductGenerator.IdentyficationType, reductGenerator.VoteType, null);

				RoughClassifier classifierTst = new RoughClassifier();
				classifierTst.ReductStoreCollection = reductGenerator.GetReductGroups();
				classifierTst.Classify(tstDataOrig);
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

				Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}",
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
									reductGenerator.ReductPool.GetAvgMeasure(new ReductMeasureLength()));
			}
		}
	}
}
