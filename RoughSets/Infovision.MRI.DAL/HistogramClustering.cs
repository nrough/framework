using System;

namespace Infovision.MRI.DAL
{
    [Serializable]
    public class HistogramClustering : MiningObjectViewModel
    {
        private int maxNumberOfRepresentatives;
        private int histogramBucketSize;
        private double bucketCountWeight;
        private double minimumClusterDistance;
        private double approximationDegree;

        private bool loadFile = false;
        private string fileNameLoad;
        private bool saveFile = true;
        private string fileNameSave;

        private ImageHistogramCluster cluster;

        public HistogramClustering()
            : base()
        {
        }

        public IImage Image
        {
            get;
            set;
        }

        public int MaxNumberOfRepresentatives
        {
            get { return this.maxNumberOfRepresentatives; }
            set { SetField(ref this.maxNumberOfRepresentatives, value, () => MaxNumberOfRepresentatives); }
        }

        public int HistogramBucketSize
        {
            get { return this.histogramBucketSize; }
            set { SetField(ref this.histogramBucketSize, value, () => HistogramBucketSize); }
        }

        public double BucketCountWeight
        {
            get { return this.bucketCountWeight; }
            set { SetField(ref this.bucketCountWeight, value, () => BucketCountWeight); }
        }

        public double MinimumClusterDistance
        {
            get { return this.minimumClusterDistance; }
            set { SetField(ref this.minimumClusterDistance, value, () => MinimumClusterDistance); }
        }

        public double ApproximationDegree
        {
            get { return this.approximationDegree; }
            set { SetField(ref this.approximationDegree, value, () => ApproximationDegree); }
        }

        public bool LoadFile
        {
            get { return this.loadFile; }
            set { SetField(ref this.loadFile, value, () => LoadFile); }
        }

        public string FileNameLoad
        {
            get { return this.fileNameLoad; }
            set { SetField(ref this.fileNameLoad, value, () => FileNameLoad); }
        }

        public bool SaveFile
        {
            get { return this.saveFile; }
            set { SetField(ref this.saveFile, value, () => SaveFile); }
        }

        public string FileNameSave
        {
            get { return this.fileNameSave; }
            set { SetField(ref this.fileNameSave, value, () => FileNameSave); }
        }

        public ImageHistogramCluster Cluster
        {
            get { return this.cluster; }
            set { this.cluster = value; }
        }

        public override Type GetMiningObjectType()
        {
            return typeof(MiningObjectHistogramCluster);
        }

        public void Train(IImage image)
        {
            cluster = new ImageHistogramCluster()
            {
                MaxNumberOfRepresentatives = this.MaxNumberOfRepresentatives,
                HistogramBucketSize = this.histogramBucketSize,
                BucketCountWeight = this.BucketCountWeight,
                MinimumClusterDistance = this.MinimumClusterDistance,
                ApproximationDegree = this.ApproximationDegree,
            };

            cluster.Image = image;
            cluster.Train();
        }

        public void LoadFromFile()
        {
            this.Cluster = ImageHistogramCluster.Load(this.FileNameLoad);

            this.MaxNumberOfRepresentatives = this.Cluster.MaxNumberOfRepresentatives;
            this.HistogramBucketSize = this.Cluster.HistogramBucketSize;
            this.BucketCountWeight = this.Cluster.BucketCountWeight;
            this.MinimumClusterDistance = this.Cluster.MinimumClusterDistance;
            this.ApproximationDegree = this.Cluster.ApproximationDegree;
        }

        public void Save(string fileName)
        {
            this.Cluster.Save(fileName);
        }

        public void Save()
        {
            this.Cluster.Save(this.fileNameSave);
        }
    }
}