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


namespace Infovision.Datamining.Roughset.Ensemble.UnitTests
{
    [TestFixture, System.Runtime.InteropServices.GuidAttribute("4E3ED8F3-DCFA-45BA-922B-F5F12FFE6095")]
    public class AccordDiscretizationFixture
    {
        #region Member fields

        private int numberOfReducts;
        private string reductFactoryKey;
        private string reductMeasureKey;
        private int epsilon;
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
            numberOfReducts = 1;
            reductFactoryKey = "ApproximateReductMajority";
            reductMeasureKey = "ReductMeasureLength";
            epsilon = 5;

            numberOfAttributes = 20;
            attributes = new int[numberOfAttributes];
            for (int i = 0; i < numberOfAttributes; i++)
            {
                attributes[i] = i + 2;
            }

            decisionIdx = 21;
            idIdx = 0;

            numberOfPermutations = 100;

            permutationList = new PermutationGenerator(attributes).Generate(numberOfPermutations);
            identificationType = IdentificationType.Confidence;
            voteType = VoteType.MajorDecision;

            nominalAttributes = new string[] { "a1", "a3", "a4", "a6", "a7", "a9", "a10", "a12", "a14", "a15", "a17", "a19", "a20", "d" };
            continuesAttributes = new string[] { "a2", "a5", "a8", "a11", "a13", "a16", "a18" };

            numberOfFolds = 5;
            numberOfHistBins = 3;
        }

        #endregion

        #region Test Methods

        [Test]
        public void LoadDataTable()
        {
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

            // Create a new codification codebook to
            // convert strings into integer symbols
            Codification codebook = new Codification(rawData, nominalAttributes);

            DataTable tmpSymbols = codebook.Apply(rawData);
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


            for (int m = 1; m <= 20; m++)
            {
                numberOfReducts = m;

                int[] folds = CrossValidation.Splittings(symbols.Rows.Count, numberOfFolds);
                CrossValidation<RoughClassifier> val = new CrossValidation<RoughClassifier>(folds, numberOfFolds);
                val.RunInParallel = false;

                val.Fitting = delegate(int k, int[] indicesTrain, int[] indicesValidation)
                {
                    DataTable trainingSet = symbols.Subtable(indicesTrain);
                    trainingSet.TableName = "Train-" + k.ToString();

                    DataTable validationSet = symbols.Subtable(indicesValidation);
                    validationSet.TableName = "Test-" + k.ToString();

                    Accord.Statistics.Visualizations.Histogram[] histograms
                        = new Accord.Statistics.Visualizations.Histogram[continuesAttributes.Length];

                    DataTable tmp = trainingSet.Clone();

                    for (int i = 0; i < continuesAttributes.Length; i++)
                    {
                        double[] values = trainingSet.AsEnumerable().Select(r => r.Field<double>(continuesAttributes[i])).ToArray();
                        histograms[i] = new Accord.Statistics.Visualizations.Histogram("hist" + continuesAttributes[i]);
                        histograms[i].AutoAdjustmentRule = BinAdjustmentRule.None;
                        histograms[i].Compute(values, numberOfBins: numberOfHistBins);

                        tmp.Columns[continuesAttributes[i]].DataType = typeof(int);

                        //TODO can we make this operation reversable, now we lost the original values

                        Dictionary<string, int> histCode = new Dictionary<string, int>(histograms[i].Bins.Count);
                        for (int j = 0; j < histograms[i].Bins.Count; j++)
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
                            row[continuesAttributes[i]] = histograms[i].Bins.SearchIndex((double)row[continuesAttributes[i]]);
                        }

                        tmp.ImportRow(row);
                    }

                    trainingSet = tmp;

                    tmp = validationSet.Clone();
                    for (int i = 0; i < continuesAttributes.Length; i++)
                    {
                        tmp.Columns[continuesAttributes[i]].DataType = typeof(int);
                    }

                    foreach (DataRow row in validationSet.Rows)
                    {
                        for (int i = 0; i < continuesAttributes.Length; i++)
                        {
                            row[continuesAttributes[i]] = histograms[i].Bins.SearchIndex((double)row[continuesAttributes[i]]);
                        }

                        tmp.ImportRow(row);
                    }

                    validationSet = tmp;

                    DataStore localDataStoreTrain = trainingSet.ToDataStore(codebook, decisionIdx, idIdx);
                    DataStore localDataStoreTest = validationSet.ToDataStore(codebook, decisionIdx, idIdx);

                    RoughClassifier roughClassifier = new RoughClassifier();
                    roughClassifier.Train(localDataStoreTrain, reductFactoryKey, epsilon, permutationList);

                    IReductStore reductStoreTst = roughClassifier.Classify(localDataStoreTest, reductMeasureKey, numberOfReducts);
                    ClassificationResult classificationResultTst = roughClassifier.Vote(localDataStoreTest, identificationType, voteType);
                    classificationResultTst.QualityRatio = reductStoreTst.GetAvgMeasure(ReductFactory.GetReductMeasure(reductMeasureKey));

                    IReductStore reductStoreTrn = roughClassifier.Classify(localDataStoreTrain, reductMeasureKey, numberOfReducts);
                    ClassificationResult classificationResultTrn = roughClassifier.Vote(localDataStoreTrain, identificationType, voteType);
                    classificationResultTrn.QualityRatio = reductStoreTrn.GetAvgMeasure(ReductFactory.GetReductMeasure(reductMeasureKey));

                    //Console.WriteLine("Training result: {0}", classificationResultTrn.Accuracy);
                    //Console.WriteLine("Validation result: {0}", classificationResultTst.Accuracy);

                    return new CrossValidationValues<RoughClassifier>(roughClassifier,
                                                                      classificationResultTrn.Accuracy,
                                                                      classificationResultTst.Accuracy);
                };

                var result = val.Compute();

                Console.WriteLine("Reducts: {0} Training: {1} Testing: {2}", numberOfReducts, result.Training.Mean, result.Validation.Mean);
            }
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

