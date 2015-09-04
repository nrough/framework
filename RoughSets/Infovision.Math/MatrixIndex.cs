using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infovision.Math
{
    public struct MatrixKey
    {
        private readonly int x;
        private readonly int y;

        public MatrixKey(int x, int y)
        {
            this.x = x;
            this.y = y;
        } 

        public int X
        {
            get { return x; }            
        }

        public int Y
        {
            get { return this.y; }
        }

        public int SmallerIndex
        {
            get 
            { 
                if (this.X < this.Y) 
                    return this.X;
                return this.Y;
            }
        }

        public int GreaterIndex
        {
            get
            {
                if (this.X < this.Y)
                    return this.Y;
                return this.X;
            }
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
                return this.X == index.X && this.Y == index.Y;
            }

            return false;            
        }

        #endregion
    }
}
