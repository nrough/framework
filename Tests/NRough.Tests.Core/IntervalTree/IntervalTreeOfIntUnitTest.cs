﻿// 
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

using System.Linq;
using System.Diagnostics;
using NUnit.Framework;
using NRough.Core.IntervalTree;

namespace NRough.Tests.Core.IntervalTree
{
  [TestFixture]
  public class IntervalTreeOfIntUnitTest {
    [Test]
    public void CreateEmptyIntervalTree() {
      IntervalTree<int,int> emptyTree = new IntervalTree<int,int>();
      Assert.IsNotNull(emptyTree);
    }

    [Test]
    public void BuildEmptyIntervalTree() {
      IntervalTree<int,int> emptyTree = new IntervalTree<int,int>();
      emptyTree.Build();
    }

    [Test]
    public void TestSeparateIntervals() {
      IntervalTree<int,int> tree = new IntervalTree<int,int>();
      tree.AddInterval(0, 10, 100);
      tree.AddInterval(20, 30, 200);

      var result = tree.Get(5);
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual(100, result[0]);
    }

    [Test]
    public void TestSeparateIntervalsIntersectionsList() {
      IntervalTree<int, int> tree = new IntervalTree<int, int>();
      tree.AddInterval(0, 10, 100);
      tree.AddInterval(20, 30, 200);

      var result = tree.GetIntersections().ToList();
      Assert.AreEqual(0, result.Count, "Expect zero intersection because the interval do not overlaps");
    }

    [Test]
    public void TwoIntersectingIntervals() {
      IntervalTree<int,int> tree = new IntervalTree<int,int>();
      tree.AddInterval(0, 10, 100);
      tree.AddInterval(3, 30, 200);

      var result = tree.Get(5);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(100, result[0]);
      Assert.AreEqual(200, result[1]);
    }

    [Test]
    public void TestIntersectingIntervalsIntersectionsList() {
      IntervalTree<int, int> tree = new IntervalTree<int, int>();
      tree.AddInterval(0, 10, 100);
      tree.AddInterval(3, 30, 200);

      var result = tree.GetIntersections().ToList();
      Assert.AreEqual(1, result.Count, "Expect one intersection because the intervals overlaps");

      int totalIntervals = result.Select(x => x.Count).Sum();
      Assert.AreEqual(2, totalIntervals, "Expect two intervals");
    }

    
    [Test]
    public void TwoNonIntersectingIntervalsWithCommonIntervalThatIntersecBoth() {

      IntervalTree<int, int> tree = new IntervalTree<int, int>();
      tree.AddInterval(8, 9, 100);
      tree.AddInterval(8, 11, 200);
      tree.AddInterval(9, 10, 300);

      var intersections = tree.GetIntersections().ToList();
      Assert.AreEqual(2, intersections.Count);
    }

    [Test]
    public void SpeedTestIntersectingIntervals_GetPoint() {
      IntervalTree<int,int> tree = new IntervalTree<int,int>();
      for (int i = 0; i < 100*1000; i++) {
        tree.AddInterval(i, i + 200, i);
      }
      tree.Build();

      Stopwatch stopWatch = Stopwatch.StartNew();
      var result = tree.Get(50*1000);
      stopWatch.Stop();

      Assert.IsTrue(stopWatch.ElapsedMilliseconds < 100);
    }

    [Test]
    public void SpeedTestIntersectingIntervals_GetRange() {
      IntervalTree<int,int> tree = new IntervalTree<int,int>();
      for (int i = 0; i < 100 * 1000; i++) {
        tree.AddInterval(i, i + 200, i);
      }
      tree.Build();

      Stopwatch stopWatch = Stopwatch.StartNew();
      var result = tree.Get(50 * 1000, 52 * 1000);
      stopWatch.Stop();

      Assert.IsTrue(stopWatch.ElapsedMilliseconds < 100);
    }

    [Test]
    public void SpeedTestBuild100kIntervals() {
      IntervalTree<int,int> tree = new IntervalTree<int,int>();
      for (int i = 0; i < 100 * 1000; i++) {
        tree.AddInterval(i, i + 200, i);
      }


      Stopwatch stopWatch = Stopwatch.StartNew();
      tree.Build();
      stopWatch.Stop();

      Assert.IsTrue(stopWatch.ElapsedMilliseconds < 3 * 1000, "Build took more then 4s - it took " + stopWatch.Elapsed);
    }
  }
}
