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
    public class PivotServiceTest3
    {
        [Test]
        public void Test1()
        {            
            string path = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\ExceptionRulesTest\bin\x64\Release3\log\";
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
            string path = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\ExceptionRulesTest\bin\x64\Release3\log\";
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
            int accuracyDecimals = 3;
            int otherDecimals = 1;

            var compareBest = new Dictionary<Tuple<string, string>, DataRow>();
            var compareBest2 = new Dictionary<Tuple<string, string>, DataRow>();
            List<string> datasetNames = new List<string>(filenames.Length);
            string[] colNames = new string[] { "acc", "attr", "numrul", "dtha", "dthm" };
            //string[] colNames2 = new string[] { "acc", "accdev", "attr", "attrdev", "numrul", "numruldev", "dtha", "dthadev", "dthm", "dthmdev" };
            string[] modelNames = null;
            bool first = true;

            string output = Path.Combine(path, "latextable-" + size.ToString() + ".tex");

            using (FileStream fileStreamWrite =
                new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter outputFile = new StreamWriter(fileStreamWrite))
                {
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

                        var dtc1 = ClassificationResult.AverageResults(dtc);
                        var dtc2 = ClassificationResult.AverageResults2(dtc);

                        dtc = dtc1;

                        //Console.WriteLine(new DataTableLatexTabularFormatter().Format("", dtc, null));

                        datasetNames.Add(datasetname);
                        dtc.Columns.Remove("ds");

                        DataColumn[] cols = new DataColumn[colNames.Length];
                        for (int i = 0; i < colNames.Length; i++)
                            cols[i] = dtc.Columns[colNames[i]];

                        DataColumn[] cols2 = new DataColumn[colNames.Length * 2];
                        for (int i = 0; i < colNames.Length; i++)
                        {
                            cols2[i*2] = dtc2.Columns[colNames[i]];
                            cols2[(i*2)+1] = dtc2.Columns[colNames[i]+"dev"];
                        }

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

                        var pivotTable2 = pivot.Pivot(
                            dtc2,
                            dtc2.Columns["model"],
                            cols2,
                            "-");

                        pivotTable.Columns.Remove("M-EPS-dthm");
                        pivotTable.Columns.Remove("m-PHICAP-NONE-dthm");

                        pivotTable2.Columns.Remove("M-EPS-dthm");
                        pivotTable2.Columns.Remove("m-PHICAP-NONE-dthm");
                        pivotTable2.Columns.Remove("M-EPS-dthmdev");
                        pivotTable2.Columns.Remove("m-PHICAP-NONE-dthmdev");

                        var dataFormatter = new DataTableLatexTabularFormatter();

                        pivotTable.Columns["eps"].ExtendedProperties.Add("format", "0.00");
                        pivotTable.Columns["eps"].ExtendedProperties.Add("formatProvider", System.Globalization.CultureInfo.InvariantCulture);

                        foreach (string modelName in modelNames)
                        {
                            int maxRowIndex = pivotTable.AsEnumerable()
                                .Select((row, index) => new { row, index })
                                .OrderByDescending(r => Math.Round(r.row.Field<double>(String.Format("{0}-acc", modelName)), accuracyDecimals, MidpointRounding.AwayFromZero))
                                .ThenByDescending(r => r.row.Field<double>("eps"))
                                .Select(r => r.index).First();

                            compareBest.Add(new Tuple<string, string>(datasetname, modelName), pivotTable.Rows[maxRowIndex]);
                            compareBest2.Add(new Tuple<string, string>(datasetname, modelName), pivotTable2.Rows[maxRowIndex]);

                            for (int i = 0; i < colNames.Length; i++)
                            {
                                if (!pivotTable.Columns.Contains(String.Format("{0}-{1}", modelName, colNames[i])))
                                    continue;

                                if (colNames[i] == "acc")
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

                                int numOfDec = otherDecimals;
                                if (colname == "acc")
                                    numOfDec = accuracyDecimals;

                                double newval = Math.Round(
                                    rowToValidate.Field<double>(String.Format("{0}-{1}", modelName, colname)),
                                    numOfDec, MidpointRounding.AwayFromZero);

                                double refval = Math.Round(
                                    referenceRow.Field<double>(String.Format("{0}-{1}", referenceModelName, colname)),
                                    accuracyDecimals, MidpointRounding.AwayFromZero);

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
                            sb.Insert(0, @"\begin{table}[!htbp]		
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
                            sb.Insert(0, @"\begin{table}[!htbp]		
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

                    //////////////////////////////// BEST RESULT TABLES ////////////////////////////////////////

                    foreach (var modelName in modelNames)
                    {
                        var firstRow = compareBest2.FirstOrDefault().Value;

                        StringBuilder sb2 = new StringBuilder();

                        int cols = colNames.Count(s => firstRow.Table.Columns.Contains(String.Format("{0}-{1}", modelName, s)));

                        if (cols == 5)
                        {
                            sb2.Append(@"\begin{table}[!htbp]
\centering
\caption{Best accuracy results (" + ConvertModelName(modelName) + @")}
\label{table:accresults" + size.ToString() + ":" + modelName.ToLower() + @"}
\scriptsize
\begin{tabular}{|l||l|l|l|l|l|l|}
\hline" + Environment.NewLine);
                        }
                        else
                        {
                            sb2.Append(@"\begin{table}[!htbp]
\centering
\caption{Best accuracy results (" + ConvertModelName(modelName) + @")}
\label{table:accresults" + size.ToString() + ":" + modelName.ToLower() + @"}
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
                            sb2.Append(ConvertDataSetName(dataset));
                            
                            DataRow bestRow = compareBest2[new Tuple<string, string>(dataset, modelName)];

                            sb2.Append(" & ");
                            sb2.Append(bestRow["eps"].ToString());

                            foreach (var colname in colNames)
                            {
                                if (bestRow.Table.Columns.Contains(String.Format("{0}-{1}", modelName, colname)))
                                {
                                    sb2.Append(" & ");
                                    if (bestRow[String.Format("{0}-{1}", modelName, colname)] is double)
                                    {
                                        int numOfDec = otherDecimals;
                                        if (colname == "acc")
                                            numOfDec = accuracyDecimals;

                                        sb2.Append(((double)bestRow[String.Format("{0}-{1}", modelName, colname)]).ToString(
                                                    "0." + new string('#', numOfDec), System.Globalization.CultureInfo.InvariantCulture));
                                        sb2.Append(" (");
                                        sb2.Append(((double)bestRow[String.Format("{0}-{1}dev", modelName, colname)]).ToString(
                                                "0." + new string('#', numOfDec), System.Globalization.CultureInfo.InvariantCulture));
                                        sb2.Append(")");
                                    }
                                    else
                                        sb2.Append(bestRow[String.Format("{0}-{1}", modelName, colname)].ToString());
                                }
                            }

                            sb2.AppendLine(@"\\ \hline");
                        }

                        sb2.AppendLine(@"\end{tabular}");
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
                            if (colname == "acc")
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

                                if (colname == "acc")
                                    wilcoxon.AlternativeHypothesis = HypothesisType.FirstIsSmallerThanSecond;
                                else
                                    wilcoxon.AlternativeHypothesis = HypothesisType.FirstIsGreaterThanSecond;

                                wilcoxon.Alpha = 0.05;
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
                        if (colname == "acc")
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

                        Console.WriteLine(data[i].ToStr2d(", ", Environment.NewLine, true, "0.###", System.Globalization.CultureInfo.InvariantCulture));
                    }
                    
                                                             
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
\resizebox*{\columnwidth}{\dimexpr\textheight-2em-\lineskip\relax}{%
\begin{tabular}{|c|llll|lllll|lllll|llll|} 
\hline
\multirow{2}{*}[-1.5cm]{{\LARGE \begin{tabular}{c}$\varepsilon$ \\ $\phi$ \end{tabular}}} 
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
