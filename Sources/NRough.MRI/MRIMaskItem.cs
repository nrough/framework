using System;
using NRough.Core;
using NRough.Core.Helpers;

namespace NRough.MRI
{
    [Serializable]
    public class MRIMaskItem
    {
        public MRIMaskItem()
        {
        }

        public MRIMaskItem(int labelValue, int radius)
            : this()
        {
            this.LabelValue = labelValue;
            this.Radius = radius;
        }

        public int LabelValue
        {
            get;
            set;
        }

        public int Radius
        {
            get;
            set;
        }

        public string DisplayValue
        {
            get { return this.ToString(); }
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode<int, int>(this.LabelValue, this.Radius);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            MRIMaskItem item = obj as MRIMaskItem;
            if (item == null)
                return false;

            return this.LabelValue == item.LabelValue
                && this.Radius == item.Radius;
        }

        public override string ToString()
        {
            return String.Format("{0}; {1}", this.LabelValue, this.Radius);
        }
    }
}