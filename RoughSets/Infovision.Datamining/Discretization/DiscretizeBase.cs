using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    [Serializable]
    public abstract class DiscretizeBase : IDiscretization
    {
        #region Members

        private long[] cuts;
        private int[] sortedIndices;

        #endregion

        #region Properties

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

        protected ReadOnlyCollection<int> SortedIndices
        {
            get { return Array.AsReadOnly(this.sortedIndices); }
        }

        public bool IsDataSorted { get; set; }

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

        public long[] Apply(long[] data)
        {
            long[] result = new long[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = Apply(data[i]);
            return result;
        }

        public long Apply(long value)
        {
            if (Cuts == null)
                throw new NullReferenceException("Cuts == null");
            for (int i = 0; i < Cuts.Length; i++)
                if (value <= Cuts[i])
                    return i;
            return Cuts.Length;
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
            for(int i=start; i < labels.Length && i < end; i++)
                if (result.ContainsKey(labels[sortedIndices[i]]))
                    result[labels[sortedIndices[i]]] += (weights == null) ? 1.0 : weights[sortedIndices[i]];
                else
                    result.Add(labels[sortedIndices[i]], (weights == null) ? 1.0 : weights[sortedIndices[i]]);
            return result;            
        }

        private bool ValidateCuts(long[] cuts)
        {
            for (int i = 1; i < cuts.Length; i++)
                if (cuts[i] <= cuts[i - 1])
                    return false;
            return true;
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
                sb.AppendLine(String.Format("{0}: <{1} {2})", 0, "-Inf", Cuts[0]));
                for (int i = 1; i < Cuts.Length; i++)
                    sb.AppendLine(String.Format("{0}: <{1} {2})", i, Cuts[i - 1], Cuts[i]));
                sb.AppendLine(String.Format("{0}: <{1} {2})", Cuts.Length, Cuts[Cuts.Length - 1], "+Inf"));
            }
            return sb.ToString();
        }
        #endregion
    }
}
