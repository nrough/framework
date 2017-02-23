using System;
using System.Globalization;
using System.Xml.Linq;

namespace NRough.MRI.DAL
{
    public class MiningObjectImageEdge : MiningObject, IMiningObjectViewImage
    {
        public MiningObjectImageEdge()
            : base()
        {
        }

        public double Noise { get; set; }
        public int Background { get; set; }
        public int Foreground { get; set; }

        public IImage Image
        {
            get;
            private set;
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
                                            new XAttribute("Value", this.Noise),
                                            new XAttribute("Name", "Noise")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.Background),
                                            new XAttribute("Name", "Background")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.Foreground),
                                            new XAttribute("Name", "Foreground"))
                                    );
                return parameters;
            }
        }

        public override void XMLParseParameters(XElement parametersElement)
        {
            base.XMLParseParameters(parametersElement);
            this.RefId = Convert.ToInt64(this.XMLGetParameterValue(parametersElement, "RefId"));

            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";

            this.Noise = Convert.ToDouble(this.XMLGetParameterValue(parametersElement, "Noise"), provider);
            this.Background = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "Background"));
            this.Foreground = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "Foreground"));
        }

        public override void ReloadReferences(MiningProject project)
        {
            base.ReloadReferences(project);

            if (this.RefId != 0)
            {
                IMiningObjectViewImage imageObject = project.GetMiningObject(this.RefId) as IMiningObjectViewImage;
                if (imageObject != null)
                {
                    InitImage(imageObject.Image);
                }
            }
        }

        private void InitImage(IImage sourceImage)
        {
            NRough.MRI.ImageITK itkImage = (NRough.MRI.ImageITK)sourceImage;

            EdgeThresholdFilter edgeFilter = new EdgeThresholdFilter(this.Noise,
                                                                (double)this.Foreground,
                                                                (double)this.Background);

            //itk.simple.Image binaryMaskImage = new MRIMaskBinaryImageFilter().Execute(itkImage);
            NRough.MRI.ImageITK edgeImage = new NRough.MRI.ImageITK(edgeFilter.Execute(itkImage));

            this.Image = edgeImage;
        }

        public override void InitFromViewModel(MiningObjectViewModel viewModel)
        {
            base.InitFromViewModel(viewModel);

            ImageEdge edgeModel = viewModel as ImageEdge;
            if (edgeModel != null)
            {
                this.Noise = edgeModel.Noise;
                this.Background = edgeModel.Background;
                this.Foreground = edgeModel.Foreground;

                this.TypeId = MiningObjectType.Types.ImageEdge;
                this.Name = "Image edge";

                InitImage(edgeModel.Image);
            }
        }
    }
}