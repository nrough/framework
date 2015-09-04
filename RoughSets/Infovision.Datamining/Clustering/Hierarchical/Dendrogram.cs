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

        public void Add(int cluster1, int cluster2)
        {
            level++;
            linkages.Add(new DendrogramLink(cluster1, cluster2, level));
        }

        public int[] ComputeLeafNodes()
        {
            int[] order = new int[this.Count + 1];
            int pos = 0;
            ReorderChildren(ca);
            WalkChildren(ca, ref order, ref pos, ca.Linkages.Rows - 1);
            return order;
        }

        private void WalkChildren(ref int[] order, ref int pos, int row)
        {
            int n = this.Count + 1;
            int node = (int)ca.Linkages[row, 0];

            if (node >= n)
            {
                WalkChildren(ca, ref order, ref pos, node - n);
            }

            if (node < n)
                order[pos++] = node;

            node = (int)ca.Linkages[row, 1];
            if (node >= n)
            {
                WalkChildren(ca, ref order, ref pos, node - n);
            }

            if (node < n)
                order[pos++] = node;
        }

        private static void ReorderChildren(ClusterAnalysis ca)
        {
            int n = ca.Linkages.Rows + 1;
            for (int i = 0; i < ca.Linkages.Rows; i++)
            {
                int l1 = (int)ca.Linkages[i, 0];
                int l2 = (int)ca.Linkages[i, 1];
                if (l1 < n && l2 < n)
                {
                    if (l1 > l2)
                    {
                        Swap(ca, i);
                    }
                }
                else if (l1 < n && l2 >= n)
                {
                    Swap(ca, i);
                }
                else if (l1 >= n && l2 >= n)
                {
                    if (l1 > l2)
                    {
                        Swap(ca, i);
                    }
                }
            }
        }

        private static void Swap(ClusterAnalysis ca, int row)
        {
            double tmp = ca.Linkages[row, 0];
            ca.Linkages[row, 0] = ca.Linkages[row, 1];
            ca.Linkages[row, 1] = tmp;
        }
    }
}
