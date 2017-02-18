using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GenericParsing;
using Raccoon.Data;
using Raccoon.MachineLearning.Roughset;
using Raccoon.Core;
using Raccoon.Core.Data;
using Raccoon.MachineLearning.Weighting;
using Raccoon.MachineLearning.Permutations;
using Raccoon.MachineLearning.Discretization;

namespace DisesorTest
{
    public class Program
    {
        #region Member variables

        private static string trainfile = @"c:\data\disesor\trainingData.csv";
        private static string trainfile_merge = @"c:\data\disesor\trainingData_merge.csv";
        private static string testfile = @"c:\data\disesor\testData.csv";
        private static string testfile_merge = @"c:\data\disesor\testData_merge.csv";
        private static string labelfile = @"c:\data\disesor\trainingLabels.csv";
        private static string outputfile = @"c:\data\disesor\result.csv";
        private static string columnNames = @"c:\data\disesor\columnNames.txt";
        //static string weightsoutput = @"c:\data\disesor\weights.csv";

        private Dictionary<string, string> metadataDict;

        //string factoryKey = ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate;
        private string factoryKey = ReductTypes.ReductEnsembleBoosting;

        //int numberOfPermutations = 100;
        //double epsilon = 0.0m;

        private RuleQualityMethod identificationFunction = RuleQualityMethods.ConfidenceW;
        private RuleQualityMethod voteFunction = RuleQualityMethods.CoverageW;
        private WeightGeneratorType weightGeneratorType = WeightGeneratorType.Relative;                

        private string innerFactoryKey = ReductTypes.GeneralizedMajorityDecisionApproximate;
        //double innerEpsilon = 0.4m;
        //int boostingNumberOfReductsInWeakClassifier = 20;
        //int boostingMaxIterations = 100;

        //RuleQualityFunction boostingIdentificationFunction = null;
        //RuleQualityFunction boostingVoteFunction = null;
        private UpdateWeightsDelegate boostingUpdateWeights = ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All;

        private CalcModelConfidenceDelegate boostingCalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
        private bool boostingCheckEnsambleErrorDuringTraining = false;
        private int numberOfWeightResets = 99;

        private double minimumVoteValue = Double.MinValue; //0.00001m;
        private bool fixedPermutations = false;
        private double reductionStepRatio = 0.1;
        private double shuffleRatio = 1.0;
        private bool useClassificationCost = true;

        public Program()
        {
            //if (boostingIdentificationFunction == null)
            //    boostingIdentificationFunction = identificationFunction;

            //if (boostingVoteFunction == null)
            //    boostingVoteFunction = voteFunction;
        }

        #endregion Member variables

        public static void Main(string[] args)
        {
            Program p = new Program();

            if (args.Length != 3)
            {
                Console.WriteLine("Invalid number of parameters. Parameters should be: #Iterations #WeakClassifierSize #Epsilon");

                throw new InvalidProgramException("Invalid number of parameters. Parameters should be: #Iterations #WeakClassifierSize #Epsilon");
            }

            int iter = Int32.Parse(args[0]);
            int weak = Int32.Parse(args[1]);
            double eps = Double.Parse(args[2], CultureInfo.InvariantCulture);

            p.FinalTest(iter, weak, eps);

            Console.Beep();
        }

