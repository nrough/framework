using System;

namespace Raccoon.MRI.DAL
{
    [Serializable]
    public class MiningObjectDisplay
    {
        public long Id
        {
            get
            {
                return this.MiningObject.Id;
            }
        }

        public string Name
        {
            get
            {
                return this.MiningObject.Name;
            }
        }

        public string DisplayName
        {
            get
            {
                return String.Format("{0} ({1})", this.Name, this.Id);
            }
        }

        public IMiningObject MiningObject
        {
            get;
            private set;
        }

        private MiningObjectDisplay()
        {
        }

        public MiningObjectDisplay(IMiningObject miningObject)
            : this()
        {
            this.MiningObject = miningObject;
        }
    }
}