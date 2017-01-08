using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GenericParsing;
using Infovision.Data;
using Infovision.MachineLearning.Experimenter.Parms;
using Infovision.MachineLearning.Roughset;
using Infovision.Core;
using Infovision.MachineLearning;
using Infovision.Core.Data;
using Infovision.MachineLearning.Weighting;
using Infovision.MachineLearning.Permutations;
using Infovision.MachineLearning.Discretization;

namespace DisesorTuning
{
    public class Program
    {
        private DataStore train, test;
        private Dictionary<string, string> metadataDict;

        private static void Main(string[] args)
        {
            Program p = new Program();

            p.LoadMetadata();
            p.LoadData();

            p.Run();
        }

        public void Run()
        {
            double reductionStepRatio = 0.025;
            double shuffleRatio = 0.33;
            double minimumVoteValue = Double.MinValue;
            bool boostingCheckEnsambleErrorDuringTraining = false;
            int numberOfWeightResets = 99;

            long[] decisionValues = train.DataStoreInfo.GetDecisionValues().ToArray();
            long warningLabel = train.DataStoreInfo.DecisionInfo.External2Internal("warning");
            long normalLabel = train.DataStoreInfo.DecisionInfo.External2Internal("normal");

            ParameterCollection parmList = new ParameterCollection(
                new IParameter[] {
					ParameterValueCollection<int>.CreateFromElements("Iterations", //0
						1, 10, 20, 50, 100, 200, 300),
					ParameterValueCollection<string>.CreateFromElements("ModelFactory", //1
						ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate,
						ReductFactoryKeyHelper.ApproximateReductRelativeWeights),
					ParameterValueCollection<int>.CreateFromElements("NumberOfReductsInWeakClassifier", //2
						1, 2, 5, 10, 20, 50, 100, 200, 300),
					new ParameterNumericRange<double>(ReductGeneratorParamHelper.Epsilon, //3
						0.0, 0.50, 0.05),
					ParameterValueCollection<RuleQualityFunction>.CreateFromElements("Voting", //4
						RuleQuality.CoverageW,
						RuleQuality.ConfidenceW,
						RuleQuality.SingleVote),
					ParameterValueCollection<RuleQualityFunction>.CreateFromElements("Identification", //5
						RuleQuality.CoverageW,
						RuleQuality.ConfidenceW),					
					ParameterValueCollection<WeightGeneratorType>.CreateFromElements("WeightGeneratorType", //6
						WeightGeneratorType.Relative,
						WeightGeneratorType.Majority),
					ParameterValueCollection<bool>.CreateFromElements("FixedPermutation",
						false,
						true)
				});

            int i = 0;
            foreach (object[] p in parmList.Values())
            {
                i++;

                string resultFile = String.Format("result_{0}.csv", i);
                string reportFile = String.Format("report_{0}.txt", i);

                int iterations = (int)p[0];
                string innerFactoryKey = (string)p[1];
                int weakClassifierSize = (int)p[2];
                double eps = (double)p[3];
                RuleQualityFunction voting = (RuleQualityFunction)p[4];
                RuleQualityFunction identification = (RuleQualityFunction)p[5];                
                WeightGeneratorType weightGeneratorType = (WeightGeneratorType)p[6];
                bool fixedPermutation = (bool)p[8];

                WeightGenerator wGen = WeightGenerator.Construct(weightGeneratorType, train);

                Args innerArgs = new Args();
                innerArgs.SetParameter(ReductGeneratorParamHelper.TrainData, train);
                innerArgs.SetParameter(ReductGeneratorParamHelper.FactoryKey, innerFactoryKey);
                innerArgs.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                innerArgs.SetParameter(ReductGeneratorParamHelper.WeightGenerator, wGen);
                innerArgs.SetParameter(ReductGeneratorParamHelper.ReductionStep,
                    (int)(train.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * reductionStepRatio));
                innerArgs.SetParameter(ReductGeneratorParamHelper.PermuatationGenerator,
                    new PermutationGeneratorFieldQuality(train, wGen, eps,
                        (int)(train.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * shuffleRatio)));

                Args args = new Args();
                args.SetParameter(ReductGeneratorParamHelper.TrainData, train);
                args.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.ReductEnsembleBoosting);
                args.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
                args.SetParameter(ReductGeneratorParamHelper.WeightGenerator, wGen);

                args.SetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, weakClassifierSize);
                args.SetParameter(ReductGeneratorParamHelper.IdentificationType, identification);
                args.SetParameter(ReductGeneratorParamHelper.VoteType, voting);
                args.SetParameter(ReductGeneratorParamHelper.MinimumVoteValue, minimumVoteValue);
                args.SetParameter(ReductGeneratorParamHelper.UpdateWeights, (UpdateWeightsDelegate)ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All);
                args.SetParameter(ReductGeneratorParamHelper.CalcModelConfidence, (CalcModelConfidenceDelegate)ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1);
                args.SetParameter(ReductGeneratorParamHelper.MaxIterations, iterations);
                args.SetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, boostingCheckEnsambleErrorDuringTraining);
                args.SetParameter(ReductGeneratorParamHelper.MaxNumberOfWeightResets, numberOfWeightResets);
                args.SetParameter(ReductGeneratorParamHelper.FixedPermutations, fixedPermutation);

