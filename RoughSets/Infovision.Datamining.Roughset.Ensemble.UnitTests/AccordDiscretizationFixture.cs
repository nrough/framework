using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using System.Data;
using System.Collections;
using GenericParsing;
using Accord.Statistics;
using Accord.Statistics.Filters;
using Accord;
using Accord.MachineLearning;
using Accord.MachineLearning.DecisionTrees;
using Accord.Controls;
using Accord.Statistics.Visualizations;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Roughset;
using Infovision.Utils;
using Infovision.Datamining.Roughset;


namespace Infovision.Datamining.Roughset.Ensemble.UnitTests
{
	[TestFixture]
	public class AccordDiscretizationFixture
	{
		#region Member fields
		
		private string reductFactoryKey;
		private string reductMeasureKey;		
		private int numberOfAttributes;
		private int[] attributes;
		private PermutationList permutationList;
		private IdentificationType identificationType;
		private VoteType voteType;
		private int decisionIdx;
		private int idIdx;
		private string filename;
		private string[] nominalAttributes;
		private string[] continuesAttributes;
		private int numberOfFolds;
		private int numberOfHistBins;
		private int numberOfPermutations;

		#endregion

		#region Constructors

		public AccordDiscretizationFixture()
		{
			filename = @"Data\german.data";			
			reductFactoryKey = "ApproximateReductMajority";
			reductMeasureKey = "ReductMeasureLength";

			numberOfAttributes = 20;
			attributes = new int[numberOfAttributes];
			for (int i = 0; i < numberOfAttributes; i++)
			{
				attributes[i] = i + 2;
			}

			decisionIdx = 21;
			idIdx = 0;
			
			identificationType = IdentificationType.Confidence;
			voteType = VoteType.MajorDecision;

			nominalAttributes = new string[] { "a1", "a3", "a4", "a6", "a7", "a9", "a10", "a12", "a14", "a15", "a17", "a19", "a20", "d" };
			continuesAttributes = new string[] { "a2", "a5", "a8", "a11", "a13", "a16", "a18" };
			
			numberOfHistBins = 3;
		}

		#endregion

		#region Test Methods        
	

        //[TestCase(2, 1, 20)]
        //[TestCase(2, 1, 5)]
		//[TestCase(2, 1, 10)]
		//[TestCase(2, 3, 5)]
		//[TestCase(2, 3, 10)]
		//[TestCase(2, 5, 5)]
		//[TestCase(2, 5, 10)]
		//[TestCase(2, 7, 5)]
		//[TestCase(2, 7, 10)]
        
