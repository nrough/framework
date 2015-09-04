using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using System.IO;

namespace Infovision.MRI.DAL
{
    public class MiningObjectSOMCluster : MiningObject
    {
        private List<long> refIds = new List<long>();
        private ImageSOMCluster somCluster;

        public MiningObjectSOMCluster()
        {
        }
        
        public double LearningRate { get; set; }
        public int NumberOfClusters { get; set; }
        public int NumberOfIterations { get; set; }
        public int Radius { get; set; }
        public string FileNameLoad { get; set; }

        public override XElement XMLParametersElement
        {
            get
            {
                XElement parameters = new XElement("Parameters",
                                        new XElement("Parameter",
                                            new XAttribute("Value", ""),
                                            new XAttribute("Name", "RefId"),
                                            from item in this.refIds
                                                select new XElement("Item",
                                                    new XAttribute("Value", item),
                                                    new XAttribute("Name", "RefId"))),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.LearningRate),
                                            new XAttribute("Name", "LearningRate")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.NumberOfClusters),
                                            new XAttribute("Name", ReductGeneratorParamHelper.NumberOfClusters)),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.NumberOfIterations),
                                            new XAttribute("Name", "NumberOfIterations")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.Radius),
                                            new XAttribute("Name", "Radius")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.FileNameLoad),
                                            new XAttribute("Name", "NetworkFileName"))
                                    );
                
                return parameters;
            }
        }

        public override void XMLParseParameters(XElement parametersElement)
        {
            base.XMLParseParameters(parametersElement);
            
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";

            this.LearningRate = Convert.ToDouble(this.XMLGetParameterValue(parametersElement, "LearningRate"), provider);
            this.NumberOfClusters = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, ReductGeneratorParamHelper.NumberOfClusters));
            this.NumberOfIterations = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "NumberOfIterations"));
            this.Radius = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "Radius"));
            this.FileNameLoad = this.XMLGetParameterValue(parametersElement, "NetworkFileName");

            var redIdParm = from refIdParameter in parametersElement.Descendants("Parameter")
                            where refIdParameter.Attribute("Name").Value == "RefId"
                            select refIdParameter;

            var items = from refId in redIdParm.Single<XElement>().Descendants("Item")
                        select Convert.ToInt64(refId.Attribute("Value").Value);

            foreach (long id in items)
            {
                refIds.Add(id);
            }
        }

        public override void ReloadReferences(MiningProject project)
        {
            base.ReloadReferences(project);

            if (somCluster == null)
            {
                SOMClustering clusterModel = new SOMClustering()
                {
                    LearningRate = this.LearningRate,
                    NumberOfClusters = this.NumberOfClusters,
                    NumberOfIterations = this.NumberOfIterations,
                    Radius = this.Radius,
                    FileNameLoad = this.FileNameLoad
                };

                foreach (long id in this.refIds)
                {
                    clusterModel.AddSelectedObject(new MiningObjectDisplay(project.GetMiningObject(id)));
                }

                if (clusterModel.SelectedObjects.Count > 0)
                {
                    long[] imageIds = new long[clusterModel.SelectedObjects.Count];
                    int i = 0;
                    foreach (MiningObjectDisplay item in clusterModel.SelectedObjects)
                    {
                        imageIds[i] = item.Id;
                        i++;
                    }

                    if (!String.IsNullOrEmpty(clusterModel.FileNameLoad)
                        && File.Exists(clusterModel.FileNameLoad))
                    {
                        clusterModel.LoadFromFile();
                    }
                    else
                    {
                        clusterModel.Train(project.GetITKImageArray(imageIds));
                    }

                    this.somCluster = clusterModel.Cluster;
                }
            }            
        }

        public override void InitFromViewModel(MiningObjectViewModel viewModel)
        {
            base.InitFromViewModel(viewModel);

            SOMClustering clusterModel = viewModel as SOMClustering;

            this.TypeId = MiningObjectType.Types.ImageSOMCluster;
            this.Name = "Image SOM cluster";
            this.RefId = 0;

            this.LearningRate = clusterModel.LearningRate;
            this.NumberOfClusters = clusterModel.NumberOfClusters;
            this.NumberOfIterations = clusterModel.NumberOfIterations;
            this.Radius = clusterModel.Radius;

            refIds = new List<long>();
            foreach (MiningObjectDisplay item in clusterModel.SelectedObjects)
            {
                refIds.Add(item.Id);
            }

            if (clusterModel.LoadFile 
                && !String.IsNullOrEmpty(clusterModel.FileNameLoad) 
                && File.Exists(clusterModel.FileNameLoad))
            {
                clusterModel.LoadFromFile();
                this.somCluster = clusterModel.Cluster;

                this.FileNameLoad = clusterModel.FileNameLoad;
            }
            else if (clusterModel.SelectedObjects.Count > 0)
            {
                clusterModel.Train(clusterModel.GetITKImageArray());
                this.somCluster = clusterModel.Cluster;                
            }
            
            if (clusterModel.SaveFile 
                && !String.IsNullOrEmpty(clusterModel.FileNameSave))
            {
                clusterModel.Save(clusterModel.FileNameSave);

                if (clusterModel.FileNameSave != clusterModel.FileNameLoad)
                {
                    this.FileNameLoad = clusterModel.FileNameSave;
                }
            }
            
        }
    }
}
