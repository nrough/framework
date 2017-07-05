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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core.Data
{
    public static class DataTableExtensions
    {
        public static DataTable Dumb(this DataTable dataTable, string filePath, string separator, bool includeHeader = false)
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
            return dataTable;
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

        public static DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            DataTable table = new DataTable();
            //special handling for value types and string
            if (typeof(T).IsValueType || typeof(T).Equals(typeof(string)))
            {
                DataColumn dc = new DataColumn("Value");
                table.Columns.Add(dc);
                foreach (T item in data)
                {
                    DataRow dr = table.NewRow();
                    dr[0] = item;
                    table.Rows.Add(dr);
                }
            }
            else
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
                foreach (PropertyDescriptor prop in properties)
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        try
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }
                        catch// (Exception ex)
                        {
                            row[prop.Name] = DBNull.Value;
                        }
                    }

                    table.Rows.Add(row);
                }
            }
            return table;
        }

        public static DataTable ToDataTable(this IQueryable items)
        {
            Type type = items.ElementType;

            // Create the result table, and gather all properties of a type        
            DataTable table = new DataTable(type.Name);

            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Add the properties as columns to the datatable
            foreach (PropertyInfo prop in props)
            {
                Type propType = prop.PropertyType;

                // Is it a nullable type? Get the underlying type 
                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propType = Nullable.GetUnderlyingType(propType);
                }

                table.Columns.Add(prop.Name, propType);
            }

            // Add the property values as rows to the datatable
            foreach (object item in items)
            {
                var values = new object[props.Length];

                if (item != null)
                {
                    for (var i = 0; i < props.Length; i++)
                    {
                        values[i] = props[i].GetValue(item, null);
                    }
                }

                table.Rows.Add(values);
            }

            return table;
        }

        public static DataTable ToDataTable(this IEnumerable items)
        {
            // Create the result table, and gather all properties of a type        
            DataTable table = new DataTable();

            PropertyInfo[] props = null;

            // Add the property values as rows to the datatable
            foreach (object item in items)
            {
                if (props == null && item != null)
                {
                    Type type = item.GetType();
                    props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    // Add the properties as columns to the datatable
                    foreach (PropertyInfo prop in props)
                    {
                        Type propType = prop.PropertyType;

                        // Is it a nullable type? Get the underlying type 
                        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            propType = Nullable.GetUnderlyingType(propType);
                        }

                        table.Columns.Add(prop.Name, propType);
                    }
                }

                // When the column headers are defined, all the rows have
                // their number of columns "fixed" to the right number
                var values = new object[props != null ? props.Length : 0];

                if (item != null)
                {
                    for (var i = 0; i < props.Length; i++)
                    {
                        values[i] = props[i].GetValue(item, null);
                    }
                }

                table.Rows.Add(values);
            }

            return table;
        }

        public static IEnumerable[] Columns(this DataTable dt)
        {
            IEnumerable[] columns = new IEnumerable[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                switch (Type.GetTypeCode(dt.Columns[i].DataType))
                {
                    case TypeCode.String:
                        columns[i] = dt.Rows.Cast<DataRow>().Select(row => row.Field<string>(i)).ToArray();
                        break;

                    case TypeCode.Double:
                        columns[i] = dt.Rows.Cast<DataRow>().Select(row => row.Field<double>(i)).ToArray();
                        break;

                    case TypeCode.Int32:
                        columns[i] = dt.Rows.Cast<DataRow>().Select(row => row.Field<int>(i)).ToArray();
                        break;

                    case TypeCode.Int64:
                        columns[i] = dt.Rows.Cast<DataRow>().Select(row => row.Field<long>(i)).ToArray();
                        break;

                    default:
                        //columns[i] = dt.Rows.Cast<DataRow>().Select(row => row[i]).ToArray();
                        throw new InvalidOperationException(String.Format("Type {0} is not supported", dt.Columns[i].DataType.Name));
                }
            }
            return columns;
        }

        public static string[] ColumnNames(this DataTable dt)
        {
            return dt.Columns
                .Cast<DataColumn>()
                .Select(x => x.ColumnName)
                .ToArray();
        }

        /// <summary>
        /// Changes the datatype of a column. More specifically it creates a new one and transfers the data to it
        /// http://stackoverflow.com/questions/9028029/how-to-change-datatype-of-a-datacolumn-in-a-datatable
        /// </summary>
        /// <param name="column">The source column</param>
        /// <param name="type">The target type</param>
        /// <param name="parser">A lambda function for converting the value</param>
        public static void ChangeType(this DataColumn column, Type type, Func<object, object> parser)
        {
            //no table? just switch the type
            if (column.Table == null)
            {
                column.DataType = type;
                return;
            }

            //clone our table
            DataTable clonedtable = column.Table.Clone();

            //get our cloned column
            DataColumn clonedcolumn = clonedtable.Columns[column.ColumnName];

            //remove from our cloned table
            clonedtable.Columns.Remove(clonedcolumn);

            //change the data type
            clonedcolumn.DataType = type;

            //change our name
            clonedcolumn.ColumnName = Guid.NewGuid().ToString();

            //add our cloned column
            column.Table.Columns.Add(clonedcolumn);

            //interpret our rows
            foreach (DataRow drRow in column.Table.Rows)
            {
                drRow[clonedcolumn] = parser(drRow[column]);
            }

            //remove our original column
            column.Table.Columns.Remove(column);

            //change our name
            clonedcolumn.ColumnName = column.ColumnName;
        }

        public static void DeleteRows(this DataTable dt, Func<DataRow, bool> predicate)
        {
            foreach (var row in dt.Rows.Cast<DataRow>().Where(predicate).ToList())
                row.Delete();
        }

        public static void DeleteColumn(this DataTable dt, string columnName, bool throwWhenColumnNotExists = false)
        {
            if (throwWhenColumnNotExists)
            {
                dt.Columns.Remove(columnName);
            }
            else if(dt.Columns.Contains(columnName))
            {
                dt.Columns.Remove(columnName);
            }
        }
    }
}
