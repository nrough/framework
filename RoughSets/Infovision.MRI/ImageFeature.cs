using System;
using System.Collections.Generic;

namespace Infovision.MRI
{
    public interface IImageFeature
    {
        Object GetValue(uint[] position);
    }

    [Serializable]
    public abstract class ImageFeature : IImageFeature
    {
        private bool isCalculatedFlag = false;
        private IImage image;
        private List<int> slices;

        public ImageFeature()
        {
        }

        public virtual IImage Image
        {
            get
            {
                return this.image;
            }

            set
            {
                this.image = value;
            }
        }

        public virtual List<int> Slices
        {
            get
            {
                return this.slices;
            }

            set
            {
                this.slices = value;
            }
        }

        public bool IsCalculated
        {
            get { return this.isCalculatedFlag; }
            protected set { this.isCalculatedFlag = value; }
        }

        public abstract object GetValue(uint[] position);
    }

    public class ImageFeatureConstant<T> : ImageFeature
    {
        private T val;

        public ImageFeatureConstant(T value)
        {
            this.val = value;
        }

        public override object GetValue(uint[] position)
        {
            return this.val;
        }
    }

    [Serializable]
    public class ImageFeatrueVoxelPosition : ImageFeature
    {
        public ImageFeatrueVoxelPosition()
            : base()
        {
            this.IsCalculated = true;
            this.Dimension = 0;
        }

        public int Dimension
        {
            get;
            protected set;
        }

        public override object GetValue(uint[] position)
        {
            return position[this.Dimension];
        }
    }

    [Serializable]
    public class ImageFeatureVoxelPositionX : ImageFeatrueVoxelPosition
    {
        public ImageFeatureVoxelPositionX() 
            : base()
        {
            this.Dimension = 0;
        }
    }

    [Serializable]
    public class ImageFeatureVoxelPositionY : ImageFeatrueVoxelPosition
    {
        public ImageFeatureVoxelPositionY()
            : base()
        {
            this.Dimension = 1;
        }
    }

    [Serializable]
    public class ImageFeatureVoxelPositionZ : ImageFeatrueVoxelPosition
    {
        public ImageFeatureVoxelPositionZ()
            : base()
        {
            this.Dimension = 2;
        }
    }

    [Serializable]
    public class ImageFeatureVoxelMagnitude : ImageFeature
    {
        public ImageFeatureVoxelMagnitude()
            : base()
        {
            this.IsCalculated = true;
        }

        public override object GetValue(uint[] position)
        {
            var mi = typeof(IImage).GetMethod("GetPixel");
            var mRef = mi.MakeGenericMethod(this.Image.PixelType);
            return mRef.Invoke(this.Image, new object[] { position });
        }
    }

    [Serializable]
    public abstract class ImageFeatureFilter : ImageFeature
    {
        protected IImage segmentationResult;

        protected ImageFeatureFilter()
            : base()
        {
        }

        public override object GetValue(uint[] position)
        {
            if (!this.IsCalculated)
            {
                Calc();
            }

            return this.segmentationResult.GetPixel<Int32>(position);
        }

        protected virtual void Calc()
        {
            this.IsCalculated = true;
        }
    }

    [Serializable]
    public class ImageFeatureHistogramCluster : ImageFeatureFilter
    {
        private ImageHistogramCluster cluster;
        private int maxNumberOfRepresentatives;
        private int histogramBucketSize;
        private double bucketCountWeight;
        private double minimumClusterDistance;
        private double approximationDegree;

        public ImageFeatureHistogramCluster() : base() 
        {
        }

        public override IImage Image
        {
            get { return base.Image; }
            set 
            {
                if (base.Image != value)
                {
                    base.Image = value;
                    if (this.Cluster != null)
                        this.Cluster.Image = value;
                    this.IsCalculated = false;
                }
            }
        }

        public override List<int> Slices
        {
            get
            {
                return base.Slices;
            }
            set
            {
                base.Slices = value;
                if (this.Cluster != null)
                    this.Cluster.Slices = value;
                this.IsCalculated = false;
            }
        }

        public ImageHistogramCluster Cluster 
        {
            get
            {
                return this.cluster;
            }

            set
            {
                this.cluster = value;
                if (this.cluster != null)
                {
                    this.MaxNumberOfRepresentatives = this.cluster.MaxNumberOfRepresentatives;
                    this.HistogramBucketSize = this.cluster.HistogramBucketSize;
                    this.BucketCountWeight = this.cluster.BucketCountWeight;
                    this.MinimumClusterDistance = this.cluster.MinimumClusterDistance;
                    this.ApproximationDegree = this.cluster.ApproximationDegree;
                    this.Image = this.cluster.Image;
                    this.Slices = this.cluster.Slices;
                }
                
                this.IsCalculated = false;
            }
        }
        
        public int MaxNumberOfRepresentatives 
        {
            get { return this.maxNumberOfRepresentatives; }
            set
            {
                this.maxNumberOfRepresentatives = value;
                if(this.Cluster != null)
                    this.Cluster.MaxNumberOfRepresentatives = value;
                this.IsCalculated = false;
            }
        }
        
        public int HistogramBucketSize 
        {
            get { return this.histogramBucketSize; }
            set
            {
                this.histogramBucketSize = value;
                if (this.Cluster != null)
                    this.Cluster.HistogramBucketSize = value;
                this.IsCalculated = false;
            } 
        }
        
