using System.Collections.Generic;
using System.Data;
using NRough.MachineLearning.Experimenter.Parms;
using itk.simple;
using NUnit.Framework;
using NRough.Core.Data;
using NRough.MRI;

namespace NRough.Tests.MRI
{
    [TestFixture, System.Runtime.InteropServices.GuidAttribute("AE5C3100-EE1D-49F0-AAE1-22DF19D49FEF")]
    public class FeatureExtractorTest
    {
        [Test, Ignore("NoReason")]
        public void FeatureGroupExtractor()
        {
            uint imageWidth = 181;
            uint imageHeight = 217;
            uint imageDepth = 181;

            IImage imageT1 = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);
            IImage imageT2 = ImageITK.ReadImageRAW(@"Data\t2_icbm_normal_1mm_pn3_rf20.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);
            IImage imagePD = ImageITK.ReadImageRAW(@"Data\pd_icbm_normal_1mm_pn3_rf20.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);
            IImage imagePH = ImageITK.ReadImageRAW(@"Data\ph_icbm_1mm_normal_crisp.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);

            ImageFeatureGroupExtractor featureExtractor = new ImageFeatureGroupExtractor();

            featureExtractor.ImageWidth = imageWidth;
            featureExtractor.ImageHeight = imageHeight;
            featureExtractor.ImageDepth = imageDepth;

            ParameterCollection histogramParamList = new ParameterCollection();

            //order is important
            histogramParamList.Add(new ParameterObjectReferenceCollection<ImageHistogramCluster>("Cluster", new ImageHistogramCluster()));

            //histogramParamList.Add(new ParameterObjectReferenceCollection<IImage>("Image", new IImage[] {imageT1, imageT2, imagePD} ));
            histogramParamList.Add(new ParameterObjectReferenceCollection<IImage>("Image", new IImage[] { imageT1 }));

            //histogramParamList.Add(new ParameterObjectReferenceCollection<List<int>>("Slices", new List<int>(new int [] {89, 90})));
            histogramParamList.Add(new ParameterObjectReferenceCollection<List<int>>("Slices", new List<int>(new int[] { 89 })));

            //histogramParamList.Add(new ParameterNumericRange<int>("HistogramBucketSize", 1, 9, 2));
            histogramParamList.Add(new ParameterNumericRange<int>("HistogramBucketSize", 4, 4, 1));

            //histogramParamList.Add(new ParameterNumericRange<int>("MinimumClusterDistance", 0, 300, 50));
            histogramParamList.Add(new ParameterNumericRange<int>("MinimumClusterDistance", 150, 150, 50));

            histogramParamList.Add(new ParameterNumericRange<double>("BucketCountWeight", 1, 2, 0.5));
            histogramParamList.Add(new ParameterNumericRange<double>("ApproximationDegree", 0, 8, 2));
            histogramParamList.Add(new ParameterNumericRange<int>("MaxNumberOfRepresentatives", 2, 5, 1));

            featureExtractor.AddFeatureGenerator("Historgram", new ImageFeatureHistogramCluster(), histogramParamList);

            ParameterCollection phantomParamList = new ParameterCollection();
            phantomParamList.Add(new ParameterObjectReferenceCollection<IImage>("Image", imagePH));
            featureExtractor.AddFeatureGenerator("Phantom", new ImageFeatureVoxelMagnitude(), phantomParamList);

            DataTable dataTable = featureExtractor.GetDataTable(60, 61, 60, 61, 60, 61);
            Assert.IsNotNull(dataTable);

            dataTable.Dumb(@"mri.csv", ";");
            Assert.IsTrue(System.IO.File.Exists(@"mri.csv"));
        }

        [Test, Ignore("NoReason")]
        public void FeatureExtractor()
        {
            uint imageWidth = 181;
            uint imageHeight = 217;
            uint imageDepth = 181;
            int trainingSliceId = 89;

            IImage imageT1 = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);
            IImage imageT2 = ImageITK.ReadImageRAW(@"Data\t2_icbm_normal_1mm_pn3_rf20.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);
            IImage imagePD = ImageITK.ReadImageRAW(@"Data\pd_icbm_normal_1mm_pn3_rf20.rawb", imageWidth, imageHeight, imageDepth, PixelIDValueEnum.sitkUInt8);

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

            DataTable dataTable = featureExtractor.GetDataTable();
            Assert.IsNotNull(dataTable);

            dataTable.Dumb(@"mri.csv", ";");
            Assert.IsTrue(System.IO.File.Exists(@"mri.csv"));
        }
    }
}