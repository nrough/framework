using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    internal class HierarchicalClusterTuple// : IComparable, IComparable<HierarchicalClusterTuple>
    {
        private readonly int x;
        private readonly int y;
        private readonly double val;
        private readonly int sizeX;
        private readonly int sizeY;        

        public HierarchicalClusterTuple(int x, int y, double val, int sizeX, int sizeY)
        {
            this.x = x;
            this.y = y;
            this.val = val;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
        }

        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return this.y; }
        }

        public double Value
        {
            get { return this.val; }
        }        

        public int SizeX
        {
            get { return this.sizeX; }
        }

        public int SizeY
        {
            get { return this.sizeY; }
        }
        
       
        /*
        public int CompareTo(object obj)
        {
            if(obj == null) return 1;
            HierarchicalClusterTuple tuple = obj as HierarchicalClusterTuple;
            if(tuple == null)
                throw new ArgumentException("Object is not HierarchicalClusterTuple");

            int valResult = this.Value.CompareTo(tuple.Value);
            if (valResult != 0)
                return valResult;                        

            if (this.X < tuple.X)
                return -1;
            if (this.X > tuple.X)
                return 1;

            if (this.Y < tuple.Y)
                return -1;
            if (this.Y > tuple.Y)
                return 1;

            return 0;
        }

        public int CompareTo(HierarchicalClusterTuple other)
        {
            if (other == null) return 1;

            int valResult = this.Value.CompareTo(tuple.Value);
            if (valResult != 0)
                return valResult;

            if (this.X < other.X)
                return -1;
            if (this.X > other.X)
                return 1;

            if (this.Y < other.Y)
                return -1;
            if (this.Y > other.Y)
                return 1;

            return 0;
        }
        */

        #region System.Object Methods

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", x, y, val);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            HierarchicalClusterTuple index;
            if (obj is HierarchicalClusterTuple)
            {
                index = (HierarchicalClusterTuple)obj;
                return this.X == index.X && this.Y == index.Y;
            }

            return false;
        }        

        #endregion
    }

    internal class HierarchicalClusterTupleValueAscendingComparer : Comparer<HierarchicalClusterTuple>
    {
        public override int Compare(HierarchicalClusterTuple t1, HierarchicalClusterTuple t2)
        {
            return t1.Value.CompareTo(t2.Value);

            /*
            int valResult = t1.Value.CompareTo(t2.Value);
            if (valResult != 0)
                return valResult;            
            if (t1.X < t2.X)
                return -1;
            if (t1.X > t2.X)
                return 1;

            if (t1.Y < t2.Y)
                return -1;
            if (t1.Y > t2.Y)
                return 1;            
            return 0;
            */
        }
    }

    internal class HierarchicalClusterTupleValueDescendingComparer : Comparer<HierarchicalClusterTuple>
    {
        public override int Compare(HierarchicalClusterTuple t1, HierarchicalClusterTuple t2)
        {
            return -1 * t1.Value.CompareTo(t2.Value);
            /*
            int valResult = t1.Value.CompareTo(t2.Value);
            if (valResult != 0)
                return - valResult;

            
            if (t1.X < t2.X)
                return 1;
            if (t1.X > t2.X)
                return -1;

            if (t1.Y < t2.Y)
                return 1;
            if (t1.Y > t2.Y)
                return -1;            
            return 0;
            */
        }
    }
}
