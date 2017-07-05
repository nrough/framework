using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NRough.Data.Pivot
{
    //https://github.com/mcarmenjc/ClientPivoting
    //TODO PivotService: Add aggregate functions
    //TODO Add filtering
    public class PivotService
    {
        private DataColumn[] GetPivotedColumns(DataTable data, DataColumn pivotColumn, DataColumn[] pivotingColumns, string separator)
        {
            string[] pivotingValues = data.AsEnumerable().Select(x => x[pivotColumn].ToString()).Distinct().ToArray();
            IList<DataColumn> resultColumns = new List<DataColumn>();

            foreach (string value in pivotingValues)
            {
                AddPivotedColumn(pivotingColumns, separator, resultColumns, value);
            }

            if (!resultColumns.Any())
            {
                AddPivotedColumn(pivotingColumns, separator, resultColumns, "1");
            }

            return resultColumns.ToArray();
        }

        private static void AddPivotedColumn(DataColumn[] pivotingColumns, string separator, IList<DataColumn> resultColumns, string value)
        {
            foreach (DataColumn column in pivotingColumns)
            {
                resultColumns.Add(new DataColumn(GetPivotedColumnName(separator, value, column), column.DataType));
            }
        }

        private static string GetPivotedColumnName(string separator, string value, DataColumn column)
        {
            return string.Format("{0}{1}{2}", value, separator, column.ColumnName);
        }

        public DataTable Pivot(DataTable data, DataColumn pivotColumn, DataColumn[] pivotColumns, string separator)
        {
            DataColumn[] fixedColumns = GetFixedColumns(data, pivotColumn, pivotColumns);
            DataColumn[] pivotedColumns = GetPivotedColumns(data, pivotColumn, pivotColumns, separator);
            DataTable result = CreateResultDataTable(data, fixedColumns, pivotedColumns);
            AddRowsToResult(data, pivotColumn, pivotColumns, separator, fixedColumns, result);
            return result;
        }

        private void AddRowsToResult(DataTable data, DataColumn pivotColumn, DataColumn[] pivotColumns, string separator, DataColumn[] fixedColumns, DataTable result)
        {
            List<DataColumn> fixedColumnsList = fixedColumns.ToList();
            foreach (DataRow row in data.Rows)
            {
                DataRow resultRow = result.AsEnumerable().FirstOrDefault(r => fixedColumnsList.TrueForAll(c => row[c.ColumnName].ToString() == r[c.ColumnName].ToString()));

                if (resultRow == null)
                {
                    resultRow = GetNewRow(result, fixedColumns, row);
                    result.Rows.Add(resultRow);
                }

                string pivotValue = row[pivotColumn].ToString();
                foreach (DataColumn column in pivotColumns)
                {
                    string columnName = GetPivotedColumnName(separator, pivotValue, column);
                    resultRow[columnName] = row[column];
                }
            }
        }

        private static DataTable CreateResultDataTable(DataTable data, DataColumn[] fixedColumns, DataColumn[] pivotedColumns)
        {
            DataTable result = new DataTable(data.TableName);
            AddColumnsToDataTable(fixedColumns, result);
            AddColumnsToDataTable(pivotedColumns, result);
            return result;
        }

        private DataRow GetNewRow(DataTable result, DataColumn[] fixedColumns, DataRow row)
        {
            DataRow newRow = result.NewRow();
            foreach (string columnName in fixedColumns.Select(x => x.ColumnName))
            {
                newRow[columnName] = row[columnName];
            }
            return newRow;
        }

        private DataRow[] GetFixedRows(DataTable data, DataColumn[] fixedColumns)
        {
            return data.DefaultView.ToTable(true, fixedColumns.Select(x => x.ColumnName).ToArray()).AsEnumerable().ToArray();
        }

        private static void AddColumnsToDataTable(DataColumn[] fixedColumns, DataTable result)
        {
            foreach (DataColumn column in fixedColumns)
                result.Columns.Add(column);
        }

        private static DataColumn[] GetFixedColumns(DataTable data, DataColumn pivotColumn, DataColumn[] pivotColumns)
        {
            DataColumn[] fixedColumns = data.Columns.Cast<DataColumn>()
                .Where(c => !pivotColumns.Any(x => x.ColumnName == c.ColumnName) && c.ColumnName != pivotColumn.ColumnName)
                .Select(c => new DataColumn(c.ColumnName, c.DataType))
                .ToArray();
            return fixedColumns;
        }
    }
}