using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericParsing;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Filters;
using Infovision.Datamining.Filters.Unsupervised.Attribute;
using Infovision.Datamining.Roughset;
using Infovision.Utils;

namespace DisesorTest
{
    public class Program
    {
        #region Member variables
        
        static string trainfile = @"c:\data\disesor\trainingData.csv";
        static string trainfile_merge = @"c:\data\disesor\trainingData_merge.csv";
        static string testfile = @"c:\data\disesor\testData.csv";
        static string testfile_merge = @"c:\data\disesor\testData_merge.csv";
        static string labelfile = @"c:\data\disesor\trainingLabels.csv";
        static string outputfile = @"c:\data\disesor\result.csv";
        static string columnNames = @"c:\data\disesor\columnNames.txt";
        //static string weightsoutput = @"c:\data\disesor\weights.csv";                        

        private Dictionary<string, string> metadataDict;

        //string factoryKey = ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate;
        string factoryKey = ReductFactoryKeyHelper.ReductEnsembleBoosting;
        //int numberOfPermutations = 100;
        //decimal epsilon = 0.0m;

        RuleQualityFunction identificationFunction = RuleQuality.ConfidenceW;
        RuleQualityFunction voteFunction = RuleQuality.CoverageW;
        WeightGeneratorType weightGeneratorType = WeightGeneratorType.Relative;
        
        bool useSupervisedDiscetization = true;
        DiscretizationType discretizationType = DiscretizationType.Supervised_FayyadAndIranisMDL_BetterEncoding;
        bool useWeightsInDiscretization = false;
        bool useBetterEncoding = true;
        bool useKokonenkoMDL = true;
        int numberOfBins = 2;

        string innerFactoryKey = ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate;                
        //decimal innerEpsilon = 0.4m;
        //int boostingNumberOfReductsInWeakClassifier = 20;
        //int boostingMaxIterations = 100;

        //RuleQualityFunction boostingIdentificationFunction = null;
        //RuleQualityFunction boostingVoteFunction = null;        
        UpdateWeightsDelegate boostingUpdateWeights = ReductEnsembleBoostingGenerator.UpdateWeightsAdaBoost_All;
        CalcModelConfidenceDelegate boostingCalcModelConfidence = ReductEnsembleBoostingGenerator.ModelConfidenceAdaBoostM1;
        bool boostingCheckEnsambleErrorDuringTraining = false;
        int numberOfWeightResets = 99;

        decimal minimumVoteValue = Decimal.MinValue; //0.00001m;
        bool fixedPermutations = false;
        double reductionStepRatio = 0.1;
        double shuffleRatio = 1.0;
        bool useClassificationCost = true;
       
        public Program()
        {
            //if (boostingIdentificationFunction == null)
            //    boostingIdentificationFunction = identificationFunction;

            //if (boostingVoteFunction == null)
            //    boostingVoteFunction = voteFunction;
        }

        #endregion

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
            decimal eps = Decimal.Parse(args[2], CultureInfo.InvariantCulture);

            p.FinalTest(iter, weak, eps);

            Console.Beep();
        }

