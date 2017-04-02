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

        public string Caption { get; set; }
        public string Label { get; set; }
        public string CustomHeader { get; set; }
        public string CustomFooter { get; set; }        

        public DataTableLatexTabularFormatter()
        {
            Caption = "";
            Label = "";
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

        private void AddRows(DataTable dt, StringBuilder sb)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var datarow = dt.Rows[i];
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string cellValue;
                    if (dt.Columns[j].DataType.GetInterface("IFormattable") != null
                        & dt.Columns[j].ExtendedProperties.ContainsKey("format"))
                    {
                        string format = (string)dt.Columns[j].ExtendedProperties["format"];
                        IFormatProvider formatProvider = null;
                        if (dt.Columns[j].ExtendedProperties.ContainsKey("formatProvider"))
                            formatProvider = (IFormatProvider)dt.Columns[j].ExtendedProperties["formatProvider"];

                        cellValue = datarow.Field<IFormattable>(j).ToString(
                                format, formatProvider);

                    }
                    else
                    {
                        cellValue = datarow[j].ToString();
                    }

                    /*
                    switch (Type.GetTypeCode(dt.Columns[j].DataType))
                    {                        
                        case TypeCode.Double:                            
                            cellValue = datarow.Field<double>(j).ToString(
                                "0.##", System.Globalization.CultureInfo.InvariantCulture);
                            if (String.IsNullOrEmpty(cellValue))
                                cellValue = "0";
                                //System.Globalization.CultureInfo.InvariantCulture));                                
                            break;                        

                        default:
                            cellValue = datarow[j].ToString();
                            break;                            
                    }
                    */

                    cellValue = ApplyFontFace(j, i, cellValue);

                    sb.Append(cellValue);

                    if (j < dt.Columns.Count - 1)
                        sb.Append(" & ");
                }
                sb.AppendLine(@"\\");
            }

            /*
            sb.Append(String.Join(
                String.Format(@"\\ {0}", Environment.NewLine), 
                dt.Rows.OfType<DataRow>()
                    .Select(x => String.Join(" & ", x.ItemArray))));
            sb.AppendLine(@"\\ ");
            */
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
        }

        public void ResetCellProperties()
        {
            CreateCellsProperty();
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
