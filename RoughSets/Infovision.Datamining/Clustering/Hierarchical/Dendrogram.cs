using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public class Dendrogram
    {
        List<DendrogramLink> linkages;
        int level;

        public Dendrogram()
        {
            linkages = new List<DendrogramLink>();
            level = -1;
        }

        public int Count
        {
            get { return linkages.Count; }
        }

        public void Add(int cluster1, int cluster2, double distance)
        {
            linkages.Add(new DendrogramLink(cluster1, cluster2, distance, ++level));
        }

        public int[] ComputeLeafNodes()
        {
            int[] order = new int[this.Count + 1];
            int pos = 0;
            ReorderChildren();
            WalkChildren(ref order, ref pos, this.Count - 1);
            return order;
        }

        private void WalkChildren(ref int[] order, ref int pos, int row)
        {
            int n = this.Count + 1;
            int node = this.linkages[row].Cluster1;

            if (node >= n)
            {
                WalkChildren(ref order, ref pos, node - n);
            }

            if (node < n)
                order[pos++] = node;

            node = this.linkages[row].Cluster2;
            if (node >= n)
            {
                WalkChildren(ref order, ref pos, node - n);
            }

            if (node < n)
                order[pos++] = node;
        }

        private void ReorderChildren()
        {
            int n = this.Count + 1;
            for (int i = 0; i < this.Count; i++)
            {                
                if (this.linkages[i].Cluster1 < n && this.linkages[i].Cluster2 < n)
                {
                    if (this.linkages[i].Cluster1 > this.linkages[i].Cluster2)
                    {
                        Swap(i);
                    }
                }
                else if (this.linkages[i].Cluster1 < n && this.linkages[i].Cluster2 >= n)
                {
                    Swap(i);
                }
                else if (this.linkages[i].Cluster1 >= n && this.linkages[i].Cluster2 >= n)
                {
                    if (this.linkages[i].Cluster1 > this.linkages[i].Cluster2)
                    {
                        Swap(i);
                    }
                }
            }
        }

        private void Swap(int row)
        {
            linkages[row] = new DendrogramLink(linkages[row].Cluster2, linkages[row].Cluster1, linkages[row].Distance, linkages[row].Level);                        
        }
    }
}
