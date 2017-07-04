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
using NRough.Core;
using NRough.Core.Helpers;

namespace NRough.MRI
{
    [Serializable]
    public class MRIMaskItem
    {
        public MRIMaskItem()
        {
        }

        public MRIMaskItem(int labelValue, int radius)
            : this()
        {
            this.LabelValue = labelValue;
            this.Radius = radius;
        }

        public int LabelValue
        {
            get;
            set;
        }

        public int Radius
        {
            get;
            set;
        }

        public string DisplayValue
        {
            get { return this.ToString(); }
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode<int, int>(this.LabelValue, this.Radius);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            MRIMaskItem item = obj as MRIMaskItem;
            if (item == null)
                return false;

            return this.LabelValue == item.LabelValue
                && this.Radius == item.Radius;
        }

        public override string ToString()
        {
            return String.Format("{0}; {1}", this.LabelValue, this.Radius);
        }
    }
}