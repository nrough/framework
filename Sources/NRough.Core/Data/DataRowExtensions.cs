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
