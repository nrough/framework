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
using System.Linq;
using System.Runtime.InteropServices;
using AForge.Neuro;
using itk.simple;

namespace NRough.MRI
{
    public class EdgeNeighbourFilter
    {
        private IImage edgeImage = null;
        private bool isEdgeImageSet = false;

        private IImage referenceImage = null;
        private bool isReferenceImageSet = false;

        private double noiseThreshold = 0.20;

        public EdgeNeighbourFilter()
        {
        }

        public IImage EdgeImage
        {
            get { return this.edgeImage; }
            set
            {
                this.edgeImage = value;
                this.IsEdgeImageSet = (this.edgeImage != null);
            }
        }

        public IImage ReferenceImage
        {
            get { return this.referenceImage; }
            set
            {
                this.referenceImage = value;
                this.IsReferenceImageSet = (this.referenceImage != null);
            }
        }

        protected bool IsReferenceImageSet
        {
            get { return this.isReferenceImageSet; }
            private set { this.isReferenceImageSet = value; }
        }

        protected bool IsEdgeImageSet
        {
            get { return this.isEdgeImageSet; }
            private set { this.isEdgeImageSet = value; }
        }

        public double NoiseThreshold
        {
            get { return this.noiseThreshold; }
            set { this.noiseThreshold = value; }
        }

        protected virtual void CreateEdgeImage(IImage image)
        {
            if (!this.IsEdgeImageSet)
            {
                EdgeNeighbourFilter edgeFilter = new EdgeNeighbourFilter();
                edgeFilter.NoiseThreshold = this.NoiseThreshold;
                this.EdgeImage = edgeFilter.Execute(image);
            }
        }

        protected virtual void CreateReferenceImage(IImage image)
        {
            if (!this.IsReferenceImageSet)
            {
                this.ReferenceImage = image;
            }
        }

        public virtual IImage Execute()
        {
            if (this.IsEdgeImageSet == false || this.IsReferenceImageSet == false)
            {
                throw new InvalidOperationException("Edge Image and Reference Image must be set. Should you use Execute(Image) method instead?");
            }

            uint width = referenceImage.Width;
            uint height = referenceImage.Height;
            bool isImage3D = referenceImage.Depth > 0 ? true : false;
            uint depth = isImage3D ? referenceImage.Depth : 1;
            byte[] pixels = new byte[(int)width * (int)height];

            VectorUInt32 inSize = new VectorUInt32(new uint[] { width, height, 0 });
            VectorUInt32 position = new VectorUInt32(new uint[] { 0, 0 });
            VectorOfImage imageSeries = new VectorOfImage((int)depth);

            ImageITK referenceImageITK = ImageITK.GetImageITK(referenceImage);

            for (int z = 0; z < depth; z++)
            {
                itk.simple.Image slice = (isImage3D == true)
                                           ? SimpleITK.Extract(referenceImageITK.ItkImage, inSize, new VectorInt32(new int[] { 0, 0, z }))
                                           : referenceImageITK.ItkImage;

                itk.simple.Image sliceConverted = SimpleITK.Cast(slice, PixelIDValueEnum.sitkUInt8);

                Marshal.Copy(sliceConverted.GetBufferAsUInt8(), pixels, 0, (int)(width * height));
                itk.simple.Image result = new itk.simple.Image(sliceConverted);

                for (uint y = 0; y < height; y++)
                {
                    for (uint x = 0; x < width; x++)
                    {
                        position[0] = x;
                        position[1] = y;

                        result.SetPixelAsUInt8(position, this.GetNeighbour(pixels, (int)width, (int)height, x, y));
                    }
                }

                result = SimpleITK.Cast(result, referenceImageITK.ItkImage.GetPixelID());
                imageSeries.Add(result);
            }

            itk.simple.Image ret = SimpleITK.JoinSeries(imageSeries);
            return new ImageITK(ret);
        }

