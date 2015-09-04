using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Infovision.MRI.DAL
{
    public class ImageNeighbour : MiningObjectViewModel
    {
        private BindingList<MiningObjectDisplay> maskAvailable = new BindingList<MiningObjectDisplay>();
        private BindingList<MiningObjectDisplay> labelsAvailable = new BindingList<MiningObjectDisplay>();

        public ImageNeighbour()
            : base()
        {
        }

        public ImageNeighbour(IMiningObject mask, IMiningObject labels)
            : this()
        {
            this.Mask = mask;
            this.Labels = labels;
        }

        public IMiningObject Mask
        {
            get;
            set;
        }

        public IMiningObject Labels
        {
            get;
            set;
        }

        public BindingList<MiningObjectDisplay> MaskAvailable
        {
            get { return this.maskAvailable; }
            private set { this.maskAvailable = value; }
        }

        public BindingList<MiningObjectDisplay> LabelsAvailable
        {
            get { return this.labelsAvailable; }
            private set { this.labelsAvailable = value; }
        }

        public override Type GetMiningObjectType()
        {
            return typeof(MiningObjectNeighbour);
        }

        public void AddMask(IMiningObject mask)
        {
            this.maskAvailable.Add(new MiningObjectDisplay(mask));
        }

        public void AddLabels(IMiningObject labels)
        {
            this.labelsAvailable.Add(new MiningObjectDisplay(labels));
        }

        public IMiningObject GetSelectedLabel(long id)
        {
            List<MiningObjectDisplay> labelList = this.labelsAvailable.ToList<MiningObjectDisplay>();
            MiningObjectDisplay selectedObject = labelList.Find(o => o.Id == id);
            return selectedObject.MiningObject;
        }

        public IMiningObject GetSelectedMask(long id)
        {
            List<MiningObjectDisplay> maskList = this.maskAvailable.ToList<MiningObjectDisplay>();
            MiningObjectDisplay selectedObject = maskList.Find(o => o.Id == id);
            return selectedObject.MiningObject;
        }
    }
}
