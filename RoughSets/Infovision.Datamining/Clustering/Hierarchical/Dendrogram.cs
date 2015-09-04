using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public class Dendrogram
    {
        private DendrogramNode root;

        public DendrogramNode Root
        {
            get { return this.root; }
            private set { this.root = value; }
        }

        public Dendrogram(HierarchicalClustering clustering)
        {
            CreateNodes(clustering, clustering.DendrogramLinkCollection.Count - 1);

        }

        private void CreateNodes(HierarchicalClustering clustering, int row)
        {
            int n = clustering.DendrogramLinkCollection.Count + 1;
        }
    }
}
