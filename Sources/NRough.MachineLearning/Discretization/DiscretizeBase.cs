﻿// 
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

using NRough.Core;
using NRough.Core.CollectionExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Discretization
{
    [Serializable]
    public abstract class DiscretizeBase : ModelBase, IDiscretizer
    {
        #region Members

        private long[] cuts;
        private int[] sortedIndices;

        #endregion

        #region Properties

        /// <summary>
        /// Number of discreet values after discretization is performed. 
        /// For methods that evaluate optimal number of buckets this is the maximum number of dicreet values.        
        /// </summary>
        public int NumberOfBuckets { get; set; } = 5;

        /// <summary>
        /// If true, cut point will always be returned as sorted array. Remember that Apply needs sorted cuts.
        /// Setting to false will in come cases, cause to maintain the order in which the cuts were calculated. 
        /// This might be helpful in order to create a hierarchy of cuts
        /// </summary>
        public bool SortCuts { get; set; } = true;

        /// <summary>
        /// Calculated cut points
        /// </summary>
        public long[] Cuts
        {
            get { return this.cuts; }
            set
            {
                if (!this.ValidateCuts(value))
                    throw new InvalidOperationException("Discretization cuts are not valid");
                this.cuts = value;
            }
        }
        
        protected int[] SortedIndices
        {
            get { return this.sortedIndices; }
        }

        public bool IsDataSorted { get; set; } = false;
        public bool UseWeights { get; set; } = false;
        public bool MissingValueAsDistinctBucket { get; set; } = true;

        #endregion

        #region Constructors

        public DiscretizeBase()
            : base()
        {
            this.cuts = new long[0];
            this.IsDataSorted = false;
        }

        #endregion

        #region Methods

        public abstract void Compute(long[] data, long[] labels, double[] weights);

        public long[] Apply(long[] values)
        {
            return DiscretizeBase.Apply(values, Cuts);
        }

        public long Apply(long value)
        {
            return DiscretizeBase.Apply(value, Cuts);            
        }

        public long Apply(long value, int cutLimit)
        {
            long[] localCuts = Cuts.SubArray(0, cutLimit);
            Array.Sort(localCuts);
            return DiscretizeBase.Apply(value, localCuts);
        }

        public long[] Apply(long[] values, int cutLimit)
        {
            long[] localCuts = Cuts.SubArray(0, cutLimit);
            Array.Sort(localCuts);
            return DiscretizeBase.Apply(values, localCuts);
        }

        public static long Apply(long value, long[] cuts)
        {
            if (cuts == null)
                throw new ArgumentNullException("cuts", "cuts == null");
            for (int i = 0; i < cuts.Length; i++)
                if (value <= cuts[i])
                    return i;
            return cuts.Length;
        }

        public static long[] Apply(long[] values, long[] cuts)
        {
            //validate cuts
            if (cuts == null) throw new ArgumentNullException("cuts");
            for (int i = 1; i < cuts.Length; i++)
                if (cuts[i] <= cuts[i - 1])
                    throw new ArgumentException("cuts[i] <= cuts[i - 1]", "cuts");

            long[] result = new long[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = DiscretizeBase.Apply(values[i], cuts);
            return result;
        }

        protected void SortIndices(long[] data)
        {
            this.sortedIndices = Enumerable.Range(0, data.Length).ToArray();
            if(!this.IsDataSorted)
                Array.Sort(this.sortedIndices, (a, b) => data[a].CompareTo(data[b]));
        }

        protected Dictionary<long, double> CountLabels(long[] labels, int start, int end, double[] weights)
        {
            var result = new Dictionary<long, double>();
            for(int i = start; i < labels.Length && i < end; i++)
                if (result.ContainsKey(labels[sortedIndices[i]]))
                    result[labels[sortedIndices[i]]] += 
                        (weights == null || this.UseWeights == false) ? 1.0 : weights[sortedIndices[i]];
                else
                    result.Add(labels[sortedIndices[i]], 
                        (weights == null || this.UseWeights == false) ? 1.0 : weights[sortedIndices[i]]);
            return result;
        }

        private bool ValidateCuts(long[] cuts)
        {
            if (cuts == null)
                return true;

            if (SortCuts)
            {
                for (int i = 1; i < cuts.Length; i++)
                    if (cuts[i] <= cuts[i - 1])
                        return false;
            }

            return true;
        }

        protected virtual void Cleanup()
        {
            this.sortedIndices = null;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Cuts == null || Cuts.Length == 0)
            {
                sb.AppendLine(String.Format("{0}: ({1} {2})", 0, "-Inf", "+Inf"));
            }
            else
            {
                sb.AppendLine(String.Format("{0}: ({1} {2}>", 0, "-Inf", Cuts[0]));
                for (int i = 1; i < Cuts.Length; i++)
                    sb.AppendLine(String.Format("{0}: ({1} {2}>", i, Cuts[i - 1], Cuts[i]));
                sb.AppendLine(String.Format("{0}: ({1} {2})", Cuts.Length, Cuts[Cuts.Length - 1], "+Inf"));
            }
            return sb.ToString();
        }        
        #endregion
    }
}
