using System;
using System.Runtime.InteropServices;
using itk.simple;

namespace NRough.MRI
{
    [Serializable]
    public class EdgeThresholdFilter
    {
        public EdgeThresholdFilter()
            : this(0.40, 1, 0)
        {
        }

        public EdgeThresholdFilter(double noiseThreshold)
            : this(noiseThreshold, 1, 0)
        {
        }

        public EdgeThresholdFilter(double noiseThreshold, double foreground)
            : this(noiseThreshold, foreground, 0)
        {
        }

        public EdgeThresholdFilter(double noiseThreshold, double foreground, double background)
        {
            this.NoiseThreshold = noiseThreshold;
            this.ForegroundPixelValue = foreground;
            this.BackgroundPixelValue = background;
        }

        public double ForegroundPixelValue { get; set; }
        public double BackgroundPixelValue { get; set; }
        public double NoiseThreshold { get; set; }

        public IImage Execute(IImage image, double noiseThreshold)
        {
            this.NoiseThreshold = noiseThreshold;
            return this.Execute(image);
        }

        public IImage Execute(IImage image, double noiseThreshold, double foreground)
        {
            this.NoiseThreshold = noiseThreshold;
            this.ForegroundPixelValue = foreground;
            return this.Execute(image);
        }

        public IImage Execute(IImage image, double noiseThreshold, double foreground, double background)
        {
            this.NoiseThreshold = noiseThreshold;
            this.ForegroundPixelValue = foreground;
            this.BackgroundPixelValue = background;
            return this.Execute(image);
        }

        public IImage Execute(IImage image)
        {
            uint width = image.Width;
            uint height = image.Height;
            bool isImage3D = (image.Depth >= 1);
            uint depth = isImage3D ? image.Depth : 1;

            VectorOfImage imageSeries = new VectorOfImage((int)depth);
            double[] pixels = new double[width * height];
            VectorUInt32 inSize = new VectorUInt32(new uint[] { width, height, 0 });
            VectorUInt32 position = new VectorUInt32(new uint[] { 0, 0 });

            ImageITK imageITK = ImageITK.GetImageITK(image);

            for (int z = 0; z < depth; z++)
            {
                itk.simple.Image slice = (isImage3D == true)
                                           ? SimpleITK.Extract(imageITK.ItkImage, inSize, new VectorInt32(new int[] { 0, 0, z }))
                                           : imageITK.ItkImage;

                itk.simple.Image sliceDouble = SimpleITK.Cast(slice, PixelIDValueEnum.sitkFloat64);
                sliceDouble = SimpleITK.Normalize(sliceDouble);
                Marshal.Copy(sliceDouble.GetBufferAsDouble(), pixels, 0, (int)width * (int)height);

                itk.simple.Image result = new itk.simple.Image(sliceDouble);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double edge = 0;

                        if (x == 0
                            || y == 0
                            || x == width - 1
                            || y == height - 1)
                        {
                            edge = 0;
                        }
                        else
                        {
                            /*
                             * (x-1; y-1) (x; y-1) (x+1; y-1)
                             * (x-1; y)   (x; y)   (x+1; y)
                             * (x-1; y+1) (x; y+1) (x+1; y+1)
                            */

                            double sum = (Math.Abs(pixels[(y - 1) * width + (x - 1)] - pixels[(y + 1) * width + (x + 1)])
                                       + Math.Abs(pixels[(y - 1) * width + x] - pixels[(y + 1) * width + x])
                                       + Math.Abs(pixels[(y - 1) * width + (x + 1)] - pixels[(y + 1) * width + (x - 1)])
                                       + Math.Abs(pixels[y * width + (x - 1)] - pixels[y * width + (x + 1)]))
                                    / (double)4;

                            edge = (sum > this.NoiseThreshold) ? this.ForegroundPixelValue : this.BackgroundPixelValue;
                        }

                        position[0] = (uint)x;
                        position[1] = (uint)y;

                        result.SetPixelAsDouble(position, edge);
                    }
                }

                result = SimpleITK.Cast(result, imageITK.ItkImage.GetPixelID());
                imageSeries.Add(result);
            }

            itk.simple.Image ret = SimpleITK.JoinSeries(imageSeries);

            return new ImageITK(ret);
        }
    }
}