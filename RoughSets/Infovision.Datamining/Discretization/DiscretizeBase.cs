using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.MachineLearning.Discretization
{
    public abstract class DiscretizeBase : IDiscretization
    {
        #region Members

        private long[] cuts;

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
