//
// NRough Framework
// http://www.nrough.net
//
// Copyright © Sebastian Widz, 2017
// authors at nrough.net

//This file is part of NRough.

//Foobar is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 3 of the License, or
//(at your option) any later version.
//
//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//
using System;

namespace NRough.MachineLearning.Roughsets
{
    public static class ReductEnsembleDistanceHelper
    {
        /// <summary>
        /// |X| + |Y| - alpha * |X && Y|
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static double ReductSim(double[] vec1, double[] vec2, double alpha)
        {
            return ReductEnsembleDistanceHelper.ReductSimDelegate(alpha).Invoke(vec1, vec2);
        }

        public static Func<double[], double[], double> ReductSimDelegate(double alpha)
        {
            Func<double[], double[], double> tverskyDistance = (p, v) =>
            {
                int[] assoc = NRough.Math.Distance.BinaryAssociation(p, v);
                int a = assoc[0];
                int b = assoc[1];
                int c = assoc[2];

                return (a + c) + (b + c) - (alpha * c);
            };

            return tverskyDistance;
        }
    }
}