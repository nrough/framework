using Infovision.Core.Data;
using RDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Datamining.RCode
{
    public static class RProxy
    {        
        public static void PlotResult(DataTable dt, string xField, string yField, 
            string groupVertical = "", string groupHorizontal = "", string yMaxField = "", string yMinField = "", 
            string title = "", double yLimitLow = 0.0, double yLimitHigh = 1.0, bool stringsAsFactors = false)
        {
            if (dt == null)
                throw new ArgumentNullException("dt");
            if (String.IsNullOrEmpty(xField))
                throw new ArgumentNullException("xField");
            if (String.IsNullOrEmpty(yField))
                throw new ArgumentNullException("yField");
            if (!dt.Columns.Contains(xField))
                throw new ArgumentException("!dt.Columns.Contains(xField)", "xField");
            if (!dt.Columns.Contains(yField))
                throw new ArgumentException("!dt.Columns.Contains(yField)", "yField");
            if (!String.IsNullOrEmpty(groupVertical) && !dt.Columns.Contains(groupVertical))
                throw new ArgumentException("groupVertical");
            if (!String.IsNullOrEmpty(groupHorizontal) && !dt.Columns.Contains(groupHorizontal))
                throw new ArgumentException("groupHorizontal");
            if (!String.IsNullOrEmpty(yMaxField) && !dt.Columns.Contains(yMaxField))
                throw new ArgumentException("yMaxField");
            if (!String.IsNullOrEmpty(yMinField) && !dt.Columns.Contains(yMinField))
                throw new ArgumentException("yMinField");

            REngine e = REngine.GetInstance();
            e.Evaluate(File.ReadAllText(@"RCode\plot-results.R"));
                                            
            DataFrame df = e.CreateDataFrame(columns: dt.Columns(),
                                             columnNames: dt.ColumnNames(),
                                             stringsAsFactors: stringsAsFactors);
            e.SetSymbol("dataset_1", df);

            StringBuilder ev = new StringBuilder();
            ev.AppendFormat(@"p <- plotresult(dt = {0}, xField = ""{1}"", yField = ""{2}"", ", "dataset_1", xField, yField);            
            if(!String.IsNullOrEmpty(groupVertical))
                ev.AppendFormat(@"groupVertical = ""{0}"", ", groupVertical);
            if (!String.IsNullOrEmpty(groupHorizontal))
                ev.AppendFormat(@"groupHorizontal = ""{0}"", ", groupHorizontal);
            if (!String.IsNullOrEmpty(title))
                ev.AppendFormat(@"title = ""{0}"", ", title);
            if (!String.IsNullOrEmpty(yMaxField))
                ev.AppendFormat(@"yMaxField = ""{0}"", ", yMaxField);
            if (!String.IsNullOrEmpty(yMinField))
                ev.AppendFormat(@"yMinField = ""{0}"", ", yMinField);
            ev.AppendFormat(@"yLimitLow = {0}, yLimitHigh = {1})", yLimitLow, yLimitHigh);

            e.Evaluate(ev.ToString());
            e.Evaluate("print(p)");                        
        }

        public static void Pdf(string outputFile, int width = 8, int height = 11)
        {
            REngine e = REngine.GetInstance();
            e.Evaluate(String.Format(@"pdf(file = paste(""{0}"","".pdf"", sep=""""), width={1}, height={2})", outputFile, width, height));
        }

        public static void DevOff()
        {
            REngine e = REngine.GetInstance();
            e.Evaluate(@"dev.off()");
        }        
    }
}
