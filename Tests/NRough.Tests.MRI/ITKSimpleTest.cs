//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

using itk.simple;
using NUnit.Framework;
using NRough.MRI;

namespace NRough.Tests.MRI
{
    [TestFixture]
    public class ITKSimpleTest
    {
        [Test]
        public void ReadImagePNG()
        {
            string fileName = @"Data\Brain-MRI.png";
            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(fileName);
            itk.simple.Image image = reader.Execute();

            Assert.IsNotNull(image);

            ImageInfo(image);
        }

        [Test]
        public void WriteImagePNG()
        {
            string inputFileName = @"Brain-MRI.png";
            string outputFileName = @"itkSimpleImage.png";

            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(inputFileName);
            itk.simple.Image input = reader.Execute();

            ImageInfo(input);

            SmoothingRecursiveGaussianImageFilter gaussian = new SmoothingRecursiveGaussianImageFilter();
            gaussian.SetSigma(2.0);
            itk.simple.Image blurredImage = gaussian.Execute(input);

            CastImageFilter caster = new CastImageFilter();
            caster.SetOutputPixelType(input.GetPixelID());
            itk.simple.Image output = caster.Execute(blurredImage);

            ImageFileWriter writer = new ImageFileWriter();
            writer.SetFileName(outputFileName);
            writer.Execute(output);

            ImageInfo(output);

            Assert.IsTrue(File.Exists(outputFileName));
        }

        [Test]
        public void ReadImageAnalyze()
        {
            string fileName = @"ANA_3CRISP00001.img";

            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(fileName);
            itk.simple.Image image = reader.Execute();

            Assert.IsNotNull(image);

            ImageInfo(image);
        }

        public void ImageInfo(itk.simple.Image image)
        {
            Console.WriteLine(image);
        }

        [Test]
        [Ignore("MINC 1.0 is not suported")]
        public void ReadImageMNC1()
        {
            string fileName = @"Data\t1_icbm_normal_1mm_pn3_rf20.mnc";
            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(fileName);
            itk.simple.Image image = reader.Execute();
            Assert.IsNotNull(image);
            ImageInfo(image);
        }

        [Test]
        [Ignore("MINC 2.0 is not suported")]
        public void ReadImageMNC2()
        {
            string fileName = @"Data\t1_icbm_normal_1mm_pn3_rf20_3.mnc";

            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(fileName);
            itk.simple.Image image = reader.Execute();

            Assert.IsNotNull(image);

            ImageInfo(image);
        }

        [Test]
        public void ReadImageRAWTest()
        {
            string fileName = @"Data\t1_icbm_normal_1mm_pn3_rf20.rawb";
            Assert.IsTrue(File.Exists(fileName));

            itk.simple.Image importedImage = SimpleITKHelper.ReadImageRAW(fileName, 181, 217, 181, PixelIDValueEnum.sitkUInt8);

            Assert.IsNotNull(importedImage);
            SimpleITK.Show(importedImage);
        }

        [Test]
        public void TestImageJ()
        {
            string fileName = @"Brain-MRI.png";

            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(fileName);
            itk.simple.Image image = reader.Execute();

            // find all java processes running ImageJ
            var query =
                "SELECT ProcessId "
                + "FROM Win32_Process "
                + "WHERE Name = 'javaw.exe' "
                + "AND CommandLine LIKE '%ImageJ%'";

            // get associated processes
            List<Process> imageJApp = null;
            using (var results = new ManagementObjectSearcher(query).Get())
            {
                imageJApp = results.Cast<ManagementObject>()
                                 .Select(mo => Process.GetProcessById((int)(uint)mo["ProcessId"]))
                                 .ToList();
            }

            foreach (Process p in imageJApp)
            {
                p.Kill();
            }

            SimpleITK.Show(image);

            // get associated processes
            imageJApp = null;
            using (var results = new ManagementObjectSearcher(query).Get())
            {
                imageJApp = results.Cast<ManagementObject>()
                                 .Select(mo => Process.GetProcessById((int)(uint)mo["ProcessId"]))
                                 .ToList();
            }

            Assert.IsTrue(imageJApp.Count > 0);

            foreach (Process p in imageJApp)
            {
                p.Kill();
            }
        }

