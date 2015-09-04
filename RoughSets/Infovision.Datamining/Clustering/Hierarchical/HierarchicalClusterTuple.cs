using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.Clustering.Hierarchical
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

        public long LongValue
        {
            get
            {
                //in case of reversed distance function (1/d(x,y)) when d(x,y) is 0.0
                //we substitte the 1/d with Double.MaxValue, then after mapping to long value with multiplication it becomes negative
                //the Abs function removes the negative sign
                //
                //the above is not correct. Double.MaxValue is far more grater than Int64.MaxValue, the conversion is still producing negative value

                if (this.val > (Double.MaxValue - 2))
                {
                    return Int64.MaxValue;
                }

                double result = System.Math.Abs(this.val * 10000000);
                return (long)result;
            }
        }

        public int SizeX
        {
            get { return this.sizeX; }
        }

        public int SizeY
        {
            get { return this.sizeY; }
        }

        public long GetLongValue()
        {
            //in case of reversed distance function (1/d(x,y)) when d(x,y) is 0.0
            //we substitte the 1/d with Double.MaxValue, then after mapping to long value with multiplication it becomes negative
            //the Abs function removes the negative sign
            //
            //the above is not correct. Double.MaxValue is far more grater than Int64.MaxValue, the conversion is still producing negative value

            if (this.val == Double.MaxValue)
            {
                return Int64.MaxValue;
            }

            double result = System.Math.Abs(this.val * 10000000);
            return (long)result;
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

        #endregion
    }

    internal class MHierarchicalClusterTupleValueComparer : IComparer<HierarchicalClusterTuple>
    {
        public int Compare(HierarchicalClusterTuple t1, HierarchicalClusterTuple t2)
        {
            if (t1.Value < t2.Value)
                return -1;
            if (t1.Value > t2.Value)
                return 1;

            if (t1.X < t2.X)
                return -1;
            if (t1.X > t2.X)
                return 1;

            if (t1.Y < t2.Y)
                return -1;
            if (t1.Y > t2.Y)
                return 1;

            return 0;
        }
    }
}
