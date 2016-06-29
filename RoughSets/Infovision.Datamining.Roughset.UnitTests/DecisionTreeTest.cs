using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using NUnit.Framework;
using Accord.MachineLearning;
using Accord.Math;
using System.Data;
using Accord.Statistics.Filters;
using Accord.MachineLearning.DecisionTrees;
using DecTrees = Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.DecisionTrees.Rules;

namespace Infovision.Datamining.Roughset.UnitTests
{
    [TestFixture]
    public class DecisionTreeTest
    {
        [Test]
        public void ID3LearnTest()
        {
            Console.WriteLine("ID3LearnTest");

            //DataStore data = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);
            
            DecisionTreeID3 treeID3 = new DecisionTreeID3();
            treeID3.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            DecisionTree.PrintTree(treeID3.Root, 2, 0);

            ClassificationResult resultID3 = treeID3.Classify(data, null);
            Console.WriteLine(resultID3);
        }

        [Test]
        public void C45LearnTest()
        {
            Console.WriteLine("C45LearnTest");

            //DataStore data = DataStore.Load(@"Data\playgolf.train", FileFormat.Rses1);
            DataStore data = DataStore.Load(@"Data\dna_modified.trn", FileFormat.Rses1);

            DecisionTreeC45 treeC45 = new DecisionTreeC45();
            treeC45.Learn(data, data.DataStoreInfo.GetFieldIds(FieldTypes.Standard).ToArray());
            
            DecisionTree.PrintTree(treeC45.Root, 2, 0);
            ClassificationResult resultC45 = treeC45.Classify(data, null);
            Console.WriteLine(resultC45);

            //DecisionTreeNode node = treeID3.Root;
            //TreeNodeTraversal.TraverseLevelOrder(node, n => Console.WriteLine(" ({0},{1}) ", n.Key, n.Value));
            
        }

        #region Accord Trees

        [Test]
        public void AccordC45Test()
        {
            Console.WriteLine("AccordC45Test");

            DataTable data = GetDataTable();
            Codification codebook = new Codification(data);
            DecisionVariable[] attributes =
            {
                new DecisionVariable("Outlook", 3), // 3 possible values (Sunny, overcast, rain)
                new DecisionVariable("Temperature", 3), // 3 possible values (Hot, mild, cool)
                new DecisionVariable("Humidity", 2), // 2 possible values (High, normal)
                new DecisionVariable("Wind", 2) // 2 possible values (Weak, strong)
            };
            int classCount = 2; // 2 possible output values for playing tennis: yes or no
            
            DecTrees.DecisionTree tree = new DecTrees.DecisionTree(attributes, classCount);
            C45Learning c45learning = new C45Learning(tree);

            // Translate our training data into integer symbols using our codebook:
            DataTable symbols = codebook.Apply(data); 
            double[][] inputs = symbols.ToArray<double>("Outlook", "Temperature", "Humidity", "Wind");
            int[] outputs = symbols.ToArray<int>("PlayTennis");

            // Learn the training instances!
            c45learning.Run(inputs, outputs);

            this.PrintTree(tree.Root, 2, 0, codebook);

            string[] conditionalAttributes = new string[] { "Outlook", "Temperature", "Humidity", "Wind" };

            int count = 0;
            int correct = 0;
            foreach (DataRow row in data.Rows)
            {
                var record = new string[] 
                { 
                    row["Outlook"].ToString(), 
                    row["Temperature"].ToString(), 
                    row["Humidity"].ToString(), 
                    row["Wind"].ToString()
                };

                string actual = row["PlayTennis"].ToString();
                int[] query = codebook.Translate(conditionalAttributes, record);
                int output = tree.Compute(query);
                string answer = codebook.Translate("PlayTennis", output);

                count++;
                if (actual == answer)
                    correct++;
            }

            Console.WriteLine("Accuracy: {0:0.0000}", (double)correct / (double)count);

            /*
            int[] query = codebook.Translate(
                new string[] {"Outlook", "Temperature", "Humidity", "Wind"},
                new string[] {"Sunny", "Hot", "High", "Strong"});

            int output = tree.Compute(query);
            string answer = codebook.Translate("PlayTennis", output); // answer will be "No".
            */
        }

