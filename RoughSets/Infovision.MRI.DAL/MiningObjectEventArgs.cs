using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infovision.MRI.DAL
{
    public class MiningObjectEventArgs : EventArgs
    {

        public MiningObjectEventArgs()
            : base()
        {
        }

        public MiningObjectEventArgs(IMiningObject miningObject)
            : this()
        {
            this.MiningObject = miningObject;
        }
        
        public IMiningObject MiningObject
        {
            get;
            protected set;
        }
        
        public static new MiningObjectEventArgs Empty
        {
            get
            {
                return new MiningObjectEventArgs();
            }
        }
    }
}
