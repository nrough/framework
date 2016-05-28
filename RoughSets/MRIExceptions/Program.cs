using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Datamining;
using Infovision.Datamining.Roughset;
using Infovision.MRI;
using Infovision.Utils;
using itk.simple;

namespace MRIExceptions
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Program p = new Program();

            p.Run();
                       
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

        private string GetTestFolder(int testId, decimal epsilon)
        {
            int epsilonInt = (int) (epsilon * 100);
            string testFolder = String.Format(@"Results\Test-{0}-{1}", testId, epsilonInt);
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
                for (decimal epsilon = Decimal.Zero; epsilon < Decimal.One; epsilon += 0.02m)
                {
                    Console.WriteLine("############# Test {0}  Epsilon {1} ###############", t, epsilon);

                    this.RunSingleTest(t, epsilon);
                }
            }

            this.CloseStream();

        }

        public void RunSingleTest(int testId, decimal epsilon)
        {

            string testFolder = this.GetTestFolder(testId, epsilon);

            int trainSlice = RandomSingleton.Random.Next(minSlice, maxSlice + 1);
            int testSlice = RandomSingleton.Random.Next(minSlice, maxSlice + 1);

            if (minSlice != (maxSlice + 1) && minSlice != maxSlice)
            {
                while (testSlice == trainSlice)
                {
                    testSlice = RandomSingleton.Random.Next(minSlice, maxSlice + 1);
                }
            }

            Console.WriteLine("Training on slice {0}", trainSlice);
            Console.WriteLine("Testing on slice {0}", testSlice);

            itk.simple.Image itkTrainImageT1 = SimpleITK.Extract(imageT1.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, trainSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK trainImageT1 = new ImageITK(itkTrainImageT1);
            trainImageT1.Save(String.Format("{0}\\TrnSliceT1-{1}.png", testFolder, trainSlice));

            itk.simple.Image itkTrainImageT2 = SimpleITK.Extract(imageT2.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, trainSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK trainImageT2 = new ImageITK(itkTrainImageT2);
            trainImageT2.Save(String.Format("{0}\\TrnSliceT2-{1}.png", testFolder, trainSlice));

            itk.simple.Image itkTrainImagePD = SimpleITK.Extract(imagePD.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, trainSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK trainImagePD = new ImageITK(itkTrainImagePD);
            trainImagePD.Save(String.Format("{0}\\TrnSlicePD-{1}.png", testFolder, trainSlice));

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
            testImageT1.Save(String.Format("{0}\\TstSliceT1-{1}.png", testFolder, testSlice));

            itk.simple.Image itkTestImageT2 = SimpleITK.Extract(imageT2.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, testSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK testImageT2 = new ImageITK(itkTestImageT2);
            testImageT2.Save(String.Format("{0}\\TstSliceT2-{1}.png", testFolder, testSlice));

            itk.simple.Image itkTestImagePD = SimpleITK.Extract(imagePD.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, testSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK testImagePD = new ImageITK(itkTestImagePD);
            testImagePD.Save(String.Format("{0}\\TstSlicePD-{1}.png", testFolder, testSlice));

            itk.simple.Image itkTestImagePH = SimpleITK.Extract(imagePH.ItkImage,
                                            new VectorUInt32(new uint[] { ImageWidth, ImageHeight, 0 }),
                                            new VectorInt32(new int[] { 0, 0, testSlice }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageITK testImagePH = new ImageITK(itkTestImagePH);


            string trainDataFilename = String.Format("{0}\\trnSlice-{1}.trn", testFolder, trainSlice);
            string testDataFilename = String.Format("{0}\\tstSlice-{1}.tst", testFolder, testSlice);

            Console.WriteLine("Generating decision table...");
            var data = this.GenerateDecisionTable(
                trainImageT1, trainImageT2, trainImagePD, trainImagePH, 
                testImageT1, testImageT2, testImagePD, testImagePH, 
                testFolder, trainDataFilename, testDataFilename);

            Console.WriteLine("Segmentation model learning...");
            RoughClassifier model = this.Learn(data.Item1, testId, epsilon);

            Console.WriteLine("Testing model accuracy...");
            this.Test(model, data.Item2, data.Item1, testId, epsilon);

            trainImagePH.ItkImage = SimpleITK.Multiply(trainImagePH.ItkImage, 10.0);
            trainImagePH.Save(String.Format("{0}\\TrnSlicePH-{1}.png", testFolder, trainSlice));

            testImagePH.ItkImage = SimpleITK.Multiply(testImagePH.ItkImage, 10.0);
            testImagePH.Save(String.Format("{0}\\TstSlicePH-{1}.png", testFolder, testSlice));

            
        }

        private Tuple<DataStore, DataStore> GenerateDecisionTable(IImage trainImageT1, IImage trainImageT2, IImage trainImagePD, IImage trainImagePH,
                                                IImage testImageT1, IImage testImageT2, IImage testImagePD, IImage testImagePH,
                                                string testFolder, string trainFileName, string testFileName)
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
            histClusterImageT1.Save(String.Format("{0}\\TrnHcT1.png", testFolder));

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
            histClusterImageT2.Save(String.Format("{0}\\TrnHcT2.png", testFolder));

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
            histClusterImagePD.Save(String.Format("{0}\\TrnHcPD.png", testFolder));

            int iterations = 1000;
            double learningRate = 0.2;
            int radius = 10;

            Console.WriteLine("Training SOM for T1 image...");
            ImageSOMCluster somClusterT1 = new ImageSOMCluster(1, 9);
            somClusterT1.Train(trainImageT1, iterations, learningRate, radius);

            IImage somImageT1 = somClusterT1.Execute(trainImageT1);
            somImageT1.Save(String.Format("{0}\\TrnSomT1.png", testFolder));

            featureExtractor.AddFeature(new ImageFeatureSOMCluster { Image = trainImageT1, Cluster = somClusterT1 }, "SOMT1");

            Console.WriteLine("Training SOM for T2 image...");
            ImageSOMCluster somClusterT2 = new ImageSOMCluster(1, 9);
            somClusterT2.Train(trainImageT2, iterations, learningRate, radius);

            IImage somImageT2 = somClusterT2.Execute(trainImageT2);
            somImageT2.Save(String.Format("{0}\\TrnSomT2.png", testFolder));

            featureExtractor.AddFeature(new ImageFeatureSOMCluster { Image = trainImageT2, Cluster = somClusterT2 }, "SOMT2");

            Console.WriteLine("Training SOM for PD image...");
            ImageSOMCluster somClusterPD = new ImageSOMCluster(1, 9);
            somClusterPD.Train(trainImagePD, iterations, learningRate, radius);

            IImage somImagePD = somClusterPD.Execute(trainImagePD);
            somImagePD.Save(String.Format("{0}\\TrnSomPD.png", testFolder));

            featureExtractor.AddFeature(new ImageFeatureSOMCluster { Image = trainImagePD, Cluster = somClusterPD }, "SOMPD");

            featureExtractor.AddFeature(new ImageFeatureMask { Image = trainImageT1 }, "Mask");

            List<MRIMaskItem> items = new List<MRIMaskItem>();
            items.Add(new MRIMaskItem { LabelValue = 150, Radius = 10 });
            items.Add(new MRIMaskItem { LabelValue = 100, Radius = 20 });
            items.Add(new MRIMaskItem { LabelValue = 51, Radius = 30 });
            MRIMaskConcentricImageFilter concentricMaskImageFilter = new MRIMaskConcentricImageFilter();
            concentricMaskImageFilter.AddMaskItems(items);

            IImage trainMaskImage = concentricMaskImageFilter.Execute(new MRIMaskBinaryImageFilter().Execute(trainImageT1));
            trainMaskImage.Save(String.Format("{0}\\TrainMask.png", testFolder));

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
            dataTable.WriteToCSVFile(trainFileName, ";");
            DataStore trainingData = DataStore.Load(trainFileName, FileFormat.Csv);

            trainingData.SetDecisionFieldId(22);

            trainingData.DataStoreInfo.SetFieldType(1, FieldTypes.Technical);
            trainingData.DataStoreInfo.SetFieldType(2, FieldTypes.Technical);
            trainingData.DataStoreInfo.SetFieldType(3, FieldTypes.Technical);
            trainingData.DataStoreInfo.SetFieldType(4, FieldTypes.Technical);
            trainingData.DataStoreInfo.SetFieldType(5, FieldTypes.Technical);

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
            testMaskImage.Save(String.Format("{0}\\TestMask.png", testFolder));

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
            testDataTable.WriteToCSVFile(testFileName, ";");
            DataStore testData = DataStore.Load(testFileName, FileFormat.Csv, trainingData.DataStoreInfo);

            testData.SetDecisionFieldId(22);

            testData.DataStoreInfo.SetFieldType(1, FieldTypes.Technical);
            testData.DataStoreInfo.SetFieldType(2, FieldTypes.Technical);
            testData.DataStoreInfo.SetFieldType(3, FieldTypes.Technical);
            testData.DataStoreInfo.SetFieldType(4, FieldTypes.Technical);
            testData.DataStoreInfo.SetFieldType(5, FieldTypes.Technical);

            return new Tuple<DataStore, DataStore>(trainingData, testData);
        }

        private RoughClassifier Learn(DataStore train, int testId, decimal epsilon)
        {
            string testFolder = this.GetTestFolder(testId, epsilon);
            
            WeightGeneratorMajority weightGenerator = new WeightGeneratorMajority(train);
            weightGenerator.Generate();
            train.SetWeights(weightGenerator.Weights);

            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, train);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, true);
            parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, 100);            

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
            generator.Run();

            IReductStoreCollection origReductStoreCollection = generator.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollection = origReductStoreCollection.FilterInEnsemble(10, new ReductStoreLengthComparer(true));

            ReductStoreCollection rsc = filteredReductStoreCollection as ReductStoreCollection;
            rsc.Save(String.Format("{0}\\reductstore.xml", testFolder));

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollection,
                RuleQualityAvg.ConfidenceW,
                RuleQualityAvg.ConfidenceW,
                train.DataStoreInfo.GetDecisionValues());
            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;
            
            int n = 1;
            uint[] position = new uint[] { 0, 0 };            
            DataFieldInfo xField = train.DataStoreInfo.GetFieldInfo(1);
            DataFieldInfo yField = train.DataStoreInfo.GetFieldInfo(2);

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

                string exceptionfilename = String.Format("{0}\\exceptionImage-{1}.", testFolder, n++);
                exceptionImage.Save(exceptionfilename + "png");                
            }

            return classifier;
        }

        private void Test(RoughClassifier model, DataStore test, DataStore train, int testId, decimal epsilon)
        {
            
            string testFolder = this.GetTestFolder(testId, epsilon);
            ClassificationResult result =  model.Classify(test);
            
            this.WriteLine("{0}|{1}|{2}", testId, epsilon, result);            
                        
            uint[] position = new uint[] { 0, 0 };            
            byte pixelValue;            
            DataFieldInfo xField = test.DataStoreInfo.GetFieldInfo(1);
            DataFieldInfo yField = test.DataStoreInfo.GetFieldInfo(2);
            DataFieldInfo decField = test.DataStoreInfo.DecisionInfo;

            ImageITK resultImage = new ImageITK
            {
                ItkImage = SimpleITKHelper.ConstructImage(ImageWidth, ImageHeight, 0, typeof(byte))
            };

            for (int i = 0; i < test.NumberOfRecords; i++)
            {
                position[0] = (uint) (int) xField.Internal2External(test.GetFieldValue(i, 1));
                position[1] = (uint) (int) yField.Internal2External(test.GetFieldValue(i, 2));
                
                if (result.GetPrediction(i) == -1)
                    pixelValue = byte.MaxValue;
                else
                    pixelValue = (byte)(int)decField.Internal2External(result.GetPrediction(i));

                resultImage.SetPixel<byte>(position, pixelValue);
            }

            resultImage.ItkImage = SimpleITK.Multiply(resultImage.ItkImage, 10.0);
            resultImage.Save(String.Format("{0}\\resultImage.png", testFolder));
            
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

            errorImage.Save(String.Format("{0}\\errorImage.png", testFolder));
            resultImage.Save(String.Format("{0}\\errorImage2.png", testFolder));
        }

        StreamWriter fileStream;

        public void OpenStream(string path)
        {
            fileStream = new StreamWriter(path, false);

            this.WriteLine("{0}|{1}|{2}", "Test", "Epsilon", ClassificationResult.ResultHeader());
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
    }
}
