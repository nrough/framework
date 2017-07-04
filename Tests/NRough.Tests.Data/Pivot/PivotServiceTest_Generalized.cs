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
using NRough.Core.BaseTypeExtensions;
using NRough.Core.CollectionExtensions;
using NRough.Core.Data;
using NRough.Data.Pivot;
using NRough.Data.Writers;
using NRough.MachineLearning.Classification;
using NRough.MachineLearning.Evaluation.HypothesisTesting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Tests.Data.Pivot
{    
    [TestFixture]
    public class PivotServiceTest_Generalized
    {
        [Test]
        public void Test1()
        {
            string path = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\ExceptionRulesTest\bin\x64\Release5\log\";
            //string path = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\ExceptionRulesTest\bin\x64\Release5\log\60\";
            string[] filenames = new string[]
            {
                "audiology-1.result", //audiology
                "breast-1.result", //breast                
                "chess-1.result", //chess
                "dermatology-1.result", //dermatology_modified
                "dna-1.result", //dna                
                "house-1.result", //house                
                "letter-1.result", //letter.disc                
                "lymphography-1.result", //lymphography
                "mushroom-1.result", //mashroom
                "monks-1-1.result", //monks-1
                "monks-2-1.result", //monks-2
                "monks-3-1.result", //monks-3                
                "pen-1.result", //pen.disc                
                "promoters-1.result", //promoters
                "sat-1.result", //sat.disc
                "semeion-1.result", //semeion
                "soybean-large-1.result", //soybean-large
                "soybean-small-1.result", //soybean-small
                "spect-1.result", //spect
                "vowel-1.result", //vowel
                "zoo-1.result" //zoo                                                                
            };

            Test(path, filenames, "Generalized majority decision reduct results ({0})", 1, "M-EPS");
        }

        [Test]
        public void Test100()
        {
            string path = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\ExceptionRulesTest\bin\x64\Release5\log\";
            string[] filenames = new string[]
            {
                "audiology-100.result", //audiology
                "breast-100.result", //breast                
                "chess-100.result", //chess
                "dermatology-100.result", //dermatology_modified
                "dna-100.result", //dna                
                "house-100.result", //house                
                "letter-100.result", //letter.disc                
                "lymphography-100.result", //lymphography
                "mushroom-100.result", //mashroom
                "monks-1-100.result", //monks-1
                "monks-2-100.result", //monks-2
                "monks-3-100.result", //monks-3                
                "pen-100.result", //pen.disc                
                "promoters-100.result", //promoters
                "sat-100.result", //sat.disc
                "semeion-100.result", //semeion
                "soybean-large-100.result", //soybean-large
                "soybean-small-100.result", //soybean-small
                "spect-100.result", //spect
                "vowel-100.result", //vowel
                "zoo-100.result" //zoo                                                                
            };

            Test(path, filenames, "Generalized majority decision reduct results ({0}) size=100", 100, "M-EPS");
        }

        [Test]
        public void Test100WithRF()
        {
            string path = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\Infovision.UnitTest.Runner\bin\x64\Release5\";
            string[] filenames = new string[]
            {
                "audiology-100.result", //audiology
                "breast-100.result", //breast                
                "chess-100.result", //chess
                "dermatology-100.result", //dermatology_modified
                "dna-100.result", //dna                
                "house-100.result", //house                
                "letter-100.result", //letter.disc                
                "lymphography-100.result", //lymphography
                "mushroom-100.result", //mashroom
                "monks-1-100.result", //monks-1
                "monks-2-100.result", //monks-2
                "monks-3-100.result", //monks-3                
                "pen-100.result", //pen.disc                
                "promoters-100.result", //promoters
                "sat-100.result", //sat.disc
                "semeion-100.result", //semeion
                "soybean-large-100.result", //soybean-large
                "soybean-small-100.result", //soybean-small
                "spect-100.result", //spect
                "vowel-100.result", //vowel
                "zoo-100.result" //zoo                                                                
            };

            Test(path, filenames, "Generalized majority decision reduct results ({0}) size=100", 100, "RandomC45");
        }


        public void Test(string path, string[] filenames, string caption, int size, string referenceModelName)
        {
            int accuracyDecimals = 2;
            int otherDecimals = 0;
            bool splitTableToParts = false;

            var compareBest = new Dictionary<Tuple<string, string>, DataRow>();
            var compareBest2 = new Dictionary<Tuple<string, string>, DataRow>();
            List<string> datasetNames = new List<string>(filenames.Length);
                        
            string[] colNames = new string[] { "acc", "recallmacro", "precisionmacro", "numrul", "dtha", "dthm" };

            string[] modelNames = null;
            bool first = true;
            bool isFirstTable = true;

            string output = Path.Combine(path, "latextable-" + size.ToString() + ".tex");

            using (FileStream fileStreamWrite =
                new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter outputFile = new StreamWriter(fileStreamWrite))
                {
                    isFirstTable = true;
                    foreach (var f in filenames)
                    {
                        string filename = Path.Combine(path, f);

                        if (!File.Exists(filename))
                            continue;

                        DataTable dtc = ClassificationResult.ReadResults(filename, '|');

                        if (dtc.Columns.Contains("Column1"))
                            dtc.Columns.Remove("Column1");

                        string datasetname = dtc.Rows[0]["ds"].ToString().Replace('_', '-');
                        if (datasetname.Substring(datasetname.Length - 5, 3) == "TST")
                            datasetname = datasetname.Substring(0, datasetname.Length - 6);

                        for (int i = 0; i < dtc.Rows.Count; i++)
                            dtc.Rows[i]["ds"] = datasetname;

                        var dtc1 = ClassificationResult.AverageResults3(dtc);
                        var dtc2 = ClassificationResult.AverageResults4(dtc);

                        dtc = dtc1;

                        //Console.WriteLine(new DataTableLatexTabularFormatter().Format("", dtc, null));

                        datasetNames.Add(datasetname);
                        dtc.Columns.Remove("ds");
                        dtc2.Columns.Remove("ds");

                        dtc.Columns.Remove("attr");                        
                        dtc2.Columns.Remove("attr");
                        dtc2.Columns.Remove("attrdev");

                        DataColumn[] cols = new DataColumn[colNames.Length];
                        for (int i = 0; i < colNames.Length; i++)
                            cols[i] = dtc.Columns[colNames[i]];

                        DataColumn[] cols2 = new DataColumn[colNames.Length * 2];
                        for (int i = 0; i < colNames.Length; i++)
                        {
                            cols2[i * 2] = dtc2.Columns[colNames[i]];
                            cols2[(i * 2) + 1] = dtc2.Columns[colNames[i] + "dev"];
                        }

                        if (first)
                        {
                            modelNames = dtc.AsEnumerable()
                                .GroupBy(r => r["model"].ToString())
                                .Select(g => g.Key).ToArray();
                        }

                        dtc.DeleteRows(r => r.Field<double>("eps") > 0.6);
                        dtc2.DeleteRows(r => r.Field<double>("eps") > 0.6);

                        var pivot = new PivotService();
                        var pivotTable = pivot.Pivot(
                            dtc,
                            dtc.Columns["model"],
                            cols,
                            "-");

                        var pivotTable2 = pivot.Pivot(
                            dtc2,
                            dtc2.Columns["model"],
                            cols2,
                            "-");

                        pivotTable.DeleteColumn("M-EPS-dthm");
                        pivotTable2.DeleteColumn("M-EPS-dthm");
                        pivotTable2.DeleteColumn("M-EPS-dthmdev");

                        pivotTable.DeleteColumn("m-PHICAP-NONE-dthm");
                        pivotTable2.DeleteColumn("m-PHICAP-NONE-dthm");
                        pivotTable2.DeleteColumn("m-PHICAP-NONE-dthmdev");

                        pivotTable.DeleteColumn("RandomC45-dthm");
                        pivotTable2.DeleteColumn("RandomC45-dthm");
                        pivotTable2.DeleteColumn("RandomC45-dthmdev");

                        var dataFormatter = new DataTableLatexTabularFormatter();

                        pivotTable.Columns["eps"].ExtendedProperties.Add("format", ".00");
                        pivotTable.Columns["eps"].ExtendedProperties.Add("formatProvider", System.Globalization.CultureInfo.InvariantCulture);
                        
                        int bottomRowIdx = 69;

                        foreach (string modelName in modelNames)
                        {
                            int maxRowIndex;
                            if (modelName != "RandomC45")
                            {
                                maxRowIndex = pivotTable.AsEnumerable()
                                    .Select((row, index) => new { row, index })
                                    .OrderByDescending(r => Math.Round(r.row.Field<double>(
                                        String.Format("{0}-acc", modelName)), accuracyDecimals, MidpointRounding.AwayFromZero))
                                    .ThenByDescending(r => r.row.Field<double>("eps"))
                                    .Select(r => r.index).First();
                            }
                            else
                            {
                                maxRowIndex = 0;
                            }

                            if (bottomRowIdx < maxRowIndex)
                                bottomRowIdx = maxRowIndex;

                            compareBest.Add(new Tuple<string, string>(datasetname, modelName), pivotTable.Rows[maxRowIndex]);
                            compareBest2.Add(new Tuple<string, string>(datasetname, modelName), pivotTable2.Rows[maxRowIndex]);

                            for (int i = 0; i < colNames.Length; i++)
                            {
                                if (!pivotTable.Columns.Contains(String.Format("{0}-{1}", modelName, colNames[i])))
                                    continue;

                                if(modelName == "RandomC45")
                                {
                                    pivotTable.Columns[String.Format("{0}-{1}", modelName, colNames[i])].ExtendedProperties.Add("active", false);
                                    continue;
                                }

                                if (colNames[i] == "acc" || colNames[i] == "precisionmacro" || colNames[i] == "recallmacro")
                                {
                                    pivotTable.Columns[String.Format("{0}-{1}", modelName, colNames[i])].ExtendedProperties.Add("format", "0." + new string('#', accuracyDecimals));
                                    pivotTable.Columns[String.Format("{0}-{1}", modelName, colNames[i])].ExtendedProperties.Add("formatProvider", System.Globalization.CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    pivotTable.Columns[String.Format("{0}-{1}", modelName, colNames[i])].ExtendedProperties.Add("format", "0." + new string('#', otherDecimals));
                                    pivotTable.Columns[String.Format("{0}-{1}", modelName, colNames[i])].ExtendedProperties.Add("formatProvider", System.Globalization.CultureInfo.InvariantCulture);
                                }


                                dataFormatter.SetCellProperty(
                                    pivotTable.Columns[String.Format("{0}-{1}", modelName, colNames[i])].Ordinal,
                                    maxRowIndex,
                                    "fontface",
                                    "textbf");
                            }
                        }

                        dataFormatter.ExtraRow = @"... & \multicolumn{5}{c|}{...} & \multicolumn{6}{c|}{...} & \multicolumn{6}{c|}{...} & \multicolumn{5}{c|}{...}\\";

                        if (bottomRowIdx == 99)
                        {
                            for (int j = bottomRowIdx - 2; j > pivotTable.Rows.Count - 23; j--)
                                dataFormatter.SetRowProperty(j, "active", "false");
                        }
                        else
                        {
                            for (int j = bottomRowIdx + 1; j < pivotTable.Rows.Count; j++)
                            {
                                dataFormatter.SetRowProperty(j, "active", "false");
                            }
                        }

                        string latexTable;
                        if (!splitTableToParts)
                        {
                            dataFormatter.Caption = String.Format(caption, ConvertDataSetName(datasetname));
                            dataFormatter.Label = String.Format("results:gmdr{1}_{0}", ConvertDataSetName(datasetname), size);
                            dataFormatter.CustomHeader = CustomHeader(dataFormatter.Caption, dataFormatter.Label, isFirstTable);
                            dataFormatter.CustomFooter = CustomFooter();

                            isFirstTable = false;
                        }
                        else
                        {

                            foreach (string modelName in modelNames.Where((m, i) => i >= 2))
                            {
                                for (int i = 0; i < colNames.Length; i++)
                                {
                                    string curColumnName = String.Format("{0}-{1}", modelName, colNames[i]);
                                    if(pivotTable.Columns.Contains(curColumnName))
                                        pivotTable.Columns[curColumnName].ExtendedProperties.Add("active", false);
                                }
                            }

                            dataFormatter.Caption = String.Format(caption, ConvertDataSetName(datasetname));
                            dataFormatter.Label = String.Format("results:gmdr{1}_{0}", ConvertDataSetName(datasetname), size);
                            dataFormatter.CustomHeader = CustomHeader_Part1(dataFormatter.Caption, dataFormatter.Label);
                            dataFormatter.CustomFooter = CustomFooter();
                            latexTable = dataFormatter.Format("G", pivotTable, null);

                            Console.WriteLine(latexTable);
                            Console.WriteLine();

                            outputFile.WriteLine(latexTable);
                            outputFile.WriteLine();

                            for (int m = 0; m < modelNames.Length; m++)
                            {
                                for (int i = 0; i < colNames.Length; i++)
                                {
                                    string curColumnName = String.Format("{0}-{1}", modelNames[m], colNames[i]);
                                    if (pivotTable.Columns.Contains(curColumnName))
                                    {
                                        if (m < 2)
                                        {
                                            pivotTable.Columns[curColumnName].ExtendedProperties["active"] = false;
                                        }
                                        else
                                        {
                                            pivotTable.Columns[curColumnName].ExtendedProperties["active"] = true;
                                        }
                                    }
                                }
                            }

                            dataFormatter.Caption = String.Format(caption, ConvertDataSetName(datasetname));
                            dataFormatter.Label = String.Format("results:gmdr{1}_{0}", ConvertDataSetName(datasetname), size);
                            dataFormatter.CustomHeader = CustomHeader_Part2(dataFormatter.Caption, dataFormatter.Label);
                            dataFormatter.CustomFooter = CustomFooter();
                        }

                        latexTable = dataFormatter.Format("G", pivotTable, null);

                        Console.WriteLine(latexTable);
                        Console.WriteLine();

                        outputFile.WriteLine(latexTable);
                        outputFile.WriteLine();

                        first = false;
                    }

                    int[][] summary = new int[3][];
                    for (int i = 0; i < summary.Length; i++)
                        summary[i] = new int[7];
                    
                    foreach (var modelName in modelNames
                        .Where(m => m != referenceModelName))
                    {
                        summary.SetAll(0);

                        StringBuilder sb = new StringBuilder();

                        int j = 0;
                        first = true;
                        foreach (var dataset in datasetNames)
                        {
                            DataRow referenceRow = compareBest[new Tuple<string, string>(dataset, referenceModelName)];
                            DataRow rowToValidate = compareBest[new Tuple<string, string>(dataset, modelName)];

                            if (first)
                            {
                                sb.Append(@"\textbf{Data}");
                                foreach (var colname in colNames)
                                {
                                    if (rowToValidate.Table.Columns.Contains(String.Format("{0}-{1}", modelName, colname))
                                        && referenceRow.Table.Columns.Contains(String.Format("{0}-{1}", referenceModelName, colname)))
                                    {
                                        sb.AppendFormat(@" &  \textbf{{{0}}}", ConvertColName(colname));
                                    }
                                }
                                sb.AppendLine(@"\\ \hline");
                            }

                            sb.AppendFormat(@"\hyperref[results:gmdr{1}_{0}]{{{0}}}", ConvertDataSetName(dataset), size);
                            j = 0;
                            foreach (var colname in colNames)
                            {
                                if (!rowToValidate.Table.Columns.Contains(String.Format("{0}-{1}", modelName, colname))
                                    || !referenceRow.Table.Columns.Contains(String.Format("{0}-{1}", referenceModelName, colname)))
                                    continue;

                                int numOfDec = otherDecimals;
                                if (colname == "acc" || colname == "precisionmacro" || colname == "recallmacro")
                                    numOfDec = accuracyDecimals;

                                double newval = Math.Round(
                                    rowToValidate.Field<double>(String.Format("{0}-{1}", modelName, colname)),
                                    numOfDec, MidpointRounding.AwayFromZero);

                                double refval = Math.Round(
                                    referenceRow.Field<double>(String.Format("{0}-{1}", referenceModelName, colname)),
                                    numOfDec, MidpointRounding.AwayFromZero);

                                int comparisonResult = (colname == "acc" || colname == "precisionmacro" || colname == "recallmacro")
                                                     ? newval.CompareTo(refval)
                                                     : refval.CompareTo(newval);

                                switch (comparisonResult)
                                {
                                    case 1:
                                        sb.Append(" & +");
                                        summary[0][j]++;
                                        break;
                                    case -1:
                                        sb.Append(" & -");
                                        summary[1][j]++;
                                        break;
                                    case 0:
                                        sb.Append(" & o");
                                        summary[2][j]++;
                                        break;
                                }
                                j++;
                            }
                            sb.AppendLine(@"\\ \hline");
                            first = false;
                        }

                        if (j == 5)
                        {
                            sb.Insert(0, @"\begin{table}[!htbp]		
\centering
\caption{Experimental Results Summary(" + ConvertModelName(modelName) + @")}
\label{table:results" + size.ToString() + ":" + modelName.ToLower() + @"}
\scriptsize
\resizebox{\columnwidth}{!}{%
\begin{tabular}{|l||c|c|c|c|c|}
\hline" + Environment.NewLine);

                            sb.AppendLine(@"\hline");
                            sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4} & {5} \\ \hline", "+",
                                summary[0][0], summary[0][1], summary[0][2], summary[0][3], summary[0][4]);
                            sb.AppendLine();
                            sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4} & {5} \\ \hline", "-",
                                summary[1][0], summary[1][1], summary[1][2], summary[1][3], summary[1][4]);
                            sb.AppendLine();
                            sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4} & {5}\\ \hline", "o",
                                summary[2][0], summary[2][1], summary[2][2], summary[2][3], summary[2][4]);
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.Insert(0, @"\begin{table}[!htbp]		
\centering
\caption{Experimental Results Summary(" + ConvertModelName(modelName) + @")}
\label{table:results:" + modelName.ToLower() + @"}
\scriptsize
\resizebox{\columnwidth}{!}{%
\begin{tabular}{|l||c|c|c|c|c|c|}
\hline" + Environment.NewLine);

                            sb.AppendLine(@"\hline");
                            sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4} & {5} & {6}\\ \hline", "+",
                                summary[0][0], summary[0][1], summary[0][2], summary[0][3], summary[0][4], summary[0][5]);
                            sb.AppendLine();
                            sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4} & {5} & {6} \\ \hline", "-",
                                summary[1][0], summary[1][1], summary[1][2], summary[1][3], summary[1][4], summary[1][5]);
                            sb.AppendLine();
                            sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4} & {5} & {6}\\ \hline", "o",
                                summary[2][0], summary[2][1], summary[2][2], summary[2][3], summary[2][4], summary[0][5]);
                            sb.AppendLine();
                        }

                        sb.AppendLine(@"\end{tabular}");
                        sb.AppendLine(@"}");
                        sb.AppendLine(@"\end{table}");

                        Console.WriteLine(sb.ToString());

                        outputFile.WriteLine(sb.ToString());
                        outputFile.WriteLine();

                    }

                    //////////////////////////////// BEST RESULT TABLES ////////////////////////////////////////

                    foreach (var modelName in modelNames)
                    {
                        summary.SetAll(0);

                        var firstRow = compareBest2.FirstOrDefault().Value;

                        StringBuilder sb2 = new StringBuilder();

                        int cols = colNames.Count(s => firstRow.Table.Columns.Contains(String.Format("{0}-{1}", modelName, s)));

                        if (cols == 6)
                        {
                            sb2.Append(@"\begin{table}[!htbp]
\centering
\caption{Best accuracy results (" + ConvertModelName(modelName) + @")}
\label{table:accresults" + size.ToString() + ":" + modelName.ToLower() + @"}
\scriptsize
\resizebox{\columnwidth}{!}{%
\begin{tabular}{|l||l|l|l|l|l|l|l|}
\hline" + Environment.NewLine);
                        }
                        else
                        {
                            sb2.Append(@"\begin{table}[!htbp]
\centering
\caption{Best accuracy results (" + ConvertModelName(modelName) + @")}
\label{table:accresults" + size.ToString() + ":" + modelName.ToLower() + @"}
\scriptsize
\resizebox{\columnwidth}{!}{%
\begin{tabular}{|l||l|l|l|l|l|l|}
\hline" + Environment.NewLine);
                        }

                        sb2.Append(@"\textbf{Data}");
                        sb2.Append(" & ");

                        if (modelName == referenceModelName)
                            sb2.Append(@"\bm{$\varepsilon$}");
                        else
                            sb2.Append(@"\bm{$\phi$}");

                        foreach (var colname in colNames)
                        {
                            if (firstRow.Table.Columns.Contains(String.Format("{0}-{1}", modelName, colname)))
                            {
                                sb2.Append(" & ");
                                sb2.Append(String.Format(@"\textbf{{{0}}}", ConvertColName(colname)));
                            }
                        }

                        sb2.AppendLine(@"\\ \hline");

                        foreach (var dataset in datasetNames)
                        {
                            //sb2.Append(ConvertDataSetName(dataset));
                            sb2.AppendFormat(@"\hyperref[results:gmdr{1}_{0}]{{{0}}}", ConvertDataSetName(dataset), size);

                            DataRow bestRow = compareBest2[new Tuple<string, string>(dataset, modelName)];
                            DataRow referenceRow = compareBest2[new Tuple<string, string>(dataset, referenceModelName)];

                            sb2.Append(" & ");                            
                            sb2.Append(bestRow.Field<double>("eps").ToString(".00", System.Globalization.CultureInfo.InvariantCulture));

                            int j = 0;
                            foreach (var colname in colNames)
                            {
                                if (bestRow.Table.Columns.Contains(String.Format("{0}-{1}", modelName, colname)))
                                {
                                    sb2.Append(" & ");
                                    if (bestRow[String.Format("{0}-{1}", modelName, colname)] is double)
                                    {
                                        int numOfDec = otherDecimals;
                                        if (colname == "acc" || colname == "precisionmacro" || colname == "recallmacro")
                                            numOfDec = accuracyDecimals;

                                        double bestValue = System.Math.Round((double)bestRow[String.Format("{0}-{1}", modelName, colname)], numOfDec, MidpointRounding.AwayFromZero);
                                        double referenceValue = referenceRow.Table.Columns.Contains(String.Format("{0}-{1}", referenceModelName, colname))
                                            ? System.Math.Round((double)referenceRow[String.Format("{0}-{1}", referenceModelName, colname)], numOfDec, MidpointRounding.AwayFromZero)
                                            : bestValue;

                                        string appendValue = bestValue.ToString(
                                                    "0." + new string('#', numOfDec), System.Globalization.CultureInfo.InvariantCulture);
                                        appendValue += " (";
                                        appendValue += ((double)bestRow[String.Format("{0}-{1}dev", modelName, colname)]).ToString(
                                               "0." + new string('#', accuracyDecimals), System.Globalization.CultureInfo.InvariantCulture);
                                        appendValue += ")";

                                        int comparison = bestValue.CompareToEpsilon(referenceValue);
                                        if (colname != "acc" && colname != "precisionmacro" && colname != "recallmacro")
                                            comparison = referenceValue.CompareToEpsilon(bestValue);

                                        switch (comparison)
                                        {
                                            case 1:
                                                appendValue = String.Format("\\textbf{{{0}}}", appendValue);
                                                summary[0][j]++;
                                                //sb2.Append("+"); 
                                                break;
                                            case -1:
                                                summary[1][j]++;
                                                //appendValue = String.Format("\\textit{{{0}}}", appendValue);
                                                //sb2.Append("-");
                                                break;
                                            case 0:
                                                summary[2][j]++;
                                                //appendValue = String.Format("\\textit{{{0}}}", appendValue);
                                                //sb2.Append("o"); 
                                                break;
                                        }
                                        j++;

                                        sb2.Append(appendValue);

                                        /*
                                        sb2.Append(((double)bestRow[String.Format("{0}-{1}", modelName, colname)]).ToString(
                                                    "0." + new string('#', numOfDec), System.Globalization.CultureInfo.InvariantCulture));
                                        sb2.Append(" (");
                                        sb2.Append(((double)bestRow[String.Format("{0}-{1}dev", modelName, colname)]).ToString(
                                                "0." + new string('#', accuracyDecimals), System.Globalization.CultureInfo.InvariantCulture));
                                        sb2.Append(")");
                                        */
                                    }
                                    else
                                        sb2.Append(bestRow[String.Format("{0}-{1}", modelName, colname)].ToString());
                                }
                            }

                            sb2.AppendLine(@"\\ \hline");
                        }

                        if (modelName != referenceModelName)
                        {
                            sb2.AppendLine(@"\hline");
                            if (cols == 6)
                            {
                                sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & & {1} & {2} & {3} & {4} & {5} & {6} \\ \hline", "+",
                                    summary[0][0], summary[0][1], summary[0][2], summary[0][3], summary[0][4], "n/a");
                                sb2.AppendLine();
                                sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & & {1} & {2} & {3} & {4} & {5} & {6} \\ \hline", "-",
                                    summary[1][0], summary[1][1], summary[1][2], summary[1][3], summary[1][4], "n/a");
                                sb2.AppendLine();
                                sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & & {1} & {2} & {3} & {4} & {5} & {6} \\ \hline", "o",
                                    summary[2][0], summary[2][1], summary[2][2], summary[2][3], summary[2][4], "n/a");
                            }
                            else
                            {
                                sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & & {1} & {2} & {3} & {4} & {5} \\ \hline", "+",
                                    summary[0][0], summary[0][1], summary[0][2], summary[0][3], summary[0][4]);
                                sb2.AppendLine();
                                sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & & {1} & {2} & {3} & {4} & {5} \\ \hline", "-",
                                    summary[1][0], summary[1][1], summary[1][2], summary[1][3], summary[1][4]);
                                sb2.AppendLine();
                                sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & & {1} & {2} & {3} & {4} & {5} \\ \hline", "o",
                                    summary[2][0], summary[2][1], summary[2][2], summary[2][3], summary[2][4]);
                            }
                            sb2.AppendLine();
                        }

                        sb2.AppendLine(@"\end{tabular}");
                        sb2.AppendLine(@"}");
                        sb2.AppendLine(@"\end{table}");

                        Console.WriteLine(sb2.ToString());

                        Console.WriteLine();
                    }                    

                    //////////////////////////////// WILCOXON ////////////////////////////////////////

                    foreach (var modelName in modelNames
                        .Where(m => m != referenceModelName))
                    {
                        Console.WriteLine("Comparison {0} vs. {1}", referenceModelName, modelName);

                        var firstRow = compareBest[new Tuple<string, string>(datasetNames.FirstOrDefault(), referenceModelName)];
                        foreach (var colname in colNames)
                        {
                            var currColumnName = String.Format("{0}-{1}", modelName, colname);
                            var currReferenceColumnName = String.Format("{0}-{1}", referenceModelName, colname);

                            int numOfDec = otherDecimals;
                            if (colname == "acc" || colname == "precisionmacro" || colname == "recallmacro")
                                numOfDec = accuracyDecimals;

                            if (firstRow.Table.Columns.Contains(currReferenceColumnName))
                            {
                                double[] serie1 = new double[datasetNames.Count];
                                double[] serie2 = new double[datasetNames.Count];

                                int i = 0;
                                foreach (var dataset in datasetNames)
                                {
                                    DataRow referenceResult = compareBest[new Tuple<string, string>(dataset, referenceModelName)];
                                    DataRow testedResult = compareBest[new Tuple<string, string>(dataset, modelName)];

                                    serie1[i] = Math.Round((double)referenceResult[currReferenceColumnName], numOfDec, MidpointRounding.AwayFromZero);
                                    serie2[i] = Math.Round((double)testedResult[currColumnName], numOfDec, MidpointRounding.AwayFromZero);
                                    i++;
                                }

                                Console.WriteLine("============== {0} =============", ConvertColName(colname));

                                string format = "0." + new string('#', numOfDec);
                                Console.WriteLine(serie1.ToStr(" ", format, null));
                                Console.WriteLine(serie2.ToStr(" ", format, null));

                                var wilcoxon = new WilcoxonSignedRankPairTest();

                                if (colname == "acc" || colname == "precisionmacro" || colname == "recallmacro")
                                    wilcoxon.AlternativeHypothesis = HypothesisType.FirstIsSmallerThanSecond;
                                else
                                    wilcoxon.AlternativeHypothesis = HypothesisType.FirstIsGreaterThanSecond;

                                wilcoxon.Alpha = 0.05;

                                wilcoxon.Exact = false;
                                wilcoxon.ExactDistributionBasedOnRanks = false;
                                wilcoxon.ExactLimit = 8;
                                wilcoxon.SplitTiesEvenly = false;
                                
                                wilcoxon.Compute(serie1, serie2);
                                Console.WriteLine(wilcoxon.ToString("DEBUG", null));
                            }
                        }
                    }

                    //////////////////////////////// FRIEDMAN ////////////////////////////////////////

                    double[][][] data = new double[colNames.Length][][];
                    for (int i = 0; i < colNames.Length; i++)
                    {
                        data[i] = new double[modelNames.Length][];
                        for (int j = 0; j < modelNames.Length; j++)
                        {
                            data[i][j] = new double[datasetNames.Count];
                            for (int k = 0; k < datasetNames.Count; k++)
                            {
                                data[i][j][k] = 0.0;
                            }
                        }
                    }

                    for (int i = 0; i < colNames.Length; i++)
                    {
                        string colname = colNames[i];
                        if (colname == "dthm")
                            continue;

                        int numOfDec = otherDecimals;
                        if (colname == "acc" || colname == "precisionmacro" || colname == "recallmacro")
                            numOfDec = accuracyDecimals;

                        Console.WriteLine("============== {0} =============", ConvertColName(colname));

                        for (int j = 0; j < modelNames.Length; j++)
                        {
                            string modelName = modelNames[j];
                            var pivotColName = String.Format("{0}-{1}", modelName, colname);

                            for (int k = 0; k < datasetNames.Count; k++)
                            {
                                string dataset = datasetNames.ElementAt(k);

                                DataRow testedResult = compareBest[new Tuple<string, string>(dataset, modelName)];
                                if (testedResult.Table.Columns.Contains(pivotColName))
                                    data[i][j][k] = Math.Round((double)testedResult[pivotColName], numOfDec, MidpointRounding.AwayFromZero);
                                else
                                    data[i][j][k] = Double.NaN;
                            }
                        }

                        string comparisonTable =
                            data[i].ToStr2d(";", Environment.NewLine,
                                true, "0." + new string('#', numOfDec),
                                System.Globalization.CultureInfo.InvariantCulture,
                                //new string[] { "Data" }.Concat(modelNames.Select(x => ConvertModelName(x))).ToArray(),
                                new string[] { "Data" }.Concat(modelNames).ToArray(),
                                datasetNames.ToArray()
                                );

                        string comparisonfilename = Path.Combine(path, "comparison_" + colname + "-" + size.ToString() + ".out");
                        using (FileStream comparisonFileStream =
                            new FileStream(comparisonfilename, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (StreamWriter comparisionStreamWriter = new StreamWriter(comparisonFileStream))
                            {
                                comparisionStreamWriter.Write(comparisonTable);
                                Console.WriteLine(comparisonTable);
                            }
                        }
                    }
                }
            }
        }

        private string CustomHeader(string caption, string label, bool first)
        {
            //string ratio = (first == true) ? "0.92" : "";
            string ratio = "";

            return @"\begin{table}[htbp]
\centering
\caption{" + caption + @"}
\label{" + label + @"}
\rowcolors{4}{gray!25}{white}
\resizebox{" + ratio + @"\columnwidth}{!}{%
\begin{tabular}{|c|lllll|llllll|llllll|lllll|} 
\hline
\multirow{2}{*}[-1.5cm]{{\LARGE \begin{tabular}{c}$\varepsilon$ \\ $\phi$ \end{tabular}}} 
& \multicolumn{5}{c|}{\textbf{$(M,\varepsilon)$}} 
& \multicolumn{6}{c|}{\textbf{$(m^{\phi},\cap)$-Exep}} 
& \multicolumn{6}{c|}{\textbf{$(m^{\phi},\cap)$-Gaps}} 
& \multicolumn{5}{c|}{\textbf{$(m^{\phi},\cap)$-None}}\\ 
\cline{2-23}
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Recall}} & \rot{\textbf{Precision}} &  \rot{\textbf{\#Rules}} & \rotl{\textbf{Rules length}}  
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Recall}} & \rot{\textbf{Precision}} &  \rot{\textbf{\#Rules}} & \rot{\textbf{Rules length}} & \rotl{\textbf{\#Exceptions}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Recall}} & \rot{\textbf{Precision}} &  \rot{\textbf{\#Rules}} & \rot{\textbf{Rules length}} & \rotl{\textbf{\#Exceptions}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Recall}} & \rot{\textbf{Precision}} &  \rot{\textbf{\#Rules}} & \rotl{\textbf{Rules length}} \\ \hline";
        }

        private string CustomHeader_Part1(string caption, string label)
        {
            return @"\begin{table}[htbp]
\centering
\caption{" + caption + @" (part 1)" + @"}
\label{" + label + @"}
\rowcolors{4}{gray!25}{white}
\resizebox{\columnwidth}{!}{%
\begin{tabular}{|c|lllllll|lllllll|} \hline
 \multirow{2}{*}[-1.5cm]{{\LARGE $\varepsilon$}} 
& \multicolumn{6}{c|}{\textbf{$(M,\varepsilon)$}} 
& \multicolumn{7}{c|}{\textbf{$(m^{\phi},\cap)$-Exep}}} 
\\ \cline{2-14}
 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Recall}} & \rot{\textbf{Precision}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Recall}} & \rot{\textbf{Precision}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}}\\ \hline";
        }

        private string CustomHeader_Part2(string caption, string label)
        {
            return @"\begin{table}[htbp]
\centering
\caption{" + caption + @" (part 2)" + @"}
\label{" + label + @"-part2" + @"}
\rowcolors{4}{gray!25}{white}
\resizebox{\columnwidth}{!}{%
\begin{tabular}{|c|lllllll|lllllll|} \hline
 \multirow{2}{*}[-1.5cm]{{\LARGE $\varepsilon$}} 
& \multicolumn{7}{c|}{\textbf{$(m^{\phi},\cap)$-Gaps}} 
& \multicolumn{6}{c|}{\textbf{$(m^{\phi},\cap)$-None}}} 
\\ \cline{2-14}
 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Recall}} & \rot{\textbf{Precision}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Recall}} & \rot{\textbf{Precision}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}}\\ \hline";
        }

        private string CustomFooter()
        {
            return @"\hline
\end{tabular}
}
\end{table}";
        }

        private string ConvertModelName(string name)
        {
            switch (name)
            {
                case "C45-Entropy-EBP": return "FULL-ENT";
                case "Reduct-Entropy-EBP": return "RED-ENT";
                case "Reduct-Majority-EBP": return "RED-MAJ";
                case "RedGam-Majority-EBP": return "RED-MAJ-EPS";

                case "M-EPS": return "$(M,\\varepsilon)$";
                case "m-EPS-CAP": return "$(m^{\\varepsilon},\\cap)$";
                case "m-PHICAP-EXEP": return "$(m^{\\phi},\\cap)$-Exep";
                case "m-PHICAP-GAPS": return "$(m^{\\phi},\\cap)$-Gaps";
                case "m-PHICAP-NONE": return "$(m^{\\phi},\\cap)$-None";
            }
            return name;
        }

        private string ConvertColName(string name)
        {
            switch (name)
            {
                case "acc": return "Accuracy";
                case "precisionmacro": return "Precision";
                case "recallmacro": return "Recall";                
                case "attr": return "Reduct length";
                case "numrul": return "\\#Rules";
                case "dtha": return "Rule length";
                case "dthm": return "\\#Exceptions";                
            }
            return name;
        }


        private string ConvertDataSetName(string name)
        {
            switch (name)
            {
                case "audiology.standardized.2.test": return "audiology";
                case "breast-cancer-wisconsin.2.data": return "breast";
                case "chess.dta": return "chess";
                case "chess.data": return "chess";
                case "dermatology-modified.data": return "dermatology";
                case "dna.test": return "dna";
                case "house-votes-84.2.data": return "house";
                case "letter.disc.tst": return "letter";
                case "lymphography.all": return "lymphography";
                case "agaricus-lepiota.2.data": return "mushroom";
                case "monks-1.test": return "monks-1";
                case "monks-2.test": return "monks-2";
                case "monks-3.test": return "monks-3";
                case "nursery.2.data": return "nursery";
                case "pendigits.disc.tst": return "pen";
                case "promoters.2.data": return "promoters";
                case "sat.disc.tst": return "sat";
                case "semeion.data": return "semeion";
                case "soybean-large.test": return "soybean-large";
                case "soybean-small.2.data": return "soybean-small";
                case "spect.test": return "spect";
                case "SPECT.test": return "spect";
                case "vowel.disc.tst": return "vowel";
                case "zoo.dta": return "zoo";

            }

            return name;
        }
    }
}
