using System;
using System.ComponentModel;

namespace Raccoon.MRI.DAL
{
    public class ImageMaskItems : BindingList<MRIMaskItem>
    {
        public bool AddMaskItem(int label, int radius)
        {
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException("radius", "Radius must be positive integer key");
            }

            if (label < 0)
            {
                throw new ArgumentOutOfRangeException("label", "Label must be non negative");
            }

            MRIMaskItem item = new MRIMaskItem(label, radius);
            if (!this.Contains(item))
            {
                this.Add(item);
                return true;
            }

            return false;
        }

        public bool RemoveMaskItem(object item)
        {
            try
            {
                MRIMaskItem localItem = item as MRIMaskItem;
                if (localItem != null)
                {
                    return this.Remove(localItem);
                }
            }
            catch
            {
            }

            return false;
        }

        protected override bool SupportsSearchingCore
        {
            get { return true; }
        }

        protected override int FindCore(PropertyDescriptor prop, object key)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (Items[i].LabelValue.Equals(key))
                    return i;
            }
            return -1;
        }
    }
}