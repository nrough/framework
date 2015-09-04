using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infovision.MRI.DAL
{
    public class Histogram : MiningObjectViewModel
    {
        private int sliceFrom;
        private int sliceTo;
        private int bucketSize = 4;
        
        public Histogram()
            : base()
        {
        }

        public IImage Image { get; set; }
        
        public int SliceFrom
        {
            get { return this.sliceFrom; }
            set { SetField(ref this.sliceFrom, value, () => SliceFrom); }
        }

        public int SliceTo
        {
            get { return this.sliceTo; }
            set { SetField(ref this.sliceTo, value, () => SliceTo); }
        }

        public int BucketSize
        {
            get { return this.bucketSize; }
            set { SetField(ref this.bucketSize, value, () => BucketSize); }
        }

        public override Type GetMiningObjectType()
        {
            return typeof(MiningObjectImageHistogram);
        }
    }
}
