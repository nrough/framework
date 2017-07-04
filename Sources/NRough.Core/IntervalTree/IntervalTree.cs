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
using System;
using System.Collections.Generic;
using System.Text;

namespace NRough.Core.IntervalTree
{
  /// <summary>
  /// An Interval Tree is essentially a map from stubedIntervals to objects, which
  /// can be queried for all data associated with a particular interval of time
  /// </summary>
  /// <typeparam name="T">Type of data store on each interval.</typeparam>
  /// <typeparam name="D">Type define the interval. Must be struct that implement IComperable</typeparam>
  /// <remarks>
  /// This code was translated from Java to C# by ido.ran@gmail.com from the web site http://www.thekevindolan.com/2010/02/interval-tree/index.html.
  /// </remarks>
  [Serializable]
  public class IntervalTree<T, D> where D : struct, IComparable<D> {

    private IntervalNode<T,D> head;
    private List<Interval<T,D>> intervalList;
    private bool inSync;
    private int size;

    /// <summary>
    /// Instantiate a new interval tree with no stubedIntervals
    /// </summary>
    public IntervalTree() {
      this.head = new IntervalNode<T,D>();
      this.intervalList = new List<Interval<T,D>>();
      this.inSync = true;
      this.size = 0;
    }

    /// <summary>
    /// Instantiate and Build an interval tree with a preset list of stubedIntervals
    /// </summary>
    /// <param name="intervalList">The list of stubedIntervals to use</param>
    public IntervalTree(List<Interval<T,D>> intervalList) {
      this.head = new IntervalNode<T,D>(intervalList);
      this.intervalList = new List<Interval<T,D>>();
      this.intervalList.AddRange(intervalList);
      this.inSync = true;
      this.size = intervalList.Count;
    }

    /// <summary>
    /// Perform a stabbing Query, returning the associated data.
    /// Will rebuild the tree if out of sync
    /// </summary>
    /// <param name="time">The time to Stab</param>
    /// <returns>The data associated with all stubedIntervals that contain time</returns>
    public List<T> Get(D time) {
      return Get(time, StubMode.Contains);
    }

    public List<T> Get(D time, StubMode mode) {
      List<Interval<T,D>> intervals = GetIntervals(time, mode);
      List<T> result = new List<T>();
      foreach (Interval<T,D> interval in intervals)
        result.Add(interval.Data);
      return result;
    }

    /// <summary>
    /// Perform a stabbing Query, returning the interval objects.
    /// Will rebuild the tree if out of sync.
    /// </summary>
    /// <param name="time">The time to Stab</param>
    /// <returns>all stubedIntervals that contain time</returns>
    public List<Interval<T,D>> GetIntervals(D time) {
      return GetIntervals(time, StubMode.Contains);
    }

    public List<Interval<T,D>> GetIntervals(D time, StubMode mode) {
      Build();

      List<Interval<T, D>> stubedIntervals;

      switch (mode) {
        case StubMode.Contains:
          stubedIntervals = head.Stab(time, ContainConstrains.None);
          break;
        case StubMode.ContainsStart:
          stubedIntervals = head.Stab(time, ContainConstrains.IncludeStart);
          break;
        case StubMode.ContainsStartThenEnd:
          stubedIntervals = head.Stab(time, ContainConstrains.IncludeStart);
          if (stubedIntervals.Count == 0) {
            stubedIntervals = head.Stab(time, ContainConstrains.IncludeEnd);
          }
          break;
        default:
          throw new ArgumentException("Invalid StubMode " + mode, "mode");
      }

      return stubedIntervals;
    }

    /// <summary>
    /// Perform an interval Query, returning the associated data.
    /// Will rebuild the tree if out of sync.
    /// </summary>
    /// <param name="start">the start of the interval to check</param>
    /// <param name="end">end of the interval to check</param>
    /// <returns>the data associated with all stubedIntervals that intersect target</returns>
    public List<T> Get(D start, D end) {
      List<Interval<T,D>> intervals = GetIntervals(start, end);
      List<T> result = new List<T>();
      foreach (Interval<T,D> interval in intervals)
        result.Add(interval.Data);
      return result;
    }