        [Test]
        public void ReadImageBrainwebAnalyze75()
        {
            string fileName = @"t1_icbm_normal_1mm_pn3_rf20.img";
            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(fileName);
            itk.simple.Image image = reader.Execute();
            Assert.IsNotNull(image);
            ImageInfo(image);
        }

        [Test]
        public void ReadImageAndPhantomVoxels()
        {
            string imageFileName = @"t1_icbm_normal_1mm_pn3_rf20.img";
            ImageFileReader imageReader = new ImageFileReader();
            imageReader.SetFileName(imageFileName);
            itk.simple.Image image = imageReader.Execute();

            string phantomFileName = @"ph_icbm_1mm_normal_crisp.img";
            ImageFileReader phantomReader = new ImageFileReader();
            phantomReader.SetFileName(phantomFileName);
            itk.simple.Image phantom = phantomReader.Execute();

            Assert.AreEqual(image.GetDepth(), phantom.GetDepth());
            Assert.AreEqual(image.GetWidth(), phantom.GetWidth());
            Assert.AreEqual(image.GetHeight(), phantom.GetHeight());

            VectorUInt32 location = new VectorUInt32(new uint[] { 0, 0, 0 });

            for (uint z = 0; z < image.GetDepth(); z++)
            {
                for (uint y = 0; y < image.GetHeight(); y++)
                {
                    for (uint x = 0; x < image.GetWidth(); x++)
                    {
                        location[0] = x;
                        location[1] = y;
                        location[2] = z;

                        int voxelValue = image.GetPixelAsUInt8(location);
                        int phantomValue = phantom.GetPixelAsUInt8(location);

                        Assert.IsTrue(voxelValue >= 0 && voxelValue <= 255 && phantomValue >= 0 && phantomValue <= 9);
                    }
                }
            }
        }

        [Test]
        public void CheckPhantomValues()
        {
            string imageFileName = @"Data\t1_icbm_normal_1mm_pn3_rf20.img";
            ImageFileReader imageReader = new ImageFileReader();
            imageReader.SetFileName(imageFileName);
            itk.simple.Image image = imageReader.Execute();

            string phantomFileName = @"Data\ph_icbm_1mm_normal_crisp.img";
            ImageFileReader phantomReader = new ImageFileReader();
            phantomReader.SetFileName(phantomFileName);
            itk.simple.Image phantom = phantomReader.Execute();

            Assert.AreEqual(image.GetDepth(), phantom.GetDepth());
            Assert.AreEqual(image.GetWidth(), phantom.GetWidth());
            Assert.AreEqual(image.GetHeight(), phantom.GetHeight());

            VectorUInt32 location = new VectorUInt32(new uint[] { 0, 0, 0 });
            uint z = 32;

            for (uint x = 0; x < image.GetWidth(); x++)
            {
                for (uint y = 0; y < image.GetHeight(); y++)
                {
                    location[0] = x;
                    location[1] = y;
                    location[2] = z;

                    int voxelValue = image.GetPixelAsUInt8(location);
                    int phantomValue = phantom.GetPixelAsUInt8(location);

                    //Console.WriteLine("[{0}, {1}, {2}] -> {3} {4}", x, y, z, voxelValue, phantomValue);
                }
            }
        }

