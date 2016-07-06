using System;
using System.Collections.Generic;

namespace Infovision.Math
{
    [Serializable]
    public struct MatrixTuple
    {
        private readonly int x;
        private readonly int y;
        private readonly double val;

        public MatrixTuple(int x, int y, double val)
        {
            this.x = x;
            this.y = y;
            this.val = val;
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

            MatrixTuple index;
            if (obj is MatrixTuple)
            {
                index = (MatrixTuple)obj;
                return this.X == index.X && this.Y == index.Y;
            }

            return false;
        }

        #endregion System.Object Methods
    }

    public class MatrixTupleValueComparer : Comparer<MatrixTuple>
    {
        public override int Compare(MatrixTuple t1, MatrixTuple t2)
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