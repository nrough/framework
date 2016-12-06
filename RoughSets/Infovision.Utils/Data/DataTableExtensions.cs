using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Core.Data
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
    }
}