    /// <summary>
    /// Perform an interval Query, returning the interval objects.
    /// Will rebuild the tree if out of sync
    /// </summary>
    /// <param name="start">the start of the interval to check</param>
    /// <param name="end">the end of the interval to check</param>
    /// <returns>all stubedIntervals that intersect target</returns>
    public List<Interval<T,D>> GetIntervals(D start, D end) {
      Build();
      return head.Query(new Interval<T,D>(start, end, default(T)));
    }

    /// <summary>
    /// Add an interval object to the interval tree's list.
    /// Will not rebuild the tree until the next Query or call to Build
    /// </summary>
    /// <param name="interval">interval the interval object to add</param>
    public void AddInterval(Interval<T,D> interval) {
      intervalList.Add(interval);
      inSync = false;
    }

    /// <summary>
    /// Add an interval object to the interval tree's list.
    /// Will not rebuild the tree until the next Query or call to Build.
    /// 
    /// </summary>
    /// <param name="begin">the beginning of the interval</param>
    /// <param name="end">the end of the interval</param>
    /// <param name="data">the data to associate</param>
    public void AddInterval(D begin, D end, T data) {
      intervalList.Add(new Interval<T,D>(begin, end, data));
      inSync = false;
    }

    /// <summary>
    /// Determine whether this interval tree is currently a reflection of all stubedIntervals in the interval list
    /// </summary>
    /// <returns>true if no changes have been made since the last Build</returns>
    public bool IsInSync() {
      return inSync;
    }

    /// <summary>
    /// Build the interval tree to reflect the list of stubedIntervals.
    /// Will not run if this is currently in sync
    /// </summary>
    public void Build() {
      if (!inSync) {
        head = new IntervalNode<T,D>(intervalList);
        inSync = true;
        size = intervalList.Count;
      }
    }

    /// <summary>
    /// Get the number of entries in the currently built interval tree
    /// </summary>
    public int CurrentSize {
      get {
        return size;
      }
    }

    /// <summary>
    /// Get the number of entries in the interval list, equal to .size() if inSync()
    /// </summary>
    public int ListSize {
      get {
        return intervalList.Count;
      }
    }

    /// <summary>
    /// Get list of all intersection stubedIntervals.
    /// </summary>
    /// <returns>Enumerable contain lists of intersecting stubedIntervals.</returns>
    public IEnumerable<ICollection<Interval<T, D>>> GetIntersections() {
      Build();

      Queue<IntervalNode<T, D>> toVisit = new Queue<IntervalNode<T, D>>();
      toVisit.Enqueue(head);

      do {
        var node = toVisit.Dequeue();
        foreach (var intersection in node.Intersections) {
          yield return intersection;
        }

        if (node.Left != null) toVisit.Enqueue(node.Left);
        if (node.Right != null) toVisit.Enqueue(node.Right);

      } while (toVisit.Count > 0);
    }

        /// <summary>
        /// Get all the stubedIntervals in this tree.
        /// This method does not build the tree.
        /// </summary>
        public IList<Interval<T, D>> Intervals
        {
            get
            {
                return intervalList.AsReadOnly();// Algorithms.ReadOnly(intervalList);
            }
        }

    public override String ToString() {
      return NodeString(head, 0);
    }

    private String NodeString(IntervalNode<T,D> node, int level) {
      if (node == null)
        return "";

      var sb = new StringBuilder();
      for (int i = 0; i < level; i++)
        sb.Append("\t");
      sb.Append(node + "\n");
      sb.Append(NodeString(node.Left, level + 1));
      sb.Append(NodeString(node.Right, level + 1));
      return sb.ToString();
    }
  }

  public enum StubMode {
    Contains,
    ContainsStart,
    ContainsStartThenEnd
  }
}
