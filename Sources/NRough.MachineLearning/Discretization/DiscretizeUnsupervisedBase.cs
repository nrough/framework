﻿//
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
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Discretization
{
    [Serializable]
    public abstract class DiscretizeUnsupervisedBase : DiscretizeBase, IDiscretizerUnsupervised
    {       
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
            this.Cleanup();
        }

        #endregion
    }
}
