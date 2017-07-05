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

using NRough.Core;
using NRough.MachineLearning.Roughsets;
using NRough.MachineLearning.Roughsets.Reducts.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification.DecisionTrees
{
    public static class DecisionTreeAttributeSelection
    {
        public static int[] ApproximateReductAttributeSelection(LocalDataStoreSlot data, int[] attributes)
        {
            Args parms = new Args(4);
            parms.SetParameter(ReductFactoryOptions.DecisionTable, data);
            parms.SetParameter(ReductFactoryOptions.ReductType, ReductTypes.ApproximateReductMajority);
            parms.SetParameter(ReductFactoryOptions.Epsilon, 0.01 );
            parms.SetParameter(ReductFactoryOptions.NumberOfReducts, 100);

            IReductGenerator generator = ReductFactory.GetReductGenerator(parms);
            generator.Run(); 

            var reducts = generator.GetReducts();
            reducts.Sort(ReductAccuracyComparer.Default);
            IReduct bestReduct = reducts.FirstOrDefault();
             
            return bestReduct.Attributes.ToArray();
        }

        public static int[] ObliviousTreeAttributeRanking(LocalDataStoreSlot data, int[] attributes)
        {
            return attributes;
        }
    }
}
