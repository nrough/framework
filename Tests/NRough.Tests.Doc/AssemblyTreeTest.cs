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
