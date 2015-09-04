using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public struct DendrogramLink
    {
        private readonly int cluster1;
        private readonly int cluster2;
        private readonly int level;

        public DendrogramLink(int cluster1, int cluster2, int level)
        {
            this.cluster1 = cluster1;
            this.cluster2 = cluster2;
            this.level = level;
        }

        public int Cluster1
        {
            get { return this.cluster1; }            
        }

        public int Cluster2
        {
            get { return this.cluster2; }
        }

        public int Level
        {
            get { return this.level; }
        }

        #region System.Object Methods

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", this.level, this.cluster1, this.cluster2);
        }

        public override int GetHashCode()
        {
            return cluster1.GetHashCode() ^ cluster2.GetHashCode() ^ level.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            DendrogramLink denLink;
            if (obj is DendrogramLink)
            {
                denLink = (DendrogramLink)obj;
                return this.Cluster1 == denLink.Cluster1 && this.Cluster2 == denLink.Cluser2 && this.level == denLink.level;
            }

            return false;            
        }

        #endregion
        
    }
}
