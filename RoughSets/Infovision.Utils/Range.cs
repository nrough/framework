using System;
using System.Text;

namespace Infovision.Utils
{
    [Serializable]
    public class Range<T>
        where T : struct, IComparable, IFormattable, IComparable<T>, IEquatable<T>
    {
        public Range(T lowerBound, T upperBound)
        {
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
        }

        #region Properties

        public T LowerBound
        {
            get;
            private set;
        }

        public T UpperBound
        {
            get;
            private set;
        }

        #endregion Properties        

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.Append(this.LowerBound);
            stringBuilder.Append(", ");
            stringBuilder.Append(this.UpperBound);
            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode(this.LowerBound, this.UpperBound);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Range<T> range = obj as Range<T>;
            if (range == null)
                return false;

            if (this.LowerBound.Equals(range.LowerBound) 
                && this.UpperBound.Equals(range.UpperBound))
                return true;

            return false;
        }
    }
}