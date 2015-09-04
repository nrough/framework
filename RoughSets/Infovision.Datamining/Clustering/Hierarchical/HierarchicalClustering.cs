using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using Accord.MachineLearning;
using AForge;
using Infovision.Math;
using Infovision.Utils;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalClustering
    {                
        private DistanceMatrix distanceMatrix;        
        private PriorityQueue<HierarchicalClusterTuple, HierarchicalClusterTupleValueAscendingComparer> queue;
        private Dictionary<int, HierarchicalCluster> clusters;
        
        private int nextClusterId = 0;
        
        private Func<double[], double[], double> distance;
        private Func<int[], int[], DistanceMatrix, double[][], double> linkage;
        private DendrogramLinkCollection dendrogram; 
        private double[][] data;

        int numberOfInstances;            

        /// <summary>
        ///   Gets or sets the distance function used
        ///   as a distance metric between data points.
        /// </summary>
        /// 
        public Func<double[], double[], double> Distance
        {
            get { return this.distance; }
            set { this.distance = value; }
        }

        public Func<int[], int[], DistanceMatrix, double[][], double> Linkage
        {
            get { return this.linkage; }
            set { this.linkage = value; }
        }

        protected int NextClusterId
        {
            get { return this.nextClusterId; }
        }

        public DendrogramLinkCollection DendrogramLinkCollection
        {
            get { return this.dendrogram; }
        }

        public int NumberOfInstances
        {
            get { return this.numberOfInstances; }
        }

        public DistanceMatrix DistanceMatrix
        {
            get { return this.distanceMatrix; }
        }        

        /// <summary>
        ///   Initializes a new instance of the HierarchicalClustering algorithm
        /// </summary>        
        public HierarchicalClustering()
            : this(Infovision.Math.Similarity.SquaredEuclidean, ClusteringLinkage.Single) { }

        /// <summary>
        ///   Initializes a new instance of the HierarchicalClustering algorithm
        /// </summary>
        ///         
        /// <param name="distance">The distance function to use. Default is to
        /// use the <see cref="Infovision.Math.Similarity.SquaredEuclidean(double[], double[])"/> distance.</param>
        /// <param name="linkage">The linkage function to use. Default is to
        /// use the <see cref="ClusteringLinkage.Single(int[], int[], DistanceMatrix)"/> linkage.</param>
        /// 
        public HierarchicalClustering(Func<double[], double[], double> distance, 
                                      Func<int[], int[], DistanceMatrix, double[][], double> linkage)
        {
            if (distance == null)
                throw new ArgumentNullException("distance");

            if(linkage == null)
                throw new ArgumentNullException("linkage");

            this.Distance = distance;
            this.Linkage = linkage;
        }

        /// <summary>
        ///   Initializes a new instance of the HierarchicalClustering algorithm
        /// </summary>
        ///         
        /// <param name="distanceMatrix">The distance matrix to use. </param>
        /// <param name="linkage">The linkage function to use. Default is to
        /// use the <see cref="ClusteringLinkage.Single(int[], int[], DistanceMatrix)"/> linkage.</param>
        /// 
        public HierarchicalClustering(DistanceMatrix matrix, Func<int[], int[], DistanceMatrix, double[][], double> linkage)
        {
            if (matrix == null)
                throw new ArgumentNullException("matrix");
            
            if (linkage == null)
                throw new ArgumentNullException("linkage");

            this.Distance = matrix.Distance;
            this.Linkage = linkage;

            this.distanceMatrix = new DistanceMatrix();
            foreach (KeyValuePair<MatrixKey, double> kvp in matrix)
                this.distanceMatrix.Add(new MatrixKey(kvp.Key.X, kvp.Key.Y), kvp.Value);
        }

        private void Initialize(double[][] points)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            data = points;

            this.numberOfInstances = points.Length;
            dendrogram = new DendrogramLinkCollection(this.numberOfInstances);
                       
            bool calculateDistanceMatrix = false;
            if (distanceMatrix == null)
            {
                int size = points.Length * (points.Length - 1) / 2;
                distanceMatrix = new DistanceMatrix(size, this.Distance);
                calculateDistanceMatrix = true;
            }
                        
            queue = new PriorityQueue<HierarchicalClusterTuple, HierarchicalClusterTupleValueAscendingComparer>();

            clusters = new Dictionary<int, HierarchicalCluster>(points.Length);
            for (int i = 0; i < points.Length; i++)
            {
                HierarchicalCluster cluster = new HierarchicalCluster(i);
                cluster.AddMemberObject(i);
                clusters.Add(i, cluster);

                for (int j = i + 1; j < points.Length; j++)
                {
                    if (calculateDistanceMatrix)
                    {
                        double distance = this.Distance(points[i], points[j]);
                        
                        /*
                        //HACK Wards linkage needs special zero distance calculation
                        if(this.linkage == ClusteringLinkage.Ward)
                            distance = this.Linkage(new int[] {i}, new int[] {j}, this.distanceMatrix, data);
                        else
                            distance = this.Distance(points[i], points[j]);
                        */

                        distanceMatrix[i, j] = distance;
                    }
                    HierarchicalClusterTuple tuple = new HierarchicalClusterTuple(i, j, distanceMatrix[i, j], 1, 1);                                       
                    queue.Enqueue(tuple);
                }
            }

            nextClusterId = points.Length;
        }
        
        /*
        private void InitDistanceMatrix(double[][] points)
        {
            if (distanceMatrix == null)
            {
                int size = points.Length * (points.Length - 1) / 2;
                distanceMatrix = new DistanceMatrix(size, this.Distance);                                
                queue = new PriorityQueue<HierarchicalClusterTuple, HierarchicalClusterTupleValueAscendingComparer>();
                for (int i = 0; i < points.Length; i++)
                {                                        
                    for (int j = i + 1; j < points.Length; j++)
                    {                                                
                        double distance = this.Distance(points[i], points[j]);                        
                        distanceMatrix[i,j] = distance;                                                           
                        HierarchicalClusterTuple tuple = new HierarchicalClusterTuple(i,j,distance,1,1);                        
                        queue.Enqueue(tuple);
                    }
                }                
            }
        }
        */

        /*
        private void InitClusters(double[][] points)
        {
            clusters = new Dictionary<int, HierarchicalCluster>(points.Length);
            for (int i = 0; i < points.Length; i++)
            {
                HierarchicalCluster cluster = new HierarchicalCluster(i);
                cluster.AddMemberObject(i);
                clusters.Add(i, cluster);
            }
            nextClusterId = points.Length;
        }
        */

        /// <summary>
        ///  Creates a hierarchy of clusters 
        /// </summary>
        /// 
        /// <param name="data">The data where to compute the algorithm.</param>        
        public virtual void Compute(double[][] data)
        {            
            if (data == null)
                throw new ArgumentNullException("data");

            if (data.Length == 0)
                return;
            
            this.Initialize(data);
            this.CreateClusters();
            this.Cleanup();            
        }
        
        protected void AddCluster(HierarchicalCluster cluster)
        {
            clusters.Add(cluster.Index, cluster);
            nextClusterId++;
        }

        protected void RemoveCluster(int key)
        {
            clusters.Remove(key);
        }

        protected int GetClusterSize(int clusterIdx)
        {
            return this.clusters[clusterIdx].Count;
        }

        protected bool HasMoreClustersToMerge()
        {
            return clusters.Count > 1;
        }

        protected virtual void CreateClusters()
        {                                    
            for (int i = 0; i < this.numberOfInstances - 1; i++ )
            {
                // use priority queue to find next best pair to cluster                
                HierarchicalClusterTuple t;
                do
                {                    
                    t = queue.Dequeue();

                } while (t != null 
                            && (clusters[t.X].MemberObjects.Count != t.SizeX 
                                || clusters[t.Y].MemberObjects.Count != t.SizeY));

                if (t != null)
                {
                    this.MergeClusters(t.X, t.Y, t.Value);

                    // update distances & queue
                    for (int j = 0; j < this.numberOfInstances; j++)
                    {
                        if (j != t.X && clusters[j].MemberObjects.Count != 0)
                        {
                            int i1 = System.Math.Min(t.X, j);
                            int i2 = System.Math.Max(t.X, j);

                            double distance = this.GetClusterDistance(i1, i2);
                            HierarchicalClusterTuple newTuple = new HierarchicalClusterTuple(i1, i2, distance, clusters[i1].MemberObjects.Count, clusters[i2].MemberObjects.Count);                            
                            queue.Enqueue(newTuple);
                        }
                    }
                }
            }                                                                        
        }        

        protected DendrogramLink GetClustersToMerge()
        {
            int[] result = new int[2] { -1, -1 };
            double minDistance = Double.MaxValue;

            foreach (KeyValuePair<MatrixKey, double> kvp in this.distanceMatrix.ReadOnlyMatrix)
            {
                Console.WriteLine("{0}: {1} {2} -> {3}", this.NextClusterId, kvp.Key.X, kvp.Key.Y, kvp.Value);
                
                if (kvp.Value < minDistance)
                {
                    result[0] = kvp.Key.X;
                    result[1] = kvp.Key.Y;
                    minDistance = kvp.Value;
                }
            }

            return new DendrogramLink(this.nextClusterId, result[0], result[1], minDistance);
        }

        protected DendrogramLink GetClustersToMergeSimple()
        {
            int[] key = new int[2] { -1, -1 };

            //TODO ToArray() is expensive
            //TODO clusters need to be private in parent class
            int[] clusterIds = clusters.Keys.ToArray();
            double minClusterDistance = double.MaxValue;

            for (int j = 0; j < clusterIds.Length; j++)
            {
                for (int k = j + 1; k < clusterIds.Length; k++)
                {
                    double minObjectDistance = this.GetClusterDistance(clusterIds[j], clusterIds[k]);

                    //TODO Remove this
                    Console.WriteLine("{0}: {1} {2} -> {3}", this.NextClusterId, clusterIds[j], clusterIds[k], minObjectDistance);

                    if (minObjectDistance < minClusterDistance)
                    {
                        minClusterDistance = minObjectDistance;

                        key[0] = clusterIds[j];
                        key[1] = clusterIds[k];
                    }
                }
            }

            return new DendrogramLink(this.NextClusterId, key[0], key[1], minClusterDistance);
        }

        protected int MergeClusters(int x, int y, double distance)
        {            
            HierarchicalCluster.MergeClustersInPlace(clusters[x], clusters[y]);            
            this.dendrogram.Add(x, y, distance, nextClusterId, clusters[x].MemberObjects.Count == this.numberOfInstances);            
            return nextClusterId++;
        }

        //protected int MergeClusters(MatrixKey key, double distance)
        //{
        //    return this.MergeClusters(key.X, key.Y, distance);                      
        //}

        //protected int MergeClusters(DendrogramLink link)
        //{
        //    return this.MergeClusters(link.Cluster1, link.Cluster2, link.Distance);            
        //}

        protected double GetClusterDistance(int i, int j)
        {
            if (clusters[i].Count == 1 && clusters[j].Count == 1)
                //assume that clusterId and objectId at the beginning are the same
                return this.distanceMatrix.GetDistance(i, j);
                        
            int[] cluster1 = clusters[i].MemberObjects.ToArray();
            int[] cluster2 = clusters[j].MemberObjects.ToArray();
            
            return this.Linkage(cluster1, cluster2, this.distanceMatrix, data);
        }        

        protected void Cleanup()
        {
            clusters = null;
            nextClusterId = 0;
        }

        public int[] GetClusterMembership(int numberOfClusters)
        {
            return this.DendrogramLinkCollection.GetClusterMembership(numberOfClusters);
        }

        public Dictionary<int, List<int>> GetClusterMembershipAsDict(int numberOfClusters)
        {
            int[] membership = this.GetClusterMembership(numberOfClusters);
            Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();
            for (int i = 0; i < membership.Length; i++)
            {
                if (result.ContainsKey(membership[i]))
                {
                    List<int> tmpList = result[membership[i]];
                    tmpList.Add(i);
                }
                else
                {
                    List<int> tmpList = new List<int>();
                    tmpList.Add(i);
                    result.Add(membership[i], tmpList);
                }
            }

            return result;
        }

        public int[] GetClusterMembership(double threshold)
        {
            return this.DendrogramLinkCollection.GetClusterMembership(threshold);
        }

        
        public Bitmap GetDendrogramAsBitmap(int width, int height)
        {
            int topMargin = 1;
            int bottomMargin = 1;
            int leftMargin = 1;
            int rightMargin = 1;

            int dendrogramWidth = width - (leftMargin + rightMargin); 
            int dendrogramHeight = height - (topMargin + bottomMargin);

            Color background = Color.White;
            Color foreground = Color.Black;            

            int xMajorScalePx = (int) System.Math.Floor((double)dendrogramWidth / this.numberOfInstances);
            
            //TODO scale if MaxHeight < 1
            double maxHeight = this.DendrogramLinkCollection.MaxHeight;
            double heightScale = 1;
            if (maxHeight < 1)
            {
                while (maxHeight < 1)
                {
                    maxHeight *= 10;
                    heightScale *= 10;
                }
            }

            if (dendrogramHeight < maxHeight)
            {

            }

            int yMajorScalePx = (int)System.Math.Floor((double)(dendrogramHeight - 40) / (maxHeight + 1));                        
                                               
            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {                
                g.Clear(background);                
                
                //draw Y axis
                Pen yAxisPen = new Pen(foreground, 1);
                System.Drawing.Point yAxisStart = new System.Drawing.Point(leftMargin + 10, topMargin + 20);
                System.Drawing.Point yAxisEnd = new System.Drawing.Point(leftMargin + 10, (height - (bottomMargin + 20)));                                                
                g.DrawLine(yAxisPen, yAxisStart, yAxisEnd);

                //draw Y major scale and dashed grid lines
                System.Drawing.Point yAxisTickBegin = new System.Drawing.Point(yAxisEnd.X - 1, yAxisEnd.Y);
                System.Drawing.Point yAxisTickEnd = new System.Drawing.Point(yAxisEnd.X + 1, yAxisTickBegin.Y);                                                                
                System.Drawing.Point dashedBegin = new System.Drawing.Point(yAxisEnd.X, yAxisEnd.Y);
                System.Drawing.Point dashedEnd = new System.Drawing.Point(width - (leftMargin + 20), dashedBegin.Y);
                
                Pen dahedGridPen = new Pen(Color.Black, 1);
                dahedGridPen.DashStyle = DashStyle.Dot;
                while (yAxisTickBegin.Y >= yAxisStart.Y)
                {
                    g.DrawLine(yAxisPen, yAxisTickBegin, yAxisTickEnd);
                    
                    yAxisTickBegin.Y -= yMajorScalePx;
                    yAxisTickEnd.Y = yAxisTickBegin.Y; 

                    g.DrawLine(dahedGridPen, dashedBegin, dashedEnd);
                    
                    dashedBegin.Y -= yMajorScalePx;
                    dashedEnd.Y = dashedBegin.Y;
                }
                yAxisPen.Dispose();
                dahedGridPen.Dispose();
                                
                
                //calculate leaf node positions 
                int[] leafOrder = this.DendrogramLinkCollection.ComputeLeafNodesFromTree();
                Dictionary<int, DendrogramLinkChartData> dendrogramChartData = new Dictionary<int, DendrogramLinkChartData>(leafOrder.Length - 1);
                int xAxisOffset = 10;
                int nodePointX = yAxisEnd.X + xAxisOffset;                
                for (int i = 0; i < leafOrder.Length; i++)
                {                                        
                    DendrogramLinkChartData linkChartData = new DendrogramLinkChartData(leafOrder[i]);
                                        
                    linkChartData.LeftNodeX = nodePointX;
                    linkChartData.LeftNodeY = yAxisEnd.Y;
                    linkChartData.RightNodeX = nodePointX;
                    linkChartData.RightNodeY = yAxisEnd.Y;
                    linkChartData.ParentNodeY = yAxisEnd.Y;

                    dendrogramChartData.Add(leafOrder[i], linkChartData);

                    nodePointX += xMajorScalePx;
                }
                leafOrder = null;

                //draw dendrogram                
                foreach (DendrogramLink link in this.DendrogramLinkCollection)
                {
                    DendrogramLinkChartData linkChartData1 = dendrogramChartData[link.Cluster1];
                    DendrogramLinkChartData linkChartData2 = dendrogramChartData[link.Cluster2];

                    DendrogramLinkChartData newLinkChartData = new DendrogramLinkChartData(link.Id);
                    newLinkChartData.LeftNodeX = linkChartData1.ParentNodeX;
                    newLinkChartData.LeftNodeY = linkChartData1.ParentNodeY;
                    newLinkChartData.RightNodeX = linkChartData2.ParentNodeX;
                    newLinkChartData.RightNodeY = linkChartData2.ParentNodeY;

                    int nodeHeight = (int) (link.Distance * heightScale * yMajorScalePx);
                    newLinkChartData.ParentNodeY = yAxisEnd.Y - nodeHeight;                                        

                    dendrogramChartData.Add(link.Id, newLinkChartData);
                }

                Font font = new Font("Tahoma", 9);
                Pen dendrogramPen = new Pen(foreground, 2);
                foreach (KeyValuePair<int, DendrogramLinkChartData> kvp in dendrogramChartData)
                {
                    kvp.Value.Draw(g, dendrogramPen, true, font);
                }
                dendrogramPen.Dispose();
                font.Dispose();                
                
                                
                g.Flush();                
                Bitmap result = new Bitmap(bitmap);                                                                                
            }

            return bitmap;
        }

        public override string ToString()
        {
            return DendrogramLinkCollection.ToString();
        }
    }
}
