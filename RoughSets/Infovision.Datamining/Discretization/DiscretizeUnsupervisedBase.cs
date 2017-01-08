﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    [Serializable]
    public abstract class DiscretizeUnsupervisedBase : DiscretizeBase, IDiscretizationUnsupervised
    {
        #region Properties

        public int NumberOfBuckets { get; set; }

        #endregion

        #region Constructors

        public DiscretizeUnsupervisedBase()
            : base() { }

        #endregion

        #region Methods

        public abstract long[] ComputeCuts(long[] data, double[] weights);

        public override void Compute(long[] data, long[] labels, double[] weights)
        {
            if (data == null) throw new ArgumentNullException("data", "data == null");            
            if (weights != null && weights.Length != data.Length)
                throw new ArgumentException("weights.Length != data.Length", "weights");

            if (!this.IsDataSorted)
                this.SortIndices(data);

            this.Cuts = ComputeCuts(data, weights);
            if (this.Cuts == null)
                this.Cuts = new long[0];
        }

        #endregion
    }
}
