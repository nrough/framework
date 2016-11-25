using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils.Data
{
    public static class DataTableExtensions
    {
        public static void WriteToCSVFile(this DataTable dataTable, string filePath, string separator, bool includeHeader = false)
        {
            StringBuilder sb = new StringBuilder();
            if (includeHeader)
            {
                IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName);
                sb.AppendLine(string.Join(separator, columnNames));
            }

            foreach (DataRow row in dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(separator, fields));
            }

            System.IO.File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        ///   Returns a subtable extracted from the current table.
        /// </summary>
        ///
        /// <param name="source">The table to return the subtable from.</param>
        /// <param name="indexes">Array of indices.</param>
        ///
        public static DataTable Subtable(this DataTable source, int[] indexes)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (indexes == null)
                throw new ArgumentNullException("indexes");

            DataTable destination = source.Clone();
            foreach (int i in indexes)
            {
                DataRow row = source.Rows[i];
                destination.ImportRow(row);
            }
            return destination;
        }
    }
}
