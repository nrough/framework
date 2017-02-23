using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Linq;
using NRough.MRI;

namespace NRough.MRI.DAL
{
    public class MiningObjectImage : MiningObject, IMiningObjectViewImage
    {
        public string FileName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
        public int Header { get; set; }
        public int SliceFrom { get; set; }
        public int SliceTo { get; set; }
        public PixelType PixelType { get; set; }
        public ImageType ImageType { get; set; }
        public Endianness Endianness { get; set; }

        private IImage image;

        public MiningObjectImage()
            : base()
        {
            this.FileName = "";
            this.PixelType = MRI.PixelType.UInt16;
            this.Endianness = MRI.Endianness.LittleEndian;
            this.ImageType = DAL.ImageType.ITKStandard;
        }

        public IImage Image
        {
            get
            {
                if (this.image == null &&
                    !String.IsNullOrEmpty(this.FileName))
                {
                    LoadImageFromFileName();
                }

                return this.image;
            }

            protected set
            {
                this.image = value;
            }
        }

        public override XElement XMLParametersElement
        {
            get
            {
                XElement parameters = new XElement("Parameters",
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.RefId),
                                            new XAttribute("Name", "RefId")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.FileName),
                                            new XAttribute("Name", "FileName")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.Width),
                                            new XAttribute("Name", "Width")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.Height),
                                            new XAttribute("Name", "Height")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.Depth),
                                            new XAttribute("Name", "Depth")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.Endianness),
                                            new XAttribute("Name", "Endianness")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.PixelType),
                                            new XAttribute("Name", "PixelType")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.Header),
                                            new XAttribute("Name", "Header")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.ImageType),
                                            new XAttribute("Name", "ImageType")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.SliceFrom),
                                            new XAttribute("Name", "SliceFrom")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.SliceTo),
                                            new XAttribute("Name", "SliceTo"))
                                    );
                return parameters;
            }
        }

        public override void XMLParseParameters(XElement parametersElement)
        {
            base.XMLParseParameters(parametersElement);

            long refId = Convert.ToInt64(this.XMLGetParameterValue(parametersElement, "RefId"));

            this.FileName = this.XMLGetParameterValue(parametersElement, "FileName");
            this.Width = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "Width"));
            this.Height = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "Height"));
            this.Depth = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "Depth"));
            this.Header = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "Header"));

            this.SliceFrom = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "SliceFrom"));
            this.SliceTo = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "SliceTo"));

            this.PixelType = (PixelType)Enum.Parse(typeof(PixelType), this.XMLGetParameterValue(parametersElement, "PixelType"));
            this.Endianness = (Endianness)Enum.Parse(typeof(Endianness), this.XMLGetParameterValue(parametersElement, "Endianness"));
            this.ImageType = (ImageType)Enum.Parse(typeof(ImageType), this.XMLGetParameterValue(parametersElement, "ImageType"));

            this.RefId = refId;
        }

        public override void ReloadReferences(MiningProject project)
        {
            LoadImageFromFileName();
        }

        private void LoadImageFromFileName()
        {
            //if file name is set load from file
            if (!String.IsNullOrEmpty(this.FileName)
                && File.Exists(this.FileName))
            {
                ImageITK imageItk = ImageITK.Construct(
                    (uint)this.Width,
                    (uint)this.Height,
                    (uint)this.Depth,
                    SimpleITKHelper.PixelType2Type(this.PixelType));

                imageItk.FileName = this.FileName;
                imageItk.Width = (uint)this.Width;
                imageItk.Height = (uint)this.Height;
                imageItk.Depth = (uint)this.Depth;
                imageItk.PixelTypeId = this.PixelType;
                imageItk.EndiannessId = this.Endianness;
                imageItk.Header = this.Header;

                imageItk.Load();

                this.Image = imageItk;
            }
        }

        public override void InitFromViewModel(MiningObjectViewModel viewModel)
        {
            base.InitFromViewModel(viewModel);

            ImageRead imageModel = viewModel as ImageRead;

            if (imageModel != null)
            {
                imageModel.Load();

                this.Image = imageModel.Image;

                this.ImageType = imageModel.ImageTypeId;
                this.Width = (int)imageModel.Width;
                this.Height = (int)imageModel.Height;
                this.Depth = (int)imageModel.Depth;
                this.Header = imageModel.Header;
                this.SliceFrom = imageModel.SliceFrom;
                this.SliceTo = imageModel.SliceTo;
                this.FileName = imageModel.FileName;
                this.Endianness = imageModel.Endianness;
                this.PixelType = imageModel.PixelType;
                this.Name = imageModel.Name;

                switch (imageModel.ImageTypeId)
                {
                    case ImageType.ITKRawImage:
                        this.TypeId = MiningObjectType.Types.ImageRAW;
                        break;

                    default:
                        this.TypeId = MiningObjectType.Types.ImageITK;
                        break;
                }
            }
        }
    }
}