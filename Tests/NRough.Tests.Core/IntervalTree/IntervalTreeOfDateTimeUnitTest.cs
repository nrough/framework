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

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using NUnit.Framework;
using NRough.Core.IntervalTree;

namespace NRough.Tests.Core.IntervalTree
{
  [TestFixture]
  public class IntervalTreeOfDateTimeUnitTest
    {

    private static readonly DateTime ZERO = new DateTime(2001, 01, 01, 10, 00, 00);

    [Test]
    public void CreateEmptyIntervalTree() {
      IntervalTree<int,DateTime> emptyTree = new IntervalTree<int,DateTime>();
      Assert.IsNotNull(emptyTree);
    }

    [Test]
    public void BuildEmptyIntervalTree() {
      IntervalTree<int,DateTime> emptyTree = new IntervalTree<int,DateTime>();
      emptyTree.Build();
    }

    [Test]
    public void TestSeparateIntervals() {
      IntervalTree<int,DateTime> tree = new IntervalTree<int,DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(10), 100);
      tree.AddInterval(ZERO.AddHours(20), ZERO.AddHours(30), 200);

      var result = tree.Get(ZERO.AddHours(5));
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual(100, result[0]);
    }

    /// <summary>
    /// 0-----5-----10------15--------20
    /// |============|
    ///       |==============|
    ///              |=================|
    /// </summary>
    [Test]
    public void TestPertialOverlapIntervals_AssertIntersectionsCount() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(10), 100);
      tree.AddInterval(ZERO.AddHours(5), ZERO.AddHours(15), 200);
      tree.AddInterval(ZERO.AddHours(10), ZERO.AddHours(20), 200);

      var result = tree.GetIntersections().ToList();
      Assert.AreEqual(2, result.Count);
    }

    /// <summary>
    /// 0-----5-----10------15--------20
    /// |=====100====|
    ///              |==200=|
    ///              |====300==========|
    /// </summary>
    [Test]
    public void OverlapOnExactEndAndStart_AssertCount() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(10), 100);
      tree.AddInterval(ZERO.AddHours(10), ZERO.AddHours(15), 200);
      tree.AddInterval(ZERO.AddHours(10), ZERO.AddHours(20), 200);

      var result = tree.Get(ZERO.AddHours(10), StubMode.ContainsStart).ToList();
      Assert.AreEqual(2, result.Count, "Expect only the two intervals start at 10");
    }

    [Test]
    public void TestPertialOverlapIntervals_AssertIntersectionsContentCount() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(10), 100);
      tree.AddInterval(ZERO.AddHours(5), ZERO.AddHours(15), 200);
      tree.AddInterval(ZERO.AddHours(10), ZERO.AddHours(20), 200);

      var result = tree.GetIntersections().ToList();
      var intersectionContentCount = result.Select(x => x.Count).ToList();
      var expectedCount = new int[] { 2, 2 };
      CollectionAssert.AreEqual(expectedCount, intersectionContentCount);
    }

    [Test]
    public void TestFullOverlapIntervals_AssertIntersectionsContentCount() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(10), 100);
      tree.AddInterval(ZERO, ZERO.AddHours(10), 200);
      tree.AddInterval(ZERO, ZERO.AddHours(10), 200);

      var result = tree.GetIntersections().ToList();
      var intersectionContentCount = result.Select(x => x.Count).ToList();
      var expectedCount = new int[] { 3 };
      CollectionAssert.AreEqual(expectedCount, intersectionContentCount);
    }

    [Test]
    public void TestEmptyIntervalTree_AssertIntersectionsCount() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();

      var result = tree.GetIntersections().ToList();
      Assert.AreEqual(0, result.Count, "There should not be any intersections because the tree it empty");
    }

    [Test]
    public void TestPertialOverlapIntervals_AssertIntersectionsContent() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(10), 100);
      tree.AddInterval(ZERO.AddHours(5), ZERO.AddHours(15), 200);
      tree.AddInterval(ZERO.AddHours(10), ZERO.AddHours(20), 300);

      var result = tree.GetIntersections().ToList();

      for (int i = 0; i < 2; i++) {
        int[] expected;
        if (i == 0) {
          expected = new int[] { 100, 200 };
        }
        else {
          expected = new int[] { 200, 300 };
        }
        
        CollectionAssert.AreEqual(expected, result[i].Select(x => x.Data).ToArray());
      }
    }

    [Test]
    public void TestStartEndOverlapIntervals_AssertIntersectionsContent() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(1), 100);
      tree.AddInterval(ZERO.AddHours(1), ZERO.AddHours(2), 200);
      tree.AddInterval(ZERO.AddHours(2), ZERO.AddHours(3), 300);
      tree.AddInterval(ZERO.AddHours(3), ZERO.AddHours(4), 400);

      var result = tree.GetIntersections().ToList();
      Assert.AreEqual(0, result.Count, "Each interval ends exactly at the start of the next interval so there is no intersection");
    }

    [Test]
    public void GetIntervalByExactStartTime() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(1), 100);

      var result = tree.Get(ZERO, StubMode.ContainsStart);
      Assert.AreEqual(1, result.Count);
    }

    [Test]
    public void GetIntervalByExactEndTime() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(1), 100);

      var result = tree.Get(ZERO.AddHours(1), StubMode.ContainsStartThenEnd);
      Assert.AreEqual(1, result.Count);
    }



    [Test]
    public void TwoIntersectingIntervals() {
      IntervalTree<int,DateTime> tree = new IntervalTree<int,DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(10), 100);
      tree.AddInterval(ZERO.AddHours(3), ZERO.AddHours(30), 200);

      var result = tree.Get(ZERO.AddHours(5));
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(100, result[0]);
      Assert.AreEqual(200, result[1]);
    }

    [Test]
    public void TestSeparateIntervalsIntersectionsList() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(10), 100);
      tree.AddInterval(ZERO.AddHours(20), ZERO.AddHours(30), 200);

      var result = tree.GetIntersections().ToList();
      Assert.AreEqual(0, result.Count, "Expect zero intersection because the interval do not overlaps");
    }

    [Test]
    public void TestIntersectingIntervalsIntersectionsList() {
      IntervalTree<int, DateTime> tree = new IntervalTree<int, DateTime>();
      tree.AddInterval(ZERO, ZERO.AddHours(10), 100);
      tree.AddInterval(ZERO.AddHours(3), ZERO.AddHours(30), 200);

      var result = tree.GetIntersections().ToList();
      Assert.AreEqual(1, result.Count, "Expect one intersection because the intervals overlaps");

      int totalIntervals = result.Select(x => x.Count).Sum();
      Assert.AreEqual(2, totalIntervals, "Expect two intervals");
    }

    [Test]
    public void SpeedTestIntersectingIntervals_GetPoint() {
      IntervalTree<int,DateTime> tree = new IntervalTree<int,DateTime>();
      for (int i = 0; i < 100*1000; i++) {
        tree.AddInterval(ZERO.AddHours(i), ZERO.AddHours(i + 200), i);
      }
      tree.Build();

      Stopwatch stopWatch = Stopwatch.StartNew();
      var result = tree.Get(ZERO.AddHours(50*1000));
      stopWatch.Stop();

      Assert.IsTrue(stopWatch.ElapsedMilliseconds < 100);
    }

    [Test]
    public void SpeedTestIntersectingIntervals_GetRange() {
      IntervalTree<int,DateTime> tree = new IntervalTree<int,DateTime>();
      for (int i = 0; i < 100 * 1000; i++) {
        tree.AddInterval(ZERO.AddHours(i), ZERO.AddHours(i + 200), i);
      }
      tree.Build();

      Stopwatch stopWatch = Stopwatch.StartNew();
      var result = tree.Get(ZERO.AddHours(50 * 1000), ZERO.AddHours(52 * 1000));
      stopWatch.Stop();

      Assert.IsTrue(stopWatch.ElapsedMilliseconds < 100);
    }

    [Test]
    public void SpeedTestBuild100kIntervals() {
      IntervalTree<int,DateTime> tree = new IntervalTree<int,DateTime>();
      for (int i = 0; i < 100 * 1000; i++) {
        tree.AddInterval(ZERO.AddHours(i), ZERO.AddHours(i + 200), i);
      }


      Stopwatch stopWatch = Stopwatch.StartNew();
      tree.Build();
      stopWatch.Stop();

      Assert.IsTrue(stopWatch.ElapsedMilliseconds < 3 * 1000, "Build took more then 4s - it took " + stopWatch.Elapsed);
    }
  }
}
