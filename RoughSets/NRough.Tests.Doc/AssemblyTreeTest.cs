using NRough.Doc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NRough.Core;

namespace NRough.Tests.Doc
{
    [TestFixture]
    public class AssemblyTreeTest
    {
        [Test]
        public void GenerateTest()
        {
            Assembly[] assemblies = new Assembly[] {
                Assembly.Load("NRough.Core"),
                Assembly.Load("NRough.Data"),
                Assembly.Load("NRough.MachineLearning"),
                Assembly.Load("NRough.Math")
            };

            var assemblyTree = new AssemblyTree("NRough");
            assemblyTree.Build(new AssemblyTreeBuilder(assemblies));
            Console.WriteLine(assemblyTree.ToString("G", new LatexForestAssemblyTreeFormatter(true)));
        }
    }
}
