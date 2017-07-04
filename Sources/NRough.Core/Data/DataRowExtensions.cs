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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core.Data
{
    public static class DataRowExtensions
    {
        public static string ToStr(this DataRow row, string separator)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < row.Table.Columns.Count; i++)
            {
                sb.Append(row[i].ToString());
                if(i != row.Table.Columns.Count - 1)
                    sb.Append(separator);
            }
            return sb.ToString();
        }

        public static string ToStr(this DataRow row, string[] columnNames, string separator)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < columnNames.Length; i++)
            {
                if (!String.IsNullOrEmpty(columnNames[i]))
                {
                    if (row.Table.Columns.Contains(columnNames[i]))
                    {
                        if(row[columnNames[i]] is double)
                            sb.Append(((double)row[columnNames[i]]).ToString(
                                    "0.##", System.Globalization.CultureInfo.InvariantCulture));
                        else
                            sb.Append(((double)row[columnNames[i]]).ToString());

                        if (i != columnNames.Length - 1)
                            sb.Append(separator);
                    }
                }
            }
            return sb.ToString();
        }
    }
}
