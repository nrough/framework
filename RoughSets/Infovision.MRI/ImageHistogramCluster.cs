using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;
using IntervalTreeLib;
using MathNet.Numerics.Statistics;

namespace Infovision.MRI
{
    [Serializable]
    public class ImageHistogramCluster : ISerializable
    {
        #region Globals
        
        private Histogram histogram;
        
        private int maxNumberOfRepresentatives;
        private int histogramBucketSize;
        private double bucketCountWeight;
        private double minimumClusterDistance;
        private double approximationDegree;

        private List<int> candidates;
        private List<int> representatives;

        private IntervalTree<int, double> clusterRanges;

        private List<int> slices;
        private IImage image;
            
        #endregion

        #region Constructors

        public ImageHistogramCluster()
        {
            this.HistogramBucketSize = 4;
            this.MaxNumberOfRepresentatives = 10;
            this.ApproximationDegree = 0;
            this.BucketCountWeight = 1;
            this.MinimumClusterDistance = 0;
            this.IsTrained = false;
        }

        protected ImageHistogramCluster(SerializationInfo si, StreamingContext context)
        {
            this.HistogramBucketSize = si.GetInt32("HistogramBucketSize");
            this.MaxNumberOfRepresentatives = si.GetInt32("MaxNumberOfRepresentatives");
            this.ApproximationDegree = si.GetDouble("ApproximationDegree");
            this.BucketCountWeight = si.GetDouble("BucketCountWeight");
            this.MinimumClusterDistance = si.GetDouble("MinimumClusterDistance");
            
            Histogram localHistogram = new Histogram();
            this.histogram = (Histogram) si.GetValue("histogram", localHistogram.GetType());

            List<int> localList = new List<int>();
            this.candidates = (List<int>) si.GetValue("candidates", localList.GetType());

            this.representatives = (List<int>) si.GetValue("representatives", localList.GetType());

            IntervalTree<int, double> localClusterRanges = new IntervalTree<int, double>();
            this.clusterRanges = (IntervalTree<int, double>) si.GetValue("clusterRanges", localClusterRanges.GetType());

            this.slices = (List<int>) si.GetValue("slices", localList.GetType());

            this.IsTrained = si.GetBoolean("IsTrained");           
        }
 
        #endregion

        #region Properties

        public int MaxNumberOfRepresentatives
        {
            get { return this.maxNumberOfRepresentatives; }
            set
            {
                if (this.maxNumberOfRepresentatives != value)
                    this.IsTrained = false;
                this.maxNumberOfRepresentatives = value;
            }
        }

        public int HistogramBucketSize
        {
            get { return this.histogramBucketSize; }
            set
            {
                if (this.histogramBucketSize != value)
                    this.IsTrained = false;
                this.histogramBucketSize = value;
            }
        }

        public double BucketCountWeight
        {
            get { return this.bucketCountWeight; }
            set
            {
                if (this.bucketCountWeight != value)
                    this.IsTrained = false;
                this.bucketCountWeight = value;
            }
        }

        public double MinimumClusterDistance
        {
            get { return this.minimumClusterDistance; }
            set
            {
                if (this.minimumClusterDistance != value)
                    this.IsTrained = false;
                this.minimumClusterDistance = value;
            }
        }

        public double ApproximationDegree
        {
            get { return this.approximationDegree; }
            set
            {
                if (this.approximationDegree != value)
                    this.IsTrained = false;
                this.approximationDegree = value;                
            }
        }
        
        public IImage Image
        {
            get { return this.image; }
            set 
            { 
                if(this.image != value)
                    this.IsTrained = false;
                this.image = value;
            }
        }

        public List<int> Slices
        {
            get { return this.slices; }
            set
            { 
                if(this.slices != value)
                    this.IsTrained = false;
                this.slices = value;
            }
        }
        
        public int NumberOfClusters 
        {
            get { return this.representatives.Count; }
        }

        public bool IsTrained { get; set; }

        #endregion

        #region Methods

        #region Serialization methods

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo si, StreamingContext context)
        {
            si.AddValue("HistogramBucketSize", this.HistogramBucketSize);
            si.AddValue("MaxNumberOfRepresentatives", this.MaxNumberOfRepresentatives);
            si.AddValue("ApproximationDegree", this.ApproximationDegree);
            si.AddValue("BucketCountWeight", this.BucketCountWeight);
            si.AddValue("MinimumClusterDistance", this.MinimumClusterDistance);
            si.AddValue("histogram", this.histogram);
            si.AddValue("candidates", this.candidates);
            si.AddValue("representatives", this.representatives);
            si.AddValue("clusterRanges", this.clusterRanges);
            si.AddValue("slices", this.slices);
            si.AddValue("IsTrained", this.IsTrained);            
        }

        #endregion

