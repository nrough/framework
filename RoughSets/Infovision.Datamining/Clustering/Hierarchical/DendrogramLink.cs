using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public struct DendrogramLink
    {
        private readonly int cluster1;
        private readonly int cluster2;
        private readonly double distance;
        private readonly int id;

        public DendrogramLink(int id, int cluster1, int cluster2, double distance)
        {
            this.id = id;
            this.cluster1 = cluster1;
            this.cluster2 = cluster2;
            this.distance = distance;            
        }

        public int Cluster1
        {
            get { return this.cluster1; }            
        }

        public int Cluster2
        {
            get { return this.cluster2; }
        }        

        public double Distance
        {
            get { return this.distance; }
        }

        public int Id
        {
            get { return this.id; }
        }

        #region System.Object Methods

        public override string ToString()
        {
            return String.Format("{0} -> d({1};{2}) = {3} ", this.id, this.cluster1, this.cluster2, this.distance);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            DendrogramLink denLink;
            if (obj is DendrogramLink)
            {
                denLink = (DendrogramLink)obj;
                return this.Id == denLink.Id;
            }

            return false;            
        }

        #endregion        
    }

    public class DendrogramLinkDistAscendingComparer : Comparer<DendrogramLink>
    {
        public override int Compare(DendrogramLink left, DendrogramLink right)
        {
            int result = left.Distance.CompareTo(right.Distance);
            if (result != 0)
            {
                return result;
            }
            else
            {
                return System.Math.Abs(left.Id).CompareTo(System.Math.Abs(right.Id));
            }
        }
    }

    public class DendrogramLinkDistDescendingComparer : Comparer<DendrogramLink>
    {
        public override int Compare(DendrogramLink left, DendrogramLink right)
        {
            int result = left.Distance.CompareTo(right.Distance);
            if (result != 0)
            {
                return -result;
            }
            else
            {
                return -System.Math.Abs(left.Id).CompareTo(System.Math.Abs(right.Id));
            }            
        }
    }
}
