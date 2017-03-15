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
        public string Caption { get; set; }
        public string Label { get; set; }
        public string CustomHeader { get; set; }
        public string CustomFooter { get; set; }

        public DataTableLatexTabularFormatter()
        {
            Caption = "";
            Label = "";
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
            AddColumnNames(dt, sb);
            AddRows(dt, sb);
            AddFooter(dt, sb);
            return sb.ToString();
        }

        private void AddRows(DataTable dt, StringBuilder sb)
        {
            sb.Append(String.Join(
                String.Format(@"\\ \hline{0}", Environment.NewLine), 
                dt.Rows.OfType<DataRow>()
                    .Select(x => String.Join(" & ", x.ItemArray))));
            sb.Append(String.Format(@"\\ \hline{0}", Environment.NewLine));
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
            sb.Append(String.Format(@"\\ \hline{0}", Environment.NewLine));
        }

        private void AddHeader(DataTable dt, StringBuilder sb)
        {
            sb.Append(String.Format(
                @"\begin{{table}}[ht]
\centering
\caption{{{0}}}
\label{{{1}}}
\scriptsize
\begin{{tabular}}{{{2}}}
\hline{3}", 
                this.Caption, this.Label, ColumnAlign(dt), Environment.NewLine));
        }

        public string ColumnAlign(DataTable dt)
        {            
            return "|" + String.Concat(Enumerable.Repeat("l|", dt.Columns.Count));
        }

        private void AddFooter(DataTable dt, StringBuilder sb)
        {
            sb.Append(String.Format(
                @"\end{{tabular}}
\end{{table}}{0}", 
                Environment.NewLine));		
        }
    }
}
