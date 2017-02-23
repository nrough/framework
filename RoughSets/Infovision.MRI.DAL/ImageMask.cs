using System;
using System.Collections.Generic;

namespace NRough.MRI.DAL
{
    public class ImageMask : MiningObjectViewModel, IMiningObjectViewImage
    {
        private List<MRIMaskItem> maskItems = new List<MRIMaskItem>();
        private IImage image;

        public IEnumerable<MRIMaskItem> MaskItems
        {
            get { return this.maskItems; }
            set { this.maskItems = new List<MRIMaskItem>(value); }
        }

        public IImage Image
        {
            get { return this.image; }
            set { this.image = value; }
        }

        public override Type GetMiningObjectType()
        {
            return typeof(MiningObjectImageMask);
        }

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
            if (!maskItems.Contains(item))
            {
                maskItems.Add(item);
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
                    return maskItems.Remove(localItem);
                }
            }
            catch
            {
            }

            return false;
        }
    }
}