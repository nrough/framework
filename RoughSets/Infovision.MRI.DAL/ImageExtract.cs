﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infovision.MRI.DAL
{
    public class ImageExtract : MiningObjectViewModel, IMiningObjectViewImage
    {
        private int slice;
        private bool viewImage;
        private IImage image = null;

        public ImageExtract()
            : base()
        {
        }

        public ImageExtract(IImage imageRef)
            : this()
        {
            this.ImageRef = imageRef;
        }

        public ImageExtract(IImage imageRef, int slice)
            : this(imageRef)
        {
            this.Slice = slice;
        }

        public int Slice
        {
            get { return this.slice; }
            set { SetField(ref this.slice, value, () => Slice); }
        }

        public bool ViewImage
        {
            get { return this.viewImage; }
            set { SetField(ref this.viewImage, value, () => ViewImage); }
        }

        public IImage Image
        {
            get
            {
                if (this.image == null)
                {
                    this.image = this.GetExtractedImage();
                }

                return this.image;
            }

            private set
            {
                this.image = value;
            }
        }

        public IImage ImageRef
        {
            get;
            set;
        }


        public override Type GetMiningObjectType()
        {
            return typeof(MiningObjectImageExtract);
        }

        public IImage GetExtractedImage()
        {
            Infovision.MRI.ImageITK itkImage = (Infovision.MRI.ImageITK) this.ImageRef;
            return new Infovision.MRI.ImageITK(SimpleITKHelper.GetSlice(itkImage.ItkImage, this.Slice));
        }
    }
}