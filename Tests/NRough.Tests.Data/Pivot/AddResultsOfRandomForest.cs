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

    public class AddResultsOfRandomForest
    {
        [Test]
        public void Test100()
        {
            string path = @"C:\Users\Admin\Source\Workspaces\RoughSets\RoughSets\Infovision.UnitTest.Runner\bin\x64\Release5";
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

            Test(path, filenames, "Generalized majority decision reduct results ({0}) size=100", 100);
        }


        public void Test(string path, string[] filenames, string caption, int size)
        {
            int accuracyDecimals = 2;
            int otherDecimals = 0;           

            var compareBest = new Dictionary<Tuple<string, string>, DataRow>();
            var compareBest2 = new Dictionary<Tuple<string, string>, DataRow>();
            List<string> datasetNames = new List<string>(filenames.Length);            
            string[] colNames = new string[] { "acc", "recallmacro", "precisionmacro", "numrul", "dtha", "dthm" };

            string[] modelNames = null;
            bool first = true;            
                       
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

                //dtc.DeleteRows(r => r.Field<double>("eps") > 0.6);
                //dtc2.DeleteRows(r => r.Field<double>("eps") > 0.6);

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
                pivotTable2.Columns.Remove("M-EPS-dthm");
                pivotTable2.Columns.Remove("M-EPS-dthmdev");

                pivotTable.Columns.Remove("m-PHICAP-NONE-dthm");
                pivotTable2.Columns.Remove("m-PHICAP-NONE-dthm");
                pivotTable2.Columns.Remove("m-PHICAP-NONE-dthmdev");

                pivotTable.Columns.Remove("RandomC45-dthm");
                pivotTable2.Columns.Remove("RandomC45-dthm");
                pivotTable2.Columns.Remove("RandomC45-dthmdev");

                var dataFormatter = new DataTableLatexTabularFormatter();

                pivotTable.Columns["eps"].ExtendedProperties.Add("format", ".00");
                pivotTable.Columns["eps"].ExtendedProperties.Add("formatProvider", System.Globalization.CultureInfo.InvariantCulture);                

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

                    compareBest.Add(new Tuple<string, string>(datasetname, modelName), pivotTable.Rows[maxRowIndex]);
                    compareBest2.Add(new Tuple<string, string>(datasetname, modelName), pivotTable2.Rows[maxRowIndex]);
                }
                    
                first = false;
            }            

            //////////////////////////////// BEST RESULT TABLES ////////////////////////////////////////

            foreach (var modelName in modelNames)
            {
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

                if (modelName == "M-EPS")
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
                    sb2.Append(bestRow.Field<double>("eps").ToString(".00", System.Globalization.CultureInfo.InvariantCulture));

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

                                sb2.Append(((double)bestRow[String.Format("{0}-{1}", modelName, colname)]).ToString(
                                            "0." + new string('#', numOfDec), System.Globalization.CultureInfo.InvariantCulture));
                                sb2.Append(" (");
                                sb2.Append(((double)bestRow[String.Format("{0}-{1}dev", modelName, colname)]).ToString(
                                        "0." + new string('#', accuracyDecimals), System.Globalization.CultureInfo.InvariantCulture));
                                sb2.Append(")");
                            }
                            else
                                sb2.Append(bestRow[String.Format("{0}-{1}", modelName, colname)].ToString());
                        }
                    }

                    sb2.AppendLine(@"\\ \hline");
                }

                sb2.AppendLine(@"\end{tabular}");
                sb2.AppendLine(@"}");
                sb2.AppendLine(@"\end{table}");

                Console.WriteLine(sb2.ToString());

                Console.WriteLine();
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

        private string ConvertModelName(string name)
        {
            switch (name)
            {
                case "C45-Entropy-EBP": return "FULL-ENT";
                case "Reduct-Entropy-EBP": return "RED-ENT";
                case "Reduct-Majority-EBP": return "RED-MAJ";
                case "RedGam-Majority-EBP": return "RED-MAJ-EPS";

                case "RandomC45": return "RF-C4.5";

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
