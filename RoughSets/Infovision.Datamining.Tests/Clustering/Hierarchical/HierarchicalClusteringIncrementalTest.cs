using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Datamining.Visualization;
using Infovision.Math;
using NUnit.Framework;

namespace Infovision.Datamining.Tests.Clustering.Hierarchical
{
    [TestFixture]    
    public class HierarchicalClusteringIncrementalTest
    {
        [Test]
        public void AddToClusterTest()
        {
            Dictionary<int, double[]> data = HierarchicalClusteringTest.GetDataAsDict();
            
            HierarchicalClustering sahn = new HierarchicalClustering(Similarity.Euclidean, ClusteringLinkage.Complete);
            sahn.Instances = data;
            sahn.Compute();
            DendrogramChart dc1 = new DendrogramChart(sahn, 640, 480);            
            Bitmap b1 = dc1.GetAsBitmap(); 
            string f1 = String.Format(@"F:\Dendrogram\sahn.bmp");
            b1.Save(f1, System.Drawing.Imaging.ImageFormat.Bmp);
            
            HierarchicalClusteringIncremental sihc = new HierarchicalClusteringIncremental(Similarity.Euclidean, ClusteringLinkage.Complete);
            sihc.Instances = data;
            sihc.Compute();
            DendrogramChart dc2 = new DendrogramChart(sihc, 640, 480);
            Bitmap b2 = dc2.GetAsBitmap();
            string f2 = String.Format(@"F:\Dendrogram\sihc.bmp");
            b2.Save(f2, System.Drawing.Imaging.ImageFormat.Bmp);

            HierarchicalClustering simple = new HierarchicalClustering(Similarity.Euclidean, ClusteringLinkage.Complete);
            simple.Instances = data;
            simple.Compute();            
            DendrogramChart dc3 = new DendrogramChart(simple, 640, 480);
            Bitmap b3 = dc3.GetAsBitmap();
            string f3 = String.Format(@"F:\Dendrogram\simple.bmp");
            b3.Save(f3, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        [Test]
        public void AddToClusterInLinkOrderTest()
        {
            Dictionary<int, double[]> data = HierarchicalClusteringTest.GetDataAsDict();

            HierarchicalClustering sahn = new HierarchicalClustering(Similarity.Euclidean, ClusteringLinkage.Complete);
            sahn.Instances = data;
            sahn.Compute();

            DendrogramChart dc1 = new DendrogramChart(sahn, 640, 480);
            Bitmap b1 = dc1.GetAsBitmap();
            string f1 = String.Format(@"F:\Dendrogram\sahn.bmp");
            b1.Save(f1, System.Drawing.Imaging.ImageFormat.Bmp);

            HierarchicalClusteringIncremental sihc = new HierarchicalClusteringIncremental(Similarity.Euclidean, ClusteringLinkage.Complete);
            sihc.MinimumNumberOfInstances = 8;

            for (int i = 0; i < data.Count; i++)
            {
                sihc.AddToCluster(i, data[i]);
            }
            
            /*
            sihc.AddToCluster(5, data[5]);
            sihc.AddToCluster(9, data[9]);
            sihc.AddToCluster(8, data[8]);
            sihc.AddToCluster(7, data[7]);
            
            sihc.AddToCluster(0, data[0]);
            sihc.AddToCluster(1, data[1]);
            sihc.AddToCluster(2, data[2]);

            sihc.AddToCluster(4, data[4]);
            sihc.AddToCluster(6, data[6]);            
            sihc.AddToCluster(3, data[3]);            
            */
            
            DendrogramChart dc2 = new DendrogramChart(sihc, 640, 480);
            Bitmap b2 = dc2.GetAsBitmap();
            string f2 = String.Format(@"F:\Dendrogram\sihc.bmp");
            b2.Save(f2, System.Drawing.Imaging.ImageFormat.Bmp);

        }
    }
}
