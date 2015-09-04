using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;
using itk.simple;
using NUnit.Framework;

namespace Infovision.MRI.UnitTests
{
    [TestFixture]
    public class HistogramTest
    {
        private uint width = 181;
        private uint height = 217;
        private uint depth = 181;
        
        
        [Test]
        public void HistogramDistance()
        {
            IImage trainImage = ImageITK.ReadImageRAW(@"t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            
            ImageHistogramCluster histCluster = new ImageHistogramCluster();

            histCluster.HistogramBucketSize = 4;
            histCluster.MinimumClusterDistance = 200;
            histCluster.BucketCountWeight = 1.5;
            histCluster.ApproximationDegree = 0.1;
            histCluster.Image = trainImage;
            histCluster.Slices = new List<int>(new int[] { 89 });
            histCluster.Train();

            double distance1 = histCluster.BucketDistance(40, 41);
            Console.WriteLine("{0} {1} {2}", 40, 41, distance1);

            double distance2 = histCluster.BucketDistance(40, 42);
            Console.WriteLine("{0} {1} {2}", 40, 42, distance2);

            double distance3 = histCluster.BucketDistance(41, 42);
            Console.WriteLine("{0} {1} {2}", 41, 42, distance3);

            Assert.Greater(distance2, distance1);
            Assert.Greater(distance2, distance3);
        }

        [Test]
        public void HistogramChart()
        {
            Int32 sliceId = 89;
            itk.simple.Image image = SimpleITK.ReadImage(@"t1_icbm_normal_1mm_pn3_rf20.img");
            itk.simple.Image slice = SimpleITK.Extract(image,
                                            new VectorUInt32(new UInt32[] { image.GetWidth(), image.GetHeight(), 0 }),
                                            new VectorInt32(new Int32[] { 0, 0, sliceId }),
                                            ExtractImageFilter.DirectionCollapseStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            VectorUInt32 vector = new VectorUInt32(new UInt32[] { 0, 0 });
            Infovision.Utils.Histogram histogram = new Infovision.Utils.Histogram();
            for (UInt32 x = 0; x < slice.GetWidth(); x++)
            {
                for (UInt32 y = 0; y < slice.GetHeight(); y++)
                {
                    vector[0] = x;
                    vector[1] = y;

                    histogram.IncreaseCount((Int64)slice.GetPixelAsUInt8(vector));
                }
            }

            //populate dataset with some demo label..
            DataSet dataSet = new DataSet();
            DataTable dt = new DataTable();
            dt.Columns.Add("Magnitude", typeof(Int64));
            dt.Columns.Add("Counter", typeof(int));

            for (Int64 i = 0; i < histogram.MaxElement; i++)
            {
                DataRow dataRow = dt.NewRow();
                dataRow[0] = i;
                dataRow[1] = histogram.GetBinValue(i);
                dt.Rows.Add(dataRow);
            }

            dataSet.Tables.Add(dt);

            //prepare chart control...
            Chart chart = new Chart();
            chart.DataSource = dataSet.Tables[0];
            chart.Width = 600;
            chart.Height = 350;

            //create serie...
            Series serie1 = new Series();
            serie1.Name = "Magnitude";
            serie1.Color = Color.FromArgb(112, 255, 200);
            serie1.BorderColor = Color.FromArgb(164, 164, 164);
            serie1.ChartType = SeriesChartType.Column;
            serie1.BorderDashStyle = ChartDashStyle.Solid;
            serie1.BorderWidth = 1;
            serie1.ShadowColor = Color.FromArgb(128, 128, 128);
            serie1.ShadowOffset = 1;
            serie1.IsValueShownAsLabel = false;
            serie1.XValueMember = "Magnitude";
            serie1.YValueMembers = "Counter";
            serie1.Font = new Font("Tahoma", 8.0f);
            serie1.BackSecondaryColor = Color.FromArgb(0, 102, 153);
            serie1.LabelForeColor = Color.FromArgb(100, 100, 100);
            chart.Series.Add(serie1);

            //create chartareas...
            ChartArea ca = new ChartArea();
            ca.Name = "ChartArea1";
            ca.BackColor = Color.White;
            ca.BorderColor = Color.FromArgb(26, 59, 105);
            ca.BorderWidth = 0;
            ca.BorderDashStyle = ChartDashStyle.Solid;
            ca.AxisX = new Axis();
            ca.AxisY = new Axis();
            chart.ChartAreas.Add(ca);

            //databind...
            chart.DataBind();

            //save result...
            String histogramFileName = @"histogram.png";
            chart.SaveImage(histogramFileName, ChartImageFormat.Png);

            Assert.IsTrue(File.Exists(histogramFileName));

            String imageJExecutable = @"C:\Program Files (x86)\ImageJ\ImageJ.exe";

            if (File.Exists(imageJExecutable))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = imageJExecutable;
                startInfo.Arguments = histogramFileName;
                Process.Start(startInfo);
            }
        }

        [Test]
        public void HistogramChartMath()
        {
            Int32 sliceId = 89;
            itk.simple.Image image = SimpleITK.ReadImage(@"t1_icbm_normal_1mm_pn3_rf20.img");
            itk.simple.Image slice = SimpleITK.Extract(image,
                                            new VectorUInt32(new UInt32[] { image.GetWidth(), image.GetHeight(), 0 }),
                                            new VectorInt32(new Int32[] { 0, 0, sliceId }),
                                            ExtractImageFilter.DirectionCollapseStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            ImageHistogram histogramChart = new ImageHistogram();
            histogramChart.HistogramBucketSize = 8;
            histogramChart.Image = new ImageITK(slice);
            Chart chart = histogramChart.GetChart();

            //save result...
            String histogramFileName = @"histogram.png";
            chart.SaveImage(histogramFileName, ChartImageFormat.Png);

            String imageJExecutable = @"C:\Program Files (x86)\ImageJ\ImageJ.exe";
            if (File.Exists(imageJExecutable))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = imageJExecutable;
                startInfo.Arguments = histogramFileName;
                Process.Start(startInfo);
            }
        }

        [Test]
        public void HistogramMathTest()
        {
            itk.simple.Image trainImage = SimpleITKHelper.ReadImageRAW(@"t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            Assert.NotNull(trainImage);

            itk.simple.Image slice = SimpleITK.Extract(trainImage,
                                            new VectorUInt32(new UInt32[] { trainImage.GetWidth(), trainImage.GetHeight(), 0 }),
                                            new VectorInt32(new Int32[] { 0, 0, (int)89 }),
                                            ExtractImageFilter.DirectionCollapseStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            Assert.NotNull(slice);

            itk.simple.Image doubleImage = SimpleITK.Cast(slice, PixelIDValueEnum.sitkFloat64.swigValue);
            StatisticsImageFilter imageStats = new StatisticsImageFilter();
            imageStats.Execute(doubleImage);

            int pixelLength = doubleImage.GetDepth() > 0
                            ? (int)(doubleImage.GetWidth() * doubleImage.GetHeight() * doubleImage.GetDepth())
                            : (int)(doubleImage.GetWidth() * doubleImage.GetHeight());

            Assert.Greater(pixelLength, 0);

            double[] pixelBuffer = new double[pixelLength];
            Assert.NotNull(pixelBuffer);

            Marshal.Copy(doubleImage.GetBufferAsDouble(), pixelBuffer, 0, pixelLength);

            MathNet.Numerics.Statistics.Histogram histogram = new MathNet.Numerics.Statistics.Histogram(pixelBuffer, 255);

            Assert.NotNull(histogram);
        }

        [Test]
        public void ImageHistogramClassifierSingleModalityTest()
        {
            string fileName = @"pd_icbm_normal_1mm_pn3_rf20.rawb";
            IImage trainImage = ImageITK.ReadImageRAW(fileName, 181, 217, 181, PixelIDValueEnum.sitkUInt8);

            ImageHistogramCluster histCluster = new ImageHistogramCluster();

            histCluster.HistogramBucketSize = 4;
            histCluster.MinimumClusterDistance = 200;
            histCluster.BucketCountWeight = 1.5;
            histCluster.ApproximationDegree = 0.1;
            histCluster.MaxNumberOfRepresentatives = 3;
            histCluster.Image = trainImage;
            histCluster.Slices = new List<int>(new int[] { 89 });
            histCluster.Train();

            IImage image = ImageITK.ReadImageRAW(fileName, 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            IImage result = histCluster.Execute(image);

            ImageITK.Show((ImageITK) image, "Histogram clustering oryginal image");
            ImageITK.Show((ImageITK) result, "Histogram clustering segmentation result");

            uint[] position = new uint[] { 0, 0, 0 };

            for (uint z = 0; z < 181; z++)
            {
                for (uint y = 0; y < 217; y++)
                {
                    for (uint x = 0; x < 181; x++)
                    {
                        position[0] = x;
                        position[1] = y;
                        position[2] = z;

                        result.GetPixel<byte>(position);
                    }
                }
            }
        }

        [Test]
        public void ImageHistogramClusterDefaultParams()
        {
            int trainSliceId = 89;

            string fileName = @"t2_icbm_normal_1mm_pn3_rf20.rawb";
            IImage trainImage = ImageITK.ReadImageRAW(fileName, width, height, depth, PixelIDValueEnum.sitkUInt8);

            IImage image = ImageITK.ReadImageRAW(fileName, width, height, depth, PixelIDValueEnum.sitkUInt8);

            ImageHistogramCluster histCluster = new ImageHistogramCluster();
            histCluster.Image = trainImage;
            histCluster.Slices = new List<int>(new int[] { trainSliceId });
            histCluster.Train();

            IImage result = histCluster.Execute(image);

            ImageITK.Show((ImageITK)image, "Histogram clustering oryginal image");
            ImageITK.Show((ImageITK)result, "Histogram clustering segmentation result");

            uint[] position = new uint[] { 0, 0, 0 };

            for (uint z = 0; z < depth; z++)
            {
                for (uint y = 0; y < height; y++)
                {
                    for (uint x = 0; x < width; x++)
                    {
                        position[0] = x;
                        position[1] = y;
                        position[2] = z;

                        result.GetPixel<byte>(position);
                    }
                }
            }
        }

        [Test]
        public void ImageHistogramClusterSave()
        {
            int trainSliceId = 69;
            string fileName = @"t1_icbm_normal_1mm_pn3_rf20.rawb";
            string saveFileName = @"histclustertest.bin";

            IImage image = ImageITK.ReadImageRAW(fileName, width, height, depth, PixelIDValueEnum.sitkUInt8);

            IImage trainImage = image.Extract(trainSliceId);

            ImageHistogramCluster histCluster = new ImageHistogramCluster();
            histCluster.Image = trainImage;
            histCluster.Train();

            Assert.IsTrue(histCluster.Save(saveFileName), "ImageHistogramCluster cannot be saved");
            
            histCluster = ImageHistogramCluster.Load(saveFileName);
            Assert.IsNotNull(histCluster.Image);

            IImage result = histCluster.Execute(image);
            Assert.IsNotNull(result);
        }

        [Test, ExpectedException("System.InvalidOperationException")]
        public void ImageHistogramNoTrain()
        {
            string fileName = @"t2_icbm_normal_1mm_pn3_rf20.rawb";
            IImage image = ImageITK.ReadImageRAW(fileName, width, height, depth, PixelIDValueEnum.sitkUInt8);
            ImageHistogramCluster histCluster = new ImageHistogramCluster();
            //histCluster.Train(trainImage, trainSliceId);

            //should throw an exception
            IImage result = histCluster.Execute(image);
        }

        [Test]
        public void TwoNearestHistogramNeighbours()
        {
            IImage trainImage = ImageITK.ReadImageRAW(@"t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);

            ImageHistogramCluster histCluster = new ImageHistogramCluster();

            histCluster.HistogramBucketSize = 4;
            histCluster.MinimumClusterDistance = 200;
            histCluster.BucketCountWeight = 1.5;
            histCluster.ApproximationDegree = 0.1;
            histCluster.Image = trainImage;
            histCluster.Slices = new List<int>(new int[] { 89 });
            histCluster.Train();
        }
    }
}
