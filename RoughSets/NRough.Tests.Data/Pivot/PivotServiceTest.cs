using NRough.Core.CollectionExtensions;
using NRough.Data.Pivot;
using NRough.Data.Writers;
using NRough.MachineLearning.Classification;
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
    public class PivotServiceTest
    {
        [Test]
        public void Test()
        {
            string path = @"C:\Users\Admin\Desktop\tree-results\2\";            
            string[] filenames = new string[]
            {
                "mylogfile_20170312184914916.txt", //audiology
                "mylogfile_20170312161349528.txt", //breast
                //"mylogfile_20170312155932711.txt", //chess
                //"mylogfile_20170312184433548.txt", //dermatology
                "mylogfile_20170312184334152.txt", //dermatology_modified
                "mylogfile_20170312185123901.txt", //dna
                //"mylogfile_20170312184953826.txt", //dna_modified
                "mylogfile_20170312160833005.txt", //house
                //"mylogfile_20170312184542506.txt", //hypothyroid
                "mylogfile_20170312222328951.txt", //letter.disc
                //"mylogfile_20170312191500356.txt", //letter
                "mylogfile_20170312184900050.txt", //lymphography
                "mylogfile_20170312160849755.txt", //mashroom
                "mylogfile_20170312190232804.txt", //monks-1                
                //"mylogfile_20170312220406657.txt", //monks-2
                "mylogfile_20170312190236807.txt", //monks-3
                //"mylogfile_20170312180716180.txt", //nursery
                //"mylogfile_20170313021600911.txt", //pen
                "mylogfile_20170313011706429.txt", //pen.disc
                //"mylogfile_20170313031255766.txt", //opt                
                //"mylogfile_20170313013503629.txt", //opt.disc
                "mylogfile_20170312161407560.txt", //promoters
                "mylogfile_20170312190240226.txt", //sat.disc
                "mylogfile_20170312161427545.txt", //semeion
                "mylogfile_20170312191429463.txt", //soybean-large
                "mylogfile_20170312160827114.txt", //soybean-small
                "mylogfile_20170312191349477.txt", //spect
                "mylogfile_20170312191356283.txt", //vowel
                "mylogfile_20170312160819692.txt" //zoo                                                                
            };

            var compareBest = new Dictionary<Tuple<string, string>, DataRow>();
            List<string> datasetNames = new List<string>(filenames.Length);
            string[] colNames = new string[] { "acc", "attr", "numrul", "dtha", "dthm" };
            string[] modelNames = null;
            bool first = true;

            string output = Path.Combine(path, "latextable.txt");
            using (StreamWriter outputFile = new StreamWriter(output))
            {                
                foreach (var f in filenames)
                {
                    string filename = Path.Combine(path, f);
                    DataTable dtc = ClassificationResult.ReadResults(filename, '|');

                    if (dtc.Columns.Contains("Column1"))
                        dtc.Columns.Remove("Column1");

                    dtc = ClassificationResult.AverageResults(dtc);

                    string datasetname = dtc.Rows[0]["ds"].ToString().Replace('_', '-');
                    datasetNames.Add(datasetname);
                    dtc.Columns.Remove("ds");
                    
                    DataColumn[] cols = new DataColumn[colNames.Length];
                    for (int i = 0; i < colNames.Length; i++)
                        cols[i] = dtc.Columns[colNames[i]];

                    if (first)
                    {
                        modelNames = dtc.AsEnumerable()
                            .GroupBy(r => r["model"].ToString())
                            .Select(g => g.Key).ToArray();
                    }

                    var pivot = new PivotService();
                    var pivotTable = pivot.Pivot(
                        dtc,
                        dtc.Columns["model"],
                        cols,
                        "-");

                    var dataFormatter = new DataTableLatexTabularFormatter();

                    foreach (string modelName in modelNames)
                    {
                        int maxRowIndex = pivotTable.AsEnumerable()
                        .Select((row, index) => new { row, index })
                        .OrderByDescending(r => Math.Round(r.row.Field<double>(String.Format("{0}-acc", modelName)), 2, MidpointRounding.AwayFromZero))
                        .ThenByDescending(r => r.row.Field<double>("eps"))
                        .Select(r => r.index).First();

                        compareBest.Add(new Tuple<string, string>(datasetname, modelName), pivotTable.Rows[maxRowIndex]);

                        for (int i = 0; i < colNames.Length; i++)
                        {
                            dataFormatter.SetCellProperty(
                                pivotTable.Columns[String.Format("{0}-{1}", modelName, colNames[i])].Ordinal,
                                maxRowIndex,
                                "fontface",
                                "textbf");
                        }
                    }

                    dataFormatter.Caption = String.Format("Reduct based decision tree results ({0})", ConvertDataSetName(datasetname));
                    dataFormatter.Label = String.Format("results:dectree_{0}", ConvertDataSetName(datasetname));
                    dataFormatter.CustomHeader = CustomHeader(dataFormatter.Caption, dataFormatter.Label);
                    dataFormatter.CustomFooter = CustomFooter();

                    string latexTable = dataFormatter.Format("G", pivotTable, null);

                    Console.WriteLine(latexTable);
                    Console.WriteLine();

                    outputFile.WriteLine(latexTable);
                    outputFile.WriteLine();

                    first = false;
                    
                }

                int[][] summary = new int[3][];
                for (int i = 0; i < summary.Length; i++)
                    summary[i] = new int[5];

                string referenceModelName = "C45-Entropy-EBP";
                foreach (var modelName in modelNames
                    .Where(m => m != referenceModelName))
                {
                    summary.SetAll(0);

                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine(@"\begin{table}[ht]		
\centering
\caption{Experimental Results Summary(" + ConvertModelName(modelName) + @")}
\label{table:results:" + ConvertModelName(modelName).ToLower() + @"}
\scriptsize
\begin{tabular}{|l||c|c|c|c|c|}
\hline");

                    sb.Append(@"\textbf{Data}");
                    foreach (var colname in colNames)
                        sb.AppendFormat(@" &  \textbf{{{0}}}", ConvertColName(colname));
                    sb.AppendLine(@"\\ \hline");

                    foreach (var dataset in datasetNames)
                    {
                        DataRow referenceRow = compareBest[new Tuple<string, string>(dataset, referenceModelName)];
                        DataRow rowToValidate = compareBest[new Tuple<string, string>(dataset, modelName)];

                        sb.AppendFormat(@"\hyperref[results:dectree_{0}]{{{0}}}", ConvertDataSetName(dataset));
                        int j = 0;
                        foreach (var colname in colNames)
                        {
                            double newval = Math.Round(
                                rowToValidate.Field<double>(String.Format("{0}-{1}", modelName, colname)),
                                2, MidpointRounding.AwayFromZero);

                            double refval = Math.Round(
                                referenceRow.Field<double>(String.Format("{0}-{1}", referenceModelName, colname)),
                                2, MidpointRounding.AwayFromZero);

                            int comparisonResult = colname == "acc"
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
                    }

                    sb.AppendLine(@"\hline");
                    sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4} & {5}\\ \hline", "+",
                        summary[0][0], summary[0][1], summary[0][2], summary[0][3], summary[0][4]);
                    sb.AppendLine();
                    sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4} & {5}\\ \hline", "-",
                        summary[1][0], summary[1][1], summary[1][2], summary[1][3], summary[1][4]);
                    sb.AppendLine();
                    sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4} & {5}\\ \hline", "o",
                        summary[2][0], summary[2][1], summary[2][2], summary[2][3], summary[2][4]);
                    sb.AppendLine();

                    sb.AppendLine(@"\end{tabular}");
                    sb.AppendLine(@"\end{table}");

                    Console.WriteLine(sb.ToString());

                    outputFile.WriteLine(sb.ToString());
                    outputFile.WriteLine();
                }
            }            
        }            

        private string CustomHeader_OLD(string caption, string label)
        {
            return @"\begin{table}[htbp]
\centering
\caption{" + caption + @"}
\label{" + label + @"}
\rowcolors{4}{gray!25}{white}
\resizebox{\columnwidth}{!}{%
\begin{tabular}{|c|n{1}{2}rrn{2}{2}n{2}{2}|n{1}{2}rrn{2}{2}n{2}{2}|n{1}{2}rrn{2}{2}n{2}{2}|n{1}{2}rrn{2}{2}n{2}{2}|} \hline
 \multirow{2}{*}[-3.5cm]{{\LARGE $\varepsilon$}} & \multicolumn{5}{c|}{\textbf{FULL-ENT}} & \multicolumn{5}{c|}{\textbf{RED-ENT}} & \multicolumn{5}{c|}{\textbf{RED-MAJ}} & \multicolumn{5}{c|}{\textbf{RED-MAJ-EPS}} \\ \cline{2-21}
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}}
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}}\\ \hline";
        }

        private string CustomHeader(string caption, string label)
        {
            return @"\begin{table}[htbp]
\centering
\caption{" + caption + @"}
\label{" + label + @"}
\rowcolors{4}{gray!25}{white}
\resizebox{\columnwidth}{!}{%
\begin{tabular}{|c|lllll|lllll|lllll|lllll|} \hline
 \multirow{2}{*}[-3.5cm]{{\LARGE $\varepsilon$}} & \multicolumn{5}{c|}{\textbf{FULL-ENT}} & \multicolumn{5}{c|}{\textbf{RED-ENT}} & \multicolumn{5}{c|}{\textbf{RED-MAJ}} & \multicolumn{5}{c|}{\textbf{RED-MAJ-EPS}} \\ \cline{2-21}
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}}
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Avg tree depth}} & \rotl{\textbf{Max tree depth}}\\ \hline";
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
            }
            return name;
        }

        private string ConvertColName(string name)
        {
            switch (name)
            {
                case "acc": return "Accuracy";
                case "attr": return "\\#Attributes";
                case "numrul": return "\\#Rules";
                case "dtha": return "Avg tree depth";
                case "dthm": return "Max tree depth";
            }
            return name;
        }


        private string ConvertDataSetName(string name)
        {
            switch (name)
            {
                case "audiology.standardized.2.test": return "audiology";
                case "breast-cancer-wisconsin.2.data": return "breast";
                case "dermatology-modified.data": return "dermatology";
                case "dna.test": return "dna";
                case "house-votes-84.2.data": return "house";
                case "letter.disc.tst": return "letter";
                case "lymphography.all": return "lymphography";
                case "agaricus-lepiota.2.data": return "mushroom";
                case "monks-1.test": return "monks-1";
                case "monks-3.test": return "monks-3";
                case "nursery.2.data": return "nursery";
                case "pendigits.disc.tst": return "pen";
                case "promoters.2.data": return "promoters";
                case "sat.disc.tst": return "sat";
                case "semeion.data": return "semeion";
                case "soybean-large.test": return "soybean-large";
                case "soybean-small.2.data": return "soybean-small";
                case "spect.test": return "spect";
                case "vowel.disc.tst": return "vowel";
                case "zoo.dta": return "zoo";                
            }

            return name;
        }
    }
}