        public void Train()
        {
            if (this.Image == null)
            {
                throw new InvalidOperationException("Image is not set");
            }

            int imageBufferLength, bufferLength;

            if (this.Slices != null && this.Slices.Count > 0)
            {
                imageBufferLength = (int)(image.Width * image.Height);
                bufferLength = imageBufferLength * this.Slices.Count;
                
            }
            else
            {
                imageBufferLength = image.Depth > 0
                                  ? (int)(image.Width * image.Height * image.Depth)
                                  : (int)(image.Width * image.Height);
                bufferLength = imageBufferLength;
            }

            double[] pixelBuffer = new double[bufferLength];

            if (this.Slices != null && this.Slices.Count > 0)
            {
                int i = 0;
                foreach (int sliceIdx in this.Slices)
                {
                    image.Extract(sliceIdx).GetData<double>().CopyTo(pixelBuffer, i * imageBufferLength);
                    i++;
                }
            }
            else
            {
                image.GetData<double>().CopyTo(pixelBuffer, 0);
            }

            int numberOfBuckets = (int)Math.Ceiling((SimpleITKHelper.MaxPixelValue(image.PixelType) + 1.0) / this.HistogramBucketSize);
            histogram = new Histogram(pixelBuffer, numberOfBuckets);

            this.CreateCandidateSet();

            this.FindRepresentatives();
            this.CreateClusters();
            
            this.IsTrained = true;
        }

        private void CreateCandidateSet()
        {
            candidates = new List<int>();
            for (int i = 0; i < histogram.BucketCount; i++)
            {
                candidates.Add(i);
            }
        }

        private void FindRepresentatives()
        {
            representatives = new List<int>();
            this.AddRepresentative(this.GetNextRepresentative());

            int repsToFindLeft = this.MaxNumberOfRepresentatives - 1;
            
            while (candidates.Count > 0 
                    && (this.MaxNumberOfRepresentatives == 0 || repsToFindLeft > 0))
            {
                int bucketIndex = this.GetNextRepresentative();
                if (this.GetMinDistanceFromReps(bucketIndex) < this.MinimumClusterDistance)
                {
                    break;
                }
                else
                {
                    this.AddRepresentative(bucketIndex);
                    repsToFindLeft--;
                }
            }

            representatives.Sort();
        }

        public string RepresentativesToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append('{');
            ret.Append(' ');
            foreach (int rep in representatives)
            {                
                ret.Append(rep);
                ret.Append(' ');
            }
            ret.Append('}');

