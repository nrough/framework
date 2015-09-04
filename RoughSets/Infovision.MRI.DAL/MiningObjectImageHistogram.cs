using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Infovision.MRI.DAL
{
    public class MiningObjectImageHistogram : MiningObject, IMiningObjectViewImage
    {
        private ImageHistogram imageHistogram;

        public MiningObjectImageHistogram()
            : base()
        {
        }

        public int BucketSize { get; set; }

        public IImage Image
        {
            get
            {
                if (this.imageHistogram != null && this.imageHistogram.Image != null)
                {
                    return new ImageBitmap(this.imageHistogram.GetChartBitmap());
                }

                throw new InvalidOperationException("Image histogram is not properly initialized.");
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
                                            new XAttribute("Value", this.BucketSize),
                                            new XAttribute("Name", "BucketSize"))
                                    );
                return parameters;
            }
        }

        public override void XMLParseParameters(XElement parametersElement)
        {
            base.XMLParseParameters(parametersElement);

            this.RefId = Convert.ToInt64(this.XMLGetParameterValue(parametersElement, "RefId"));
            this.BucketSize = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "BucketSize"));
        }

        public override void ReloadReferences(MiningProject project)
        {
            base.ReloadReferences(project);

            if (this.imageHistogram == null)
            {
                this.imageHistogram = new ImageHistogram();
                this.imageHistogram.HistogramBucketSize = this.BucketSize;
            }            

            if (this.imageHistogram.Image == null && this.RefId != 0)
            {
                IMiningObjectViewImage imageObject = project.GetMiningObject(this.RefId) as IMiningObjectViewImage;
                if (imageObject != null)
                {
                    this.imageHistogram.Image = imageObject.Image;
                }
            }
        }

        public override void InitFromViewModel(MiningObjectViewModel viewModel)
        {
            base.InitFromViewModel(viewModel);

            Histogram histogramModel = viewModel as Histogram;

            this.imageHistogram = new ImageHistogram(histogramModel.Image);
            this.imageHistogram.HistogramBucketSize = histogramModel.BucketSize;

            this.TypeId = MiningObjectType.Types.ImageHistogram;
            this.BucketSize = histogramModel.BucketSize;
            this.Name = "Image histogram";
        }
    }
}
