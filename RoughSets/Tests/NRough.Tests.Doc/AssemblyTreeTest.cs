using NRough.Doc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NRough.Core;
using System.IO;

namespace NRough.Tests.Doc
{
    [TestFixture]
    public class AssemblyTreeTest
    {
        [Test]
        public void GenerateTest()
        {
            string filename = @"C:\Users\Admin\Documents\GitHub\PhD\Autoreferat\nrough_forest.tex";

            Assembly[] assemblies = new Assembly[] {
                Assembly.Load("NRough.Core"),
                Assembly.Load("NRough.Data"),
                Assembly.Load("NRough.MachineLearning"),
                Assembly.Load("NRough.Math")
            };

            var assemblyTree = new AssemblyTree("NRough");
            assemblyTree.Build(new AssemblyTreeBuilder(assemblies));
            string forest = assemblyTree.ToString("G", new LatexForestAssemblyTreeFormatter(true));
            Console.WriteLine(forest);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
            {
                file.Write(forest);
            }
        }
    }
}
