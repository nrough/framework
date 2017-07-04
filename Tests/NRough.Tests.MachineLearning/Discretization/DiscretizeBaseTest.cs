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
using NRough.MachineLearning.Discretization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.MachineLearning.Discretization
{
    public abstract class DiscretizeBaseTest
    {
        protected long[] data = {
                        10, 12, 13, 14, 16, 40, 41, 42,
                        5, 5, 5, 5, 5, 7, 8, 9,
                        43, 44, 45, 45, 46, 47, 48, 49 };

        protected long[] dataNotExisting = { 0, 1, 2, 3, 4, 5, 6,
                                    11, 15, 16, 17, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34,
                                    50, 60, 70, 80 };

        public abstract IDiscretizer GetDiscretizer();

        public void ShowInfo(IDiscretizer discretizer, long[] dataExisting, long[] dataNotExisting)
        {
            Console.WriteLine(discretizer.ToString());

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", data[i], discretizer.Apply(dataExisting[i]));

            for (int i = 0; i < data.Length; i++)
                Console.WriteLine("{0} {1}", dataNotExisting[i], discretizer.Apply(dataNotExisting[i]));
        }
    }
}
