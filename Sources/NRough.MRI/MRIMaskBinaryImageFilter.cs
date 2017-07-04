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
using itk.simple;

namespace NRough.MRI
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
            if (!imageITK.PixelType.Equals(typeof(double)))
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