        [Test]
        public void AccordID3Test()
        {
            Console.WriteLine("AccordID3Test");

            DataTable data = GetDataTable();
            Codification codebook = new Codification(data);
            DecisionVariable[] attributes =
            {
                new DecisionVariable("Outlook", 3), // 3 possible values (Sunny, overcast, rain)
                new DecisionVariable("Temperature", 3), // 3 possible values (Hot, mild, cool)
                new DecisionVariable("Humidity", 2), // 2 possible values (High, normal)
                new DecisionVariable("Wind", 2) // 2 possible values (Weak, strong)
            };
            int classCount = 2; // 2 possible output values for playing tennis: yes or no

            DecTrees.DecisionTree tree = new DecTrees.DecisionTree(attributes, classCount);
            ID3Learning id3learning = new ID3Learning(tree);

            // Translate our training data into integer symbols using our codebook:
            DataTable symbols = codebook.Apply(data);
            int[][] inputs = symbols.ToArray<int>("Outlook", "Temperature", "Humidity", "Wind");
            int[] outputs = symbols.ToArray<int>("PlayTennis");

            // Learn the training instances!
            id3learning.Run(inputs, outputs);

            this.PrintTree(tree.Root, 2, 0, codebook);

            string[] conditionalAttributes = new string[] { "Outlook", "Temperature", "Humidity", "Wind" };

            int count = 0;
            int correct = 0;
            foreach (DataRow row in data.Rows)
            {
                var record = new string[] 
                { 
                    row["Outlook"].ToString(), 
                    row["Temperature"].ToString(), 
                    row["Humidity"].ToString(), 
                    row["Wind"].ToString()
                };

                string actual = row["PlayTennis"].ToString();
                int[] query = codebook.Translate(conditionalAttributes, record);
                int output = tree.Compute(query);
                string answer = codebook.Translate("PlayTennis", output);

                count++;
                if (actual == answer)
                    correct++;
            }

            Console.WriteLine("Accuracy: {0:0.0000}", (double)correct / (double)count);

            /*
            int[] query = codebook.Translate(
                new string[] {"Outlook", "Temperature", "Humidity", "Wind"},
                new string[] {"Sunny", "Hot", "High", "Strong"});

            int output = tree.Compute(query);
            string answer = codebook.Translate("PlayTennis", output); // answer will be "No".
            */
        }

        public DataTable GetDataTable()
        {
            DataTable data = new DataTable("PlayGolf");

            data.Columns.Add("Day");
            data.Columns.Add("Outlook");
            data.Columns.Add("Temperature");
            data.Columns.Add("Humidity");
            data.Columns.Add("Wind");
            data.Columns.Add("PlayTennis");

            data.Rows.Add("D1", "Sunny", "Hot", "High", "Weak", "No");
            data.Rows.Add("D2", "Sunny", "Hot", "High", "Strong", "No");
            data.Rows.Add("D3", "Overcast", "Hot", "High", "Weak", "Yes");
            data.Rows.Add("D4", "Rain", "Mild", "High", "Weak", "Yes");
            data.Rows.Add("D5", "Rain", "Cool", "Normal", "Weak", "Yes");
            data.Rows.Add("D6", "Rain", "Cool", "Normal", "Strong", "No");
            data.Rows.Add("D7", "Overcast", "Cool", "Normal", "Strong", "Yes");
            data.Rows.Add("D8", "Sunny", "Mild", "High", "Weak", "No");
            data.Rows.Add("D9", "Sunny", "Cool", "Normal", "Weak", "Yes");
            data.Rows.Add("D10", "Rain", "Mild", "Normal", "Weak", "Yes");
            data.Rows.Add("D11", "Sunny", "Mild", "Normal", "Strong", "Yes");
            data.Rows.Add("D12", "Overcast", "Mild", "High", "Strong", "Yes");
            data.Rows.Add("D13", "Overcast", "Hot", "Normal", "Weak", "Yes");
            data.Rows.Add("D14", "Rain", "Mild", "High", "Strong", "No");

            return data;
        }

        public void PrintTree(DecisionNode node, int indentSize, int currentLevel, Codification codebook)
        {
            if (!node.IsRoot)
            {
                var currentNode = string.Format("{0}({1})", new string(' ', indentSize * currentLevel), node);
                Console.WriteLine(currentNode);
                if (node.Output != null)
                {
                    currentNode = string.Format("{0}(Play = {1})", new string(' ', indentSize * (currentLevel + 1)), node.Output);
                    Console.WriteLine(currentNode);
                }
            }

            if (node.Branches != null)
                foreach (var child in node.Branches)
                    PrintTree(child, indentSize, currentLevel + 1, codebook);
        }

        #endregion 
    }
}
