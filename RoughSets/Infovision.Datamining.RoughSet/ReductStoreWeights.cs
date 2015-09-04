﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infovision.Datamining.Roughset
{
    public class ReductStoreWeights : ReductStore
    {
        //TODO beeing super set is OK, but weights must be different
        protected override bool CanAddReduct(IReduct reduct)
        {
            if (this.IsSuperSet(reduct))
                return false;
            return true;
        }

        public override bool IsSuperSet(IReduct reduct, bool checkApproxDegree = false)
        {
            return base.IsSuperSet(reduct, checkApproxDegree);
        }
    }
}
