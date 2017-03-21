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
    public class PivotServiceTest2
    {
        [Test]
        public void Test1()
        {
            //string path = @"C:\Users\Admin\Desktop\tree-results\3\";
            string path = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\ExceptionRulesTest\bin\x64\Release\log\";
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
                "mashroom-1.result", //mashroom
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

            Test(path, filenames, "Generalized majority decision reduct results ({0})", 1);
        }

        [Test]
        public void Test100()
        {
            //string path = @"C:\Users\Admin\Desktop\tree-results\4\";
            string path = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\ExceptionRulesTest\bin\x64\Release2\log\";
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
                "mashroom-100.result", //mashroom
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

            Test(path, filenames, "Generalized majority decision reduct results ({0}) size=100", 100);
        }
        
        public void Test(string path, string[] filenames, string caption, int size)
        {            
            var compareBest = new Dictionary<Tuple<string, string>, DataRow>();
            List<string> datasetNames = new List<string>(filenames.Length);
            string[] colNames = new string[] { "acc", "attr", "numrul", "dtha", "dthm" };
            string[] modelNames = null;
            bool first = true;

            string output = Path.Combine(path, "latextable.tex");
            using (StreamWriter outputFile = new StreamWriter(output))
            {
                foreach (var f in filenames)
                {                    
                    string filename = Path.Combine(path, f);

                    if (!File.Exists(filename))
                        continue;
                        
                    DataTable dtc = ClassificationResult.ReadResults(filename, '|');

                    if (dtc.Columns.Contains("Column1"))
                        dtc.Columns.Remove("Column1");

                    for (int i = 0; i < dtc.Rows.Count; i++)
                        dtc.Rows[i]["numrul"] = dtc.Rows[i].Field<double>("numrul") + dtc.Rows[i].Field<double>("dthm");

                    dtc = ClassificationResult.AverageResults(dtc);

                    string datasetname = dtc.Rows[0]["ds"].ToString().Replace('_', '-');
                    if (datasetname.Substring(datasetname.Length - 5, 3) == "TST")
                        datasetname = datasetname.Substring(0, datasetname.Length - 6);

                    datasetNames.Add(datasetname);
                    dtc.Columns.Remove("ds");

                    dtc.AsEnumerable()
                        .Where(r => r.Field<string>("model") == "m-EPS-CAP")
                        .ToList().ForEach(row => row.Delete());

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

                    pivotTable.Columns.Remove("M-EPS-dthm");
                    pivotTable.Columns.Remove("m-PHICAP-NONE-dthm");

                    for (int i = 0; i < pivotTable.Rows.Count; i++)
                    {
                        pivotTable.Rows[i]["m-PHICAP-EXEP-attr"] = pivotTable.Rows[i].Field<double>("m-PHICAP-GAPS-attr");
                        pivotTable.Rows[i]["m-PHICAP-GAPS-dtha"] = pivotTable.Rows[i].Field<double>("m-PHICAP-EXEP-dtha");
                    }

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
                            if (!pivotTable.Columns.Contains(String.Format("{0}-{1}", modelName, colNames[i])))
                                continue;

                            dataFormatter.SetCellProperty(
                                pivotTable.Columns[String.Format("{0}-{1}", modelName, colNames[i])].Ordinal,
                                maxRowIndex,
                                "fontface",
                                "textbf");
                        }
                    }

                    dataFormatter.Caption = String.Format(caption, ConvertDataSetName(datasetname));
                    dataFormatter.Label = String.Format("results:gmdr{1}_{0}", ConvertDataSetName(datasetname), size);
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

                string referenceModelName = "M-EPS";
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
                        first = false;
                    }

                    if (j == 4)
                    {
                        sb.Insert(0, @"\begin{table}[ht]		
\centering
\caption{Experimental Results Summary(" + ConvertModelName(modelName) + @")}
\label{table:results" + size.ToString() + ":" + modelName.ToLower() + @"}
\scriptsize
\begin{tabular}{|l||c|c|c|c|}
\hline" + Environment.NewLine);

                        sb.AppendLine(@"\hline");
                        sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4}\\ \hline", "+",
                            summary[0][0], summary[0][1], summary[0][2], summary[0][3]);
                        sb.AppendLine();
                        sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4}\\ \hline", "-",
                            summary[1][0], summary[1][1], summary[1][2], summary[1][3]);
                        sb.AppendLine();
                        sb.AppendFormat(@"\multicolumn{{1}}{{|c||}}{{{0}}} & {1} & {2} & {3} & {4}\\ \hline", "o",
                            summary[2][0], summary[2][1], summary[2][2], summary[2][3]);
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.Insert(0, @"\begin{table}[ht]		
\centering
\caption{Experimental Results Summary(" + ConvertModelName(modelName) + @")}
\label{table:results:" + modelName.ToLower() + @"}
\scriptsize
\begin{tabular}{|l||c|c|c|c|c|}
\hline" + Environment.NewLine);

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
                    }

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
\begin{tabular}{|c|lllll|lllll|lllll|lllll|lllll|} \hline
\multirow{2}{*}[-3.5cm]{{\LARGE $\varepsilon$}} & \multicolumn{5}{c|}{\textbf{$(M,\varepsilon)$}} & \multicolumn{5}{c|}{\textbf{$(m^{\varepsilon},\cap)$}} & \multicolumn{5}{c|}{\textbf{$(m^{\phi},\cap)$-Exep}} & \multicolumn{5}{c|}{\textbf{$(m^{\phi},\cap)$-Gaps}} & \multicolumn{5}{c|}{\textbf{$(m^{\phi},\cap)$-None}}\\ \cline{2-26}
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Rules length}} & \rotl{\textbf{\#Exceptions}}
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Rules length}} & \rotl{\textbf{\#Exceptions}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Rules length}} & \rotl{\textbf{\#Exceptions}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Rules length}} & \rotl{\textbf{\#Exceptions}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{\#Attributes}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Rules length}} & \rotl{\textbf{\#Exceptions}}\\ \hline";
        }

        private string CustomHeader(string caption, string label)
        {
            return @"\begin{table}[htbp]
\centering
\caption{" + caption + @"}
\label{" + label + @"}
\rowcolors{4}{gray!25}{white}
\resizebox{\columnwidth}{!}{%
\begin{tabular}{|c|llll|lllll|lllll|llll|} 
\hline
\multirow{2}{*}[-3.5cm]{{\LARGE $\varepsilon$}} 
& \multicolumn{4}{c|}{\textbf{$(M,\varepsilon)$}} 
& \multicolumn{5}{c|}{\textbf{$(m^{\phi},\cap)$-Exep}} 
& \multicolumn{5}{c|}{\textbf{$(m^{\phi},\cap)$-Gaps}} 
& \multicolumn{4}{c|}{\textbf{$(m^{\phi},\cap)$-None}}\\ 
\cline{2-19}
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Reduct length}} & \rot{\textbf{\#Rules}} & \rotl{\textbf{Rules length}}  
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Reduct length}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Rules length}} & \rotl{\textbf{\#Exceptions}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Reduct length}} & \rot{\textbf{\#Rules}} & \rot{\textbf{Rules length}} & \rotl{\textbf{\#Exceptions}} 
 & \rot{\textbf{Accuracy}} & \rot{\textbf{Reduct length}} & \rot{\textbf{\#Rules}} & \rotl{\textbf{Rules length}} \\ \hline";
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
