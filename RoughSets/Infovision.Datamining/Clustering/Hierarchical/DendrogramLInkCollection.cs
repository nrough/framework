using System;  
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Utils;

namespace Infovision.Datamining.Clustering.Hierarchical
{
    public class DendrogramLinkCollection : IEnumerable<DendrogramLink>
    {        
        private DendrogramLink[] linkages;                

        public int Count
        {
            get { return linkages.Length; }
        }        

        public DendrogramLink this[int index]
        {
            get { return linkages[index]; }
            set { linkages[index] = value; }
        }

        public DendrogramLinkCollection(int numOfInstances)
        {            
            linkages = new DendrogramLink[numOfInstances - 1];                   
        }

        public DendrogramLink GetLast()
        {            
            return linkages[this.Count - 1];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (DendrogramLink link in this)
                sb.AppendLine(link.ToString());            
            return sb.ToString();
        }

        /// <summary>
        ///   Returns an enumerator that iterates through a collection.
        /// </summary>
        /// 
        /// <returns>
        ///   An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// 
        public IEnumerator<DendrogramLink> GetEnumerator()
        {
            return linkages.AsEnumerable().GetEnumerator();
        }

        /// <summary>
        ///   Returns an enumerator that iterates through a collection.
        /// </summary>
        /// 
        /// <returns>
        ///   An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// 
        IEnumerator IEnumerable.GetEnumerator()
        {
            return linkages.GetEnumerator();
        }

        public void Sort(Comparer<DendrogramLink> comparer)
        {
            Array.Sort<DendrogramLink>(linkages, comparer);
        }
    }
}
