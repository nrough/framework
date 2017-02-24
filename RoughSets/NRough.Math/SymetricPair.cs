using System;

namespace NRough.Math
{
    [Serializable]
    public struct SymetricPair<T1, T2>
        where T1 : IEquatable<T1>
        where T2 : IEquatable<T2>
    {
        private readonly T1 item1;
        private readonly T2 item2;

        public T1 Item1 { get { return item1; } }
        public T2 Item2 { get { return this.item2; } }

        public SymetricPair(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }

        #region System.Object Methods

        public override string ToString()
        {
            return String.Format("{0} {1}", item1, item2);
        }

        public override int GetHashCode()
        {
            return item1.GetHashCode() ^ item2.GetHashCode();
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            SymetricPair<T1, T2> index;
            if (obj is SymetricPair<T1, T2>)
            {
                index = (SymetricPair<T1, T2>)obj;
                return (this.Item1.Equals(index.Item1) && this.Item2.Equals(index.Item2)) 
                    || (this.Item1.Equals(index.Item2) && this.Item2.Equals(index.Item1));
            }

            return false;
        }
        public static bool operator ==(SymetricPair<T1, T2> a, SymetricPair<T1, T2> b) { return a.Equals(b); }
        public static bool operator !=(SymetricPair<T1, T2> a, SymetricPair<T1, T2> b) { return !a.Equals(b); }

        #endregion System.Object Methods
    }
}