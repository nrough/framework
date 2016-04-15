using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiscUtil;

namespace Infovision.Datamining.Filters.Unsupervised.Attribute
{
    public class BinaryDiscretization<T>
        where T : struct, IComparable, IFormattable, IComparable<T>, IEquatable<T>
    {
        private double[] cuts;

        public double[] Cuts
        {
            get { return cuts; }
            set { cuts = value; }
        }

        public BinaryDiscretization(double splitPoint)
        {
            this.cuts = new double[1];
            this.cuts[0] = splitPoint;
        }

        public int Search(T value)
        {
            if (this.cuts == null)
                return 0;

            for (int i = 0; i < cuts.Length; i++)
                if (Operator.Convert<T, double>(value).CompareTo(cuts[i]) <= 0)
                    return i;

            return cuts.Length;
        }

        public int[] Discretize(T[] values)
        {
            int[] result = new int[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = this.Search(values[i]);
            return result;
        }
        
    }
}
