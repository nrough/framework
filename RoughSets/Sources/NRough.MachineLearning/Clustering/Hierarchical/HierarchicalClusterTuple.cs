using System;
using System.Collections.Generic;

namespace NRough.MachineLearning.Clustering.Hierarchical
{
    [Serializable]
    internal class HierarchicalClusterTuple
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

        #endregion System.Object Methods
    }

    internal class HierarchicalClusterTupleValueAscendingComparer : Comparer<HierarchicalClusterTuple>
    {
        public override int Compare(HierarchicalClusterTuple t1, HierarchicalClusterTuple t2)
        {
            return t1.Value.CompareTo(t2.Value);
        }
    }

    internal class HierarchicalClusterTupleValueDescendingComparer : Comparer<HierarchicalClusterTuple>
    {
        public override int Compare(HierarchicalClusterTuple t1, HierarchicalClusterTuple t2)
        {
            return -t1.Value.CompareTo(t2.Value);
        }
    }
}