using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using GenericParsing;
using NRough.Data;
using NRough.MachineLearning;
using NRough.MachineLearning.Roughsets;
using NRough.MRI;
using NRough.Core;
using itk.simple;
using NRough.Core.Data;
using NRough.MachineLearning.Weighting;
using NRough.MachineLearning.Permutations;
using NRough.MachineLearning.Classification;

namespace MRIExceptions
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Program p = new Program();

            //p.Run();

            //p.InsertDB(p.GetTableResult_MRIExceptionsTest(@"c:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\MRIExceptions\bin\x64\Release\Results\Accuracy.result", 99, 5));

            Console.WriteLine("Done");
            Console.ReadKey();
        }

        private const uint ImageWidth = 181;
        private const uint ImageHeight = 217;
        private const uint ImageDepth = 181;

        private void CleanResultsFolder()
        {
            System.IO.DirectoryInfo di = new DirectoryInfo("Results");

            foreach (FileInfo file in di.GetFiles())
                file.Delete();

            foreach (DirectoryInfo dir in di.GetDirectories())
                dir.Delete(true);
        }

        private string GetTestFolder(int testId)
        {
            string testFolder = String.Format(@"Results\Test-{0}", testId);
            if (!Directory.Exists(testFolder))
            {
                Directory.CreateDirectory(testFolder);
            }
            return testFolder;
        }

        private int minSlice = 66;
        private int maxSlice = 122;
        private ImageITK imageT1, imageT2, imagePD, imagePH;

        public void Run()
        {
            RandomSingleton.Seed = Environment.TickCount * Environment.TickCount;
            this.CleanResultsFolder();

            imageT1 = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", ImageWidth, ImageHeight, ImageDepth, PixelIDValueEnum.sitkUInt8);
            imageT2 = ImageITK.ReadImageRAW(@"Data\t2_icbm_normal_1mm_pn3_rf20.rawb", ImageWidth, ImageHeight, ImageDepth, PixelIDValueEnum.sitkUInt8);
            imagePD = ImageITK.ReadImageRAW(@"Data\pd_icbm_normal_1mm_pn3_rf20.rawb", ImageWidth, ImageHeight, ImageDepth, PixelIDValueEnum.sitkUInt8);
            imagePH = ImageITK.ReadImageRAW(@"Data\ph_icbm_1mm_normal_crisp.rawb", ImageWidth, ImageHeight, ImageDepth, PixelIDValueEnum.sitkUInt8);

            this.OpenStream(@"Results\Accuracy.result");

            for (int t = 0; t < 20; t++)
            {
                Console.WriteLine("############# Test {0} ###############", t);

                this.RunSingleTest(t);
            }

            this.CloseStream();
        }

        public void RunSingleTest(int testId)
        {
            string testFolder = this.GetTestFolder(testId);

            int trainSlice = RandomSingleton.Random.Next(minSlice, maxSlice + 1);
            int testSlice = RandomSingleton.Random.Next(minSlice, maxSlice + 1);

            if (minSlice != (maxSlice + 1) && minSlice != maxSlice)
            {
                while (testSlice == trainSlice)
                {
                    testSlice = RandomSingleton.Random.Next(minSlice, maxSlice + 1);
                }
            }

            Console.WriteLine("Slice {0} and {1} selected", trainSlice, testSlice);

            itk.simple.Image itkTrainImageT1 = SimpleITK.Extract(imageT1.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, trainSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK trainImageT1 = new ImageITK(itkTrainImageT1);
            trainImageT1.Save(String.Format("{0}\\SliceT1-{1}.png", testFolder, trainSlice));

            itk.simple.Image itkTrainImageT2 = SimpleITK.Extract(imageT2.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, trainSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK trainImageT2 = new ImageITK(itkTrainImageT2);
            trainImageT2.Save(String.Format("{0}\\SliceT2-{1}.png", testFolder, trainSlice));

            itk.simple.Image itkTrainImagePD = SimpleITK.Extract(imagePD.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, trainSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK trainImagePD = new ImageITK(itkTrainImagePD);
            trainImagePD.Save(String.Format("{0}\\SlicePD-{1}.png", testFolder, trainSlice));

            itk.simple.Image itkTrainImagePH = SimpleITK.Extract(imagePH.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, trainSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK trainImagePH = new ImageITK(itkTrainImagePH);

            itk.simple.Image itkTestImageT1 = SimpleITK.Extract(imageT1.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, testSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK testImageT1 = new ImageITK(itkTestImageT1);
            testImageT1.Save(String.Format("{0}\\SliceT1-{1}.png", testFolder, testSlice));

            itk.simple.Image itkTestImageT2 = SimpleITK.Extract(imageT2.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, testSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK testImageT2 = new ImageITK(itkTestImageT2);
            testImageT2.Save(String.Format("{0}\\SliceT2-{1}.png", testFolder, testSlice));

            itk.simple.Image itkTestImagePD = SimpleITK.Extract(imagePD.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, testSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK testImagePD = new ImageITK(itkTestImagePD);
            testImagePD.Save(String.Format("{0}\\SlicePD-{1}.png", testFolder, testSlice));

            itk.simple.Image itkTestImagePH = SimpleITK.Extract(imagePH.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, testSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK testImagePH = new ImageITK(itkTestImagePH);

            Console.WriteLine("Generating decision tables...");

            string trainDataFilename = String.Format("{0}\\Slice-{1}.trn", testFolder, trainSlice);
            string testDataFilename = String.Format("{0}\\Slice-{1}.tst", testFolder, testSlice);

            var data_1 = this.GenerateDecisionTable(
                testFolder,
                trainImageT1, trainImageT2, trainImagePD, trainImagePH, trainDataFilename, trainSlice,
                testImageT1, testImageT2, testImagePD, testImagePH, testDataFilename, testSlice);

            trainDataFilename = String.Format("{0}\\Slice-{1}.trn", testFolder, testSlice);
            testDataFilename = String.Format("{0}\\Slice-{1}.tst", testFolder, trainSlice);

            var data_2 = this.GenerateDecisionTable(
                testFolder,
                testImageT1, testImageT2, testImagePD, testImagePH, testDataFilename, testSlice,
                trainImageT1, trainImageT2, trainImagePD, trainImagePH, trainDataFilename, trainSlice);

            for (double epsilon = 0.0; epsilon < 1.0; epsilon += 0.02)
            {
                Console.WriteLine("Segmentation model learning and test eps={0}", epsilon);

                PermutationCollection permutations = new PermutationGenerator(data_1.Item1).Generate(100);

                RoughClassifier model_1 = this.Learn(data_1.Item1, testId, epsilon, trainSlice, testSlice, permutations);

                this.Test(model_1, data_1.Item2, data_1.Item1, testId, epsilon, trainSlice, testSlice);

                //We reverse slices and dataset
                RoughClassifier model_2 = this.Learn(data_2.Item1, testId, epsilon, testSlice, trainSlice, permutations);
                this.Test(model_2, data_2.Item2, data_2.Item1, testId, epsilon, testSlice, trainSlice);
            }

            trainImagePH.ItkImage = SimpleITK.Multiply(trainImagePH.ItkImage, 10.0);
            trainImagePH.Save(String.Format("{0}\\SlicePH-{1}.png", testFolder, trainSlice));

            testImagePH.ItkImage = SimpleITK.Multiply(testImagePH.ItkImage, 10.0);
            testImagePH.Save(String.Format("{0}\\SlicePH-{1}.png", testFolder, testSlice));
        }

        private Tuple<DataStore, DataStore> GenerateDecisionTable(string testFolder, IImage trainImageT1, IImage trainImageT2, IImage trainImagePD, IImage trainImagePH, string trainFileName, int trainSlice, IImage testImageT1, IImage testImageT2, IImage testImagePD, IImage testImagePH, string testFileName, int testSlice)
        {
            ImageFeatureExtractor featureExtractor = new ImageFeatureExtractor();

            featureExtractor.ImageWidth = ImageWidth;
            featureExtractor.ImageHeight = ImageHeight;
            featureExtractor.ImageDepth = 1;

            featureExtractor.AddFeature(new ImageFeatureVoxelPositionX { Image = trainImageT1 }, "PositionX");
            featureExtractor.AddFeature(new ImageFeatureVoxelPositionY { Image = trainImageT1 }, "PositionY");

            featureExtractor.AddFeature(new ImageFeatureVoxelMagnitude { Image = trainImageT1 }, "MagnitureT1");
            featureExtractor.AddFeature(new ImageFeatureVoxelMagnitude { Image = trainImageT2 }, "MagnitudeT2");
            featureExtractor.AddFeature(new ImageFeatureVoxelMagnitude { Image = trainImagePD }, "MagnitudePD");

            Console.WriteLine("Training Histogram clustering for T1 image...");
            ImageHistogramCluster histClusterT1 = new ImageHistogramCluster();
            histClusterT1.HistogramBucketSize = 4;
            histClusterT1.MinimumClusterDistance = 200;
            histClusterT1.BucketCountWeight = 1.5;
            histClusterT1.ApproximationDegree = 0.1;
            histClusterT1.MaxNumberOfRepresentatives = 3;
            histClusterT1.Image = trainImageT1;
            histClusterT1.Train();

            IImage histClusterImageT1 = histClusterT1.Execute(trainImageT1);
            histClusterImageT1.Save(String.Format("{0}\\TrnHcT1-{1}.png", testFolder, trainSlice));

            featureExtractor.AddFeature(new ImageFeatureHistogramCluster { Image = trainImageT1, Cluster = histClusterT1 }, "HistogramT1");

            Console.WriteLine("Training Histogram clustering for T2 image...");
            ImageHistogramCluster histClusterT2 = new ImageHistogramCluster();
            histClusterT2.HistogramBucketSize = 4;
            histClusterT2.MinimumClusterDistance = 200;
            histClusterT2.BucketCountWeight = 1.5;
            histClusterT2.ApproximationDegree = 0.1;
            histClusterT2.MaxNumberOfRepresentatives = 3;
            histClusterT2.Image = trainImageT2;
            histClusterT2.Train();
            featureExtractor.AddFeature(new ImageFeatureHistogramCluster { Image = trainImageT2, Cluster = histClusterT2 }, "HistogramT2");

            IImage histClusterImageT2 = histClusterT2.Execute(trainImageT2);
            histClusterImageT2.Save(String.Format("{0}\\TrnHcT2-{1}.png", testFolder, trainSlice));

            Console.WriteLine("Training Histogram clustering for PD image...");
            ImageHistogramCluster histClusterPD = new ImageHistogramCluster();
            histClusterPD.HistogramBucketSize = 4;
            histClusterPD.MinimumClusterDistance = 200;
            histClusterPD.BucketCountWeight = 1.5;
            histClusterPD.ApproximationDegree = 0.1;
            histClusterPD.MaxNumberOfRepresentatives = 3;
            histClusterPD.Image = trainImagePD;
            histClusterPD.Train();
            featureExtractor.AddFeature(new ImageFeatureHistogramCluster { Image = trainImagePD, Cluster = histClusterPD }, "HistogramPD");

            IImage histClusterImagePD = histClusterPD.Execute(trainImagePD);
            histClusterImagePD.Save(String.Format("{0}\\TrnHcPD-{1}.png", testFolder, trainSlice));

            int iterations = 1000;
            double learningRate = 0.2;
            int radius = 10;

            Console.WriteLine("Training SOM for T1 image...");
            ImageSOMCluster somClusterT1 = new ImageSOMCluster(1, 9);
            somClusterT1.Train(trainImageT1, iterations, learningRate, radius);

            IImage somImageT1 = somClusterT1.Execute(trainImageT1);
            somImageT1.Save(String.Format("{0}\\TrnSomT1-{1}.png", testFolder, trainSlice));

            featureExtractor.AddFeature(new ImageFeatureSOMCluster { Image = trainImageT1, Cluster = somClusterT1 }, "SOMT1");

            Console.WriteLine("Training SOM for T2 image...");
            ImageSOMCluster somClusterT2 = new ImageSOMCluster(1, 9);
            somClusterT2.Train(trainImageT2, iterations, learningRate, radius);

            IImage somImageT2 = somClusterT2.Execute(trainImageT2);
            somImageT2.Save(String.Format("{0}\\TrnSomT2-{1}.png", testFolder, trainSlice));

            featureExtractor.AddFeature(new ImageFeatureSOMCluster { Image = trainImageT2, Cluster = somClusterT2 }, "SOMT2");

            Console.WriteLine("Training SOM for PD image...");
            ImageSOMCluster somClusterPD = new ImageSOMCluster(1, 9);
            somClusterPD.Train(trainImagePD, iterations, learningRate, radius);

            IImage somImagePD = somClusterPD.Execute(trainImagePD);
            somImagePD.Save(String.Format("{0}\\TrnSomPD-{1}.png", testFolder, trainSlice));

            featureExtractor.AddFeature(new ImageFeatureSOMCluster { Image = trainImagePD, Cluster = somClusterPD }, "SOMPD");

            featureExtractor.AddFeature(new ImageFeatureMask { Image = trainImageT1 }, "Mask");

            List<MRIMaskItem> items = new List<MRIMaskItem>();
            items.Add(new MRIMaskItem { LabelValue = 150, Radius = 10 });
            items.Add(new MRIMaskItem { LabelValue = 100, Radius = 20 });
            items.Add(new MRIMaskItem { LabelValue = 51, Radius = 30 });
            MRIMaskConcentricImageFilter concentricMaskImageFilter = new MRIMaskConcentricImageFilter();
            concentricMaskImageFilter.AddMaskItems(items);

            IImage trainMaskImage = concentricMaskImageFilter.Execute(new MRIMaskBinaryImageFilter().Execute(trainImageT1));
            trainMaskImage.Save(String.Format("{0}\\TrainMask-{1}.png", testFolder, trainSlice));

            EdgeThresholdFilter edgeFilterT1 = new EdgeThresholdFilter();
            featureExtractor.AddFeature(new ImageFeatureEdge { Image = trainImageT1, EdgeFilter = edgeFilterT1 }, "EdgeT1");

            EdgeThresholdFilter edgeFilterT2 = new EdgeThresholdFilter();
            featureExtractor.AddFeature(new ImageFeatureEdge { Image = trainImageT2, EdgeFilter = edgeFilterT2 }, "EdgeT2");

            EdgeThresholdFilter edgeFilterPD = new EdgeThresholdFilter();
            featureExtractor.AddFeature(new ImageFeatureEdge { Image = trainImagePD, EdgeFilter = edgeFilterPD }, "EdgePD");

            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourHistogram { Image = trainImageT1, EdgeFilter = edgeFilterT1, Cluster = histClusterT1 }, "NbrHistT1");
            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourHistogram { Image = trainImageT2, EdgeFilter = edgeFilterT2, Cluster = histClusterT2 }, "NbrHistT2");
            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourHistogram { Image = trainImagePD, EdgeFilter = edgeFilterPD, Cluster = histClusterPD }, "NbrHistPD");

            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourSOM { Image = trainImageT1, EdgeFilter = edgeFilterT1, Cluster = somClusterT1 }, "NbrSOMT1");
            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourSOM { Image = trainImageT2, EdgeFilter = edgeFilterT2, Cluster = somClusterT2 }, "NbrSOMT2");
            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourSOM { Image = trainImagePD, EdgeFilter = edgeFilterPD, Cluster = somClusterPD }, "NbrSOMPD");

            featureExtractor.AddFeature(new ImageFeatureVoxelMagnitude { Image = trainImagePH }, "Label");

            DataTable dataTable = featureExtractor.GetDataTable();
            dataTable.Dumb(trainFileName, ";");
            DataStore trainingData = DataStore.Load(trainFileName, DataFormat.CSV);

            trainingData.SetDecisionFieldId(22);

            trainingData.DataStoreInfo.SetFieldType(1, FieldGroup.Sys);
            trainingData.DataStoreInfo.SetFieldType(2, FieldGroup.Sys);
            trainingData.DataStoreInfo.SetFieldType(3, FieldGroup.Sys);
            trainingData.DataStoreInfo.SetFieldType(4, FieldGroup.Sys);
            trainingData.DataStoreInfo.SetFieldType(5, FieldGroup.Sys);

            ImageFeatureExtractor featureExtractorTest = new ImageFeatureExtractor();

            featureExtractorTest.ImageWidth = ImageWidth;
            featureExtractorTest.ImageHeight = ImageHeight;
            featureExtractorTest.ImageDepth = 1;

            featureExtractorTest.AddFeature(new ImageFeatureVoxelPositionX { Image = testImageT1 }, "PositionX");
            featureExtractorTest.AddFeature(new ImageFeatureVoxelPositionY { Image = testImageT1 }, "PositionY");

            featureExtractorTest.AddFeature(new ImageFeatureVoxelMagnitude { Image = testImageT1 }, "MagnitureT1");
            featureExtractorTest.AddFeature(new ImageFeatureVoxelMagnitude { Image = testImageT2 }, "MagnitudeT2");
            featureExtractorTest.AddFeature(new ImageFeatureVoxelMagnitude { Image = testImagePD }, "MagnitudePD");

            featureExtractorTest.AddFeature(new ImageFeatureHistogramCluster { Image = testImageT1, Cluster = histClusterT1 }, "HistogramT1");
            featureExtractorTest.AddFeature(new ImageFeatureHistogramCluster { Image = testImageT2, Cluster = histClusterT2 }, "HistogramT2");
            featureExtractorTest.AddFeature(new ImageFeatureHistogramCluster { Image = testImagePD, Cluster = histClusterPD }, "HistogramPD");
            featureExtractorTest.AddFeature(new ImageFeatureSOMCluster { Image = testImageT1, Cluster = somClusterT1 }, "SOMT1");
            featureExtractorTest.AddFeature(new ImageFeatureSOMCluster { Image = testImageT2, Cluster = somClusterT2 }, "SOMT2");
            featureExtractorTest.AddFeature(new ImageFeatureSOMCluster { Image = testImagePD, Cluster = somClusterPD }, "SOMPD");

            featureExtractorTest.AddFeature(new ImageFeatureMask { Image = testImageT1 }, "Mask");

            IImage testMaskImage = concentricMaskImageFilter.Execute(new MRIMaskBinaryImageFilter().Execute(testImageT1));
            testMaskImage.Save(String.Format("{0}\\TestMask-{1}.png", testFolder, testSlice));

            EdgeThresholdFilter testEdgeFilterT1 = new EdgeThresholdFilter();
            EdgeThresholdFilter testEdgeFilterT2 = new EdgeThresholdFilter();
            EdgeThresholdFilter testEdgeFilterPD = new EdgeThresholdFilter();

            featureExtractorTest.AddFeature(new ImageFeatureEdge { Image = testImageT1, EdgeFilter = testEdgeFilterT1 }, "EdgeT1");
            featureExtractorTest.AddFeature(new ImageFeatureEdge { Image = testImageT2, EdgeFilter = testEdgeFilterT2 }, "EdgeT2");
            featureExtractorTest.AddFeature(new ImageFeatureEdge { Image = testImagePD, EdgeFilter = testEdgeFilterPD }, "EdgePD");

            featureExtractorTest.AddFeature(new ImageFeatureEdgeNeighbourHistogram { Image = testImageT1, EdgeFilter = testEdgeFilterT1, Cluster = histClusterT1 }, "NbrHistT1");
            featureExtractorTest.AddFeature(new ImageFeatureEdgeNeighbourHistogram { Image = testImageT2, EdgeFilter = testEdgeFilterT2, Cluster = histClusterT2 }, "NbrHistT2");
            featureExtractorTest.AddFeature(new ImageFeatureEdgeNeighbourHistogram { Image = testImagePD, EdgeFilter = testEdgeFilterPD, Cluster = histClusterPD }, "NbrHistPD");

            featureExtractorTest.AddFeature(new ImageFeatureEdgeNeighbourSOM { Image = testImageT1, EdgeFilter = testEdgeFilterT1, Cluster = somClusterT1 }, "NbrSOMT1");
            featureExtractorTest.AddFeature(new ImageFeatureEdgeNeighbourSOM { Image = testImageT2, EdgeFilter = testEdgeFilterT2, Cluster = somClusterT2 }, "NbrSOMT2");
            featureExtractorTest.AddFeature(new ImageFeatureEdgeNeighbourSOM { Image = testImagePD, EdgeFilter = testEdgeFilterPD, Cluster = somClusterPD }, "NbrSOMPD");

            featureExtractorTest.AddFeature(new ImageFeatureVoxelMagnitude { Image = testImagePH }, "Label");

            DataTable testDataTable = featureExtractorTest.GetDataTable();
            testDataTable.Dumb(testFileName, ";");
            DataStore testData = DataStore.Load(testFileName, DataFormat.CSV, trainingData.DataStoreInfo);

            testData.SetDecisionFieldId(22);

            testData.DataStoreInfo.SetFieldType(1, FieldGroup.Sys);
            testData.DataStoreInfo.SetFieldType(2, FieldGroup.Sys);
            testData.DataStoreInfo.SetFieldType(3, FieldGroup.Sys);
            testData.DataStoreInfo.SetFieldType(4, FieldGroup.Sys);
            testData.DataStoreInfo.SetFieldType(5, FieldGroup.Sys);

            return new Tuple<DataStore, DataStore>(trainingData, testData);
        }

        private RoughClassifier Learn(DataStore train, int testId, double epsilon, int trainSlice, int testSlice, PermutationCollection permutations)
        {
            int epsilonInt = (int)(epsilon * 100);
            string testFolder = this.GetTestFolder(testId);

            //Remember to change ###############################################
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(train);
            //WeightGeneratorRelative weightGenerator = new WeightGeneratorRelative(train);
            weightGenerator.Generate();
            train.SetWeights(weightGenerator.Weights);

            Args parms = new Args();
            parms.SetParameter(ReductFactoryOptions.DecisionTable, train);

            //Remember to change ###############################################
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.GeneralizedMajorityDecisionApproximate);
            parms.SetParameter(ReductFactoryOptions.WeightGenerator, weightGenerator);
            parms.SetParameter(ReductFactoryOptions.Epsilon, epsilon);
            parms.SetParameter(ReductFactoryOptions.UseExceptionRules, true);
            parms.SetParameter(ReductFactoryOptions.NumberOfReducts, 100);
            parms.SetParameter(ReductFactoryOptions.EquivalenceClassSortDirection, SortDirection.Descending);
            parms.SetParameter(ReductFactoryOptions.PermutationCollection, permutations);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run();

            IReductStoreCollection origReductStoreCollection = generator.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollection = origReductStoreCollection.FilterInEnsemble(10, new ReductStoreLengthComparer(true));

            ReductStoreCollection rsc = filteredReductStoreCollection as ReductStoreCollection;
            rsc.Save(String.Format("{0}\\reductstore-{1}-{2}.xml", testFolder, trainSlice, epsilonInt));

            ReductStoreCollection rscAll = origReductStoreCollection as ReductStoreCollection;
            rscAll.Save(String.Format("{0}\\reductstoreAll-{1}-{2}.xml", testFolder, trainSlice, epsilonInt));

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollection,
                RuleQualityAvgMethods.ConfidenceW,
                RuleQualityAvgMethods.ConfidenceW,
                train.DataStoreInfo.GetDecisionValues());

            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;

            int n = 1;
            uint[] position = new uint[] { 0, 0 };
            AttributeInfo xField = train.DataStoreInfo.GetFieldInfo(1);
            AttributeInfo yField = train.DataStoreInfo.GetFieldInfo(2);

            foreach (ReductStore rst in filteredReductStoreCollection)
            {
                ImageITK exceptionImage = new ImageITK
                {
                    ItkImage = SimpleITKHelper.ConstructImage(ImageWidth, ImageHeight, 0, typeof(byte))
                };

                foreach (IReduct r in rst.Where(k => k.IsException == true))
                {
                    foreach (EquivalenceClass eq in r.EquivalenceClasses)
                    {
                        foreach (int objectIdx in eq.Instances.Keys)
                        {
                            position[0] = (uint)(int)xField.Internal2External(train.GetFieldValue(objectIdx, 1));
                            position[1] = (uint)(int)yField.Internal2External(train.GetFieldValue(objectIdx, 2));
                            exceptionImage.SetPixel<byte>(position, byte.MaxValue);
                        }
                    }
                }

                string exceptionfilename = String.Format("{0}\\exceptionImage-{1}-{2}-{3}", testFolder, trainSlice, epsilonInt, n++);
                exceptionImage.Save(exceptionfilename + ".png");
            }

            return classifier;
        }

        private void Test(
            RoughClassifier model, DataStore test, DataStore train,
            int testId, double epsilon, int trainSlice, int testSlice)
        {
            string testFolder = this.GetTestFolder(testId);
            int epsilonInt = (int)(epsilon * 100);

            ClassificationResult result = model.Classify(test);

            //Remember to change ###############################################
            result.AvgNumberOfAttributes = model.ReductStoreCollection.GetWeightedAvgMeasure(new ReductMeasureLength(), true);
            //result.QualityRatio = model.ReductStoreCollection.GetAvgMeasure(new ReductMeasureLength());

            this.WriteLine("{0}|{1}|{2}|{3}|{4}", testId, epsilon, trainSlice, testSlice, result);

            uint[] position = new uint[] { 0, 0 };
            byte pixelValue;
            AttributeInfo xField = test.DataStoreInfo.GetFieldInfo(1);
            AttributeInfo yField = test.DataStoreInfo.GetFieldInfo(2);
            AttributeInfo decField = test.DataStoreInfo.DecisionInfo;

            ImageITK resultImage = new ImageITK
            {
                ItkImage = SimpleITKHelper.ConstructImage(ImageWidth, ImageHeight, 0, typeof(byte))
            };

            for (int i = 0; i < test.NumberOfRecords; i++)
            {
                position[0] = (uint)(int)xField.Internal2External(test.GetFieldValue(i, 1));
                position[1] = (uint)(int)yField.Internal2External(test.GetFieldValue(i, 2));

                if (result.GetPrediction(i) == -1)
                    pixelValue = byte.MaxValue;
                else
                    pixelValue = (byte)(int)decField.Internal2External(result.GetPrediction(i));

                resultImage.SetPixel<byte>(position, pixelValue);
            }

            resultImage.ItkImage = SimpleITK.Multiply(resultImage.ItkImage, 10.0);
            resultImage.Save(String.Format("{0}\\resultImage-{1}-{2}.png", testFolder, testSlice, epsilonInt));

            ImageITK errorImage = new ImageITK
            {
                ItkImage = SimpleITKHelper.ConstructImage(ImageWidth, ImageHeight, 0, typeof(byte))
            };

            for (int i = 0; i < test.NumberOfRecords; i++)
            {
                position[0] = (uint)(int)xField.Internal2External(test.GetFieldValue(i, 1));
                position[1] = (uint)(int)yField.Internal2External(test.GetFieldValue(i, 2));

                if (result.GetPrediction(i) != result.GetActual(i))
                {
                    resultImage.SetPixel<byte>(position, byte.MaxValue);
                    errorImage.SetPixel<byte>(position, byte.MaxValue);
                }
            }

            errorImage.Save(String.Format("{0}\\errorImage-{1}-{2}.png", testFolder, testSlice, epsilonInt));
            resultImage.Save(String.Format("{0}\\errorImageOverlay-{1}-{2}.png", testFolder, testSlice, epsilonInt));
        }

        private StreamWriter fileStream;

        public void OpenStream(string path)
        {
            fileStream = new StreamWriter(path, false);

            this.WriteLine("{0}|{1}|{2}|{3}|{4}", "Test", "Epsilon", "TrainSlice", "TestSlice", ClassificationResult.TableHeader());
        }

        public void CloseStream()
        {
            if (fileStream != null)
            {
                fileStream.Flush();
                fileStream.Close();
                fileStream = null;
            }
        }

        public void WriteLine(string format, params object[] paramteters)
        {
            if (fileStream != null)
            {
                fileStream.WriteLine(format, paramteters);
                fileStream.Flush();
            }

            Console.WriteLine(format, paramteters);
        }

        public void InsertDB(DataTable table)
        {
            string connectionString = @"Server=HUJOLOPTER\SQL2014;Database=RoughDB;Integrated Security=True;";
            using (SqlConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                {
                    s.DestinationTableName = table.TableName;
                    s.BulkCopyTimeout = 300; //5 minutes

                    foreach (var column in table.Columns)
                        s.ColumnMappings.Add(column.ToString(), column.ToString());

                    s.WriteToServer(table);
                }
            }
        }

        private DataTable DefineResultTable()
        {
            DataTable table = new DataTable("dbo.RESULT");

            this.AddColumn(table, "RESULTID", "System.Int64");
            this.AddColumn(table, "EXPERIMENTID", "System.Int32");
            this.AddColumn(table, "DATASETID", "System.Int32");
            this.AddColumn(table, "FOLDID", "System.Int32");
            this.AddColumn(table, "TESTID", "System.Int32");
            this.AddColumn(table, "ENSEMBLESIZE", "System.Int32");
            this.AddColumn(table, "IDENTIFICATIONTYPEID", "System.Int32");
            this.AddColumn(table, "VOTINGTYPEID", "System.Int32");
            this.AddColumn(table, "MODELTYPEID", "System.Int32");
            this.AddColumn(table, "EPSILON", "System.Double");
            this.AddColumn(table, "CLASSIFIED", "System.Int32");
            this.AddColumn(table, "MISCLASSIFIED", "System.Int32");
            this.AddColumn(table, "UNCLASSIFIED", "System.Int32");
            this.AddColumn(table, "WEIGHTCLASSIFIED", "System.Double");
            this.AddColumn(table, "WEIGHTUNCLASSIFIED", "System.Double");
            this.AddColumn(table, "WEIGHTMISCLASSIFIED", "System.Double");
            this.AddColumn(table, "ACCURACY", "System.Double");
            this.AddColumn(table, "BALANCEDACCURACY", "System.Double");
            this.AddColumn(table, "CONFIDENCE", "System.Double");
            this.AddColumn(table, "COVERAGE", "System.Double");
            this.AddColumn(table, "AVERAGEREDUCTLENGTH", "System.Double");
            this.AddColumn(table, "MODELCREATIONTIME", "System.Int64");
            this.AddColumn(table, "CLASSIFICATIONTIME", "System.Int64");
            this.AddColumn(table, "WEIGHTINGTYPEID", "System.Int32");
            this.AddColumn(table, "EXCEPTIONRULETYPEID", "System.Int32");

            this.AddColumn(table, "EXCEPTIONRULEHITCOUNTER", "System.Int32");
            this.AddColumn(table, "EXCEPTIONRULELENGTHSUM", "System.Int32");
            this.AddColumn(table, "STANDARDRULEHITCOUNTER", "System.Int32");
            this.AddColumn(table, "STANDARDRULELENGTHSUM", "System.Int32");

            this.AddColumn(table, "MRISLICETRN", "System.Int32");
            this.AddColumn(table, "MRISLICETST", "System.Int32");

            return table;
        }

        private DataColumn AddColumn(DataTable table, string name, string type)
        {
            DataColumn c = new DataColumn();
            c.DataType = Type.GetType(type);
            c.ColumnName = name;
            table.Columns.Add(c);
            return c;
        }

        public DataTable GetTableResult_MRIExceptionsTest(string filename, int datasetid, int experimentid)
        {
            DataTable table = this.DefineResultTable();

            DataTable tmpTable;
            using (GenericParserAdapter gpa = new GenericParserAdapter(filename))
            {
                gpa.ColumnDelimiter = "|".ToCharArray()[0];
                gpa.FirstRowHasHeader = true;
                gpa.IncludeFileLineNumber = false;
                gpa.TrimResults = true;

                tmpTable = gpa.GetDataTable();
            }

            long i = 1;
            DataRow dataSetRow = null;
            foreach (DataRow row in tmpTable.Rows)
            {
                dataSetRow = table.NewRow();

                dataSetRow["RESULTID"] = i;
                dataSetRow["EXPERIMENTID"] = experimentid;
                dataSetRow["DATASETID"] = datasetid;

                dataSetRow["FOLDID"] = 0;
                dataSetRow["TESTID"] = Int32.Parse(row["Test"].ToString());
                dataSetRow["ENSEMBLESIZE"] = 10;
                dataSetRow["IDENTIFICATIONTYPEID"] = 4; //ConfidenceW
                dataSetRow["VOTINGTYPEID"] = 4; //ConfidenceW
                dataSetRow["MODELTYPEID"] = 11; //GeneralizedMajorityDecisionApproximate
                dataSetRow["EPSILON"] = Double.Parse(row["Epsilon"].ToString());
                dataSetRow["CLASSIFIED"] = Int32.Parse(row["Classified"].ToString());
                dataSetRow["MISCLASSIFIED"] = Int32.Parse(row["Misclassified"].ToString());
                dataSetRow["UNCLASSIFIED"] = Int32.Parse(row["Unclassified"].ToString());
                dataSetRow["WEIGHTCLASSIFIED"] = Double.Parse(row["WeightClassified"].ToString());
                dataSetRow["WEIGHTUNCLASSIFIED"] = Double.Parse(row["WeightMisclassified"].ToString());
                dataSetRow["WEIGHTMISCLASSIFIED"] = Double.Parse(row["WeightUnclassified"].ToString());
                dataSetRow["ACCURACY"] = Double.Parse(row["Accuracy"].ToString());
                dataSetRow["BALANCEDACCURACY"] = Double.Parse(row["BalancedAccuracy"].ToString());
                dataSetRow["CONFIDENCE"] = Double.Parse(row["Confidence"].ToString());
                dataSetRow["COVERAGE"] = Double.Parse(row["Coverage"].ToString());
                dataSetRow["AVERAGEREDUCTLENGTH"] = Double.Parse(row["AverageNumberOfAttributes"].ToString());
                dataSetRow["MODELCREATIONTIME"] = Int64.Parse(row["ModelCreationTime"].ToString());
                dataSetRow["CLASSIFICATIONTIME"] = Int64.Parse(row["ClassificationTime"].ToString());
                dataSetRow["WEIGHTINGTYPEID"] = 1; //1=Majority 2=Relative
                dataSetRow["EXCEPTIONRULETYPEID"] = 3; //Exceptions

                dataSetRow["EXCEPTIONRULEHITCOUNTER"] = Int32.Parse(row["ExceptionRuleHitCounter"].ToString());
                dataSetRow["EXCEPTIONRULELENGTHSUM"] = Int32.Parse(row["ExceptionRuleLengthSum"].ToString());
                dataSetRow["STANDARDRULEHITCOUNTER"] = Int32.Parse(row["StandardRuleHitCounter"].ToString());
                dataSetRow["STANDARDRULELENGTHSUM"] = Int32.Parse(row["StandardRuleLengthSum"].ToString());

                dataSetRow["MRISLICETRN"] = Int32.Parse(row["TrainSlice"].ToString());
                dataSetRow["MRISLICETST"] = Int32.Parse(row["TestSlice"].ToString());

                i++;

                table.Rows.Add(dataSetRow);
            }

            return table;
        }
    }
}