        [TestCase(2, 7, 20)]
		public void LoadDataTable(int cvFolds, int numberOfReducts, int epsilon)
		{
			Console.WriteLine("------ numberOfReducts: {0}, epsilon: {1} ------", numberOfReducts, epsilon);            
			permutationList = new PermutationGenerator(attributes).Generate(numberOfReducts);
			
			DataTable rawData;
			using (GenericParserAdapter gpa = new GenericParserAdapter(filename))
			{
				gpa.ColumnDelimiter = " ".ToCharArray()[0];
				gpa.FirstRowHasHeader = false;
				gpa.IncludeFileLineNumber = true;

				rawData = gpa.GetDataTable();
			}

			rawData.Columns[0].ColumnName = "id";
			for (int i = 1; i <= numberOfAttributes; i++)
				rawData.Columns[i].ColumnName = "a" + i.ToString();
			rawData.Columns[decisionIdx].ColumnName = "d";

			rawData.WriteToCSVFile(String.Format("{0}.csv", "RawData"), " ");

			// Create a new codification codebook to
			// convert strings into integer symbols
			Codification codebook = new Codification(rawData, nominalAttributes);

			DataTable tmpSymbols = codebook.Apply(rawData);
			tmpSymbols.WriteToCSVFile(String.Format("{0}.csv", "TmpSymbols"), " ");

			DataTable symbols = tmpSymbols.Clone();
			symbols.Columns["id"].DataType = typeof(int);
			foreach (string s in continuesAttributes)
			{
				symbols.Columns[s].DataType = typeof(double);
			}
			foreach (DataRow row in tmpSymbols.Rows)
			{
				symbols.ImportRow(row);
			}
			tmpSymbols.Dispose();
			tmpSymbols = null;
						
            int[] folds = CrossValidation.Splittings(symbols.Rows.Count, cvFolds);
			CrossValidation<RoughClassifier> val = new CrossValidation<RoughClassifier>(folds, cvFolds);
			val.RunInParallel = false;           

			val.Fitting = delegate(int k, int[] indicesTrain, int[] indicesValidation)
			{
				DataTable trainingSet = symbols.Subtable(indicesTrain);
				trainingSet.TableName = "Train-" + k.ToString();
				trainingSet.WriteToCSVFile(String.Format("{0}.csv", trainingSet.TableName), " ");

				DataTable validationSet = symbols.Subtable(indicesValidation);
				validationSet.TableName = "Test-" + k.ToString();
				validationSet.WriteToCSVFile(String.Format("{0}.csv", validationSet.TableName), " ");

				//Accord.Statistics.Visualizations.Histogram[] histograms
				//	= new Accord.Statistics.Visualizations.Histogram[continuesAttributes.Length];
				
				Infovision.Datamining.Filters.Unsupervised.Attribute.Discretization[] discretizations
					= new Infovision.Datamining.Filters.Unsupervised.Attribute.Discretization[continuesAttributes.Length];

				DataTable tmp = trainingSet.Clone();

				for (int i = 0; i < continuesAttributes.Length; i++)
				{
					double[] values = trainingSet.AsEnumerable().Select(r => r.Field<double>(continuesAttributes[i])).ToArray();

					discretizations[i] = new Infovision.Datamining.Filters.Unsupervised.Attribute.Discretization();
					discretizations[i].Compute(values, numberOfHistBins);
					discretizations[i].WriteToCSVFile(String.Format("{0}-{1}.cut", trainingSet.TableName, continuesAttributes[i]));

					tmp.Columns[continuesAttributes[i]].DataType = typeof(int);					                    										

					Dictionary<string, int> histCode = new Dictionary<string, int>(discretizations[i].Cuts.Length + 1);
					for (int j = 0; j <= discretizations[i].Cuts.Length; j++)
					{
						histCode.Add(j.ToString(), j);
					}

					//in the second pass codification is already created, we need to substitute the coding
					if (codebook.Columns.Contains(continuesAttributes[i]))
						codebook.Columns.Remove(continuesAttributes[i]);

					codebook.Columns.Add(new Codification.Options(continuesAttributes[i], histCode));					
				}
			
				foreach (DataRow row in trainingSet.Rows)
				{
					for (int i = 0; i < continuesAttributes.Length; i++)
					{
						var discValue = discretizations[i].Search((double)row[continuesAttributes[i]]);						
						row[continuesAttributes[i]] = discValue;
					}

					tmp.ImportRow(row);
				}

				trainingSet = tmp;
				tmp.Dispose();
				tmp = null;

				tmp = validationSet.Clone();
				for (int i = 0; i < continuesAttributes.Length; i++)
				{
					tmp.Columns[continuesAttributes[i]].DataType = typeof(int);
				}

				foreach (DataRow row in validationSet.Rows)
				{
					for (int i = 0; i < continuesAttributes.Length; i++)
					{											
						var discValue = discretizations[i].Search((double)row[continuesAttributes[i]]);
						row[continuesAttributes[i]] = discValue;
					}

					tmp.ImportRow(row);
				}

				validationSet = tmp;

				//trainingSet.WriteToCSVFile(String.Format("{0}-Disc.csv", trainingSet.TableName), " ");
				//validationSet.WriteToCSVFile(String.Format("{0}-Disc.csv", validationSet.TableName), " ");

				DataStore localDataStoreTrain = trainingSet.ToDataStore(codebook, decisionIdx, idIdx);
				DataStore localDataStoreTest = validationSet.ToDataStore(codebook, decisionIdx, idIdx);

				//localDataStoreTrain.WriteToCSVFile(String.Format("{0}-Store.csv", trainingSet.TableName), " ");
				//localDataStoreTest.WriteToCSVFile(String.Format("{0}-Store.csv", validationSet.TableName), " ");

				//localDataStoreTrain.WriteToCSVFileExt(String.Format("{0}-StoreExt.csv", trainingSet.TableName), " ");
				//localDataStoreTest.WriteToCSVFileExt(String.Format("{0}-StoreExt.csv", validationSet.TableName), " ");

				//Console.WriteLine(localDataStoreTrain.DataStoreInfo.ToStringInfo());
				//Console.WriteLine(localDataStoreTest.DataStoreInfo.ToStringInfo());

				RoughClassifier roughClassifier = new RoughClassifier();
				roughClassifier.Train(localDataStoreTrain, reductFactoryKey, epsilon, permutationList);

                double[][] discernVerctor = new double[roughClassifier.ReductStore.Count][];
                
                for(int i=0; i<roughClassifier.ReductStore.Count; i++)
                {
                    var reduct = roughClassifier.ReductStore.GetReduct(i);
                    discernVerctor[i] = GetDiscernibilityVector(localDataStoreTrain, reduct, reduct.Weights);

                    var measure = new InformationMeasureMajority().Calc(reduct);
                    Console.WriteLine("B = {0} M(B) = {1}", reduct, measure);
                }
                    

				IReductStore reductStoreTst = roughClassifier.Classify(localDataStoreTest, reductMeasureKey, numberOfReducts);
				ClassificationResult classificationResultTst = roughClassifier.Vote(localDataStoreTest, identificationType, voteType);
				classificationResultTst.QualityRatio = reductStoreTst.GetAvgMeasure(ReductFactory.GetReductMeasure(reductMeasureKey));

				IReductStore reductStoreTrn = roughClassifier.Classify(localDataStoreTrain, reductMeasureKey, numberOfReducts);
				ClassificationResult classificationResultTrn = roughClassifier.Vote(localDataStoreTrain, identificationType, voteType);
				classificationResultTrn.QualityRatio = reductStoreTrn.GetAvgMeasure(ReductFactory.GetReductMeasure(reductMeasureKey));

				//Console.WriteLine("Training result: {0}", classificationResultTrn.Accuracy);
				//Console.WriteLine("Validation result: {0}", classificationResultTst.Accuracy);

				Console.WriteLine("CV: {0} Training: {1} Testing: {2}", k, classificationResultTrn.Accuracy, classificationResultTst.Accuracy);
				
				return new CrossValidationValues<RoughClassifier>(roughClassifier,
																	classificationResultTrn.Accuracy,
																	classificationResultTst.Accuracy);
			};

			var result = val.Compute();

			Console.WriteLine("Reducts: {0} Training: {1} Testing: {2}", numberOfReducts, result.Training.Mean, result.Validation.Mean);
		}

