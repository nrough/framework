﻿using itk.simple;

namespace Infovision.MRI
{
    public class MRIMaskBinaryImageFilter
    {
        public MRIMaskBinaryImageFilter()
        {
            this.HistogramBucketSize = 4;
            this.MaskValue = 255;
            this.BackgroundValue = 0;
        }

        public int HistogramBucketSize { get; set; }
        public byte MaskValue { get; set; }
        public byte BackgroundValue { get; set; }

        public virtual IImage Execute(IImage image)
        {
            MathNet.Numerics.Statistics.Histogram histogram 
                = ImageHistogram.GetHistogram(image, SimpleITKHelper.GetNumberOfHistogramBuckets(image, this.HistogramBucketSize));

            int j = ImageHistogram.FindLocalMinima(histogram, ImageHistogram.FindGlobalMaxima(histogram));

            BinaryThresholdImageFilter binaryImageFilter = new BinaryThresholdImageFilter();
            binaryImageFilter.SetInsideValue(this.MaskValue);
            binaryImageFilter.SetOutsideValue(this.BackgroundValue);
            binaryImageFilter.SetLowerThreshold(histogram[j].LowerBound);
            binaryImageFilter.SetUpperThreshold(SimpleITKHelper.MaxPixelValue(image.PixelType));

            ImageITK imageITK = ImageITK.GetImageITK(image);

            itk.simple.Image doubleImage;
            if ( ! imageITK.PixelType.Equals(typeof(double)))
            {
                doubleImage = SimpleITK.Cast(imageITK.ItkImage, PixelIDValueEnum.sitkFloat64);
            }
            else
            {
                doubleImage = imageITK.ItkImage;
            }

            itk.simple.Image binaryImage = binaryImageFilter.Execute(doubleImage);

            MRIHoleFillImageFilter holeFillImageFilter = new MRIHoleFillImageFilter();
            IImage result = holeFillImageFilter.Execute(new ImageITK(binaryImage));
            return result;
        }
    }
}