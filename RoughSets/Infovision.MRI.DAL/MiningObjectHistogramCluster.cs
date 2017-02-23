using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NRough.MRI.DAL
{
    public class MiningObjectHistogramCluster : MiningObject
    {
        private List<ImageHistogramInterval> intervalList = new List<ImageHistogramInterval>();

        public double ApproximationDegree { get; set; }
        public double BucketCountWeight { get; set; }
        public int HistogramBucketSize { get; set; }
        public int MaxNumberOfRepresentatives { get; set; }
        public double MinimumClusterDistance { get; set; }
        public string FileNameLoad { get; set; }

        private ImageHistogramCluster histogramCluster;

        public MiningObjectHistogramCluster()
            : base()
        {
        }

        public List<ImageHistogramInterval> IntervalList
        {
            get
            {
                return this.intervalList;
            }

            private set
            {
                this.intervalList = value;
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
                                            new XAttribute("Value", this.ApproximationDegree),
                                            new XAttribute("Name", "ApproximationDegree")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.BucketCountWeight),
                                            new XAttribute("Name", "BucketCountWeight")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.HistogramBucketSize),
                                            new XAttribute("Name", "HistogramBucketSize")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.MaxNumberOfRepresentatives),
                                            new XAttribute("Name", "MaxNumberOfRepresentatives")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.MinimumClusterDistance),
                                            new XAttribute("Name", "MinimumClusterDistance")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", this.FileNameLoad),
                                            new XAttribute("Name", "FileNameLoad")),
                                        new XElement("Parameter",
                                            new XAttribute("Value", ""),
                                            new XAttribute("Name", "IntervalList"),
                                            from interval in this.IntervalList
                                            select new XElement("Interval",
                                                new XAttribute("LowerBound", interval.LowerBound),
                                                new XAttribute("UpperBound", interval.UpperBound),
                                                new XAttribute("Label", interval.Label)))
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

            this.ApproximationDegree = Convert.ToDouble(this.XMLGetParameterValue(parametersElement, "ApproximationDegree"), provider);
            this.BucketCountWeight = Convert.ToDouble(this.XMLGetParameterValue(parametersElement, "BucketCountWeight"), provider);
            this.HistogramBucketSize = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "HistogramBucketSize"));
            this.MaxNumberOfRepresentatives = Convert.ToInt32(this.XMLGetParameterValue(parametersElement, "MaxNumberOfRepresentatives"));
            this.MinimumClusterDistance = Convert.ToDouble(this.XMLGetParameterValue(parametersElement, "MinimumClusterDistance"), provider);
            this.FileNameLoad = this.XMLGetParameterValue(parametersElement, "FileNameLoad");

            var intervalsParm = from intervalParameter in parametersElement.Descendants("Parameter")
                                where intervalParameter.Attribute("Name").Value == "IntervalList"
                                select intervalParameter;

            var intervals = from interval in intervalsParm.Single<XElement>().Descendants("Interval")
                            select new ImageHistogramInterval(
                                Convert.ToDouble(interval.Attribute("LowerBound").Value, provider),
                                Convert.ToDouble(interval.Attribute("UpperBound").Value, provider),
                                Convert.ToInt32(interval.Attribute("Label").Value));

            foreach (ImageHistogramInterval interval in intervals)
            {
                intervalList.Add(interval);
            }
        }

        public override void ReloadReferences(MiningProject project)
        {
            base.ReloadReferences(project);

            if (histogramCluster == null)
            {
                if (!String.IsNullOrEmpty(this.FileNameLoad) && File.Exists(this.FileNameLoad))
                {
                    histogramCluster = ImageHistogramCluster.Load(this.FileNameLoad);
                }
                else
                {
                    histogramCluster = new ImageHistogramCluster
                    {
                        ApproximationDegree = this.ApproximationDegree,
                        BucketCountWeight = this.BucketCountWeight,
                        HistogramBucketSize = this.HistogramBucketSize,
                        MaxNumberOfRepresentatives = this.MaxNumberOfRepresentatives,
                        MinimumClusterDistance = this.MinimumClusterDistance
                    };

                    IMiningObjectViewImage refObject = project.GetMiningObject(this.RefId) as IMiningObjectViewImage;
                    ImageITK image = (ImageITK)refObject.Image;

                    histogramCluster.Image = image;
                    histogramCluster.Train();
                }
            }
        }

        public override void InitFromViewModel(MiningObjectViewModel viewModel)
        {
            base.InitFromViewModel(viewModel);

            HistogramClustering clusterModel = viewModel as HistogramClustering;

            this.TypeId = MiningObjectType.Types.ImageHistogramCluster;
            this.Name = "Image histogram cluster";

            this.ApproximationDegree = clusterModel.ApproximationDegree;
            this.BucketCountWeight = clusterModel.BucketCountWeight;
            this.HistogramBucketSize = clusterModel.HistogramBucketSize;
            this.MaxNumberOfRepresentatives = clusterModel.MaxNumberOfRepresentatives;
            this.MinimumClusterDistance = clusterModel.MinimumClusterDistance;

            if (clusterModel.LoadFile
                && !String.IsNullOrEmpty(clusterModel.FileNameLoad)
                && File.Exists(clusterModel.FileNameLoad))
            {
                clusterModel.LoadFromFile();
                this.histogramCluster = clusterModel.Cluster;
                this.FileNameLoad = clusterModel.FileNameLoad;
            }
            else if (clusterModel.Image != null)
            {
                ImageITK image = clusterModel.Image as ImageITK;
                clusterModel.Train(image);

                this.histogramCluster = clusterModel.Cluster;
            }

            this.intervalList = new List<ImageHistogramInterval>(clusterModel.Cluster.GetIntervals());

            if (clusterModel.SaveFile
                && !String.IsNullOrEmpty(clusterModel.FileNameSave)
                && clusterModel.Cluster != null)
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