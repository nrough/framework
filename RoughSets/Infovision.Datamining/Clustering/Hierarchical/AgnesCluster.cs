using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.MachineLearning;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public class AgnesCluster
    {       
        private int index;
        List<int> objects = new List<int>();

        public AgnesCluster(int index)
        {
            this.Index = index;
        }
                
        public int Index
        {
            get { return this.index; }
            set { this.index = value; }
        }

        private List<int> Objects
        {
            get { return this.objects; }
            set { this.objects = value; }
        }        

        public static AgnesCluster MergeClusters(int newClusterIndex, AgnesCluster cluster1, AgnesCluster cluster2)
        {
            AgnesCluster result = new AgnesCluster(newClusterIndex);            
            result.Objects = cluster1.Objects.Concat(cluster2.Objects).ToList();
            Console.WriteLine("{0} merged with {1} to {2}", cluster1.Index, cluster2.Index, result.Index); 
            return result;
        }
           
    }    
}
