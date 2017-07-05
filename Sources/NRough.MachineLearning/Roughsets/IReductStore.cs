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

using System.Collections.Generic;

namespace NRough.MachineLearning.Roughsets
{
    public interface IReductStore : IEnumerable<IReduct>
    {
        int Count { get; }
        double Weight { get; set; }
        bool AllowDuplicates { get; set; }
        bool IsActive { get; set; }

        void AddReduct(IReduct reduct);

        IReduct GetReduct(int index);

        bool IsSuperSet(IReduct reduct);

        IReductStore FilterReducts(int numberOfReducts, IComparer<IReduct> comparer);

        double GetAvgMeasure(IReductMeasure reductMeasure);

        double GetWeightedAvgMeasure(IReductMeasure reductMeasure, bool includeExceptions);
        double GetSumMeasure(IReductMeasure reductMeasure, bool includeExceptions);

        void GetMeanStdDev(IReductMeasure reductMeasure, out double mean, out double stdDev);

        void GetMeanAveDev(IReductMeasure reductMeasure, out double mean, out double aveDev);

        bool Exist(IReduct reduct);

        void Save(string fileName);
    }
}