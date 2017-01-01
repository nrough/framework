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

        public abstract long[] ComputeCuts(long[] data, long[] labels, double[] weights);

        public void Compute(long[] data, long[] labels, double[] weights)
        {
            if (data == null) throw new ArgumentNullException("data", "data == null");
            if (labels != null && labels.Length != data.Length)
                throw new ArgumentException("labels.Length != data.Length", "labels");
            if(weights != null && weights.Length != data.Length)
                throw new ArgumentException("weights.Length != data.Length", "weights");

            this.Cuts = ComputeCuts(data, labels, weights);
            if (this.Cuts == null)
                this.Cuts = new long[0];
        }

        #endregion
    }
}
