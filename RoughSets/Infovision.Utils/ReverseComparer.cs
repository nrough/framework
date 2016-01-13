using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{
    public sealed class ReverseComparer<T> : IComparer<T>
    {
        private readonly IComparer<T> inner;
        public ReverseComparer() : this(null) { }
        public ReverseComparer(IComparer<T> inner)
        {
            this.inner = inner ?? Comparer<T>.Default;
        }
        int IComparer<T>.Compare(T x, T y) { return inner.Compare(y, x); }
    }
}
