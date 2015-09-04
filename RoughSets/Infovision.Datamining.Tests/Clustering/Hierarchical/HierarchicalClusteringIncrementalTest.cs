using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Datamining.Clustering.Hierarchical;
using Infovision.Math;
using NUnit.Framework;

namespace Infovision.Datamining.Tests.Clustering.Hierarchical
{
    [TestFixture]    
    public class HierarchicalClusteringIncrementalTest
    {

        [Test]
        public void AddInstanceTest()
        {
            double[][] data = HierarchicalClusteringTest.GetData();
            
            HierarchicalClustering sahn = new HierarchicalClustering(Similarity.Euclidean, ClusteringLinkage.Complete);
            sahn.Compute(data);
            
            Bitmap b1 = sahn.GetDendrogramAsBitmap(640, 480);
            string f1 = String.Format(@"F:\Dendrogram\sahn.bmp");
            b1.Save(f1, System.Drawing.Imaging.ImageFormat.Bmp);

            
            HierarchicalClusteringIncremental sihc = new HierarchicalClusteringIncremental(Similarity.Euclidean, ClusteringLinkage.Complete);            
            for (int i = 0; i < data.Length; i++)
            {
                sihc.AddInstance(i, data[i]);
            }

            Bitmap b2 = sihc.GetDendrogramAsBitmap(640, 480);
            string f2 = String.Format(@"F:\Dendrogram\sihc.bmp");
            b2.Save(f2, System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
