using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using Infovision.Datamining.Benchmark;
using Infovision.Datamining.Roughset;
using System.Reflection;

using GenericParsing;
using System.IO;

namespace LoadSQLRoughsetDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();

            //p.InsertDB(p.GetTableDataset());
            //p.InsertDB(p.GetTableWeightingType());
            //p.InsertDB(p.GetTableDecisionRuleMeasure());
            //p.InsertDB(p.GetTableModelType());
            //p.InsertDB(p.GetTableExceptionRuleType());

            //string path = @"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\VotingVsRuleInduction\bin\x64\Release\results";
            /*
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "dna.result"), p.DatasetToInt("dna"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "zoo.result"), p.DatasetToInt("zoo"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "breast.result"), p.DatasetToInt("breast"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "soybean-small.result"), p.DatasetToInt("soybean-small"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "soybean-large.result"), p.DatasetToInt("soybean-large"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "house.result"), p.DatasetToInt("house"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "audiology.result"), p.DatasetToInt("audiology"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "promoters.result"), p.DatasetToInt("promoters"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "spect.result"), p.DatasetToInt("spect"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "chess.result"), p.DatasetToInt("chess"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "mashroom.result"), p.DatasetToInt("mashroom"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "semeion.result"), p.DatasetToInt("semeion"), 1));            
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "letter.result"), p.DatasetToInt("letter"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "pen.result"), p.DatasetToInt("pen"), 1));
            p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "opt.result"), p.DatasetToInt("opt"), 1));
            
            */

            //p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "nursery.result"), p.DatasetToInt("nursery"), 1));
            //p.InsertDB(p.GetTableResult_VotingVsRuleInduction(Path.Combine(path, "connect.result"), p.DatasetToInt("connect"), 1));

            string path2 = @"C:\Users\Sebastian\Source\Workspaces\RoughSets\RoughSets\MajorityGeneralizedDecisionTest\bin\x64\Release\results";
            int experimentId = 7;

            
            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "dna.result"), p.DatasetToInt("dna"), experimentId));
            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "zoo.result"), p.DatasetToInt("zoo"), experimentId));
            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "breast.result"), p.DatasetToInt("breast"), experimentId));


            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "soybean-small.result"), p.DatasetToInt("soybean-small"), experimentId));
            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "soybean-large.result"), p.DatasetToInt("soybean-large"), experimentId));
            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "house.result"), p.DatasetToInt("house"), experimentId));
            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "audiology.result"), p.DatasetToInt("audiology"), experimentId));
            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "promoters.result"), p.DatasetToInt("promoters"), experimentId));
            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "spect.result"), p.DatasetToInt("spect"), experimentId));
            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "chess.result"), p.DatasetToInt("chess"), experimentId));


            p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "letter.result"), p.DatasetToInt("letter"), experimentId));
            p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "pen.result"), p.DatasetToInt("pen"), experimentId));
            p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "opt.result"), p.DatasetToInt("opt"), experimentId));
            
            p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "mashroom.result"), p.DatasetToInt("mashroom"), experimentId));
            p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "semeion.result"), p.DatasetToInt("semeion"), experimentId));
            p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "nursery.result"), p.DatasetToInt("nursery"), experimentId));

            p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "monks-1.result"), p.DatasetToInt("monks-1"), experimentId));
            p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "monks-2.result"), p.DatasetToInt("monks-2"), experimentId));
            p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "monks-3.result"), p.DatasetToInt("monks-3"), experimentId));

            
            //p.InsertDB(p.GetTableResult_MajorityGeneralizedDecisionTest(Path.Combine(path2, "connect.result"), p.DatasetToInt("connect"), experimentId));
        }

        public void InsertDB(DataTable table)
        {
            string connectionString = @"Server=HUJOLOPTER\SQL2014;Database=RoughDB;Integrated Security=True;";                                                          
            using (SqlConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                {
                    s.DestinationTableName = table.TableName;
                    s.BulkCopyTimeout = 300; //5 minutes

                    foreach (var column in table.Columns)
                        s.ColumnMappings.Add(column.ToString(), column.ToString());

                    s.WriteToServer(table);
                }
            }
        }

        public DataTable GetTableDataset()
        {
            DataTable table = new DataTable("dbo.DATASET");

            DataColumn idColumn = new DataColumn();
            idColumn.DataType = Type.GetType("System.Int32");
            idColumn.ColumnName = "DATASETID";

            DataColumn nameColumn = new DataColumn();
            nameColumn.ColumnName = "NAME";

            table.Columns.Add(idColumn);
            table.Columns.Add(nameColumn);

            int i = 0;
            foreach (var kvp in BenchmarkDataHelper.GetDataFiles())
            {
                DataRow dataSetRow = table.NewRow();

                dataSetRow[idColumn.ColumnName] = i++;
                dataSetRow[nameColumn.ColumnName] = kvp.Key;

                table.Rows.Add(dataSetRow);
            }

            return table;
        }

        public DataTable GetTableWeightingType()
        {
            DataTable table = new DataTable("dbo.WEIGHTINGTYPE");

            DataColumn idColumn = new DataColumn();
            idColumn.DataType = Type.GetType("System.Int32");
            idColumn.ColumnName = "WEIGHTINGTYPEID";

            DataColumn nameColumn = new DataColumn();
            nameColumn.ColumnName = "NAME";

            table.Columns.Add(idColumn);
            table.Columns.Add(nameColumn);

            int i = 0;
            DataRow dataSetRow = null;

            dataSetRow = table.NewRow();
            dataSetRow[idColumn.ColumnName] = i++;
            dataSetRow[nameColumn.ColumnName] = "Majority";
            table.Rows.Add(dataSetRow);

            dataSetRow = table.NewRow();
            dataSetRow[idColumn.ColumnName] = i++;
            dataSetRow[nameColumn.ColumnName] = "Relative";
            table.Rows.Add(dataSetRow);            

            return table;
        }

        public DataTable GetTableDecisionRuleMeasure()
        {
            DataTable table = new DataTable("dbo.DECISIONRULEMEASURE");

            DataColumn idColumn = new DataColumn();
            idColumn.DataType = Type.GetType("System.Int32");
            idColumn.ColumnName = "RULEQUALITYID";

            DataColumn nameColumn = new DataColumn();
            nameColumn.ColumnName = "NAME";

            table.Columns.Add(idColumn);
            table.Columns.Add(nameColumn);

            int i = 0;
            DataRow dataSetRow = null;

            var methods = typeof(RuleQuality).GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var methodInfo in methods)
            {
                dataSetRow = table.NewRow();
                dataSetRow[idColumn.ColumnName] = i++;
                dataSetRow[nameColumn.ColumnName] = methodInfo.Name;
                table.Rows.Add(dataSetRow);
            }
                        
            return table;
        }

        public DataTable GetTableModelType()
        {
            DataTable table = new DataTable("dbo.MODELTYPE");

            DataColumn idColumn = new DataColumn();
            idColumn.DataType = Type.GetType("System.Int32");
            idColumn.ColumnName = "MODELTYPEID";

            DataColumn nameColumn = new DataColumn();
            nameColumn.ColumnName = "NAME";

            table.Columns.Add(idColumn);
            table.Columns.Add(nameColumn);

            int i = 0;
            DataRow dataSetRow = null;

            var factoryKeys = ReductFactory.GetReductFactoryKeys();
                        
            foreach (var factoryKey in factoryKeys)
            {
                dataSetRow = table.NewRow();
                dataSetRow[idColumn.ColumnName] = i++;
                dataSetRow[nameColumn.ColumnName] = factoryKey;
                table.Rows.Add(dataSetRow);
            }

            return table;
        }

        public DataTable GetTableExceptionRuleType()
        {
            DataTable table = new DataTable("dbo.EXCEPTIONRULETYPE");

            DataColumn idColumn = new DataColumn();
            idColumn.DataType = Type.GetType("System.Int32");
            idColumn.ColumnName = "EXCEPTIONTYPEID";

            DataColumn nameColumn = new DataColumn();
            nameColumn.ColumnName = "NAME";

            table.Columns.Add(idColumn);
            table.Columns.Add(nameColumn);

            int i = 1;
            DataRow dataSetRow = null;

            dataSetRow = table.NewRow();
            dataSetRow[idColumn.ColumnName] = i++;
            dataSetRow[nameColumn.ColumnName] = "NONE";
            table.Rows.Add(dataSetRow);

            dataSetRow = table.NewRow();
            dataSetRow[idColumn.ColumnName] = i++;
            dataSetRow[nameColumn.ColumnName] = "GAPS";
            table.Rows.Add(dataSetRow);

            dataSetRow = table.NewRow();
            dataSetRow[idColumn.ColumnName] = i++;
            dataSetRow[nameColumn.ColumnName] = "RULES";
            table.Rows.Add(dataSetRow);

            return table;
        }

        private DataColumn AddColumn(DataTable table, string name, string type)
        {
            DataColumn c = new DataColumn();
            c.DataType = Type.GetType(type);
            c.ColumnName = name;
            table.Columns.Add(c);
            return c;
        }

        private DataTable DefineResultTable()
        {
            DataTable table = new DataTable("dbo.RESULT");

            this.AddColumn(table, "RESULTID", "System.Int64");
            this.AddColumn(table, "EXPERIMENTID", "System.Int32");
            this.AddColumn(table, "DATASETID", "System.Int32");
            this.AddColumn(table, "FOLDID", "System.Int32");
            this.AddColumn(table, "TESTID", "System.Int32");
            this.AddColumn(table, "ENSEMBLESIZE", "System.Int32");
            this.AddColumn(table, "IDENTIFICATIONTYPEID", "System.Int32");
            this.AddColumn(table, "VOTINGTYPEID", "System.Int32");
            this.AddColumn(table, "MODELTYPEID", "System.Int32");
            this.AddColumn(table, "EPSILON", "System.Double");
            this.AddColumn(table, "CLASSIFIED", "System.Int32");
            this.AddColumn(table, "MISCLASSIFIED", "System.Int32");
            this.AddColumn(table, "UNCLASSIFIED", "System.Int32");
            this.AddColumn(table, "WEIGHTCLASSIFIED", "System.Double");
            this.AddColumn(table, "WEIGHTUNCLASSIFIED", "System.Double");
            this.AddColumn(table, "WEIGHTMISCLASSIFIED", "System.Double");
            this.AddColumn(table, "ACCURACY", "System.Double");
            this.AddColumn(table, "BALANCEDACCURACY", "System.Double");
            this.AddColumn(table, "CONFIDENCE", "System.Double");
            this.AddColumn(table, "COVERAGE", "System.Double");
            this.AddColumn(table, "AVERAGEREDUCTLENGTH", "System.Double");
            this.AddColumn(table, "MODELCREATIONTIME", "System.Int64");
            this.AddColumn(table, "CLASSIFICATIONTIME", "System.Int64");
            this.AddColumn(table, "WEIGHTINGTYPEID", "System.Int32");
            this.AddColumn(table, "EXCEPTIONRULETYPEID", "System.Int32");

            this.AddColumn(table, "EXCEPTIONRULEHITCOUNTER", "System.Int32");
            this.AddColumn(table, "EXCEPTIONRULELENGTHSUM", "System.Int32");
            this.AddColumn(table, "STANDARDRULEHITCOUNTER", "System.Int32");
            this.AddColumn(table, "STANDARDRULELENGTHSUM", "System.Int32");

            this.AddColumn(table, "MRISLICETRN", "System.Int32");
            this.AddColumn(table, "MRISLICETST", "System.Int32");

            return table;
        }

        public DataTable GetTableResult_MajorityGeneralizedDecisionTest(string filename, int datasetid, int experimentid)
        {
            DataTable table = this.DefineResultTable();

            DataTable tmpTable;
            using (GenericParserAdapter gpa = new GenericParserAdapter(filename))
            {
                gpa.ColumnDelimiter = "|".ToCharArray()[0];
                gpa.FirstRowHasHeader = true;
                gpa.IncludeFileLineNumber = false;
                gpa.TrimResults = true;

                tmpTable = gpa.GetDataTable();
            }

            long i = 1;
            DataRow dataSetRow = null;
            foreach (DataRow row in tmpTable.Rows)
            {
                dataSetRow = table.NewRow();

                dataSetRow["RESULTID"] = i;
                dataSetRow["EXPERIMENTID"] = experimentid;
                dataSetRow["DATASETID"] = datasetid;

                dataSetRow["FOLDID"] = Int32.Parse(row["Fold"].ToString());
                dataSetRow["TESTID"] = Int32.Parse(row["Test"].ToString());
                dataSetRow["ENSEMBLESIZE"] = Int32.Parse(row["EnsembleSize"].ToString());
                dataSetRow["IDENTIFICATIONTYPEID"] = this.RuleQualityToInt(row["Ident"].ToString());
                dataSetRow["VOTINGTYPEID"] = this.RuleQualityToInt(row["Vote"].ToString());
                dataSetRow["MODELTYPEID"] = this.FactoryKeyToInt(row["Factory"].ToString());
                dataSetRow["EPSILON"] = Double.Parse(row["Epsilon"].ToString());
                dataSetRow["CLASSIFIED"] = Int32.Parse(row["Classified"].ToString());
                dataSetRow["MISCLASSIFIED"] = Int32.Parse(row["Misclassified"].ToString());
                dataSetRow["UNCLASSIFIED"] = Int32.Parse(row["Unclassified"].ToString());
                dataSetRow["WEIGHTCLASSIFIED"] = Double.Parse(row["WeightClassified"].ToString());
                dataSetRow["WEIGHTUNCLASSIFIED"] = Double.Parse(row["WeightMisclassified"].ToString());
                dataSetRow["WEIGHTMISCLASSIFIED"] = Double.Parse(row["WeightUnclassified"].ToString());
                dataSetRow["ACCURACY"] = Double.Parse(row["Accuracy"].ToString());
                dataSetRow["BALANCEDACCURACY"] = Double.Parse(row["BalancedAccuracy"].ToString());
                dataSetRow["CONFIDENCE"] = Double.Parse(row["Confidence"].ToString());
                dataSetRow["COVERAGE"] = Double.Parse(row["Coverage"].ToString());
                dataSetRow["AVERAGEREDUCTLENGTH"] = Double.Parse(row["AverageReductLength"].ToString());
                dataSetRow["MODELCREATIONTIME"] = Int64.Parse(row["ModelCreationTime"].ToString());
                dataSetRow["CLASSIFICATIONTIME"] = Int64.Parse(row["ClassificationTime"].ToString());                
                dataSetRow["WEIGHTINGTYPEID"] = this.WeightTypeToInt_MajorityGeneralizedDecisionTest(row["Weight"].ToString());

                dataSetRow["MODELTYPEID"] = this.FactoryKeyToInt_MajorityGeneralizedDecisionTest(
                    row["Factory"].ToString(), row["Weight"].ToString());

                dataSetRow["EXCEPTIONRULETYPEID"] = ExceptionRuleToInt_MajorityGeneralizedDecisionTest(
                    row["Factory"].ToString(), row["Weight"].ToString());

                dataSetRow["EXCEPTIONRULEHITCOUNTER"] = Int32.Parse(row["ExceptionRuleHitCounter"].ToString());
                dataSetRow["EXCEPTIONRULELENGTHSUM"] = Int32.Parse(row["ExceptionRuleLengthSum"].ToString());
                dataSetRow["STANDARDRULEHITCOUNTER"] = Int32.Parse(row["StandardRuleHitCounter"].ToString());
                dataSetRow["STANDARDRULELENGTHSUM"] = Int32.Parse(row["StandardRuleLengthSum"].ToString());

                dataSetRow["MRISLICETRN"] = 0;
                dataSetRow["MRISLICETST"] = 0;

                i++;

                table.Rows.Add(dataSetRow);
            }

            return table;
        }

        private int FactoryKeyToInt_MajorityGeneralizedDecisionTest(string factoryKey, string weight)
        {
            int result = 0;
            switch (factoryKey)
            {
                case "NOEX": 
                    result = 11; //GeneralizedMajorityDecisionApproximate
                    break;

                case "GAPS": 
                    result = 11; //GeneralizedMajorityDecisionApproximate
                    break;
                
                case "EXEP": 
                    result = 11; //GeneralizedMajorityDecisionApproximate
                    break;
                
                
                case "GMDR": 
                    result = 12; //GeneralizedMajorityDecision
                    break;
                
                
                case "ADR":
                    switch (weight)
                    {
                        case "Majority": result = 16; break; //ApproximateReductMajorityWeights
                        case "Relative": result = 17; break; //ApproximateReductRelativeWeights
                    }
                    break;                
            }
            return result;
        }

        private int ExceptionRuleToInt_MajorityGeneralizedDecisionTest(string factoryKey, string weight)
        {
            int result = 0;
            switch (factoryKey)
            {
                case "NOEX": result = 1; break;
                case "GAPS": result = 2; break;
                case "EXEP": result = 3; break;
                case "GMDR": result = 1; break;
                case "ADR" : result = 1; break;
            }
            return result;
        }

        private int WeightTypeToInt_MajorityGeneralizedDecisionTest(string factoryKey)
        {
            int result = 0;
            switch (factoryKey)
            {
                case "Majority": result = 1; break;
                case "Relative": result = 2; break;                
            }
            return result;
        }

        public DataTable GetTableResult_VotingVsRuleInduction(string filename, int datasetid, int experimentid)
        {
            DataTable table = this.DefineResultTable();                                               

            DataTable tmpTable;
            using (GenericParserAdapter gpa = new GenericParserAdapter(filename))
            {
                gpa.ColumnDelimiter = "|".ToCharArray()[0];
                gpa.FirstRowHasHeader = true;
                gpa.IncludeFileLineNumber = false;
                gpa.TrimResults = true;

                tmpTable = gpa.GetDataTable();
            }

            long i = 1;
            DataRow dataSetRow = null;
            foreach (DataRow row in tmpTable.Rows)
            {
                dataSetRow = table.NewRow();

                dataSetRow["RESULTID"] = i;
                dataSetRow["EXPERIMENTID"] = experimentid;
                dataSetRow["DATASETID"] = datasetid;                
                dataSetRow["FOLDID"] = Int32.Parse(row["Fold"].ToString());
                dataSetRow["TESTID"] = Int32.Parse(row["Test"].ToString());
                dataSetRow["ENSEMBLESIZE"] = Int32.Parse(row["NumberOfReducts"].ToString());
                dataSetRow["IDENTIFICATIONTYPEID"] = this.RuleQualityToInt(row["IdentificationType"].ToString());
                dataSetRow["VOTINGTYPEID"] = this.RuleQualityToInt(row["VoteType"].ToString());
                dataSetRow["MODELTYPEID"] = this.FactoryKeyToInt(row["FactoryKey"].ToString());
                dataSetRow["EPSILON"] = Double.Parse(row["Epsilon"].ToString());
                dataSetRow["CLASSIFIED"] = Int32.Parse(row["Classified"].ToString());
                dataSetRow["MISCLASSIFIED"] = Int32.Parse(row["Misclassified"].ToString());
                dataSetRow["UNCLASSIFIED"] = Int32.Parse(row["Unclassified"].ToString());
                dataSetRow["WEIGHTCLASSIFIED"] = Double.Parse(row["WeightClassified"].ToString());
                dataSetRow["WEIGHTUNCLASSIFIED"] = Double.Parse(row["WeightMisclassified"].ToString());
                dataSetRow["WEIGHTMISCLASSIFIED"] = Double.Parse(row["WeightUnclassified"].ToString());
                dataSetRow["ACCURACY"] = Double.Parse(row["Accuracy"].ToString());
                dataSetRow["BALANCEDACCURACY"] = Double.Parse(row["BalancedAccuracy"].ToString());
                dataSetRow["CONFIDENCE"] = Double.Parse(row["Confidence"].ToString());
                dataSetRow["COVERAGE"] = Double.Parse(row["Coverage"].ToString());
                dataSetRow["AVERAGEREDUCTLENGTH"] = Double.Parse(row["AverageReductLength"].ToString());
                dataSetRow["MODELCREATIONTIME"] = Int64.Parse(row["ModelCreationTime"].ToString());
                dataSetRow["CLASSIFICATIONTIME"] = Int64.Parse(row["ClassificationTime"].ToString());
                dataSetRow["WEIGHTINGTYPEID"] = this.WeightTypeToInt_VotingVsRuleInduction(row["FactoryKey"].ToString());
                dataSetRow["EXCEPTIONRULETYPEID"] = 1;

                dataSetRow["EXCEPTIONRULEHITCOUNTER"] = Int32.Parse(row["ExceptionRuleHitCounter"].ToString());
                dataSetRow["EXCEPTIONRULELENGTHSUM"] = Int32.Parse(row["ExceptionRuleLengthSum"].ToString());
                dataSetRow["STANDARDRULEHITCOUNTER"] = Int32.Parse(row["StandardRuleHitCounter"].ToString());
                dataSetRow["STANDARDRULELENGTHSUM"] = Int32.Parse(row["StandardRuleLengthSum"].ToString());

                dataSetRow["MRISLICETRN"] = 0;
                dataSetRow["MRISLICETST"] = 0;

                i++;

                table.Rows.Add(dataSetRow);
            }

            return table;
        }

        private int RuleQualityToInt(string rule)
        {
            int result = 0;
            switch (rule)
            {
                case "Support": result = 1; break;
                case "SupportW":	result=	2; break;
                case "Confidence":	result=	3; break;
                case "ConfidenceW":	result=	4; break;
                case "Coverage":	result=	5; break;
                case "CoverageW":	result=	6; break;
                case "Ratio":	result=	7; break;
                case "RatioW":	result=	8; break;
                case "Strength":	result=	9; break;
                case "StrengthW":	result=	10; break;
                case "ConfidenceRelative":	result=	11; break;
                case "ConfidenceRelativeW":	result=	12; break;
                case "SingleVote": result = 13; break;
            }

            return result;
        }

        private int FactoryKeyToInt(string factoryKey)
        {
            int result = 0;
            switch(factoryKey)
            {
                case "Bireduct": result =	1; break;
                case "BireductRelative": result =	2; break;
                case "GammaBireduct": result =	3; break;
                case "ReductEnsembleBoosting": result =	4; break;
                case "ReductEnsembleBoostingVarEps": result =	5; break;
                case "ReductEnsembleBoostingVarEpsWithAttributeDiversity": result =	6; break;
                case "ReductEnsembleBoostingWithAttributeDiversity": result =	7; break;
                case "ReductEnsembleBoostingWithDendrogram": result =	8; break;
                case "ReductEnsembleBoostingWithDiversity": result =	9; break;
                case "ReductEnsemble": result =	10; break;
                case "GeneralizedMajorityDecisionApproximate": result =	11; break;
                case "GeneralizedMajorityDecision": result =	12; break;
                case "ApproximateReductRelative	": result =13; break;
                case "ApproximateReductMajority": result =	14; break;
                case "ApproximateReductPositive": result =	15; break;
                case "ApproximateReductMajorityWeights": result =	16; break;
                case "ApproximateReductRelativeWeights": result =	17; break;
                case "RandomSubset": result = 18; break;
            }
            return result;
        }

        private int WeightTypeToInt_VotingVsRuleInduction(string factoryKey)
        {
            int result = 0;
            switch (factoryKey)
            {
                case "Bireduct": result = 1; break;
                case "BireductRelative": result = 2; break;                
                case "ApproximateReductRelative	": result = 2; break;
                case "ApproximateReductMajority": result = 1; break;                
                case "ApproximateReductMajorityWeights": result = 1; break;
                case "ApproximateReductRelativeWeights": result = 2; break;                
            }
            return result;
        }

        private int DatasetToInt(string dataset)
        {
            int result = 0;
            switch (dataset)
            {                
                case "golf": result =	1; break;
                case "testGMDR": result =	2; break;
                case "dna": result =	3; break;
                case "dna-orig": result =	4; break;
                case "zoo": result =	5; break;
                case "monks-1": result =	6; break;
                case "monks-2": result =	7; break;
                case "monks-3": result =	8; break;
                case "spect": result =	9; break;
                case "letter": result =	10; break;
                case "pen": result =	11; break;
                case "opt": result =	12; break;
                case "semeion": result =	13; break;
                case "chess": result =	14; break;
                case "nursery": result =	15; break;
                case "breast": result =	16; break;
                case "soybean-small": result =	17; break;
                case "connect": result =	18; break;
                case "soybean-large": result =	19; break;
                case "house": result =	20; break;
                case "audiology": result =	21; break;
                case "promoters": result =	22; break;
                case "mashroom": result =	23; break;
                case "german": result =	24; break;
                case "sat": result = 25; break;
            }
            return result;
        }
    }
}