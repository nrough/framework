﻿// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using itk.simple;

namespace NRough.MRI
{
    public class MRIHoleFillImageFilter
    {
        public MRIHoleFillImageFilter()
        {
        }

        public IImage Execute(IImage image)
        {
            uint width = image.Width;
            uint height = image.Height;
            uint depth = (image.Depth >= 1) ? image.Depth : 1;
            bool isImage3D = (image.Depth >= 1);

            VectorOfImage imageSeries = new VectorOfImage((int)depth);
            VectorUInt32 inSize = new VectorUInt32(new uint[] { width, height, 0 });
            VectorInt32 inIndex = new VectorInt32(new int[] { 0, 0, 0 });

            ImageITK imageITK = ImageITK.GetImageITK(image);

            for (int z = 0; z < depth; z++)
            {
                inIndex[2] = z;
                itk.simple.Image slice = (isImage3D == true)
                                           ? SimpleITK.Extract(imageITK.ItkImage, inSize, inIndex)
                                           : new itk.simple.Image(imageITK.ItkImage);

                GrayscaleFillholeImageFilter holeFillImageFilter = new GrayscaleFillholeImageFilter();
                itk.simple.Image sliceResult = holeFillImageFilter.Execute(slice);

                imageSeries.Add(sliceResult);
            }

            itk.simple.Image result = SimpleITK.JoinSeries(imageSeries);
            return new ImageITK(result);
        }
    }
}