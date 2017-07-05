// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

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