    public static class DataTableExtensions
    {
        /// <summary>
        ///   Returns a subtable extracted from the current table.
        /// </summary>
        /// 
        /// <param name="source">The table to return the subtable from.</param>
        /// <param name="indexes">Array of indices.</param>
        /// 
        public static DataTable Subtable(this DataTable source, int[] indexes)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (indexes == null)
                throw new ArgumentNullException("indexes");

            DataTable destination = source.Clone();
            foreach (int i in indexes)
            {
                DataRow row = source.Rows[i];
                destination.ImportRow(row);
            }
            return destination;
        }

        public static DataStore ToDataStore(this DataTable source, Codification codification, int decisionIdx = -1, int idIdx = -1)
        {
            if (idIdx == -1 && codification.Columns.Count != source.Columns.Count)
                throw new InvalidOperationException("Number of columns in source tabe and codification description must be the same");
            if (idIdx != -1 && codification.Columns.Count != (source.Columns.Count - 1))
                throw new InvalidOperationException("Number of columns in source tabe and codification description must be the same");

            DataStoreInfo dataStoreInfo = new DataStoreInfo();
            dataStoreInfo.NumberOfRecords = source.Rows.Count;
            dataStoreInfo.NumberOfFields = source.Columns.Count;

            int[] fieldIds = new int[dataStoreInfo.NumberOfFields];

            int i = 0;
            foreach (DataColumn col in source.Columns)
            {
                DataFieldInfo fieldInfo = new DataFieldInfo(i + 1, col.DataType);
                fieldInfo.Name = col.ColumnName;
                fieldInfo.NameAlias = col.ColumnName;

                fieldIds[i] = fieldInfo.Id;

                //TODO Codification may not contain all columns e.g. continues attributes
                if (codification.Columns.Contains(col.ColumnName))
                {
                    foreach (KeyValuePair<string, int> kvp in codification.Columns[col.ColumnName].Mapping)
                    {
                        fieldInfo.AddInternal((long)kvp.Value, kvp.Key);
                    }
                }
                else if (i == idIdx)
                {
                    for (int j = 1; j <= source.Rows.Count; j++)
                        fieldInfo.AddInternal((long)j, j);
                }


                if (i == decisionIdx)
                {
                    dataStoreInfo.DecisionFieldId = fieldInfo.Id;
                    dataStoreInfo.AddFieldInfo(fieldInfo, FieldTypes.Decision);
                }
                else if (i == idIdx)
                {
                    dataStoreInfo.AddFieldInfo(fieldInfo, FieldTypes.Identifier);
                }
                else
                {
                    dataStoreInfo.AddFieldInfo(fieldInfo, FieldTypes.Standard);
                }

                i++;
            }

            DataStore result = new DataStore(dataStoreInfo);
            result.Name = source.TableName;
            long[] vector = new long[dataStoreInfo.NumberOfFields];

            foreach (DataRow row in source.Rows)
            {
                for (int j = 0; j < source.Columns.Count; j++)
                {
                    vector[j] = Int64.Parse(row[j].ToString());
                }

                DataRecordInternal dataStoreRecord = new DataRecordInternal(fieldIds, vector);
                dataStoreRecord.ObjectId = vector[idIdx];

                result.Insert(dataStoreRecord);
            }

            return result;
        }
    }

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
}