        public double[] GetDiscernibilityVector(DataStore data, IReduct reduct, double[] weightVector)
        {
            double[] dicernVector = new double[data.NumberOfRecords];            
            foreach (EquivalenceClassInfo eqClass in reduct.EquivalenceClassMap)
            {
                foreach (int objectIdx in eqClass.GetObjectIndexes(eqClass.MostFrequentDecision))
                {
                    dicernVector[objectIdx] = weightVector[objectIdx];
                }
            }

            return dicernVector;
        }

		public void HistogramTest()
		{

			/*
			Assert.AreEqual(4, codebook["a1"].Symbols, "a1");
			Assert.AreEqual(5, codebook["a3"].Symbols, "a3");
			Assert.AreEqual(11 - 1, codebook["a4"].Symbols, "a4"); //A47 : (vacation - does not exist?)
			Assert.AreEqual(5, codebook["a6"].Symbols, "a6");
			Assert.AreEqual(5, codebook["a7"].Symbols, "a7");
			Assert.AreEqual(5 - 1, codebook["a9"].Symbols, "a9");  //A95 : (female : single - does not exists?)
			Assert.AreEqual(3, codebook["a10"].Symbols, "a10");
			Assert.AreEqual(4, codebook["a12"].Symbols, "a12");
			Assert.AreEqual(3, codebook["a14"].Symbols, "a14");
			Assert.AreEqual(3, codebook["a15"].Symbols, "a15");
			Assert.AreEqual(4, codebook["a17"].Symbols, "a17");
			Assert.AreEqual(2, codebook["a19"].Symbols, "a19");
			Assert.AreEqual(2, codebook["a20"].Symbols, "a20");
			Assert.AreEqual(2, codebook["d"].Symbols);
			*/

			/*
			Histogram[] histograms = new Histogram[continuesAttributes.Length];
			DataTable tmp = symbols.Clone();

			for (int i = 0; i < continuesAttributes.Length; i++ )
			{
				double[] values = symbols.AsEnumerable().Select(r => r.Field<double>(continuesAttributes[i])).ToArray();
				histograms[i] = new Histogram("hist" + continuesAttributes[i]);
				histograms[i].AutoAdjustmentRule = BinAdjustmentRule.None;
				histograms[i].InclusiveUpperBound = false;
				histograms[i].Compute(values, numberOfBins: 3);

				Console.WriteLine("{0} was discretized into {1} bins", continuesAttributes[i], histograms[i].Bins.Count);
				foreach (HistogramBin bin in histograms[i].Bins)
				{
					Console.WriteLine("{0}-{1} ({2})", bin.Range.Min, bin.Range.Max, bin.Value);
				}

				tmp.Columns[continuesAttributes[i]].DataType = typeof(int);
			}

			foreach (DataRow row in symbols.Rows)
			{
				for (int i = 0; i < continuesAttributes.Length; i++ )
				{
					row[continuesAttributes[i]] = histograms[i].Bins.SearchIndex((double)row[continuesAttributes[i]]);
				}

				tmp.ImportRow(row);
			}

			symbols = tmp;
			*/
		}

