using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq.Expressions;
using Raccoon.MRI;

namespace Raccoon.MRI.DAL
{
    [Serializable]
    public class ImageRead : MiningObjectViewModel, IMiningObjectViewImage
    {
        private long id;
        private string name = String.Empty;
        private long? parentId = null;

        private string fileName = String.Empty;

        private uint width = 0;
        private uint height = 0;
        private uint depth = 0;

        private Endianness endianness = Endianness.LittleEndian;
        private PixelType pixelType = PixelType.Unknown;
        private int header = 0;

        private int sliceFrom = 0;
        private int sliceTo = 0;

        private ImageType imageType = ImageType.Unknown;

        private IImage image = null;

        private bool viewImage;

        public ImageRead()
            : base()
        {
        }

        public ImageRead(IImage image, ImageType imageType, string fileName)
            : this()
        {
            this.Image = image;
            this.Width = image.Width;
            this.Height = image.Height;
            this.Depth = image.Depth;
            this.PixelType = SimpleITKHelper.Type2PixelType(image.PixelType);
            this.FileName = fileName;
            this.ImageTypeId = imageType;
        }

        public ImageRead(IImage image, ImageType imageType)
            : this(image, imageType, String.Empty)
        {
        }

        public ImageRead(IImage image)
            : this(image, ImageType.Unknown, String.Empty)
        {
        }

        #region Properties

        public string FileName
        {
            get { return this.fileName; }
            set
            {
                SetField(ref this.fileName, value, () => FileName);
                SetField(ref this.name, value, () => Name);
            }
        }

        public uint Width
        {
            get { return this.width; }
            set { SetField(ref this.width, value, () => Width); }
        }

        public uint Height
        {
            get { return this.height; }
            set { SetField(ref this.height, value, () => Height); }
        }

        public uint Depth
        {
            get { return this.depth; }
            set { SetField(ref this.depth, value, () => Depth); }
        }

        public Endianness Endianness
        {
            get { return this.endianness; }
            set { SetField(ref this.endianness, value, () => Endianness); }
        }

        public PixelType PixelType
        {
            get { return this.pixelType; }
            set { SetField(ref this.pixelType, value, () => PixelType); }
        }

        public int SliceFrom
        {
            get { return this.sliceFrom; }
            set { SetField(ref this.sliceFrom, value, () => SliceFrom); }
        }

        public int SliceTo
        {
            get { return this.sliceTo; }
            set { SetField(ref this.sliceTo, value, () => SliceTo); }
        }

        public int Header
        {
            get { return this.header; }
            set { SetField(ref this.header, value, () => Header); }
        }

        public ImageType ImageTypeId
        {
            get { return this.imageType; }
            set { SetField(ref this.imageType, value, () => ImageTypeId); }
        }

        public long Id
        {
            get { return this.id; }
            set { SetField(ref this.id, value, () => Id); }
        }

        public long? ParentId
        {
            get { return this.parentId; }
            set { SetField(ref this.parentId, value, () => ParentId); }
        }

        public string Name
        {
            get { return this.name; }
            set { SetField(ref this.name, value, () => Name); }
        }

        public bool ViewImage
        {
            get { return this.viewImage; }
            set { SetField(ref this.viewImage, value, () => ViewImage); }
        }

        public IImage Image
        {
            get { return this.image; }
            set { this.image = value; }
        }

        public Bitmap Bitmap
        {
            get { return this.Image.GetBitmap(); }
        }

        public Bitmap this[int index]
        {
            get { return this.image.GetBitmap((uint)index); }
        }

        #endregion Properties

        public override Type GetMiningObjectType()
        {
            return typeof(MiningObjectImage);
        }

        public double GetPixelValue(uint[] position)
        {
            return this.Image.GetPixel<double>(position);
        }

        public void Load()
        {
            Raccoon.MRI.IImage image;
            switch (this.ImageTypeId)
            {
                case ImageType.ITKStandard:
                    image = new Raccoon.MRI.ImageITK();
                    break;

                case ImageType.ITKRawImage:
                    image = new Raccoon.MRI.ImageITKRaw(this.FileName,
                                                           this.Width,
                                                           this.Height,
                                                           this.Depth,
                                                           this.PixelType,
                                                           this.Endianness,
                                                           (uint)this.Header);
                    break;

                default:
                    throw new NotImplementedException();
            }

            image.Load();
            this.Image = image;

            this.Width = image.Width;
            this.Height = image.Height;
            this.Depth = image.Depth;
            this.PixelType = SimpleITKHelper.Type2PixelType(image.PixelType);
        }
    }
}