                args.SetParameter(ReductGeneratorParamHelper.InnerParameters, innerArgs);

                IReductGenerator generator = ReductFactory.GetReductGenerator(args);
                generator.Run();
                IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();

                RoughClassifier classifier = new RoughClassifier(
                    reductStoreCollection,
                    identification,
                    voting,
                    decisionValues);

                classifier.MinimumVoteValue = minimumVoteValue;

                int unclassified = 0;
                double[] votes = new double[test.NumberOfRecords];
                int[] indices = Enumerable.Range(0, test.NumberOfRecords).ToArray();

                for (int k = 0; k < test.NumberOfRecords; k++)
                {
                    DataRecordInternal record = test.GetRecordByIndex(k);
                    var prediction = classifier.Classify(record);

                    double sum = 0.0;
                    foreach (var kvp in prediction)
                    {
                        if (kvp.Key != Infovision.MachineLearning.Classification.Classifier.UnclassifiedOutput)
                            sum += kvp.Value;
                    }

                    if (prediction.Count == 0 || (prediction.Count == 1 && prediction.ContainsKey(Infovision.MachineLearning.Classification.Classifier.UnclassifiedOutput)))
                        unclassified++;

                    double warning = prediction.ContainsKey(warningLabel) ? prediction[warningLabel] : 0.0;
                    votes[i] = sum > 0 ? warning / sum : 0.0;
                }

                Array.Sort(votes, indices);

                votes = null;

                int[] rank = new int[test.NumberOfRecords];
                for (int k = 0; k < test.NumberOfRecords; k++)
                    rank[indices[k]] = k;