        [Test]
        public void ExtractImageFromImageSeries()
        {
            string imageFileName = @"Data\t1_icbm_normal_1mm_pn3_rf20.img";
            ImageFileReader imageReader = new ImageFileReader();
            imageReader.SetFileName(imageFileName);
            itk.simple.Image image = imageReader.Execute();

            uint z = 89;

            itk.simple.Image slice = new itk.simple.Image(image.GetWidth(), image.GetHeight(), PixelIDValueEnum.swigToEnum(image.GetPixelIDValue()));
            slice.SetSpacing(image.GetSpacing());

            Assert.AreEqual(image.GetPixelIDValue(), slice.GetPixelIDValue());

            VectorUInt32 sliceLocation = new VectorUInt32(new uint[] { 0, 0 });
            VectorUInt32 location = new VectorUInt32(new uint[] { 0, 0, 0 });

            for (uint x = 0; x < image.GetWidth(); x++)
            {
                for (uint y = 0; y < image.GetHeight(); y++)
                {
                    sliceLocation[0] = x;
                    sliceLocation[1] = y;

                    location[0] = x;
                    location[1] = y;
                    location[2] = z;

                    slice.SetPixelAsUInt8(sliceLocation, image.GetPixelAsUInt8(location));
                }
            }

            string outputFileName = @"slice.img";

            ImageFileWriter writer = new ImageFileWriter();
            writer.SetFileName(outputFileName);
            writer.Execute(slice);

            imageReader = new ImageFileReader();
            imageReader.SetFileName(outputFileName);
            slice = imageReader.Execute();

            for (uint x = 0; x < image.GetWidth(); x++)
            {
                for (uint y = 0; y < image.GetHeight(); y++)
                {
                    sliceLocation[0] = x;
                    sliceLocation[1] = y;

                    location[0] = x;
                    location[1] = y;
                    location[2] = z;

                    Assert.AreEqual(image.GetPixelAsUInt8(location), slice.GetPixelAsUInt8(sliceLocation));
                }
            }

            SimpleITK.Show(image);
            SimpleITK.Show(slice);

            VectorDouble sliceDirection = slice.GetDirection();
            VectorUInt32 sliceSize = slice.GetSize();

            Console.WriteLine("Direction");
            foreach (double d in sliceDirection)
            {
                Console.WriteLine("{0} ", d);
            }

            Console.WriteLine("Size");
            foreach (uint d in sliceDirection)
            {
                Console.WriteLine("{0} ", d);
            }
        }

        [Test]
        public void LaplacianFilter()
        {
            string imageFileName = @"Data\t1_icbm_normal_1mm_pn3_rf20.img";
            ImageFileReader imageReader = new ImageFileReader();
            imageReader.SetFileName(imageFileName);
            itk.simple.Image image = imageReader.Execute();

            itk.simple.Image slice = new itk.simple.Image(image.GetWidth(), image.GetHeight(), PixelIDValueEnum.swigToEnum(image.GetPixelIDValue()));
            slice.SetSpacing(image.GetSpacing());

            VectorUInt32 sliceLocation = new VectorUInt32(new uint[] { 0, 0 });
            VectorUInt32 location = new VectorUInt32(new uint[] { 0, 0, 0 });

            uint z = 32;
            for (uint x = 0; x < image.GetWidth(); x++)
            {
                for (uint y = 0; y < image.GetHeight(); y++)
                {
                    sliceLocation[0] = x;
                    sliceLocation[1] = y;

                    location[0] = x;
                    location[1] = y;
                    location[2] = z;

                    slice.SetPixelAsUInt8(sliceLocation, image.GetPixelAsUInt8(location));
                }
            }

            SmoothingRecursiveGaussianImageFilter gaussian = new SmoothingRecursiveGaussianImageFilter();
            gaussian.SetSigma(2.0);
            slice = gaussian.Execute(slice);

            slice = SimpleITK.Cast(slice, PixelIDValueEnum.sitkFloat32);
            LaplacianImageFilter laplacian = new LaplacianImageFilter();
            itk.simple.Image laplacianImage = laplacian.Execute(slice);

            SimpleITK.Show(laplacianImage, "Laplacian");
        }