		#endregion
	}    

	public static class HistogramExtensions
	{
		public static void WriteToCSVFile(this Accord.Statistics.Visualizations.Histogram histogram, string filePath)
		{
			StringBuilder sb = new StringBuilder();
			int i = 1;
			foreach (HistogramBin bin in histogram.Bins)
			{
				sb.AppendLine(String.Format("{0} {1} {2}", i, bin.Range.Min, bin.Range.Max));
				i++;
			}

			System.IO.File.WriteAllText(filePath, sb.ToString());
		}
	}    

	public class EpsilonDoubleComparer : IEqualityComparer<double>
	{
		private double epsilon = 0.00000001;

		public EpsilonDoubleComparer()
		{
		}

		public EpsilonDoubleComparer(double epsilon)
			: base()
		{
			this.epsilon = epsilon;
		}

		public bool Equals(double a, double b)
		{
			//return System.Math.Abs(a - b) < this.epsilon;
			return NearlyEqual(a, b, this.epsilon);
		}

		public int GetHashCode(double a)
		{            
			return a.GetHashCode();
		}

		public static bool NearlyEqual(double a, double b, double epsilon) 
		{
			double absA = Math.Abs(a);
			double absB = Math.Abs(b);
			double diff = Math.Abs(a - b);

			if (a == b) 
			{ 
				// shortcut, handles infinities
				return true;
			} 
			else if (a == 0 || b == 0 || diff < Double.MinValue)
			{
				// a or b is zero or both are extremely close to it
				// relative error is less meaningful here
				return diff < (epsilon * Double.MinValue);
			}
			else 
			{ 
				// use relative error
				return diff / (absA + absB) < epsilon;
			}
		}
	}

	/*
	public class BenchmarkDataSet
	{
		private int numberOfAttributes;
		private int[] attributes;
		private int decisionIdx;
		private int idIdx;
		private string filename;
		private string[] nominalAttributes;
		private string[] continuesAttributes;
	}
	*/
}
