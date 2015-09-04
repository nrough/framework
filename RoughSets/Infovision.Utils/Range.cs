using System;
using System.Text;

namespace Infovision.Utils
{
    [Serializable]
    public class Range
    {
        public Range(Double lowerBound, Double upperBound)
        {
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
        }

        #region Properties

        public Double LowerBound
        {
            get;
            private set;
        }

        public Double UpperBound
        {
            get;
            private set;
        }

        public Double Length
        {
            get { return this.UpperBound - this.LowerBound; }
        }

        #endregion

        #region System.Object

        public override String ToString()
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

            Range range = obj as Range;
            if (range == null)
                return false;

            if (this.LowerBound != range.LowerBound
                || this.UpperBound != range.UpperBound)
                return false;

            return true;
        }

        #endregion
    }
}