            return ret.ToString();
        }
        
        public string ClusterIntervalsToString()
        {
            StringBuilder ret = new StringBuilder();

            foreach(Interval<int, double> interval in clusterRanges.Intervals)
            {
                ret.AppendFormat("({0}; {1}) : {2}", interval.Start, interval.End, interval.Data);
                ret.Append(Environment.NewLine);
            }

            return ret.ToString();
        }

        private void CreateClusters()
        {
            int[] clusterLabels = new int[histogram.BucketCount];
            List<Interval<int, double>> intervalList = new List<Interval<int, double>>();

            int fromIndex = 0, toIndex = 0;
            for (int i = 0; i < histogram.BucketCount; i++)
            {
                int minDistanceIndex = -1;
                double minDistance = Double.MaxValue;

                int secondMinDistanceIndex = -1;
                double secondMinDistance = Double.MaxValue;

                int j = 0;
                foreach (int rep in representatives)
                {
                    j++;
                    double distance = this.BucketDistance(i, rep);
                    if (distance < minDistance)
                    {
                        secondMinDistance = minDistance;
                        secondMinDistanceIndex = minDistanceIndex;
                        
                        minDistance = distance;
                        minDistanceIndex = j;
                    }
                    else if (distance < secondMinDistance)
                    {
                        secondMinDistance = distance;
                        secondMinDistanceIndex = j;
                    }
                }

                //Console.WriteLine("{0} {1}:{2} {3}:{4}", i, minDistanceIndex, minDistance, secondMinDistanceIndex, secondMinDistance);
                
                //x in BND(xi, xi+1) <=> min(d(x, xi), d(x, x(i+1))) > eps
                
                double val1, val2, min;
                if (minDistanceIndex > -1 && secondMinDistance > -1)
                {
                    val1 = minDistance / secondMinDistance;
                    val2 = secondMinDistance / minDistance;
                    min = (val1 < val2) ? val1 : val2;
                }
                else if (minDistanceIndex > -1)
                {
                    min = 1;
                    secondMinDistanceIndex = minDistanceIndex;
                }
                else
                {
                    min = 1;
                    minDistanceIndex = 0;
                    secondMinDistanceIndex = 0;
                }

                //Boundary region
                if (this.ApproximationDegree > 0 && min >= this.ApproximationDegree)
                {
                    if (minDistanceIndex < secondMinDistanceIndex)
                    {
                        clusterLabels[i] = (minDistanceIndex * 10) + secondMinDistanceIndex;
                    }
                    else
                    {
                        clusterLabels[i] = (secondMinDistanceIndex * 10) + minDistanceIndex;
                    }

                }
                //Lower approximations
                else 
                {
                    clusterLabels[i] = minDistanceIndex;
                }

                if ((i > 0 && clusterLabels[i - 1] != clusterLabels[i]) || (i == histogram.BucketCount - 1))
                {
                    Interval<int, double> clusterInterval = new Interval<int, double>(histogram[fromIndex].LowerBound,
                                                                                      histogram[toIndex].UpperBound, clusterLabels[i-1]);
                    
                    intervalList.Add(clusterInterval);                    
                    
                    //Console.WriteLine(" {0} {1} : {2}", clusterInterval.Start, clusterInterval.End, clusterInterval.Label);                    
                    fromIndex = i;
                    toIndex = fromIndex;
                }
                else
                {
                    toIndex = i;
                }
            }
            
            clusterRanges = new IntervalTree<int, double>(intervalList);
        }

        private void AddRepresentative(int bucketIndex)
        {
            representatives.Add(bucketIndex);
            candidates.Remove(bucketIndex);
        }

        private int GetNextRepresentative()
        {
            if (representatives.Count == 0)
                return this.FindMaxCandidateBucket();

            int maxIndex = -1;
            double maxValue = -1;
            
            foreach (int cand in candidates)
            {
                double weightedDistance = this.GetMinDistanceFromReps(cand) + (this.BucketCountWeight * histogram[cand].Count);
                if (weightedDistance > maxValue)
                {
                    maxValue = weightedDistance;
                    maxIndex = cand;
                }
            }

            return maxIndex;
        }

        private double GetMinDistanceFromReps(int bucketIndex)
        {
            double minDistance = double.MaxValue;
            foreach (int rep in representatives)
            {
                double distance = this.BucketDistance(bucketIndex, rep);
                if (minDistance > distance)
                {
                    minDistance = distance;
                }
            }
            
            return minDistance;
        }

        private int FindMaxCandidateBucket()
        {
            double maxValue = -1;
            int maxIndex = -1;
            
            foreach (int can in candidates)
            {
                if (histogram[can].Count > maxValue)
                {
                    maxValue = histogram[can].Count;
                    maxIndex = can;
                }
            }

            return maxIndex;
        }

        public double BucketDistance(int x, int y)
        {
            if (x == y)
                return 0;

            if (y < x)
            {
                int tmp = x;
                x = y;
                y = tmp;
            }

            double distance = 0;
            for (int i = x + 1; i <= y; i++)
            {
                distance += Math.Sqrt(1 + Math.Pow(histogram[i - 1].Count - histogram[i].Count, 2));
            }

            return distance;
        }

        public List<ImageHistogramInterval> GetIntervals()
        {
            List<Interval<int, double>> intervalList = clusterRanges.GetIntervals(histogram.LowerBound, histogram.UpperBound);
            List<ImageHistogramInterval> ret = new List<ImageHistogramInterval>(intervalList.Count);
            foreach(Interval<int, double> interval in intervalList)
            {
                ret.Add(new ImageHistogramInterval(interval.Start, interval.End, interval.Data));
            }

            return ret;
        }

        public IImage Execute(IImage img)
        {
            if (this.IsTrained == false)
            {
                this.Train();
            }

            IImage result = ImageITK.Construct(img.Width, img.Height, img.Depth, typeof(byte));
            uint[] position = new uint[3];

            for (uint z = 0; z < img.Depth; z++)
            {
                for (uint y = 0; y < img.Height; y++)
                {
                    for (uint x = 0; x < img.Width; x++)
                    {
                        position[0] = x;
                        position[1] = y;
                        position[2] = z;

                        byte pixelValue = (byte)this.GetClusterId(img.GetPixel<double>(position));
                        result.SetPixel<byte>(position, pixelValue);
                    }
                }
            }

            return result;
        }

        private int GetClusterId(double pixelValue)
        {
            List<int> clusters = clusterRanges.Get(pixelValue, StubMode.ContainsStart);
            
            if (clusters.Count == 0)
            {
                return 0;
            }

            return clusters.ElementAt<int>(0);
        }

        public static ImageHistogramCluster Load(string fileName)
        {
            ImageHistogramCluster histogramCluster;

            //--- deserialize the object
            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                histogramCluster = (ImageHistogramCluster)formatter.Deserialize(stream);
                stream.Close();
            }
            
            return histogramCluster;
        }

        /// <summary>
        /// Save the state of the class
        /// </summary>
        public bool Save(string fileName)
        {
            bool bSuccess = false;
            try
            {
                //--- serialize the instance using a BinaryFormatter
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, this);
                    stream.Close();
                    bSuccess = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }

            return bSuccess;
        }
   
        #endregion
    }
}
