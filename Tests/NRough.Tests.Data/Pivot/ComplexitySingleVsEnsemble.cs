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
    public class ComplexitySingleVsEnsemble
    {
        [Test]
        public void RunTest()
        {
            string path = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\ExceptionRulesTest\bin\x64\Release5\log\";
            string[] filenames100 = new string[]
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

            string[] filenames1 = new string[]
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

            if (filenames1.Length != filenames100.Length)
                throw new InvalidOperationException("filenames1.Length != filenames100.Length");

            int accuracyDecimals = 2;
            int otherDecimals = 0;

            string referenceModelName = "M-EPS";

            List<string> datasetNames = new List<string>(filenames1.Length);
            //string[] colNames = new string[] { "acc", "recallmacro", "precisionmacro", "numrul", "dtha", "dthm" };
            string[] colNames = new string[] { "acc", "precisionmacro", "numrul", "dtha", "dthm" };

            string[] modelNames = null;

            var compare1 = new Dictionary<Tuple<string, string>, DataRow>();
            var compare100 = new Dictionary<Tuple<string, string>, DataRow>();

            bool first = true;
            for (int f = 0; f < filenames1.Length; f++)
            {
                string filename1 = Path.Combine(path, filenames1[f]);
                string filename100 = Path.Combine(path, filenames100[f]);

                if (!File.Exists(filename1))
                    throw new InvalidOperationException("!File.Exists(filename1)");
                if (!File.Exists(filename100))
                    throw new InvalidOperationException("!File.Exists(filename100)");

                DataTable dtc1 = ClassificationResult.ReadResults(filename1, '|');
                DataTable dtc100 = ClassificationResult.ReadResults(filename100, '|');

                if (dtc1.Columns.Contains("Column1"))
                    dtc1.Columns.Remove("Column1");

                if (dtc100.Columns.Contains("Column1"))
                    dtc100.Columns.Remove("Column1");

                string datasetname1 = dtc1.Rows[0]["ds"].ToString().Replace('_', '-');
                if (datasetname1.Substring(datasetname1.Length - 5, 3) == "TST")
                    datasetname1 = datasetname1.Substring(0, datasetname1.Length - 6);

                string datasetname100 = dtc100.Rows[0]["ds"].ToString().Replace('_', '-');
                if (datasetname100.Substring(datasetname100.Length - 5, 3) == "TST")
                    datasetname100 = datasetname100.Substring(0, datasetname100.Length - 6);

                if (datasetname1 != datasetname100)
                    throw new InvalidOperationException("datasetname1 != datasetname100");

                for (int i = 0; i < dtc1.Rows.Count; i++)
                    dtc1.Rows[i]["ds"] = datasetname1;

                for (int i = 0; i < dtc100.Rows.Count; i++)
                    dtc100.Rows[i]["ds"] = datasetname100;

                var dtc1_avg = ClassificationResult.AverageResults3(dtc1);
                var dtc100_avg = ClassificationResult.AverageResults3(dtc100);

                datasetNames.Add(datasetname1);
                dtc1_avg.Columns.Remove("ds");
                dtc1_avg.Columns.Remove("attr");
                dtc1_avg.Columns.Remove("recallmacro");
                //dtc1_avg.Columns.Remove("precisionmacro");

                dtc100_avg.Columns.Remove("ds");
                dtc100_avg.Columns.Remove("attr");
                dtc100_avg.Columns.Remove("recallmacro");
                //dtc100_avg.Columns.Remove("precisionmacro");                

                DataColumn[] cols1 = new DataColumn[colNames.Length];
                for (int i = 0; i < colNames.Length; i++)
                    cols1[i] = dtc1_avg.Columns[colNames[i]];

                DataColumn[] cols100 = new DataColumn[colNames.Length];
                for (int i = 0; i < colNames.Length; i++)
                    cols100[i] = dtc100_avg.Columns[colNames[i]];

                var pivot = new PivotService();

                var pivotTable100 = pivot.Pivot(
                    dtc100_avg,
                    dtc100_avg.Columns["model"],
                    cols100,
                    "-");

                var pivotTable1 = pivot.Pivot(
                    dtc1_avg,
                    dtc1_avg.Columns["model"],
                    cols1,
                    "-");                

                pivotTable1.Columns.Remove("M-EPS-dthm");
                pivotTable1.Columns.Remove("m-PHICAP-NONE-dthm");

                pivotTable100.Columns.Remove("M-EPS-dthm");
                pivotTable100.Columns.Remove("m-PHICAP-NONE-dthm");

                if (first)
                {
                    modelNames = dtc1_avg.AsEnumerable()
                        .GroupBy(r => r["model"].ToString())
                        .Select(g => g.Key).ToArray();
                }

                foreach (string modelName in modelNames)
                {
                    int rowIndex1 = pivotTable1.AsEnumerable()
                        .Select((row, index) => new { row, index })
                        .OrderByDescending(r => Math.Round(r.row.Field<double>(
                            String.Format("{0}-acc", modelName)), accuracyDecimals, MidpointRounding.AwayFromZero))
                        .ThenByDescending(r => r.row.Field<double>("eps"))
                        .Select(r => r.index)
                        .First();

                    double acc1 = Math.Round(
                        pivotTable1.Rows[rowIndex1].Field<double>(
                            String.Format("{0}-acc", modelName)), 
                        accuracyDecimals, 
                        MidpointRounding.AwayFromZero);

                    var query100 = pivotTable100.AsEnumerable()
                        .Select((row, index) => new { row, index })
                        .OrderByDescending(r => Math.Round(r.row.Field<double>(
                            String.Format("{0}-acc", modelName)), accuracyDecimals, MidpointRounding.AwayFromZero))
                        .ThenByDescending(r => r.row.Field<double>("eps"))
                        .Where(r => Math.Round(r.row.Field<double>(
                            String.Format("{0}-acc", modelName)), accuracyDecimals, MidpointRounding.AwayFromZero) <= acc1);

                    int rowIndex100 = 0;
                    if (query100.Any())
                    {
                        rowIndex100 = query100
                            .Select(r => r.index)
                            .First();
                    }
                    else
                    {
                        query100 = pivotTable100.AsEnumerable()
                            .Select((row, index) => new { row, index })
                            .OrderBy(r => Math.Round(r.row.Field<double>(
                                String.Format("{0}-acc", modelName)), accuracyDecimals, MidpointRounding.AwayFromZero))
                            .ThenBy(r => r.row.Field<double>("eps"))
                            .Where(r => Math.Round(r.row.Field<double>(
                                String.Format("{0}-acc", modelName)), accuracyDecimals, MidpointRounding.AwayFromZero) > acc1);

                        rowIndex100 = query100
                            .Select(r => r.index)
                            .First();
                    }

                    

                    compare1.Add(new Tuple<string, string>(datasetname1, modelName), pivotTable1.Rows[rowIndex1]);
                    compare100.Add(new Tuple<string, string>(datasetname100, modelName), pivotTable100.Rows[rowIndex100]);
                }

                first = false;
            }

            int[][] summary = new int[3][];
            for (int i = 0; i < summary.Length; i++)
                summary[i] = new int[7];

            foreach (var modelName in modelNames)
            {
                summary.SetAll(0);

                var firstRow = compare1.FirstOrDefault().Value;
                StringBuilder sb2 = new StringBuilder();
                int cols = colNames.Count(s => firstRow.Table.Columns.Contains(String.Format("{0}-{1}", modelName, s)));

                if (cols == 5)
                {
                    sb2.Append(@"\begin{table}[!htbp]
\centering
\caption{Ensemble complexity comparison with single classifier (" + ConvertModelName(modelName) + @")}
\label{table:accresultsens:" + modelName.ToLower() + @"}
\scriptsize
\begin{tabular}{|l||l|l|l|l|l|l|}
\hline" + Environment.NewLine);
                }
                else
                {
                    sb2.Append(@"\begin{table}[!htbp]
\centering
\caption{Ensemble complexity comparison with single classifier (" + ConvertModelName(modelName) + @")}
\label{table:accresultsens:" + modelName.ToLower() + @"}
\scriptsize
\begin{tabular}{|l||l|l|l|l|l|}
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
                    int j = 0;
                    sb2.Append(ConvertDataSetName(dataset));

                    DataRow singleRow = compare1[new Tuple<string, string>(dataset, modelName)];
                    DataRow bestRow = compare100[new Tuple<string, string>(dataset, modelName)];

                    sb2.Append(" & ");

                    double epsEnsemble = bestRow.Field<double>("eps");
                    double epsSingle = singleRow.Field<double>("eps");

                    int epsComparison = epsEnsemble.CompareTo(epsSingle);
                    string appendValueEps = epsEnsemble.ToString(
                                            ".00" , System.Globalization.CultureInfo.InvariantCulture);
                    switch (epsComparison)
                    {
                        case 1:
                            appendValueEps = String.Format("\\textbf{{{0}}}", appendValueEps);
                            summary[0][j]++;
                            //sb2.Append("+"); 
                            break;
                        
                        case -1:
                            summary[1][j]++;
                            //appendValueEps += "^{-}"
                            //sb2.Append("-");
                            break;

                        case 0:
                            summary[2][j]++;
                            //appendValueEps = String.Format("\\textbf{{{0}}}", appendValueEps);
                            //sb2.Append("o"); 
                            break;
                    }
                    j++;

                    appendValueEps = appendValueEps + " (" + epsSingle.ToString(
                                            ".00" , System.Globalization.CultureInfo.InvariantCulture) + ")";

                    sb2.Append(appendValueEps);

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

                                double ensembleValue = System.Math.Round((double)bestRow[String.Format("{0}-{1}", modelName, colname)], numOfDec, MidpointRounding.AwayFromZero);
                                double singleValue = System.Math.Round((double)singleRow[String.Format("{0}-{1}", modelName, colname)], numOfDec, MidpointRounding.AwayFromZero);

                                string appendValue = ensembleValue.ToString(
                                            "0." + new string('#', numOfDec), System.Globalization.CultureInfo.InvariantCulture);

                                if (colname != "acc")
                                {
                                    int comparison = singleValue.CompareTo(ensembleValue);
                                    if(colname == "precisionmacro")
                                        comparison = ensembleValue.CompareTo(singleValue);

                                    switch (comparison)
                                    {
                                        case 1:
                                            appendValue = String.Format("\\textbf{{{0}}}", appendValue);
                                            summary[0][j]++;
                                            //sb2.Append("+"); 
                                            break;
                                        
                                        case -1:
                                            //appendValue = String.Format("\\textit{{{0}}}", appendValue);
                                            //sb2.Append("-");
                                            summary[1][j]++;
                                            break;

                                        case 0:
                                            //appendValue = String.Format("\\textit{{{0}}}", appendValue);
                                            //sb2.Append("o"); 
                                            summary[2][j]++;
                                            break;
                                    }
                                    j++;

                                    appendValue = appendValue + " (" + singleValue.ToString(
                                            "0." + new string('#', numOfDec), System.Globalization.CultureInfo.InvariantCulture) + ")";
                                }

                                sb2.Append(appendValue);

                                /*                                
                                if (colname != "acc" && colname != "precisionmacro" && colname != "recallmacro")
                                {
                                    sb2.Append(" (");

                                    int comparison = singleValue.CompareTo(ensembleValue);
                                    switch (comparison)
                                    {
                                        case 1: sb2.Append("+"); break;
                                        case 0: sb2.Append("o"); break;
                                        case -1: sb2.Append("-"); break;
                                    }

                                    sb2.Append(")");
                                }
                                */

                            }
                            else
                                sb2.Append(bestRow[String.Format("{0}-{1}", modelName, colname)].ToString());
                        }
                    }

                    sb2.AppendLine(@"\\ \hline");
                }

                //if (modelName != referenceModelName)
                //{
                    sb2.AppendLine(@"\hline");
                    if (cols == 5)
                    {
                        sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} &  & {2} & {3} & {4} & {5} \\ \hline", "+",
                            summary[0][0], summary[0][1], summary[0][2], summary[0][3], summary[0][4]);
                        sb2.AppendLine();
                        sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} &  & {2} & {3} & {4} & {5} \\ \hline", "-",
                            summary[1][0], summary[1][1], summary[1][2], summary[1][3], summary[1][4]);
                        sb2.AppendLine();
                        sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} &  & {2} & {3} & {4} & {5} \\ \hline", "o",
                            summary[2][0], summary[2][1], summary[2][2], summary[2][3], summary[2][4]);
                    }
                    else
                    {
                        sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} &   & {2} & {3} & {4}  \\ \hline", "+",
                            summary[0][0], summary[0][1], summary[0][2], summary[0][3]);
                        sb2.AppendLine();
                        sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} &   & {2} & {3} & {4}  \\ \hline", "-",
                            summary[1][0], summary[1][1], summary[1][2], summary[1][3]);
                        sb2.AppendLine();
                        sb2.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} &   & {2} & {3} & {4}  \\ \hline", "o",
                            summary[2][0], summary[2][1], summary[2][2], summary[2][3]);
                        
                    }
                    sb2.AppendLine();
                //}

                sb2.AppendLine(@"\end{tabular}");
                sb2.AppendLine(@"\end{table}");

                Console.WriteLine(sb2.ToString());
                Console.WriteLine();
            }


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