                using (StreamWriter file = new System.IO.StreamWriter(resultFile))
                {
                    for (int k = 0; k < test.NumberOfRecords; k++)
                    {
                        file.WriteLine(rank[k].ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        private void LoadData()
        {
            string trainfile = @"c:\data\disesor\trainingData.csv";
            string trainfile_merge = @"c:\data\disesor\trainingData_merge.csv";
            string testfile = @"c:\data\disesor\testData.csv";
            string testfile_merge = @"c:\data\disesor\testData_merge.csv";
            string labelfile = @"c:\data\disesor\trainingLabels.csv";
                       
            Console.Write("Loading raw train data...");
            DataTable rawTrainData;
            using (GenericParserAdapter gpa = new GenericParserAdapter(trainfile))
            {
                gpa.ColumnDelimiter = ",".ToCharArray()[0];
                gpa.FirstRowHasHeader = false;
                gpa.IncludeFileLineNumber = false;

                rawTrainData = gpa.GetDataTable();
            }
            Console.WriteLine("Done");

            Console.Write("Updating raw train data...");
            foreach (DataRow row in rawTrainData.Rows)
            {
                string oldValue = row.Field<string>(0);
                string newValue = metadataDict[oldValue];
                row.SetField(0, newValue);
            }
            Console.WriteLine("Done");

            Console.Write("Saving merged raw train data...");
            rawTrainData.Dumb(trainfile_merge, ",");
            rawTrainData = null;
            Console.WriteLine("Done");

            Console.Write("Loading raw test data...");
            DataTable rawTestData;
            using (GenericParserAdapter gpa = new GenericParserAdapter(testfile))
            {
                gpa.ColumnDelimiter = ",".ToCharArray()[0];
                gpa.FirstRowHasHeader = false;
                gpa.IncludeFileLineNumber = false;

                rawTestData = gpa.GetDataTable();
            }
            Console.WriteLine("Done");

            Console.Write("Updating raw test data...");
            foreach (DataRow row in rawTestData.Rows)
            {
                string oldValue = row.Field<string>(0);
                string newValue = metadataDict[oldValue];
                row.SetField(0, newValue);
            }
            Console.WriteLine("Done");

            Console.Write("Saving merged raw test data...");
            rawTestData.Dumb(testfile_merge, ",");
            rawTestData = null;
            Console.WriteLine("Done");

            Console.Write("Loading training data store...");
            train = DataStore.Load(trainfile_merge, FileFormat.Csv);
            Console.WriteLine("Done");

            Console.Write("Loading test data...");
            test = DataStore.Load(testfile_merge, FileFormat.Csv, train.DataStoreInfo);
            test.SetDecisionFieldId(-1);
            Console.WriteLine("Done");

            Console.Write("Loading labels...");
            DataStore labels = DataStore.Load(labelfile, FileFormat.Csv);
            int decisionFieldId = train.AddColumn<string>(labels.GetColumn<string>(1));
            labels = null;
            train.SetDecisionFieldId(decisionFieldId);

            Console.WriteLine("Done");

            Console.Write("Discretizing data...");
            var discretizer = new DataStoreDiscretizer(new DiscretizeFayyad());                            
            foreach (DataFieldInfo field in train.DataStoreInfo.GetFields(FieldTypes.Standard))
            {
                Console.WriteLine("Atribute {0} has type {1} and {2} distinct values.",
                    field.Id,
                    field.FieldValueType,
                    field.NumberOfValues);                       

                if (field.CanDiscretize())
                {
                    long[] cuts = discretizer.GetCuts(train, field.Id, null);
                    Console.WriteLine(this.Cuts2Sting(cuts));
                }
            }
                
            discretizer.Discretize(train, train.Weights);
            discretizer.Discretize(test, train);
            
            Console.WriteLine("Done");
        }

        private void LoadMetadata()
        {
            Console.Write("Loading metadata...");
            metadataDict = new Dictionary<string, string>(24);
            /*
            metadataDict.Add("146", "_146,ściana 5,Partia F,416,ZZ,2,a");
            metadataDict.Add("149", "_149,ściana 5,Partia F,418,ZZ,2.2,b");
            metadataDict.Add("155", "_155,ściana 3,Partia H,502,ZZ,2.7,b");
            metadataDict.Add("171", "_171,ściana 1,Partia F,409,ZZ,2,a");
            metadataDict.Add("264", "_264,sc. i100,Z,405/2,ZZ,3.5,b");
            metadataDict.Add("373", "_373,1_Ściana M-12,G-1,707/2,ZZ,1.6,b");
            metadataDict.Add("437", "_437,1_Ściana M-5,G-1,712/1-2,ZZ,3,b");
            metadataDict.Add("470", "_470,sc. i101,Z,405/2,ZZ,3.8,c");
            metadataDict.Add("479", "_479,2_Ściana W-4,G - 2,505,ZZ,4,a");
            metadataDict.Add("490", "_490,śc.h51,B,405/1,ZZ,1.9,a");
            metadataDict.Add("508", "_508,śc.i61,B,405/2,ZZ,2.8,a");
            metadataDict.Add("541", "_541,KG1 Sc_521,Dz,510,ZZ,4.4,b");
            metadataDict.Add("575", "_575,1_Ściana M-4,G-1,712/1-2,ZZ,3,b");
            metadataDict.Add("583", "_583,KG1 Sc_550,Dw,510,ZZ,4,b");
            metadataDict.Add("599", "_599,3_Ściana C-2a,G-3,505,ZZ,3,a");
            metadataDict.Add("607", "_607,KG2 Sc_510,Az,501,ZZ,3.8,b");
            metadataDict.Add("641", "_641,2_Ściana C-3,G-2,503-504,ZZ,3.8,a");
            metadataDict.Add("689", "_689,KG2 Sc_560,Dw,510,ZZ,3,b");
            metadataDict.Add("703", "_703,1_Ściana M-3,G-1,712/1-2,ZZ,3,a");
            metadataDict.Add("725", "_725,Ściana 2,12,506,ZZ,2.2,b");
            metadataDict.Add("765", "_765,Ściana 713,13,401,ZZ,1.4,a");
            metadataDict.Add("777", "_777,Ściana 003,9,504,ZZ,3.4,b");
            metadataDict.Add("793", "_793,Ściana 839a,0,405,ZZ,3.4,b");
            metadataDict.Add("799", "_799,Ściana 026,9,504,ZZ,3.2,a");
            */

            metadataDict.Add("146", "_146,ściana 5,Partia F,416,2,a");
            metadataDict.Add("149", "_149,ściana 5,Partia F,418,2.2,b");
            metadataDict.Add("155", "_155,ściana 3,Partia H,502,2.7,b");
            metadataDict.Add("171", "_171,ściana 1,Partia F,409,2,a");
            metadataDict.Add("264", "_264,sc. i100,Z,405/2,3.5,b");
            metadataDict.Add("373", "_373,1_Ściana M-12,G-1,707/2,1.6,b");
            metadataDict.Add("437", "_437,1_Ściana M-5,G-1,712/1-2,3,b");
            metadataDict.Add("470", "_470,sc. i101,Z,405/2,3.8,c");
            metadataDict.Add("479", "_479,2_Ściana W-4,G - 2,505,4,a");
            metadataDict.Add("490", "_490,śc.h51,B,405/1,1.9,a");
            metadataDict.Add("508", "_508,śc.i61,B,405/2,2.8,a");
            metadataDict.Add("541", "_541,KG1 Sc_521,Dz,510,4.4,b");
            metadataDict.Add("575", "_575,1_Ściana M-4,G-1,712/1-2,3,b");
            metadataDict.Add("583", "_583,KG1 Sc_550,Dw,510,4,b");
            metadataDict.Add("599", "_599,3_Ściana C-2a,G-3,505,3,a");
            metadataDict.Add("607", "_607,KG2 Sc_510,Az,501,3.8,b");
            metadataDict.Add("641", "_641,2_Ściana C-3,G-2,503-504,3.8,a");
            metadataDict.Add("689", "_689,KG2 Sc_560,Dw,510,3,b");
            metadataDict.Add("703", "_703,1_Ściana M-3,G-1,712/1-2,3,a");
            metadataDict.Add("725", "_725,Ściana 2,12,506,2.2,b");
            metadataDict.Add("765", "_765,Ściana 713,13,401,1.4,a");
            metadataDict.Add("777", "_777,Ściana 003,9,504,3.4,b");
            metadataDict.Add("793", "_793,Ściana 839a,0,405,3.4,b");
            metadataDict.Add("799", "_799,Ściana 026,9,504,3.2,a");

            foreach (var id in metadataDict.Keys.ToArray())
            {
                metadataDict[id] = metadataDict[id].Replace(' ', '_');
            }
            Console.WriteLine("Done");
        }

        private string Cuts2Sting(long[] cuts)
        {
            StringBuilder sb = new StringBuilder();

            if (cuts == null || cuts.Length == 0)
            {
                sb.AppendLine("No Cuts !!!");
                return sb.ToString();
            }

            if (cuts.Length > 0)
                sb.AppendLine(String.Format("{0}: <{1} {2})", 0, "-Inf", cuts[0]));

            for (int i = 1; i < cuts.Length; i++)
                sb.AppendLine(String.Format("{0}: <{1} {2})", i, cuts[i - 1], cuts[i]));

            if (cuts.Length > 0)
                sb.AppendLine(String.Format("{0}: <{1} {2})", cuts.Length, cuts[cuts.Length - 1], "+Inf"));

            return sb.ToString();
        }
    }
}