        [Test]
        public void ExtractSingleSlice()
        {
            uint width = 181, height = 217;
            int sliceId = 89;
            itk.simple.Image image = SimpleITK.ReadImage(@"Data\t1_icbm_normal_1mm_pn3_rf20.img");
            itk.simple.Image slice = SimpleITK.Extract(image,
                                            new VectorUInt32(new uint[] { width, height, 0 }),
                                            new VectorInt32(new int[] { 0, 0, sliceId }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);
            Assert.IsNotNull(slice);
            SimpleITK.Show(slice, "Extracted slice 89");
        }

        [Test]
        public void NeighbouirhoodFilter()
        {
            uint width = 181, height = 217;
            int sliceId = 89;
            itk.simple.Image image = SimpleITK.ReadImage(@"Data\t1_icbm_normal_1mm_pn3_rf20.img");
            itk.simple.Image slice = SimpleITK.Extract(image,
                                            new VectorUInt32(new uint[] { width, height, 0 }),
                                            new VectorInt32(new int[] { 0, 0, sliceId }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            VectorUIntList seeds = new VectorUIntList();
            VectorUInt32 seedValues = new VectorUInt32();
            seedValues.Add(135);
            seedValues.Add(135);
            seeds.Add(seedValues);

            double lowerThreshold = 100;
            double upperThreshold = 180;

            VectorUInt32 radius = new VectorUInt32(new uint[] { 1, 1 });
            double replaceValue = 255;

            itk.simple.Image neighbourhood = SimpleITK.NeighborhoodConnected(slice, seeds, lowerThreshold, upperThreshold, radius, replaceValue);
            SimpleITK.Show(neighbourhood, "Neighbourhood");
        }

        [Test]
        public void ImageStatistics()
        {
            uint width = 181, height = 217;
            int sliceId = 89;
            itk.simple.Image image = SimpleITK.ReadImage(@"Data\t1_icbm_normal_1mm_pn3_rf20.img");
            itk.simple.Image slice = SimpleITK.Extract(image,
                                            new VectorUInt32(new uint[] { width, height, 0 }),
                                            new VectorInt32(new int[] { 0, 0, sliceId }),
                                            ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);

            StatisticsImageFilter imageStats = new StatisticsImageFilter();
            imageStats.Execute(slice);

            Console.WriteLine("Min: {0}", imageStats.GetMinimum());
            Console.WriteLine("Max: {0}", imageStats.GetMaximum());
            Console.WriteLine("Mean: {0}", imageStats.GetMean());
            Console.WriteLine("Sigma: {0}", imageStats.GetSigma());
            Console.WriteLine("Variance: {0}", imageStats.GetVariance());
            Console.WriteLine("Sum: {0}", imageStats.GetSum());
        }

        [Test]
        public void ImageSOMClassifierTest()
        {
            Assert.IsNotNull(SimpleITKHelper.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8));
            Assert.IsNotNull(SimpleITKHelper.ReadImageRAW(@"Data\t2_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8));
            Assert.IsNotNull(SimpleITKHelper.ReadImageRAW(@"Data\pd_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8));

            ImageSOMCluster som = new ImageSOMCluster(3, 9);
            som.Train(new IImage[] {
                        ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8),
                        ImageITK.ReadImageRAW(@"Data\t2_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8),
                        ImageITK.ReadImageRAW(@"Data\pd_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8)},
                      100,
                      0.1,
                      15,
                      89);

            IImage result = som.Execute(new IImage[] {
                                        ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8),
                                        ImageITK.ReadImageRAW(@"Data\t2_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8),
                                        ImageITK.ReadImageRAW(@"Data\pd_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8)});

            ImageITK.Show((ImageITK)result);
        }

        [Test]
        public void ImageSOMClassifierSingleModalityTest()
        {
            Assert.IsNotNull(SimpleITKHelper.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8));

            ImageSOMCluster som = new ImageSOMCluster(1, 9);
            som.Train(ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8),
                      100,
                      0.1,
                      15,
                      89);

            IImage image = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            IImage result = som.Execute(image);
            ImageITK.Show((ImageITK)result);
        }

        [Test]
        public void SimpleContourExtractorImageFilter()
        {
            itk.simple.Image trainImage = SimpleITKHelper.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);

            HConvexImageFilter convexImage = new HConvexImageFilter();
            itk.simple.Image img = convexImage.Execute(trainImage, 80, true);
            SimpleITK.Show(img);
        }

        [Test]
        public void MRIMaskImageFilter()
        {
            IImage image = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);

            MRIMaskBinaryImageFilter mriMaskImageFilter = new MRIMaskBinaryImageFilter();
            IImage result = mriMaskImageFilter.Execute(image);
            ImageITK.Show((ImageITK)result);

            for (int i = 60; i < 80; i++)
            {
                IImage slice = image.Extract(i);
                result = mriMaskImageFilter.Execute(slice);
                ImageITK.Show((ImageITK)result);
            }
        }

        [Test]
        public void MRIMaskImageFilterBinaryHoleFilling()
        {
            itk.simple.Image image = SimpleITKHelper.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            SimpleITK.Show(image);

            itk.simple.Image doubleImage = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat64);
            int numberOfBuckets = (int)Math.Ceiling(SimpleITKHelper.MaxPixelValue(image.GetPixelIDValue()) / (double)4);
            MathNet.Numerics.Statistics.Histogram histogram = ImageHistogram.GetHistogram(doubleImage, numberOfBuckets);

            int j = ImageHistogram.FindLocalMinima(histogram, ImageHistogram.FindGlobalMaxima(histogram));

            BinaryThresholdImageFilter binaryImageFilter = new BinaryThresholdImageFilter();
            binaryImageFilter.SetInsideValue(255);
            binaryImageFilter.SetOutsideValue(0);
            binaryImageFilter.SetLowerThreshold(histogram[j].LowerBound);
            binaryImageFilter.SetUpperThreshold(SimpleITKHelper.MaxPixelValue(image.GetPixelIDValue()));
            itk.simple.Image binaryImage = binaryImageFilter.Execute(doubleImage);

            binaryImage = SimpleITK.Cast(binaryImage, itk.simple.PixelIDValueEnum.sitkFloat64);

            SimpleITK.Show(binaryImage, "Binary Threshold");

            itk.simple.Image negativeBinaryImage = SimpleITK.UnaryMinus(binaryImage);
            SimpleITK.Show(negativeBinaryImage, "Negative");

            GrayscaleFillholeImageFilter holeFillImageFilter = new GrayscaleFillholeImageFilter();
            itk.simple.Image holeFillImage = holeFillImageFilter.Execute(negativeBinaryImage, false);

            SimpleITK.Show(holeFillImage, "GreyScale Hole Filling");

            SubtractImageFilter substractImageFilter = new SubtractImageFilter();
            itk.simple.Image substractedImage = substractImageFilter.Execute(holeFillImage, negativeBinaryImage);

            SimpleITK.Show(substractedImage, "Result");

            /*
            OtsuThresholdImageFilter otsuImageFilter = new OtsuThresholdImageFilter();
            otsuImageFilter.SetInsideValue(0);
            otsuImageFilter.SetOutsideValue(255);
            otsuImageFilter.SetNumberOfHistogramBins( (uint) Math.Ceiling(SimpleITKHelper.MaxPixelValue(image.GetPixelIDValue()) / (double) 4) );
            itk.simple.ImageRead otsuImage = otsuImageFilter.Execute(doubleImage);
            SimpleITK.Show(otsuImage, "Otsu Threshold");
            */

            /*
            VotingBinaryHoleFillingImageFilter holeFillingBinary = new VotingBinaryHoleFillingImageFilter();
            holeFillingBinary.SetBackgroundValue(0);
            holeFillingBinary.SetForegroundValue(255);
            holeFillingBinary.SetRadius(new VectorUInt32(new uint[] {30, 30, 30}));
            itk.simple.ImageRead result1 = holeFillingBinary.Execute(otsuImage);
            SimpleITK.Show(result1, "Voting Binary Hole Filling");
            */

            /*
            VotingBinaryIterativeHoleFillingImageFilter holeFillingBinaryIterative = new VotingBinaryIterativeHoleFillingImageFilter();
            holeFillingBinaryIterative.SetBackgroundValue(0);
            holeFillingBinaryIterative.SetForegroundValue(255);
            holeFillingBinaryIterative.SetRadius(new VectorUInt32(new uint[] { 30, 30, 30 }));
            holeFillingBinaryIterative.SetMaximumNumberOfIterations(10);
            itk.simple.ImageRead result3 = holeFillingBinaryIterative.Execute(otsuImage);

            SimpleITK.Show(result3, "Voting Binary Hole Filling Iterative");
            */
        }

        [Test]
        public void Rescale()
        {
            itk.simple.Image image = SimpleITKHelper.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            SimpleITK.Show(image);

            itk.simple.Image result = SimpleITK.ShiftScale(image, 100, 0.6);
            SimpleITK.Show(result);
        }

        [Test]
        public void GreyscaleHoleFillImageFilter()
        {
            itk.simple.Image image = SimpleITK.UnaryMinus(SimpleITK.GaussianSource());
            SimpleITK.Show(image);
            SimpleITK.Show(SimpleITK.GrayscaleFillhole(image));
            SimpleITK.Show(SimpleITK.Subtract(SimpleITK.GrayscaleFillhole(image), image));
        }

        [Test]
        public void MRIHoleFillImageFilter()
        {
            IImage image = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            ImageITK.Show((ImageITK)image);

            IImage thresholdImage = new MRIMaskBinaryImageFilter().Execute(image);
            ImageITK.Show((ImageITK)thresholdImage);

            IImage holeFillImage = new MRIHoleFillImageFilter().Execute(thresholdImage);
            ImageITK.Show((ImageITK)holeFillImage);
        }

        [Test]
        public void ShrinkImageFilter()
        {
            itk.simple.Image image = SimpleITKHelper.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            SimpleITK.Show(image);

            ShrinkImageFilter shrinkFilter = new ShrinkImageFilter();
            shrinkFilter.SetShrinkFactors(new VectorUInt32(new uint[] { 2, 1, 1 }));

            itk.simple.Image shrinkImage = shrinkFilter.Execute(image);
            SimpleITK.Show(shrinkImage);
        }

        [Test]
        public void ErodeFilter()
        {
            IImage image = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            ImageITK.Show((ImageITK)image);

            ImageITK maskImage = (ImageITK)new MRIMaskBinaryImageFilter().Execute(image);
            ImageITK.Show(maskImage);

            BinaryErodeImageFilter binaryErode20 = new BinaryErodeImageFilter();
            binaryErode20.SetBackgroundValue(0);
            binaryErode20.SetForegroundValue(255);
            binaryErode20.SetKernelRadius(10);
            binaryErode20.SetKernelType(KernelEnum.sitkCross);
            itk.simple.Image erodedImage20 = binaryErode20.Execute(maskImage.ItkImage);
            SimpleITK.Show(erodedImage20);

            BinaryErodeImageFilter binaryErode40 = new BinaryErodeImageFilter();
            binaryErode40.SetBackgroundValue(0);
            binaryErode40.SetForegroundValue(255);
            binaryErode40.SetKernelRadius(50);
            binaryErode40.SetKernelType(KernelEnum.sitkCross);
            itk.simple.Image erodedImage40 = binaryErode40.Execute(maskImage.ItkImage);
            SimpleITK.Show(erodedImage40);

            itk.simple.Image overlyImage = SimpleITK.Subtract(maskImage.ItkImage, erodedImage20);
            SimpleITK.Show(overlyImage);

            overlyImage = SimpleITK.Subtract(erodedImage20, erodedImage40);
            SimpleITK.Show(overlyImage);
        }

        [Test]
        public void MRIBinaryMask()
        {
            IImage image = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            ImageITK.Show((ImageITK)image);

            IImage maskImage = new MRIMaskBinaryImageFilter().Execute(image);
            //SimpleITK.Show(image);

            MRIMaskConcentricImageFilter binarySubMask = new MRIMaskConcentricImageFilter();

            binarySubMask.AddMaskItem(new MRIMaskItem { LabelValue = 10, Radius = 10 });
            binarySubMask.AddMaskItem(new MRIMaskItem { LabelValue = 20, Radius = 10 });
            binarySubMask.AddMaskItem(new MRIMaskItem { LabelValue = 30, Radius = 10 });
            binarySubMask.AddMaskItem(new MRIMaskItem { LabelValue = 40, Radius = 10 });
            binarySubMask.AddMaskItem(new MRIMaskItem { LabelValue = 50, Radius = 10 });
            binarySubMask.AddMaskItem(new MRIMaskItem { LabelValue = 60, Radius = 10 });

            IImage maskConcentric = binarySubMask.Execute(maskImage);
            ImageITK.Show((ImageITK)maskConcentric);
        }

        [Test]
        public void ErodeMultiple()
        {
            IImage image = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8);
            ImageITK.Show((ImageITK)image);
            IImage slice = image.Extract(89);
            IImage maskImage = new MRIMaskBinaryImageFilter().Execute(slice);
            ImageITK maskImageItk = ImageITK.GetImageITK(maskImage);

            BinaryErodeImageFilter binaryErode = new BinaryErodeImageFilter();
            binaryErode.SetBackgroundValue(0);
            binaryErode.SetForegroundValue(255);
            binaryErode.SetKernelRadius(10);
            binaryErode.SetKernelType(KernelEnum.sitkCross);

            itk.simple.Image erodedImage = new itk.simple.Image(maskImageItk.ItkImage);
            erodedImage.CopyInformation(maskImageItk.ItkImage);

            SimpleITK.Show(erodedImage);

            for (int i = 0; i < 5; i++)
            {
                erodedImage = binaryErode.Execute(erodedImage);
                SimpleITK.Show(erodedImage);
            }
        }

        [Test]
        public void PixelIDValues()
        {
            Console.WriteLine("sitkComplexFloat32: {0}", PixelIDValueEnum.sitkComplexFloat32.swigValue);
            Console.WriteLine("sitkComplexFloat64: {0}", PixelIDValueEnum.sitkComplexFloat64.swigValue);
            Console.WriteLine("sitkFloat32: {0}", PixelIDValueEnum.sitkFloat32.swigValue);
            Console.WriteLine("sitkFloat64: {0}", PixelIDValueEnum.sitkFloat64.swigValue);
            Console.WriteLine("sitkInt16: {0}", PixelIDValueEnum.sitkInt16.swigValue);
            Console.WriteLine("sitkInt32: {0}", PixelIDValueEnum.sitkInt32.swigValue);
            Console.WriteLine("sitkInt64: {0}", PixelIDValueEnum.sitkInt64.swigValue);
            Console.WriteLine("sitkInt8: {0}", PixelIDValueEnum.sitkInt8.swigValue);
            Console.WriteLine("sitkLabelUInt16: {0}", PixelIDValueEnum.sitkLabelUInt16.swigValue);
            Console.WriteLine("sitkLabelUInt32: {0}", PixelIDValueEnum.sitkLabelUInt32.swigValue);
            Console.WriteLine("sitkLabelUInt64: {0}", PixelIDValueEnum.sitkLabelUInt64.swigValue);
            Console.WriteLine("sitkLabelUInt8: {0}", PixelIDValueEnum.sitkLabelUInt8.swigValue);
            Console.WriteLine("sitkUInt16: {0}", PixelIDValueEnum.sitkUInt16.swigValue);
            Console.WriteLine("sitkUInt32: {0}", PixelIDValueEnum.sitkUInt32.swigValue);
            Console.WriteLine("sitkUInt64: {0}", PixelIDValueEnum.sitkUInt64.swigValue);
            Console.WriteLine("sitkUInt8: {0}", PixelIDValueEnum.sitkUInt8.swigValue);
            Console.WriteLine("sitkUnknown: {0}", PixelIDValueEnum.sitkUnknown.swigValue);
            Console.WriteLine("sitkVectorFloat32: {0}", PixelIDValueEnum.sitkVectorFloat32.swigValue);
            Console.WriteLine("sitkVectorFloat64: {0}", PixelIDValueEnum.sitkVectorFloat64.swigValue);
            Console.WriteLine("sitkVectorInt16: {0}", PixelIDValueEnum.sitkVectorInt16.swigValue);
            Console.WriteLine("sitkVectorInt32: {0}", PixelIDValueEnum.sitkVectorInt32.swigValue);
            Console.WriteLine("sitkVectorInt64: {0}", PixelIDValueEnum.sitkVectorInt64.swigValue);
            Console.WriteLine("sitkVectorInt8: {0}", PixelIDValueEnum.sitkVectorInt8.swigValue);
            Console.WriteLine("sitkVectorUInt16: {0}", PixelIDValueEnum.sitkVectorUInt16.swigValue);
            Console.WriteLine("sitkVectorUInt32: {0}", PixelIDValueEnum.sitkVectorUInt32.swigValue);
            Console.WriteLine("sitkVectorUInt64: {0}", PixelIDValueEnum.sitkVectorUInt64.swigValue);
            Console.WriteLine("sitkVectorUInt8: {0}", PixelIDValueEnum.sitkVectorUInt8.swigValue);
        }

        [Test]
        public void GetImageBuffer()
        {
            uint width = 181;
            uint height = 217;
            uint depth = 181;
            int sliceIdx = 69;

            itk.simple.Image image = SimpleITKHelper.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb",
                                                                 width,
                                                                 height,
                                                                 depth,
                                                                 PixelIDValueEnum.sitkUInt8);

            itk.simple.Image slice = SimpleITKHelper.GetSlice(image, sliceIdx);

            byte[] pixels = new byte[width * height];
            Marshal.Copy(slice.GetBufferAsUInt8(), pixels, 0, (int)width * (int)height);
            VectorUInt32 position = new VectorUInt32(new uint[] { 0, 0 });

            for (uint x = 0; x < width; x++)
            {
                for (uint y = 0; y < height; y++)
                {
                    position[0] = x;
                    position[1] = y;
                    byte pixelValue = slice.GetPixelAsUInt8(position);
                    Assert.AreEqual(pixelValue, pixels[y * width + x]);
                }
            }

            SimpleITK.Show(slice);
        }

        [Test]
        public void MyEdgeDetector()
        {
            uint width = 181;
            uint height = 217;
            uint depth = 181;

            IImage image = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb",
                                                                 width,
                                                                 height,
                                                                 depth,
                                                                 PixelIDValueEnum.sitkUInt8);

            ImageITK.Show((ImageITK)new EdgeThresholdFilter().Execute(image, 0.40, byte.MaxValue, 0));
        }

        [Test]
        public void EdgeNeighbourHistogramFilter()
        {
            uint width = 181;
            uint height = 217;
            uint depth = 181;

            IImage image = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb",
                                                                 width,
                                                                 height,
                                                                 depth,
                                                                 PixelIDValueEnum.sitkUInt8);

            EdgeThresholdFilter edge = new EdgeThresholdFilter();
            EdgeNeighbourHistogramFilter neighbour = new EdgeNeighbourHistogramFilter();
            neighbour.EdgeImage = edge.Execute(image, 0.40, 200, 0);

            ImageHistogramCluster histCluster = new ImageHistogramCluster();

            histCluster.HistogramBucketSize = 4;
            histCluster.MinimumClusterDistance = 200;
            histCluster.BucketCountWeight = 1.5;
            histCluster.ApproximationDegree = 0.1;
            histCluster.MaxNumberOfRepresentatives = 3;
            histCluster.Image = image;
            histCluster.Train();

            neighbour.ReferenceImage = histCluster.Execute(image);

            ImageITK.Show((ImageITK)neighbour.Execute());
        }

        [Test]
        public void EdgeNeighbourSOMFilter()
        {
            uint width = 181;
            uint height = 217;
            uint depth = 181;

            IImage image = ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb",
                                                                 width,
                                                                 height,
                                                                 depth,
                                                                 PixelIDValueEnum.sitkUInt8);

            EdgeThresholdFilter edge = new EdgeThresholdFilter();
            EdgeNeighbourSOMFilter neighbour = new EdgeNeighbourSOMFilter();
            neighbour.EdgeImage = edge.Execute(image, 0.40, 200, 0);

            ImageSOMCluster som = new ImageSOMCluster(3, 9);
            som.Train(new IImage[] {
                        ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8),
                        ImageITK.ReadImageRAW(@"Data\t2_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8),
                        ImageITK.ReadImageRAW(@"Data\pd_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8)},
                      100,
                      0.1,
                      15,
                      89);

            neighbour.ReferenceImage = som.Execute(new IImage[] {
                                        ImageITK.ReadImageRAW(@"Data\t1_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8),
                                        ImageITK.ReadImageRAW(@"Data\t2_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8),
                                        ImageITK.ReadImageRAW(@"Data\pd_icbm_normal_1mm_pn3_rf20.rawb", 181, 217, 181, PixelIDValueEnum.sitkUInt8)});

            ImageITK.Show((ImageITK)neighbour.Execute());
        }
    }
}