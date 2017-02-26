using System;
using System.Collections.Generic;
using itk.simple;

namespace NRough.MRI
{
    public class MRIMaskConcentricImageFilter
    {
        private List<MRIMaskItem> items = new List<MRIMaskItem>();
        private double foregroundValue;

        public MRIMaskConcentricImageFilter()
        {
            this.BackgroundValue = 0;
            this.ForegroundValue = 255;

            //0.5.1 this.KernelType = BinaryErodeImageFilter.KernelType.Cross;
            this.KernelType = KernelEnum.sitkCross;
        }

        public double BackgroundValue { get; set; }

        public double ForegroundValue
        {
            get
            {
                return this.foregroundValue;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("Foreground must be positive non zero double.");
                this.foregroundValue = value;
            }
        }

        //public BinaryErodeImageFilter.KernelType KernelType { get; set; }
        public KernelEnum KernelType { get; set; }

        public bool AddMaskItem(MRIMaskItem item)
        {
            if (!this.items.Contains(item))
            {
                this.items.Add(item);
                return true;
            }
            return false;
        }

        public void AddMaskItems(System.Collections.Generic.IEnumerable<MRIMaskItem> items)
        {
            this.items = new List<MRIMaskItem>(items);
        }

        public bool AddMaskItem(int label, int radius)
        {
            return this.AddMaskItem(new MRIMaskItem { LabelValue = label, Radius = radius });
        }

        public bool RemoveMaskItem(MRIMaskItem item)
        {
            try
            {
                return this.items.Remove(item);
            }
            catch { }
            return false;
        }

        public bool RemoveMaskItem(int label, int radius)
        {
            return this.RemoveMaskItem(new MRIMaskItem { LabelValue = label, Radius = radius });
        }

        public IImage Execute(IImage image)
        {
            bool isImage3D = (image.Depth >= 1) ? true : false;
            uint width = image.Width;
            uint height = image.Height;
            uint depth = (image.Depth >= 1) ? image.Depth : 1;

            VectorOfImage imageSeries = new VectorOfImage((int)depth);

            BinaryErodeImageFilter binaryErode = new BinaryErodeImageFilter();
            binaryErode.SetBackgroundValue(this.BackgroundValue);
            binaryErode.SetForegroundValue(this.ForegroundValue);
            binaryErode.SetKernelType(this.KernelType);

            ExtractImageFilter extractImage = new ExtractImageFilter();
            extractImage.SetDirectionCollapseToStrategy(ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX);
            VectorUInt32 sliceSize = new VectorUInt32(new uint[] { width, height, 0 });
            extractImage.SetSize(sliceSize);
            VectorInt32 sliceIndex = new VectorInt32(new int[] { 0, 0, 0 });

            ShiftScaleImageFilter scaleImageFilter = new ShiftScaleImageFilter();
            AddImageFilter addImageFilter = new AddImageFilter();
            SubtractImageFilter subtractImageFilter = new SubtractImageFilter();

            CastImageFilter castImageFilter = new CastImageFilter();
            castImageFilter.SetOutputPixelType(PixelIDValueEnum.sitkComplexFloat64.swigValue);

            itk.simple.Image outerMask;
            ImageITK imageITK = ImageITK.GetImageITK(image);

            for (int z = 0; z < depth; z++)
            {
                sliceIndex[2] = z;

                outerMask = (isImage3D == true)
                      ? extractImage.Execute(imageITK.ItkImage, sliceSize, sliceIndex, ExtractImageFilter.DirectionCollapseToStrategyType.DIRECTIONCOLLAPSETOSUBMATRIX)
                      : new itk.simple.Image(imageITK.ItkImage);

                itk.simple.Image labelMap = new itk.simple.Image(outerMask.GetWidth(), outerMask.GetHeight(), PixelIDValueEnum.swigToEnum(outerMask.GetPixelIDValue()));
                labelMap.CopyInformation(outerMask);

                binaryErode.SetForegroundValue(this.ForegroundValue);

                int i = 0;

                itk.simple.Image innerMask = null;

                foreach (MRIMaskItem item in this.items)
                {
                    binaryErode.SetKernelRadius((uint)item.Radius);
                    innerMask = binaryErode.Execute(outerMask);

                    itk.simple.Image singleLabel = subtractImageFilter.Execute(outerMask, innerMask);

                    //scaleImageFilter.SetShift((int)labelValues[i] - (int)binaryErode.GetForegroundValue());
                    scaleImageFilter.SetScale(item.LabelValue / binaryErode.GetForegroundValue());

                    innerMask = scaleImageFilter.Execute(innerMask);
                    labelMap = addImageFilter.Execute(labelMap, singleLabel);

                    if (i == this.items.Count - 1)
                    {
                        labelMap = addImageFilter.Execute(labelMap, innerMask);
                    }
                    else
                    {
                        binaryErode.SetForegroundValue((double)item.LabelValue);
                        i++;
                        outerMask = innerMask;
                    }
                }

                imageSeries.Add(labelMap);
            }

            itk.simple.Image result = SimpleITK.JoinSeries(imageSeries);

            return new ImageITK(result);
        }
    }
}