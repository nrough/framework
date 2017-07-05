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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Writers
{
    public class DataTableLatexTabularFormatter : IFormatProvider, ICustomFormatter
    {        
        Dictionary<Tuple<int, int>, Dictionary<string, string>> cellsProperties;
        Dictionary<int, Dictionary<string, string>> rowProperties;

        public string Caption { get; set; }
        public string Label { get; set; }
        public string CustomHeader { get; set; }
        public string CustomFooter { get; set; }
        public string ExtraRow { get; set; }

        public DataTableLatexTabularFormatter()
        {
            Caption = "";
            Label = "";
            ExtraRow = null;

            CreateCellsProperty();
        }

        public virtual object GetFormat(Type formatType)
        {
            return formatType.IsAssignableFrom(typeof(DataTable)) ? this : null;
        }

        public virtual string Format(string format, object args, IFormatProvider formatProvider)
        {
            DataTable dt = args as DataTable;
            if (dt == null)
                return args.ToString();                        

            StringBuilder sb = new StringBuilder();
            AddHeader(dt, sb);            
            AddRows(dt, sb);
            AddFooter(dt, sb);
            return sb.ToString();
        }

        private bool IsRowActive(int row)
        {
            
            if (rowProperties.ContainsKey(row))
                if (rowProperties[row].ContainsKey("active"))
                    if(rowProperties[row]["active"].ToLower() == "false")
                        return false;

            return true;
        }

        private void AddRows(DataTable dt, StringBuilder sb)
        {
            bool isNonActiveRow = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (!IsRowActive(i))
                {
                    if (isNonActiveRow == false && !String.IsNullOrEmpty(ExtraRow))
                    {
                        sb.AppendLine(ExtraRow);
                        isNonActiveRow = true;
                    }
                    continue;
                }
                else
                {
                    isNonActiveRow = false;
                }

                var datarow = dt.Rows[i];

                int k = 0;
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    var columnProperties = dt.Columns[j].ExtendedProperties;

                    if (columnProperties.ContainsKey("active"))
                        if ((bool)columnProperties["active"] == false)
                            continue;

                    string cellValue;
                    if (dt.Columns[j].DataType.GetInterface("IFormattable") != null
                        & columnProperties.ContainsKey("format"))
                    {
                        string format = (string)columnProperties["format"];
                        IFormatProvider formatProvider = null;
                        if (columnProperties.ContainsKey("formatProvider"))
                            formatProvider = (IFormatProvider)columnProperties["formatProvider"];

                        cellValue = datarow.Field<IFormattable>(j).ToString(
                                format, formatProvider);
                    }
                    else
                    {
                        cellValue = datarow[j].ToString();
                    }                    

                    cellValue = ApplyFontFace(j, i, cellValue);

                    if(k > 0)
                        sb.Append(" & ");

                    sb.Append(cellValue);
                    k++;                                            
                }
                sb.AppendLine(@"\\");
            }            
        }        

        private string ApplyFontFace(int col, int row, string value)
        {
            string fontface = GetCellProperty(col, row, "fontface");
            if (String.IsNullOrEmpty(fontface))
                return value;
            return String.Format(@"\{0}{{{1}}}", fontface, value);
        }

        private void AddColumnNames(DataTable dt, StringBuilder sb)
        {
            bool first = true;
            for(int i=0; i<dt.Columns.Count; i++)
            {
                if (!first)
                    sb.Append(" & ");
                first = false;

                sb.Append(String.Format("\\rot{{{0}}}", dt.Columns[i].ColumnName));
            }
            sb.AppendLine(@"\\ \hline");
        }

        private void AddHeader(DataTable dt, StringBuilder sb)
        {
            if (CustomHeader != null)
            {
                sb.AppendLine(CustomHeader);
                return;
            }
                
            sb.Append(String.Format(
                @"\begin{{table}}[ht]
\centering
\caption{{{0}}}
\label{{{1}}}
\scriptsize
\begin{{tabular}}{{{2}}}
\hline{3}", 
                this.Caption, this.Label, ColumnAlign(dt), Environment.NewLine));

            AddColumnNames(dt, sb);
        }

        public string ColumnAlign(DataTable dt)
        {            
            return "|" + String.Concat(Enumerable.Repeat("l|", dt.Columns.Count));
        }

        private void AddFooter(DataTable dt, StringBuilder sb)
        {
            if (CustomFooter != null)
            {
                sb.AppendLine(CustomFooter);
                return;
            }

            sb.Append(String.Format(
                @"\end{{tabular}}
\end{{table}}{0}", 
                Environment.NewLine));		
        }

        private void CreateCellsProperty()
        {
            cellsProperties = new Dictionary<Tuple<int, int>, Dictionary<string, string>>();
            rowProperties = new Dictionary<int, Dictionary<string, string>>();
        }

        public void ResetCellProperties()
        {
            CreateCellsProperty();
        }

        public void SetRowProperty(int row, string propertyName, string propertyValue)
        {
            Dictionary<string, string> rowProp;            
            if (rowProperties.TryGetValue(row, out rowProp))
            {
                if (rowProp.ContainsKey(propertyName))
                    rowProp[propertyName] = propertyValue;
                else
                    rowProp.Add(propertyName, propertyValue);
            }
            else
            {
                rowProp = new Dictionary<string, string>();
                rowProp.Add(propertyName, propertyValue);
                rowProperties.Add(row, rowProp);
            }
        }

        public void SetCellProperty(int col, int row, string propertyName, string propertyValue)
        {
            Dictionary<string, string> cellProp;
            var cellId = new Tuple<int, int>(col, row);
            if (cellsProperties.TryGetValue(cellId, out cellProp))
            {                
                if (cellProp.ContainsKey(propertyName))
                    cellProp[propertyName] = propertyValue;
                else
                    cellProp.Add(propertyName, propertyValue);
            }
            else
            {
                cellProp = new Dictionary<string, string>();
                cellProp.Add(propertyName, propertyValue);
                cellsProperties.Add(cellId, cellProp);
            }
        }

        public string GetRowProperty(int row, string propertyName)
        {
            Dictionary<string, string> rowProp;            
            if (rowProperties.TryGetValue(row, out rowProp))
            {
                string result;
                if (rowProp.TryGetValue(propertyName, out result))
                    return result;
            }
            return null;
        }

        public string GetCellProperty(int col, int row, string propertyName)
        {
            Dictionary<string, string> cellProp;
            var cellId = new Tuple<int, int>(col, row);            
            if (cellsProperties.TryGetValue(cellId, out cellProp))
            {
                string result;
                if (cellProp.TryGetValue(propertyName, out result))
                    return result;
            }
            return null;
        }
    }
}
