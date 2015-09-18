using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Infovision.MRI.DAL
{
    public class MiningObjectImageMask : MiningObject, IMiningObjectViewImage
    {
        private ImageMask imageMask;
        
        public MiningObjectImageMask()
            : base()
        {
        }

        public IImage Image
        {
            get
            {
                return imageMask.Image;
            }
        }
        
        public override XElement XMLParametersElement
        {
            get
            {
                if (this.imageMask == null)
                {
                    throw new InvalidOperationException();
                }

                XElement parameters = new XElement("Parameters",
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.RefId),
                                            new XAttribute("Name", "RefId")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", ""),
                                            new XAttribute("Name", "MaskItems"),
                                            from maskItem in imageMask.MaskItems
                                            select new XElement("Item",
                                                        new XAttribute("LabelValue", maskItem.LabelValue),
                                                        new XAttribute("Radius", maskItem.Radius)))

                                    );
                return parameters;
            }
        }

        public override void XMLParseParameters(XElement parametersElement)
        {
            base.XMLParseParameters(parametersElement);
            this.RefId = Convert.ToInt64(this.XMLGetParameterValue(parametersElement, "RefId"));

            var maskItemsParm = from maskItemParameter in parametersElement.Descendants("Parameter")
                                where maskItemParameter.Attribute("Name").Value == "MaskItems"
                                select maskItemParameter;
            
            var maskItems = from maskItem in maskItemsParm.Single<XElement>().Descendants("Item")
                            select maskItem;
                            

            ImageMask localImageMask = new ImageMask();
            foreach (XElement item in maskItems)
            {
                int labelValue = Convert.ToInt32(item.Attribute("LabelValue").Value);
                int radius = Convert.ToInt32(item.Attribute("Radius").Value);

                localImageMask.AddMaskItem(labelValue, radius);
            }

            this.imageMask = localImageMask;
        }

        public override void ReloadReferences(MiningProject project)
        {
 	        base.ReloadReferences(project);

            if (this.imageMask == null)
            {
                this.imageMask = new ImageMask();
            }

            if (this.imageMask.Image == null && this.RefId != 0)
            {
                IMiningObjectViewImage imageObject = project.GetMiningObject(this.RefId) as IMiningObjectViewImage;
                if (imageObject != null)
                {
                    Infovision.MRI.ImageITK itkImage = (Infovision.MRI.ImageITK)imageObject.Image;
                    IImage binaryMaskImage = new MRIMaskBinaryImageFilter().Execute(itkImage);
                    MRIMaskConcentricImageFilter imageMaskFilter = new MRIMaskConcentricImageFilter();

                    foreach (var item in imageMask.MaskItems)
                    {
                        imageMaskFilter.AddMaskItem(item);
                    }

                    Infovision.MRI.ImageITK maskImage = new Infovision.MRI.ImageITK(imageMaskFilter.Execute(binaryMaskImage));
                    imageMask.Image = maskImage;
                }
            }
        }

        public override void InitFromViewModel(MiningObjectViewModel viewModel)
        {
            base.InitFromViewModel(viewModel);

            ImageMask maskModel = viewModel as ImageMask;

            if (maskModel != null)
            {
                this.TypeId = MiningObjectType.Types.ImageMask;
                this.Name = "Image mask";
                this.imageMask = maskModel;
            }            
        }
    }
}