        private void FinalTest(int iterations, int weakClassifiers, decimal eps)
        {
            Console.WriteLine("Algorithm: {0}", factoryKey);
            Console.WriteLine("Number of permutations: {0}", weakClassifiers);
            Console.WriteLine("Epsilon: {0}", eps);
            Console.WriteLine("Decision identification: {0}", identificationFunction.Method.Name);
            Console.WriteLine("Voting method: {0}", voteFunction.Method.Name);
            Console.WriteLine("Minimum vote value: {0}", minimumVoteValue);
            Console.WriteLine("Weighting generator: {0}", weightGeneratorType);
            Console.WriteLine();
                                    
            Console.WriteLine("Use weights in discretization: {0}", useWeightsInDiscretization);            
            Console.WriteLine("Is discretization (S)upervised or (U)nsupervised: {0}", useSupervisedDiscetization ? "S" : "U");
            Console.WriteLine();
            
            if (useSupervisedDiscetization)
            {
                Console.WriteLine("(S) Use better encoding: {0}", useSupervisedDiscetization);
                Console.WriteLine("(S) Use Kononenko MDL criteriaon: {0}", useKokonenkoMDL);
                Console.WriteLine("(S) Use Fayyad and Iranis MDL criteriaon: {0}", !useKokonenkoMDL);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("(U) Discretization type: {0}", discretizationType);
                
                if (discretizationType == DiscretizationType.Unsupervised_EqualWidth)
                    Console.WriteLine("(U) Number of bins: {0}", numberOfBins);
                
                Console.WriteLine();
            }

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
            rawTrainData.WriteToCSVFile(trainfile_merge, ",");
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
            rawTestData.WriteToCSVFile(testfile_merge, ",");
            rawTestData = null;
            Console.WriteLine("Done");

            Console.Write("Loading training data store...");
            DataStore train = DataStore.Load(trainfile_merge, FileFormat.Csv);
            Console.WriteLine("Done");
            
            Console.Write("Loading test data...");
            DataStore test = DataStore.Load(testfile_merge, FileFormat.Csv, train.DataStoreInfo);
            test.SetDecisionFieldId(-1);
            Console.WriteLine("Done");

            Console.Write("Loading labels...");
            DataStore labels = DataStore.Load(labelfile, FileFormat.Csv);
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
            foreach(string name in names)
            {
                fieldId++;
                DataFieldInfo trainInfo = train.DataStoreInfo.GetFieldInfo(fieldId);
                trainInfo.Alias = name;

                DataFieldInfo testInfo = test.DataStoreInfo.GetFieldInfo(fieldId);
                testInfo.Alias = name;
            }
            
            Console.Write("Discretizing data...");
           
            if (!useSupervisedDiscetization)
            {
                var discretizer = DataStoreDiscretizer.Construct(discretizationType);
                discretizer.NumberOfBins = numberOfBins;

                foreach (DataFieldInfo field in train.DataStoreInfo.GetFields(FieldTypes.Standard))
                {
                    Console.WriteLine("Atribute {0} {1} has type {2} and {3} distinct values. {4} be discretized", 
                        field.Id,
                        field.Alias,
                        field.FieldValueType,
                        field.Values().Count,
                        field.CanDiscretize() ? "Can" : "Cannot");
                    
                    if (field.CanDiscretize())
                    {
                        double[] cuts = discretizer.GetCuts(train, field.Id, useWeightsInDiscretization ? Array.ConvertAll(wGen.Weights, x => (double)x) : null);
                        Console.WriteLine(this.Cuts2Sting(cuts));
                    }
                }

                discretizer.Discretize(ref train, ref test, useWeightsInDiscretization ? Array.ConvertAll(wGen.Weights, x => (double)x) : null);
            }
            else
            {
                var discretizer = new Infovision.Datamining.Filters.Supervised.Attribute.DataStoreDiscretizer()
                {
                    UseBetterEncoding = useBetterEncoding,
                    UseKononenko = useKokonenkoMDL
                };

                foreach (DataFieldInfo field in train.DataStoreInfo.GetFields(FieldTypes.Standard))
                {
                    Console.WriteLine("Atribute {0} {1} as type {2} and {3} distinct values. {4} be discretized", 
                        field.Id,
                        field.Alias,
                        field.FieldValueType,
                        field.Values().Count,
                        field.CanDiscretize() ? "Can" : "Cannot");

                    if (field.CanDiscretize())
                    {
                        double[] cuts = discretizer.GetCuts(train, field.Id, useWeightsInDiscretization ? Array.ConvertAll(wGen.Weights, x => (double)x) : null);
                        Console.WriteLine(this.Cuts2Sting(cuts));
                    }
                }

                discretizer.Discretize(ref train, ref test, useWeightsInDiscretization ? Array.ConvertAll(wGen.Weights, x => (double)x) : null);
            }            
            Console.WriteLine("Done");            

            Args innerArgs = new Args();
            innerArgs.SetParameter(ReductGeneratorParamHelper.TrainData, train);
            innerArgs.SetParameter(ReductGeneratorParamHelper.FactoryKey, innerFactoryKey);
            //innerArgs.SetParameter(ReductGeneratorParamHelper.Epsilon, innerEpsilon);            
            innerArgs.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            innerArgs.SetParameter(ReductGeneratorParamHelper.WeightGenerator, wGen);
            innerArgs.SetParameter(ReductGeneratorParamHelper.ReductionStep, 
                (int)(train.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * reductionStepRatio)); //10% reduction step
            
            innerArgs.SetParameter(ReductGeneratorParamHelper.PermuatationGenerator, 
                new PermutationGeneratorFieldQuality(train, wGen, eps, 
                    (int)(train.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard) * shuffleRatio)));

