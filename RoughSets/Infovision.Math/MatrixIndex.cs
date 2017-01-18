﻿using System;

namespace Raccoon.Math
{
    [Serializable]
    public struct MatrixKey
    {
        private readonly int x;
        private readonly int y;

        public int X { get { return x; } }
        public int Y { get { return this.y; } }

        public MatrixKey(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        #region System.Object Methods

        public override string ToString()
        {
            return String.Format("{0} {1}", x, y);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            MatrixKey index;
            if (obj is MatrixKey)
            {
                index = (MatrixKey)obj;
                return (this.X == index.X && this.Y == index.Y) || (this.X == index.Y && this.Y == index.X);
            }

            return false;
        }

        #endregion System.Object Methods
    }
}