        private void FinalTest(int iterations, int weakClassifiers, double eps)
        {
            Console.WriteLine("Algorithm: {0}", factoryKey);
            Console.WriteLine("Number of permutations: {0}", weakClassifiers);
            Console.WriteLine("Epsilon: {0}", eps);
            Console.WriteLine("Decision identification: {0}", identificationFunction.Method.Name);
            Console.WriteLine("Voting method: {0}", voteFunction.Method.Name);
            Console.WriteLine("Minimum vote key: {0}", minimumVoteValue);
            Console.WriteLine("Weighting generator: {0}", weightGeneratorType);
            Console.WriteLine();            
            Console.WriteLine("Using Fayyad and Irani MDL supervised discretization");

            if (factoryKey.Contains("Boosting"))
            {
                Console.WriteLine("Boosting - Inner model: {0}", innerFactoryKey);
                //Console.WriteLine("Boosting - Numer of reducts in single model: {0}", boostingNumberOfReductsInWeakClassifier);
                Console.WriteLine("Boosting - Numer of reducts in single model: {0}", weakClassifiers);
                //Console.WriteLine("Boosting - Identification function: {0}", boostingIdentificationFunction.Method.Name);
                //Console.WriteLine("Boosting - Voting fnction: {0}", boostingVoteFunction.Method.Name);
                //Console.WriteLine("Boosting - Max iterations: {0}", boostingMaxIterations);
                Console.WriteLine("Boosting - Max iterations: {0}", iterations);
                Console.WriteLine("Boosting - Update weights method: {0}", boostingUpdateWeights.Method.Name);
                Console.WriteLine("Boosting - Model confidence calulate method: {0}", boostingCalcModelConfidence.Method.Name);
                Console.WriteLine("Boosting - Check error during training: {0}", boostingCheckEnsambleErrorDuringTraining);
                Console.WriteLine("Boosting - Inner model epsilon: {0}", eps);
                Console.WriteLine("Boosting - Max number of weights resets: {0}", numberOfWeightResets);
                Console.WriteLine("Boosting - Fixed permutations: {0}", fixedPermutations);
                Console.WriteLine("Boosting - Classification cost: {0}", useClassificationCost);
                if (useClassificationCost)
                {
                    //TODO print costs
                }
            }

            Console.WriteLine("Reduction step ratio: {0}", reductionStepRatio);
            Console.WriteLine("Permutation shuffle ratio: {0}", shuffleRatio);

            this.LoadMetadata();

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
            DataStore train = DataStore.Load(trainfile_merge, FileFormat.CSV);
            Console.WriteLine("Done");

            Console.Write("Loading test data...");
            DataStore test = DataStore.Load(testfile_merge, FileFormat.CSV, train.DataStoreInfo);
            test.SetDecisionFieldId(-1);
            Console.WriteLine("Done");

            Console.Write("Loading labels...");
            DataStore labels = DataStore.Load(labelfile, FileFormat.CSV);
            int decisionFieldId = train.AddColumn<string>(labels.GetColumn<string>(1));
            labels = null;
            train.SetDecisionFieldId(decisionFieldId);
            long[] decisionValues = train.DataStoreInfo.GetDecisionValues().ToArray();
            long warningLabel = train.DataStoreInfo.DecisionInfo.External2Internal("warning");
            long normalLabel = train.DataStoreInfo.DecisionInfo.External2Internal("normal");
            Console.WriteLine("Done");

            WeightGenerator wGen = WeightGenerator.Construct(weightGeneratorType, train);

            int fieldId = 0;
            string[] names = System.IO.File.ReadAllLines(columnNames);
            foreach (string name in names)
            {
                fieldId++;
                DataFieldInfo trainInfo = train.DataStoreInfo.GetFieldInfo(fieldId);
                trainInfo.Alias = name;

                DataFieldInfo testInfo = test.DataStoreInfo.GetFieldInfo(fieldId);
                testInfo.Alias = name;
            }

            Console.Write("Discretizing data...");

            var discretizer = new DataStoreDiscretizer();
                                
            foreach (DataFieldInfo field in train.DataStoreInfo.GetFields(FieldGroup.Standard))
            {
                Console.WriteLine("Atribute {0} {1} as type {2} and {3} distinct values. {4} be discretized",
                    field.Id,
                    field.Alias,
                    field.FieldValueType,
                    field.NumberOfValues,
                    field.CanDiscretize() ? "Can" : "Cannot");

                if (field.CanDiscretize())
                {
                    long[] cuts = discretizer.GetCuts(train, field.Id, wGen.Weights);
                    Console.WriteLine(this.Cuts2Sting(cuts));
                }
            }

            discretizer.Discretize(train, wGen.Weights);
            DataStoreDiscretizer.Discretize(test, train);
                        
            Console.WriteLine("Done");

            Args innerArgs = new Args();
            innerArgs.SetParameter(ReductFactoryOptions.DecisionTable, train);
            innerArgs.SetParameter(ReductFactoryOptions.ReductType, innerFactoryKey);
            //innerArgs.SetParameter(ReductGeneratorParamHelper.Epsilon, innerEpsilon);
            innerArgs.SetParameter(ReductFactoryOptions.Epsilon, eps);
            innerArgs.SetParameter(ReductFactoryOptions.WeightGenerator, wGen);
            innerArgs.SetParameter(ReductFactoryOptions.ReductionStep,
                (int)(train.DataStoreInfo.GetNumberOfFields(FieldGroup.Standard) * reductionStepRatio)); //10% reduction step

            innerArgs.SetParameter(ReductFactoryOptions.PermuatationGenerator,
                new PermutationGeneratorFieldQuality(train, wGen, eps,
                    (int)(train.DataStoreInfo.GetNumberOfFields(FieldGroup.Standard) * shuffleRatio)));

            Args args = new Args();
            args.SetParameter(ReductFactoryOptions.DecisionTable, train);
            args.SetParameter(ReductFactoryOptions.ReductType, factoryKey);
            args.SetParameter(ReductFactoryOptions.Epsilon, eps);
            args.SetParameter(ReductFactoryOptions.PermutationCollection,
                ReductFactory.GetPermutationGenerator(args).Generate(weakClassifiers));
            args.SetParameter(ReductFactoryOptions.WeightGenerator, wGen);

            //args.SetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, boostingNumberOfReductsInWeakClassifier);
            args.SetParameter(ReductFactoryOptions.NumberOfReductsInWeakClassifier, weakClassifiers);
            args.SetParameter(ReductFactoryOptions.IdentificationType, identificationFunction);
            args.SetParameter(ReductFactoryOptions.VoteType, voteFunction);
            args.SetParameter(ReductFactoryOptions.MinimumVoteValue, minimumVoteValue);
            args.SetParameter(ReductFactoryOptions.UpdateWeights, boostingUpdateWeights);
            args.SetParameter(ReductFactoryOptions.CalcModelConfidence, boostingCalcModelConfidence);
            //args.SetParameter(ReductGeneratorParamHelper.MaxIterations, boostingMaxIterations);
            args.SetParameter(ReductFactoryOptions.MaxIterations, iterations);
            args.SetParameter(ReductFactoryOptions.CheckEnsembleErrorDuringTraining, boostingCheckEnsambleErrorDuringTraining);
            args.SetParameter(ReductFactoryOptions.MaxNumberOfWeightResets, numberOfWeightResets);
            args.SetParameter(ReductFactoryOptions.FixedPermutations, fixedPermutations);
            args.SetParameter(ReductFactoryOptions.UseClassificationCost, useClassificationCost);

            args.SetParameter(ReductFactoryOptions.InnerParameters, innerArgs);

            /*
            using (StreamWriter f = new StreamWriter(weightsoutput))
            {
                for(int i = 0; i < wGen.Weights.Length; i++)
                {
                    f.WriteLine(wGen.Weights[i].ToString(CultureInfo.InvariantCulture));
                }
            }
            */

            //InformationMeasureWeights measure = new InformationMeasureWeights();
            //double q = measure.Calc(new ReductWeights(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard), wGen.Weights, epsilon));
            //Console.WriteLine("Traing dataset quality: {0}", q);
            //measure = null;

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            Console.WriteLine("Reduct generation...");

            IReductGenerator generator = ReductFactory.GetReductGenerator(args);
            generator.Run();
            IReductStoreCollection reductStoreCollection = generator.GetReductStoreCollection();
            Console.WriteLine("Done");

            //foreach (IReductStore reductStore in reductStoreCollection)
            //{
            //    foreach (IReduct reduct in reductStore)
            //    {
            //        Console.WriteLine(reduct);
            //        //Console.WriteLine(reduct.Weights.ToStr());
            //    }
            //}

            Console.Write("Classification...");
            RoughClassifier classifier = new RoughClassifier(
                reductStoreCollection,
                identificationFunction,
                voteFunction,
                decisionValues);

            classifier.MinimumVoteValue = minimumVoteValue;

            int unclassified = 0;
            double[] votes = new double[test.NumberOfRecords];
            int[] indices = Enumerable.Range(0, test.NumberOfRecords).ToArray();

            for (int i = 0; i < test.NumberOfRecords; i++)
            {
                DataRecordInternal record = test.GetRecordByIndex(i);
                var prediction = classifier.Classify(record);

                double sum = 0.0;
                foreach (var kvp in prediction)
                {
                    if (kvp.Key != -1)
                        sum += kvp.Value;
                }

                if (prediction.Count == 0 || (prediction.Count == 1 && prediction.ContainsKey(-1)))
                    unclassified++;

                double warning = prediction.ContainsKey(warningLabel) ? prediction[warningLabel] : 0.0;
                votes[i] = sum > 0 ? warning / sum : 0.0;
            }

            Array.Sort(votes, indices);

            votes = null;

            int[] rank = new int[test.NumberOfRecords];
            for (int i = 0; i < test.NumberOfRecords; i++)
            {
                rank[indices[i]] = i;
            }

            using (StreamWriter file = new System.IO.StreamWriter(outputfile))
            {
                for (int i = 0; i < test.NumberOfRecords; i++)
                {
                    file.WriteLine(rank[i].ToString(CultureInfo.InvariantCulture));
                }
            }

            Console.WriteLine("Done");
            Console.WriteLine("Unclassified: {0}", unclassified);
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