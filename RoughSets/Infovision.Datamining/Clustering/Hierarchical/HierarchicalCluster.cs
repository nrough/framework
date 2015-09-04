using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.MachineLearning;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    [Serializable]
    public class HierarchicalCluster
    {               
        List<int> objects = new List<int>();        
                
        public int Index {get; set; }
        
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
