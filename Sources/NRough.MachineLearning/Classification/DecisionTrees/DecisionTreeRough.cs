// 
// NRough Framework
// http://www.nrough.net
// 
// Copyright (c) Sebastian Widz, 2017
// authors at nrough.net
// 
// This file is part of NRough Framework.
// 
// NRough Framework is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
// 
// NRough Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with NRough Framework; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Linq;
using System.Collections.Generic;
using NRough.Math;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    /// <summary>
    /// Decision tree learning where in each node a split is selected based on attribute generating highest measure M. 
    /// Measure M is calculated based on object weights.
    /// </summary>
    public class DecisionTreeRough : DecisionTreeBase 
    {
        public DecisionTreeRough()
            : base()
        {
            this.ImpurityFunction = ImpurityMeasure.Majority2;
            //this.ImpurityNormalize = ImpurityMeasure.SplitInformationNormalize;
        }

        public DecisionTreeRough(string modelName)
            : base(modelName)
        {
            this.ImpurityFunction = ImpurityMeasure.Majority2;
            //this.ImpurityNormalize = ImpurityMeasure.SplitInformationNormalize;
        }               
    }
}
