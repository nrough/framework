using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    [Serializable]
    public abstract class DiscretizeSupervisedBase : DiscretizeBase, IDiscretizationSupervised
    {       
        #region Constructors

        public DiscretizeSupervisedBase()
            : base() { }

        #endregion

        #region Methods

        public virtual long[] ComputeCuts(long[] data, long[] labels, double[] weights)
        {
            return null;
        }

        public void Compute(long[] data, long[] labels, double[] weights)
        {
            if (data == null) throw new ArgumentNullException("data", "data == null");
            if (labels != null && labels.Length != data.Length)
                throw new ArgumentException("labels.Length != data.Length", "labels");
            if(weights != null && weights.Length != data.Length)
                throw new ArgumentException("weights.Length != data.Length", "weights");

            //sort
            //calculate prior counts
            //calculate impurity measure
            //for each cut calculate impurity measure for left and right subcounts
            //select best split
            //call recurently compute cuts on subarrays
            //merge cuts
            //stop criterion 
            //- all values are the same
            //- array length is >= 1
            //- only a single label
            //- gain is not positive
            //- a criterion MDL Fayaad & Irani is met
            
        

            this.Cuts = ComputeCuts(data, labels, weights);
            if (this.Cuts == null)
                this.Cuts = new long[0];
        }

        #endregion
    }
}
