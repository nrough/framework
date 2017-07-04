//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;
using System.Collections.Generic;
using System.Linq;

namespace NRough.MachineLearning.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalCluster
    {
        private List<int> objects = new List<int>();

        public int Index { get; set; }

        private List<int> Objects
        {
            get { return this.objects; }
            set { this.objects = value; }
        }

        public int Count
        {
            get { return this.objects.Count; }
        }

        //public System.Collections.ObjectModel.ReadOnlyCollection<int> MemberObjects
        //{
        //    get { return this.objects.AsReadOnly(); }
        //}

        public List<int> MemberObjects
        {
            get { return this.objects; }
        }

        public HierarchicalCluster(int index)
        {
            this.Index = index;
        }

        public static HierarchicalCluster MergeClusters(int newClusterIndex, HierarchicalCluster cluster1, HierarchicalCluster cluster2)
        {
            HierarchicalCluster result = new HierarchicalCluster(newClusterIndex);
            result.Objects = cluster1.Objects.Concat(cluster2.Objects).ToList();
            return result;
        }

        public static void MergeClustersInPlace(HierarchicalCluster destination, HierarchicalCluster source)
        {
            destination.Objects = destination.Objects.Concat(source.Objects).ToList();
            source.Objects.RemoveAll(i => (i >= 0));
        }

        public void AddMemberObject(int objectId)
        {
            this.Objects.Add(objectId);
        }
    }
}