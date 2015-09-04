using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Infovision.MRI.DAL
{
    public class MiningObjectNeighbour : MiningObject
    {

        private IMiningObject maskObject;
        private IMiningObject labelsObject;
        private long maskId;
        private long labelsId;
        
        public MiningObjectNeighbour()
            : base()
        {
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
                                            new XAttribute("Value", maskObject.Id),
                                            new XAttribute("Name", "MaskRefId")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", labelsObject.Id),
                                            new XAttribute("Name", "LabelsRefId"))
                                    );
                return parameters;
            }
        }

        public override void XMLParseParameters(XElement parametersElement)
        {
            base.XMLParseParameters(parametersElement);
            this.RefId = Convert.ToInt64(this.XMLGetParameterValue(parametersElement, "RefId"));
            this.maskId = Convert.ToInt64(this.XMLGetParameterValue(parametersElement, "MaskRefId"));
            this.labelsId = Convert.ToInt64(this.XMLGetParameterValue(parametersElement, "LabelsRefId"));
        }

        public override void ReloadReferences(MiningProject project)
        {
            base.ReloadReferences(project);

            if (this.maskId != 0)
            {
                this.maskObject = project.GetMiningObject(this.maskId);
            }

            if (this.labelsId != 0)
            {
                this.labelsObject = project.GetMiningObject(this.labelsId);
            }
        }

        public override void InitFromViewModel(MiningObjectViewModel viewModel)
        {
            base.InitFromViewModel(viewModel);

            ImageNeighbour neighbourModel = viewModel as ImageNeighbour;
            if (neighbourModel != null)
            {
                this.TypeId = MiningObjectType.Types.ImageNeighbour;
                this.Name = "Image neighbour";
                this.maskObject = neighbourModel.Mask;
                this.maskId = neighbourModel.Mask.Id;
                this.labelsObject = neighbourModel.Labels;
                this.labelsId = neighbourModel.Labels.Id;
            }
        }
    }
}