        private byte GetNeighbour(byte[] pixelData, int width, int height, uint x, uint y)
        {
            Dictionary<byte, int> pixelCount = new Dictionary<byte, int>(8);
            int count;
            byte pixelValue;

            if (y > 0 && x > 0)
            {
                pixelValue = pixelData[(y - 1) * width + (x - 1)];
                pixelCount[pixelValue] = pixelCount.TryGetValue(pixelValue, out count) ? ++count : 1;
            }

            if (y > 0)
            {
                pixelValue = pixelData[(y - 1) * width + x];
                pixelCount[pixelValue] = pixelCount.TryGetValue(pixelValue, out count) ? ++count : 1;
            }

            if (y > 0 && x < width - 1)
            {
                pixelValue = pixelData[(y - 1) * width + (x + 1)];
                pixelCount[pixelValue] = pixelCount.TryGetValue(pixelValue, out count) ? ++count : 1;
            }

            if (x > 0)
            {
                pixelValue = pixelData[y * width + (x - 1)];
                pixelCount[pixelValue] = pixelCount.TryGetValue(pixelValue, out count) ? ++count : 1;
            }

            pixelValue = pixelData[y * width + x];
            pixelCount[pixelValue] = pixelCount.TryGetValue(pixelValue, out count) ? ++count : 1;

            if (x < width - 1)
            {
                pixelValue = pixelData[y * width + (x + 1)];
                pixelCount[pixelValue] = pixelCount.TryGetValue(pixelValue, out count) ? ++count : 1;
            }

            if (x > 0 && y < height - 1)
            {
                pixelValue = pixelData[(y + 1) * width + (x - 1)];
                pixelCount[pixelValue] = pixelCount.TryGetValue(pixelValue, out count) ? ++count : 1;
            }

            if (y < height - 1)
            {
                pixelValue = pixelData[(y + 1) * width + x];
                pixelCount[pixelValue] = pixelCount.TryGetValue(pixelValue, out count) ? ++count : 1;
            }

            if (x < width - 1 && y < height - 1)
            {
                pixelValue = pixelData[(y + 1) * width + (x + 1)];
                pixelCount[pixelValue] = pixelCount.TryGetValue(pixelValue, out count) ? ++count : 1;
            }

            KeyValuePair<byte, int> mostFrequent = pixelCount.First();
            foreach (KeyValuePair<byte, int> kvp in pixelCount)
            {
                if (kvp.Value > mostFrequent.Value)
                    mostFrequent = kvp;
            }

            return mostFrequent.Key;
        }

        public virtual IImage Execute(IImage image)
        {
            CreateEdgeImage(image);
            CreateReferenceImage(image);

            return this.Execute();
        }
    }

    public class EdgeNeighbourHistogramFilter : EdgeNeighbourFilter
    {
        private double histogramApproximationDegree = 0;
        private int histogramBucketSize = 4;
        private int histogramMaxNumberOfRepresentatives = 3;
        private double histogramMinClusterDistance = 15;
        private double histogramBucketCountWeight = 1;

        public double HistogramBucketCountWeight
        {
            get { return histogramBucketCountWeight; }
            set { histogramBucketCountWeight = value; }
        }

        public double HistogramMinClusterDistance
        {
            get { return histogramMinClusterDistance; }
            set { histogramMinClusterDistance = value; }
        }

        public int HistogramMaxNumberOfRepresentatives
        {
            get { return histogramMaxNumberOfRepresentatives; }
            set { histogramMaxNumberOfRepresentatives = value; }
        }

        public double HistogramApproximationDegree
        {
            get { return this.histogramApproximationDegree; }
            set { this.histogramApproximationDegree = value; }
        }

        public int HistogramBucketSize
        {
            get { return this.histogramBucketSize; }
            set { this.histogramBucketSize = value; }
        }

        protected override void CreateReferenceImage(IImage image)
        {
            if (!this.IsReferenceImageSet)
            {
                ImageHistogramCluster histogramCluster = new ImageHistogramCluster();
                histogramCluster.ApproximationDegree = this.HistogramApproximationDegree;
                histogramCluster.HistogramBucketSize = this.HistogramBucketSize;
                histogramCluster.MaxNumberOfRepresentatives = this.HistogramMaxNumberOfRepresentatives;
                histogramCluster.MinimumClusterDistance = this.HistogramMinClusterDistance;
                histogramCluster.BucketCountWeight = this.HistogramBucketCountWeight;
                this.ReferenceImage = histogramCluster.Execute(image);
            }
        }
    }

    public class EdgeNeighbourSOMFilter : EdgeNeighbourFilter
    {
        private DistanceNetwork trainedDistanceNetwork = null;

        public DistanceNetwork DistanceNetwork
        {
            get { return trainedDistanceNetwork; }
            set { trainedDistanceNetwork = value; }
        }

        protected override void CreateReferenceImage(IImage image)
        {
            if (!this.IsReferenceImageSet)
            {
                if (this.DistanceNetwork == null)
                {
                    throw new InvalidOperationException("AForge.Neuro.DistanceNetwork is not set");
                }

                ImageSOMCluster somCluster = new ImageSOMCluster(this.DistanceNetwork);
                this.ReferenceImage = somCluster.Execute(image);
            }
        }
    }
}