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
using NRough.Core.Data;
using RDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.RCode
{
    public static class RProxy
    {
        public static void PlotResultSimple(DataTable dt, string xField, string yField,
            string groupBy = "", string yMaxField = "", string yMinField = "",
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
            ev.AppendFormat(@"p <- plotresultsimple(dt = {0}, xField = ""{1}"", yField = ""{2}"", ", "dataset_1", xField, yField);

            if (!String.IsNullOrEmpty(groupBy))
                ev.AppendFormat(@"groupBy = ""{0}"", ", groupBy);
            if (!String.IsNullOrEmpty(yMaxField))
                ev.AppendFormat(@"yMaxField = ""{0}"", ", yMaxField);
            if (!String.IsNullOrEmpty(yMinField))
                ev.AppendFormat(@"yMinField = ""{0}"", ", yMinField);
            if (!String.IsNullOrEmpty(title))
                ev.AppendFormat(@"title = ""{0}"", ", title);
                        
            ev.AppendFormat(@"yLimitLow = {0}, yLimitHigh = {1})", yLimitLow, yLimitHigh);

            e.Evaluate(ev.ToString());

            /*
            //->            
            var res = dt.AsEnumerable().AsQueryable().Where(r => r.Field<double>("eps") == -1.0);
            DataTable dummyDt = dt.Clone();
            foreach (DataRow row in res)
                dummyDt.ImportRow(row);

            DataFrame df2 = e.CreateDataFrame(
                columns: dummyDt.Columns(),
                columnNames: dummyDt.ColumnNames(),
                stringsAsFactors: stringsAsFactors);
            e.SetSymbol("dummy", df2);

            e.Evaluate(String.Format(@"p <- p + geom_hline(data = {0}, aes_string(yintercept = ""{1}""), colour=""#990000"", linetype=""dashed"")", "dummy", yField));
            //e.Evaluate(String.Format(@"p <- p + geom_hline(data = {0}, aes_string(yintercept = ""{1}""), colour = ""#770000"", alpha = 0.1)", "dummy", yMinField));
            //e.Evaluate(String.Format(@"p <- p + geom_hline(data = {0}, aes_string(yintercept = ""{1}""), colour = ""#770000"", alpha = 0.1)", "dummy", yMaxField));
            //e.Evaluate(String.Format(@"p <- p + annotate(data = {0}, mapping = 
            //    ""rect"", xmin = -Inf, xmax = Inf, aes_string(ymin = ""{1}"", ymax = ""{2}""), fill = ""#770000"", alpha = .1, color = NA)", "dummy", yMinField, yMaxField));
            e.Evaluate(String.Format(@"p <- p + geom_rect(data = {0}, xmin = -Inf, xmax=Inf, aes_string(ymin = ""{1}"", ymax=""{2}""), fill = ""#770000"", alpha = 0.1, linetype=""dashed"")", "dummy", yMinField, yMaxField));
            e.Evaluate(@"p <- p + scale_x_continuous(limits = c(0, NA))");

            //<-
            */

            e.Evaluate("print(p)");
        }

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

            //->            
            var res = dt.AsEnumerable().AsQueryable().Where(r => r.Field<double>("eps") == -1.0);
            DataTable dummyDt = dt.Clone();
            foreach (DataRow row in res)
                dummyDt.ImportRow(row);

            DataFrame df2 = e.CreateDataFrame(
                columns: dummyDt.Columns(), 
                columnNames: dummyDt.ColumnNames(), 
                stringsAsFactors: stringsAsFactors);
            e.SetSymbol("dummy", df2);            
            
            e.Evaluate(String.Format(@"p <- p + geom_hline(data = {0}, aes_string(yintercept = ""{1}""), colour=""#990000"", linetype=""dashed"")","dummy", yField));
            //e.Evaluate(String.Format(@"p <- p + geom_hline(data = {0}, aes_string(yintercept = ""{1}""), colour = ""#770000"", alpha = 0.1)", "dummy", yMinField));
            //e.Evaluate(String.Format(@"p <- p + geom_hline(data = {0}, aes_string(yintercept = ""{1}""), colour = ""#770000"", alpha = 0.1)", "dummy", yMaxField));
            //e.Evaluate(String.Format(@"p <- p + annotate(data = {0}, mapping = 
            //    ""rect"", xmin = -Inf, xmax = Inf, aes_string(ymin = ""{1}"", ymax = ""{2}""), fill = ""#770000"", alpha = .1, color = NA)", "dummy", yMinField, yMaxField));
            e.Evaluate(String.Format(@"p <- p + geom_rect(data = {0}, xmin = -Inf, xmax=Inf, aes_string(ymin = ""{1}"", ymax=""{2}""), fill = ""#770000"", alpha = 0.1, linetype=""dashed"")", "dummy", yMinField, yMaxField));
            e.Evaluate(@"p <- p + scale_x_continuous(limits = c(0, NA))");

           //<-

            e.Evaluate("print(p)");            
        }

        public static void Pdf(string outputFile, int width = 8, int height = 11)
        {
            REngine e = REngine.GetInstance();
            e.Evaluate(String.Format(@"pdf(file = paste(""{0}"","".pdf"", sep=""""), width={1}, height={2})", outputFile, width, height));
            //e.Evaluate(String.Format(@"pdf(file = paste(""{0}"","".pdf"", sep=""""))", outputFile));
        }

        public static void DevOff()
        {
            REngine e = REngine.GetInstance();
            e.Evaluate(@"invisible(dev.off())");
        }        
    }
}