            Args args = new Args();
            args.SetParameter(ReductGeneratorParamHelper.TrainData, train);
            args.SetParameter(ReductGeneratorParamHelper.FactoryKey, factoryKey);
            args.SetParameter(ReductGeneratorParamHelper.Epsilon, eps);
            args.SetParameter(ReductGeneratorParamHelper.PermutationCollection, 
                ReductFactory.GetPermutationGenerator(args).Generate(weakClassifiers));
            args.SetParameter(ReductGeneratorParamHelper.WeightGenerator, wGen);

            //args.SetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, boostingNumberOfReductsInWeakClassifier);
            args.SetParameter(ReductGeneratorParamHelper.NumberOfReductsInWeakClassifier, weakClassifiers);
            args.SetParameter(ReductGeneratorParamHelper.IdentificationType, identificationFunction);
            args.SetParameter(ReductGeneratorParamHelper.VoteType, voteFunction);
            args.SetParameter(ReductGeneratorParamHelper.MinimumVoteValue, minimumVoteValue);
            args.SetParameter(ReductGeneratorParamHelper.UpdateWeights, boostingUpdateWeights);
            args.SetParameter(ReductGeneratorParamHelper.CalcModelConfidence, boostingCalcModelConfidence);
            //args.SetParameter(ReductGeneratorParamHelper.MaxIterations, boostingMaxIterations);
            args.SetParameter(ReductGeneratorParamHelper.MaxIterations, iterations);
            args.SetParameter(ReductGeneratorParamHelper.CheckEnsembleErrorDuringTraining, boostingCheckEnsambleErrorDuringTraining);
            args.SetParameter(ReductGeneratorParamHelper.MaxNumberOfWeightResets, numberOfWeightResets);
            args.SetParameter(ReductGeneratorParamHelper.FixedPermutations, fixedPermutations);
            args.SetParameter(ReductGeneratorParamHelper.UseClassificationCost, useClassificationCost);

            args.SetParameter(ReductGeneratorParamHelper.InnerParameters, innerArgs);

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
            //decimal q = measure.Calc(new ReductWeights(train, train.DataStoreInfo.GetFieldIds(FieldTypes.Standard), wGen.Weights, epsilon));
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
            decimal[] votes = new decimal[test.NumberOfRecords];
            int[] indices = Enumerable.Range(0, test.NumberOfRecords).ToArray();
            
            for (int i = 0; i < test.NumberOfRecords; i++)
            {
                DataRecordInternal record = test.GetRecordByIndex(i);
                var prediction = classifier.Classify(record);

                decimal sum = Decimal.Zero;
                foreach (var kvp in prediction)
                {
                    if (kvp.Key != -1)
                        sum += kvp.Value;
                }

                if(prediction.Count == 0 || (prediction.Count == 1 && prediction.ContainsKey(-1)))
                    unclassified++;

                decimal warning = prediction.ContainsKey(warningLabel) ? prediction[warningLabel] : Decimal.Zero;
                votes[i] = sum > 0 ? warning / sum : Decimal.Zero;                 
            }

            Array.Sort(votes, indices);

            votes = null;
                
            int[] rank = new int[test.NumberOfRecords];
            for (int i = 0; i < test.NumberOfRecords; i++)
            {
                rank[indices[i]] = i;
            }
            
            using(StreamWriter file = new System.IO.StreamWriter(outputfile))
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

        private string Cuts2Sting(double[] cuts)
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
