using System;
using System.Collections.Generic;
using System.Data;
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
            
            DataStore decTable = p.GenerateDecisionTable(@"Results\datatable.csv", 0, 0, 89, 181, 217, 90);
            RoughClassifier  model = p.Learn(decTable, 0.10m);
            p.Test(model, @"Results\datatable.csv", decTable.DataStoreInfo);

            Console.ReadKey();
        }        

        private DataStore GenerateDecisionTable(string outputFilename, uint x0, uint y0, uint z0, uint xn, uint yn, uint zn)
        {
            uint imageWidth = 181;
            uint imageHeight = 217;
            uint imageDepth = 181;
            int trainingSliceId = 89;

            IImage imageT1 = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);
            IImage imageT2 = ImageITK.ReadImageRAW(@"Data\t2_icbm_normal_1mm_pn3_rf20.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);
            IImage imagePD = ImageITK.ReadImageRAW(@"Data\pd_icbm_normal_1mm_pn3_rf20.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);
            IImage imagePH = ImageITK.ReadImageRAW(@"Data\ph_icbm_1mm_normal_crisp.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);

            ImageFeatureExtractor featureExtractor = new ImageFeatureExtractor();

            featureExtractor.ImageWidth = imageWidth;
            featureExtractor.ImageHeight = imageHeight;
            featureExtractor.ImageDepth = imageDepth;

            featureExtractor.AddFeature(new ImageFeatureVoxelPositionX { Image = imageT1 }, "PositionX");
            featureExtractor.AddFeature(new ImageFeatureVoxelPositionY { Image = imageT1 }, "PositionY");
            featureExtractor.AddFeature(new ImageFeatureVoxelPositionZ { Image = imageT1 }, "PositionZ");
            featureExtractor.AddFeature(new ImageFeatureVoxelMagnitude { Image = imageT1 }, "MagnitureT1");
            featureExtractor.AddFeature(new ImageFeatureVoxelMagnitude { Image = imageT2 }, "MagnitudeT2");
            featureExtractor.AddFeature(new ImageFeatureVoxelMagnitude { Image = imagePD }, "MagnitudePD");

            int iterations = 100;
            double learningRate = 0.1;
            int radius = 15;

            ImageHistogramCluster histClusterT1 = new ImageHistogramCluster();
            histClusterT1.HistogramBucketSize = 4;
            histClusterT1.MinimumClusterDistance = 200;
            histClusterT1.BucketCountWeight = 1.5;
            histClusterT1.ApproximationDegree = 0.1;
            histClusterT1.MaxNumberOfRepresentatives = 3;
            histClusterT1.Image = imageT1;
            histClusterT1.Slices = new List<int>(new int[] { trainingSliceId });
            histClusterT1.Train();
            featureExtractor.AddFeature(new ImageFeatureHistogramCluster { Image = imageT1, Cluster = histClusterT1 }, "HistogramT1");

            ImageHistogramCluster histClusterT2 = new ImageHistogramCluster();
            histClusterT2.HistogramBucketSize = 4;
            histClusterT2.MinimumClusterDistance = 200;
            histClusterT2.BucketCountWeight = 1.5;
            histClusterT2.ApproximationDegree = 0.1;
            histClusterT2.MaxNumberOfRepresentatives = 3;
            histClusterT2.Image = imageT2;
            histClusterT2.Slices = new List<int>(new int[] { trainingSliceId });
            histClusterT2.Train();
            featureExtractor.AddFeature(new ImageFeatureHistogramCluster { Image = imageT2, Cluster = histClusterT2 }, "HistogramT2");

            ImageHistogramCluster histClusterPD = new ImageHistogramCluster();
            histClusterPD.HistogramBucketSize = 4;
            histClusterPD.MinimumClusterDistance = 200;
            histClusterPD.BucketCountWeight = 1.5;
            histClusterPD.ApproximationDegree = 0.1;
            histClusterPD.MaxNumberOfRepresentatives = 3;
            histClusterPD.Image = imagePD;
            histClusterPD.Slices = new List<int>(new int[] { trainingSliceId });
            histClusterPD.Train();
            featureExtractor.AddFeature(new ImageFeatureHistogramCluster { Image = imagePD, Cluster = histClusterPD }, "HistogramPD");

            ImageSOMCluster somClusterT1 = new ImageSOMCluster(1, 9);
            somClusterT1.Train(imageT1, iterations, learningRate, radius, trainingSliceId);
            featureExtractor.AddFeature(new ImageFeatureSOMCluster { Image = imageT1, Cluster = somClusterT1 }, "SOMT1");

            ImageSOMCluster somClusterT2 = new ImageSOMCluster(1, 9);
            somClusterT2.Train(imageT2, iterations, learningRate, radius, trainingSliceId);
            featureExtractor.AddFeature(new ImageFeatureSOMCluster { Image = imageT2, Cluster = somClusterT2 }, "SOMT2");

            ImageSOMCluster somClusterPD = new ImageSOMCluster(1, 9);
            somClusterPD.Train(imagePD, iterations, learningRate, radius, trainingSliceId);
            featureExtractor.AddFeature(new ImageFeatureSOMCluster { Image = imagePD, Cluster = somClusterPD }, "SOMPD");

            featureExtractor.AddFeature(new ImageFeatureMask { Image = imageT1 }, "Mask");

            EdgeThresholdFilter edgeFilterT1 = new EdgeThresholdFilter();
            featureExtractor.AddFeature(new ImageFeatureEdge { Image = imageT1, EdgeFilter = edgeFilterT1 }, "EdgeT1");

            EdgeThresholdFilter edgeFilterT2 = new EdgeThresholdFilter();
            featureExtractor.AddFeature(new ImageFeatureEdge { Image = imageT2, EdgeFilter = edgeFilterT2 }, "EdgeT2");

            EdgeThresholdFilter edgeFilterPD = new EdgeThresholdFilter();
            featureExtractor.AddFeature(new ImageFeatureEdge { Image = imagePD, EdgeFilter = edgeFilterPD }, "EdgePD");

            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourHistogram { Image = imageT1, EdgeFilter = edgeFilterT1, Cluster = histClusterT1 }, "NbrHistT1");
            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourHistogram { Image = imageT2, EdgeFilter = edgeFilterT2, Cluster = histClusterT2 }, "NbrHistT2");
            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourHistogram { Image = imagePD, EdgeFilter = edgeFilterPD, Cluster = histClusterPD }, "NbrHistPD");

            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourSOM { Image = imageT1, EdgeFilter = edgeFilterT1, Cluster = somClusterT1 }, "NbrSOMT1");
            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourSOM { Image = imageT1, EdgeFilter = edgeFilterT1, Cluster = somClusterT1 }, "NbrSOMT2");
            featureExtractor.AddFeature(new ImageFeatureEdgeNeighbourSOM { Image = imageT1, EdgeFilter = edgeFilterT1, Cluster = somClusterT1 }, "NbrSOMPD");

            featureExtractor.AddFeature(new ImageFeatureVoxelMagnitude { Image = imagePH }, "Label");

            DataTable dataTable = featureExtractor.GetDataTable(x0, y0, z0, xn, yn, zn);

            dataTable.WriteToCSVFile(outputFilename, ";");

            DataStore trainingData = DataStore.Load(outputFilename, FileFormat.Csv);

            trainingData.SetDecisionFieldId(23);

            trainingData.DataStoreInfo.SetFieldType(1, FieldTypes.Technical);
            trainingData.DataStoreInfo.SetFieldType(2, FieldTypes.Technical);
            trainingData.DataStoreInfo.SetFieldType(3, FieldTypes.Technical);
            trainingData.DataStoreInfo.SetFieldType(4, FieldTypes.Technical);

            return trainingData;
        }

        private RoughClassifier Learn(DataStore trainingData, decimal epsilon)
        {            
            WeightGeneratorRelative weightGenerator = new WeightGeneratorRelative(trainingData);
            weightGenerator.Generate();
            trainingData.SetWeights(weightGenerator.Weights);

            Args parms = new Args();
            parms.SetParameter(ReductGeneratorParamHelper.TrainData, trainingData);
            parms.SetParameter(ReductGeneratorParamHelper.FactoryKey, ReductFactoryKeyHelper.GeneralizedMajorityDecisionApproximate);
            parms.SetParameter(ReductGeneratorParamHelper.WeightGenerator, weightGenerator);
            parms.SetParameter(ReductGeneratorParamHelper.Epsilon, epsilon);            
            parms.SetParameter(ReductGeneratorParamHelper.UseExceptionRules, true);
            parms.SetParameter(ReductGeneratorParamHelper.NumberOfReducts, 100);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms) as ReductGeneralizedMajorityDecisionGenerator;
            generator.Run();

            IReductStoreCollection origReductStoreCollection = generator.GetReductStoreCollection();
            IReductStoreCollection filteredReductStoreCollection = origReductStoreCollection.FilterInEnsemble(10, new ReductStoreLengthComparer(true));

            RoughClassifier classifier = new RoughClassifier(
                filteredReductStoreCollection,
                RuleQualityAvg.ConfidenceW,
                RuleQualityAvg.ConfidenceW,
                trainingData.DataStoreInfo.GetDecisionValues());
            classifier.UseExceptionRules = true;
            classifier.ExceptionRulesAsGaps = false;

            return classifier;
        }

        private void Test(RoughClassifier model, string filename, DataStoreInfo referenceDataStoreInfo)
        {
            DataStore test = DataStore.Load(filename, FileFormat.Csv, referenceDataStoreInfo);
            ClassificationResult result =  model.Classify(test);

            Console.WriteLine(ClassificationResult.ResultHeader());
            Console.WriteLine(result);
        }
    }
}
