using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Infovision.MachineLearning.Clustering.Hierarchical;
using Infovision.Math;
using Infovision.Core;
using NUnit.Framework;

namespace Infovision.MachineLearning.Tests.Clustering.Hierarchical
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
            string f1 = String.Format(@"sahn.bmp");
            b1.Save(f1, System.Drawing.Imaging.ImageFormat.Bmp);

            var sihc = new HierarchicalClusteringSIHC(Similarity.Euclidean, ClusteringLinkage.Complete);

            foreach (KeyValuePair<int, double[]> kvp in data)
                sihc.AddToCluster(kvp.Key, kvp.Value);

            DendrogramChart dc2 = new DendrogramChart(sihc, 640, 480);
            Bitmap b2 = dc2.GetAsBitmap();
            string f2 = String.Format(@"sihc.bmp");
            b2.Save(f2, System.Drawing.Imaging.ImageFormat.Bmp);

            HierarchicalClustering simple = new HierarchicalClustering(Similarity.Euclidean, ClusteringLinkage.Complete);
            simple.Instances = data;
            simple.Compute();
            DendrogramChart dc3 = new DendrogramChart(simple, 640, 480);
            Bitmap b3 = dc3.GetAsBitmap();
            string f3 = String.Format(@"simple.bmp");
            b3.Save(f3, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        [Test]
        public void AddToClusterInLinkOrderTest()
        {
            var data = HierarchicalClusteringTest.GetDataAsDict();

            HierarchicalClustering sahn = new HierarchicalClustering(Similarity.Euclidean, ClusteringLinkage.Complete);
            sahn.Instances = data;
            sahn.Compute();

            DendrogramChart dc1 = new DendrogramChart(sahn, 640, 480);
            Bitmap b1 = dc1.GetAsBitmap();
            string f1 = String.Format(@"sahn_0.bmp");
            b1.Save(f1, System.Drawing.Imaging.ImageFormat.Bmp);

            int[] keys = data.Keys.ToArray();
            for (int t = 0; t < 10; t++)
            {
                int[] tmp = keys.ShuffleDuplicate();

                HierarchicalClusteringSIHC sihc = new HierarchicalClusteringSIHC(Similarity.Euclidean, ClusteringLinkage.Complete);
                sihc.MinimumNumberOfInstances = 5;

                for (int i = 0; i < tmp.Length; i++)
                    sihc.AddToCluster(tmp[i], data[tmp[i]]);

                DendrogramChart dc2 = new DendrogramChart(sihc, 640, 480);
                Bitmap b2 = dc2.GetAsBitmap();
                string f2 = String.Format(@"sihc_{0}.bmp", t);
                b2.Save(f2, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }
    }
}