        public double BucketCountWeight 
        {
            get { return this.bucketCountWeight; }
            set
            {
                this.bucketCountWeight = value;
                if (this.Cluster != null)
                    this.Cluster.BucketCountWeight = value;
                this.IsCalculated = false;
            } 
        }

        public double MinimumClusterDistance 
        {
            get { return this.minimumClusterDistance; }
            set
            {
                this.minimumClusterDistance = value;
                if (this.Cluster != null)
                    this.Cluster.MinimumClusterDistance = value;
                this.IsCalculated = false;
            } 
        }
        
        public double ApproximationDegree 
        {
            get { return this.approximationDegree; }
            set
            {
                this.approximationDegree = value;
                if (this.Cluster != null)
                    this.Cluster.ApproximationDegree = value;
                this.IsCalculated = false;
            } 
        }

        protected override void Calc()
        {
            if (this.Cluster == null)
            {
                this.Cluster = new ImageHistogramCluster
                {
                    MaxNumberOfRepresentatives = this.MaxNumberOfRepresentatives,
                    HistogramBucketSize = this.HistogramBucketSize,
                    BucketCountWeight = this.BucketCountWeight,
                    MinimumClusterDistance = this.MinimumClusterDistance,
                    ApproximationDegree = this.ApproximationDegree,
                    Image = this.Image,
                    Slices = this.Slices
                };
            }

            this.Cluster.MaxNumberOfRepresentatives = this.MaxNumberOfRepresentatives;
            this.Cluster.HistogramBucketSize = this.HistogramBucketSize;
            this.Cluster.BucketCountWeight = this.BucketCountWeight;
            this.Cluster.MinimumClusterDistance = this.MinimumClusterDistance;
            this.Cluster.ApproximationDegree = this.ApproximationDegree;
            this.Cluster.Image = this.Image;
            this.Cluster.Slices = this.Slices;
            
            segmentationResult = this.Cluster.Execute(this.Image);

            base.Calc();
        }
    }

    [Serializable]
    public class ImageFeatureSOMCluster : ImageFeatureFilter
    {
        public ImageFeatureSOMCluster()
            : base ()
        {
        }

        public ImageSOMCluster Cluster
        {
            get; set;
        }

        protected override void Calc()
        {
            segmentationResult = this.Cluster.Execute(this.Image);
            base.Calc();
        }        
    }

    [Serializable]
    public class ImageFeatureMask : ImageFeatureFilter
    {
        private List<MRIMaskItem> items = new List<MRIMaskItem>();

        public ImageFeatureMask()
            : base()
        {
            items.Add(new MRIMaskItem { LabelValue = 150, Radius = 15 });
            items.Add(new MRIMaskItem { LabelValue = 100, Radius = 20 });
            items.Add(new MRIMaskItem { LabelValue = 51, Radius = 30 });
        }

        protected override void Calc()
        {
            IImage maskImage = new MRIMaskBinaryImageFilter().Execute(this.Image);
            MRIMaskConcentricImageFilter concentricMaskImageFilter = new MRIMaskConcentricImageFilter();
            concentricMaskImageFilter.AddMaskItems(this.items);
            segmentationResult = concentricMaskImageFilter.Execute(maskImage);

            base.Calc();
        }
    }

    [Serializable]
    public class ImageFeatureEdge : ImageFeatureFilter
    {
        public ImageFeatureEdge()
            : base()
        {
        }

        public EdgeThresholdFilter EdgeFilter
        {
            get; set;
        }

        protected override void Calc()
        {
            segmentationResult = this.EdgeFilter.Execute(this.Image);

            base.Calc();
        }
    }

    [Serializable]
    public abstract class ImageFeatureEdgeNeighbour : ImageFeatureFilter
    {
        public ImageFeatureEdgeNeighbour()
            : base()
        {
        }

        public EdgeThresholdFilter EdgeFilter
        {
            get; set;
        }
    }

    [Serializable]
    public class ImageFeatureEdgeNeighbourSOM : ImageFeatureEdgeNeighbour
    {
        public ImageFeatureEdgeNeighbourSOM()
            : base()
        {            
        }

        public ImageSOMCluster Cluster
        {
            get; set;
        }

        protected override void Calc()
        {
            EdgeNeighbourSOMFilter edgeNbr = new EdgeNeighbourSOMFilter();
            edgeNbr.EdgeImage = this.EdgeFilter.Execute(this.Image);
            edgeNbr.ReferenceImage = this.Cluster.Execute(this.Image);
            this.segmentationResult = edgeNbr.Execute();

            base.Calc();
        }
    }

    [Serializable]
    public class ImageFeatureEdgeNeighbourHistogram : ImageFeatureEdgeNeighbour
    {
        public ImageFeatureEdgeNeighbourHistogram()
            : base()
        {
        }

        public ImageHistogramCluster Cluster { get; set; }

        protected override void Calc()
        {
            EdgeNeighbourHistogramFilter edgeNbr = new EdgeNeighbourHistogramFilter();
            edgeNbr.EdgeImage = this.EdgeFilter.Execute(this.Image);
            edgeNbr.ReferenceImage = this.Cluster.Execute(this.Image);
            this.segmentationResult = edgeNbr.Execute();

            base.Calc();
        }
    }


}
