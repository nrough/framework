using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using MiscUtil;

namespace Infovision.Utils
{
	[Serializable]
	public class PascalSet<T> : ICloneable, ICollection, IEnumerable<T>
		where T : struct, IComparable, IFormattable, IComparable<T>, IEquatable<T> 
	{
		// Private member variables
		//private int cardinality;
		
		#region PascalSet<int> Properties
		/// <summary>
		/// Returns the lower bound of the set.
		/// </summary>
		public virtual T LowerBound
		{
			get;
			private set;
		}

		/// <summary>
		/// Returns the upper bound of the set.
		/// </summary>
		public virtual T UpperBound
		{
			get;
			private set;
		}

		public BitArray Data
		{
			get;
			private set;
		}

		//private bool IsCardinalityCalculated { get; set; }

		#endregion

		#region Constructors
		/// <summary>
		/// Creates a new PascalSet<int> instance with a specified lower and upper bound.
		/// </summary>
		/// <param name="lowerBound">The lower bound for the set.  Can be any legal integer key.</param>
		/// <param name="upperBound">The upper bound for the set.  Can be any legal integer key.</param>
		public PascalSet(T lowerBound, T upperBound)
		{
			//cardinality is not yet calculated
			//cardinality = 0;
			//this.IsCardinalityCalculated = true;
			
			// make sure lowerbound is less than or equal to upperbound
			//if (lowerBound > upperBound)
			if(lowerBound.CompareTo(upperBound) > 0)
				throw new ArgumentException("The set's lower bound cannot be greater than its upper bound.", "lowerBound");

			this.LowerBound = lowerBound;
			this.UpperBound = upperBound;

			// Create the BitArray
			//int size = upperBound - lowerBound + 1;
			int size = (int)Convert.ChangeType(Operator<T>.Add(
				Operator<T>.Subtract(upperBound, lowerBound), 
				Operator<T>.One), typeof(int));
			this.Data = new BitArray(size);
		}

		/// <summary>
		/// Creates a new PascalSet instance whose initial values are assigned from an integer array.
		/// </summary>
		/// <param name="lowerBound">The lower bound for the set.  Can be any legal integer key.</param>
		/// <param name="upperBound">The upper bound for the set.  Can be any legal integer key.</param>
		/// <param name="initialData">An integer array that is used as the initial values of the array.</param>
		public PascalSet(T lowerBound, T upperBound, IEnumerable<T> initialData)
		{
			//cardinality is not yet calculated
			//cardinality = 0;
			
			// make sure lowerbound is less than or equal to upperbound
			//if (lowerBound > upperBound)
			if (lowerBound.CompareTo(upperBound) > 0)
				throw new ArgumentException("The set's lower bound cannot be greater than its upper bound.", "lowerBound");

			this.LowerBound = lowerBound;
			this.UpperBound = upperBound;

			// Create the BitArray
			//int size = upperBound - lowerBound + 1;
			int size = (int)Convert.ChangeType(Operator<T>.Add(
				Operator<T>.Subtract(upperBound, lowerBound),
				Operator<T>.One), typeof(int));
			this.Data = new BitArray(size);
			
			// Populuate the BitArray with the passed-in initialData array.
			foreach(T val in initialData)
			{
				if(val.CompareTo(LowerBound) >= 0 && val.CompareTo(UpperBound) <= 0)
				{
					//int index = val - this.LowerBound;
					int index = (int)Convert.ChangeType(
						Operator<T>.Subtract(val, LowerBound),
						typeof(int));
					Data.Set(index, true);
					//cardinality++;
				}
				else
				{
					throw new ArgumentException("Attempting to add an element with key that is outside of the set's universe.", "initialData");
				}
			}

			//this.IsCardinalityCalculated = true;
		}

		public PascalSet(T lowerBound, T upperBound, BitArray data)
		{
			//cardinality is not yet calculated
			//this.cardinality = 0;
			
			// make sure lowerbound is less than or equal to upperbound
			//if (lowerBound > upperBound)
			if (lowerBound.CompareTo(upperBound) > 0)
				throw new ArgumentException("The set's lower bound cannot be greater than its upper bound.", "lowerBound");

			this.LowerBound = lowerBound;
			this.UpperBound = upperBound;

			// Create the BitArray
			//int size = upperBound - lowerBound + 1;
			int size = (int)Convert.ChangeType(Operator<T>.Add(
				Operator<T>.Subtract(upperBound, lowerBound),
				Operator<T>.One), typeof(int));
			this.Data = new BitArray(size);
			if (this.Data.Length != data.Length)
				throw new ArgumentException("data length does not match the upper and lower bound settings", "data");            

			// Populuate the BitArray with the passed-in data array.
			for (int i = 0; i < data.Length; i++)
			{
				this.Data[i] = data[i];
				//if (data[i])
				//	this.cardinality++;
			}

			//this.IsCardinalityCalculated = true;
		}

		public PascalSet(PascalSet<T> set) : this(set.LowerBound, set.UpperBound, set.Data) {}
		
		#endregion

		#region Methods

		public int GetCardinality()
		{
			int[] ints = new int[(this.Data.Count >> 5) + 1];
			this.Data.CopyTo(ints, 0);
			int count = 0;

			// fix for not truncated bits in last integer that may have been set to true with SetAll()
			ints[ints.Length - 1] &= ~(-1 << (this.Data.Count % 32));

			for (int i = 0; i < ints.Length; i++)
			{
				int c = ints[i];
				// magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
				unchecked
				{
					c = c - ((c >> 1) & 0x55555555);
					c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
					c = ((c + (c >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
				}
				count += c;
			}

			return count;
		}

		/// <summary>
		/// Determines if two PascalSets are "compatible."  Specifically, it checks to ensure that the PascalSets
		/// share the same lower and upper bounds.
		/// <returns><b>True</b> if the PascalSets share the same bounds, <b>False</b> otherwise.</returns>
		protected virtual bool AreSimilar(PascalSet<T> pascalSet)
		{
			return this.LowerBound.Equals(pascalSet.LowerBound) 
				&& this.UpperBound.Equals(pascalSet.UpperBound);
		}

		#region Union
		/// <summary>
		/// Unions a set of integers with the current PascalSet.
		/// </summary>
		/// <param name="list">An variable number of integers.</param>
		/// <returns>A new PascalSet, which is the union of the <b>this</b> PascalSet<int> and the passed-in integers.</returns>
		public virtual PascalSet<T> Union(params T[] list)
		{
			// create a deep copy of this
			PascalSet<T> result = (PascalSet<T>)Clone();
			//result.IsCardinalityCalculated = false;

			// For each integer passed in, if it's within the bounds add it to the results's BitArray.
			for (int i = 0; i < list.LongLength; i++)
			{
				T val = list[i];
				if (val.CompareTo(this.LowerBound) >= 0 
					&& val.CompareTo(this.UpperBound) <= 0)
				{
					//int index = val - this.LowerBound;
					int index = (int)Convert.ChangeType(
						Operator<T>.Subtract(val, LowerBound),
						typeof(int));
					result.Data.Set(index, true);
				}
				else
				{
					throw new ArgumentException("Attempting to add an element with key that is outside of the set's universe.", "list"); 
				}
			}

			return result;		// return the new PascalSet
		}

		/// <summary>
		/// Unions a passed-in PascalSet<int> with the current PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet.</param>
		/// <returns>A new PascalSet<int> whose elements are the union of <b>pascalSet</b> and <b>this</b>.</returns>
		/// <remarks><b>pascalSet</b> and <b>this</b> must be "similar" PascalSets.</remarks>
		public virtual PascalSet<T> Union(PascalSet<T> pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to union two dissimilar sets.  Union can only occur between two sets with the same universe.", "pascalSet");

			// do a bit-wise OR to union together this.data and s.data
			PascalSet<T> result = (PascalSet<T>)Clone();
			//result.IsCardinalityCalculated = false;
			result.Data.Or(pascalSet.Data);

			return result;
		}

		// Overloaded + operator for union...
		public static PascalSet<T> operator +(PascalSet<T> pascalSet1, PascalSet<T> pascalSet2)
		{
			return pascalSet1.Union(pascalSet2);
		}

		public PascalSet<T> Add(PascalSet<T> pascalSet)
		{
			return this.Union(pascalSet);
		}

		// Overloaded + operator for union with one integer element
		public static PascalSet<T> operator +(PascalSet<T> pascalSet, T element)
		{
			return pascalSet.Union(new T[] { element });
		}

		public PascalSet<T> Add(T element)
		{
			return this.Union(new T[] { element });
		}

		//Adds integer element to a set. No new object is created
		public virtual void AddElement(T element)
		{
			if (element.CompareTo(LowerBound) >= 0 
				&& element.CompareTo(UpperBound) <= 0)
			{
				//int index = element - this.LowerBound;
				int index = (int)Convert.ChangeType(
					Operator<T>.Subtract(element, LowerBound),
					typeof(int));

				if (this.Data.Get(index) == false)
				{
					//cardinality++;
					this.Data.Set(index, true);
				}
			}
			else
			{
				throw new ArgumentException("Attempting to add an element with key that is outside of the set's universe.", "element"); 
			}
		}

		//Removes integer element from a set. No new object is created
		public virtual void RemoveElement(T element)
		{
			if (element.CompareTo(LowerBound) >= 0 
				&& element.CompareTo(UpperBound) <= 0)
			{
				//int index = element - this.LowerBound;
				int index = (int)Convert.ChangeType(
					Operator<T>.Subtract(element, LowerBound),
					typeof(int));
				if (this.Data.Get(index) == true)
				{
					//this.cardinality--;
					this.Data.Set(index, false);
				}
			}
			else
			{
				throw new ArgumentException("Attempting to remove an element with key that is outside of the set's universe.", "element"); 
			}
		}

		#endregion

		#region Intersection
		/// <summary>
		/// Intersects a set of integers with the current PascalSet.
		/// </summary>
		/// <param name="list">An variable number of integers.</param>
		/// <returns>A new PascalSet, which is the intersection of the <b>this</b> PascalSet<int> and the passed-in integers.</returns>
		public virtual PascalSet<T> Intersection(params T[] list)
		{
			PascalSet<T> result = new PascalSet<T>(this.LowerBound, this.UpperBound);
			//result.IsCardinalityCalculated = false;

			for (int i = 0; i < list.Length; i++)
			{
				// only add the element to result if its in this.data
				T val = list[i];
				if (val.CompareTo(this.LowerBound) >= 0 
					&& val.CompareTo(this.UpperBound) <= 0)
				{
					//int index = val - this.LowerBound;
					int index = (int)Convert.ChangeType(
						Operator<T>.Subtract(val, LowerBound),
						typeof(int));
					if (this.Data.Get(index))
					{
						result.Data.Set(index, true);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Intersects a passed-in PascalSet<int> with the current PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet.</param>
		/// <returns>A new PascalSet<int> whose elements are the intersection of <b>pascalSet</b> and <b>this</b>.</returns>
		/// <remarks><b>pascalSet</b> and <b>this</b> must be "similar" PascalSets.</remarks>
		public virtual PascalSet<T> Intersection(PascalSet<T> pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to intersect two dissimilar sets. Intersection can only occur between two sets with the same universe.", "pascalSet");

			// do a bit-wise AND to intersect this.data and s.data
			PascalSet<T> result = (PascalSet<T>)Clone();
			//result.IsCardinalityCalculated = false;
			result.Data.And(pascalSet.Data);

			return result;
		}

		// Overloaded * operator for intersection
		public static PascalSet<T> operator *(PascalSet<T> pascalSet1, PascalSet<T> pascalSet2)
		{
			return pascalSet1.Intersection(pascalSet2);
		}

		public PascalSet<T> Multiply(PascalSet<T> pascalSet)
		{
			return this.Intersection(pascalSet);
		}
		#endregion

		#region Difference
		/// <summary>
		/// Set differences a set of integers with the current PascalSet.
		/// </summary>
		/// <param name="list">An variable number of integers.</param>
		/// <returns>A new PascalSet, which is the set difference of the <b>this</b> PascalSet<int> and the passed-in integers.</returns>
		public virtual PascalSet<T> Difference(params T[] list)
		{
			PascalSet<T> result = new PascalSet<T>(this.LowerBound, this.UpperBound, list);
			//result.IsCardinalityCalculated = false;
			return Difference(result);
		}

		/// <summary>
		/// Set differences a passed-in PascalSet<int> with the current PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet.</param>
		/// <returns>A new PascalSet<int> whose elements are the set difference of <b>pascalSet</b> and <b>this</b>.</returns>
		/// <remarks><b>pascalSet</b> and <b>this</b> must be "similar" PascalSets.</remarks>
		public virtual PascalSet<T> Difference(PascalSet<T> pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to apply set difference to two dissimilar sets.  Set difference can only occur between two sets with the same universe.", "pascalSet");

			// do a bit-wise XOR and then an AND to achieve set difference
			PascalSet<T> result = (PascalSet<T>)Clone();
			//result.IsCardinalityCalculated = false;
			result.Data.Xor(pascalSet.Data).And(this.Data);

			return result;
		}

		// Overloaded - operator for set difference
		public static PascalSet<T> operator -(PascalSet<T> pascalSet1, PascalSet<T> pascalSet2)
		{
			return pascalSet1.Difference(pascalSet2);
		}

		public PascalSet<T> Subtract(PascalSet<T> pascalSet)
		{
			return this.Difference(pascalSet);
		}

		// Overloaded - operator for set difference - substraction of one integer element
		public static PascalSet<T> operator -(PascalSet<T> pascalSet, T element)
		{
			return pascalSet.Difference(new T[] { element });
		}

		public PascalSet<T> Subtract(T element)
		{
			return this.Difference(new T[] { element });
		}
		
		#endregion

		#region Complement
		/// <summary>
		/// Complements a PascalSet.
		/// </summary>
		/// <returns>A new PascalSet<int> that is the complement of <b>this</b>.</returns>
		public virtual PascalSet<T> Complement()
		{
			PascalSet<T> result = (PascalSet<T>)Clone();
			//result.IsCardinalityCalculated = false;
			result.Data.Not();
			return result;
		}
		#endregion

		#region Element Of
		/// <summary>
		/// Determines if a passed-in key is an element of the PascalSet.
		/// </summary>
		/// <param name="element">The integer to check if it exists in the set.</param>
		/// <returns><b>True</b> is <b>element</b> is in the set, <b>False</b> otherwise</returns>
		public virtual bool ContainsElement(T element)
		{
			if (element.CompareTo(this.LowerBound) < 0 
				|| element.CompareTo(this.UpperBound) > 0)
				return false;

			//int index = element - this.LowerBound;
			int index = (int)Convert.ChangeType(
				Operator<T>.Subtract(element, LowerBound),
				typeof(int));

			return this.Data.Get(index);
		}

		#endregion

		#region Subset
		/// <summary>
		/// Determins if this set is a subset of the integers passed-in.
		/// </summary>
		/// <param name="list">A variable number of integers.</param>
		/// <returns><b>True</b> if <b>this</b> is a subset of the passed-in integers; <b>False</b> otherwise.</returns>
		public virtual bool Subset(params T[] list)
		{
			PascalSet<T> temp = new PascalSet<T>(this.LowerBound, this.UpperBound, list);
			//temp.IsCardinalityCalculated = false;
			return Subset(temp);
		}

		/// <summary>
		/// Determins if this set is a subset of the passed-in PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet<int> that is "similar" to <b>this</b>.</param>
		/// <returns><b>True</b> if <b>this</b> is a subset of <b>pascalSet</b>; <b>False</b> otherwise.</returns>
		public virtual bool Subset(PascalSet<T> pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to compare two dissimilar sets. Subset comparisons can only occur between two sets with the same universe.", "pascalSet");

			// Get the BitArray's underlying array
			const int INT_SIZE = 32;
			int arraySize = (Data.Length + INT_SIZE - 1) / INT_SIZE;
			int[] thisBits = new int[arraySize];
			int[] sBits = new int[arraySize];
			Data.CopyTo(thisBits, 0);
			pascalSet.Data.CopyTo(sBits, 0);

			// now, enumerate through the int array elements
			for (int i = 0; i < thisBits.Length; i++)
			{
				// do a bitwise AND between thisBits[i] and sBits[i];
				int result = thisBits[i] & sBits[i];

				// see if result == thisBits[i] - if it doesn't, then not a subset
				if (result != thisBits[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Determins if this set is a proper subset of the integers passed-in.
		/// </summary>
		/// <param name="list">A variable number of integers.</param>
		/// <returns><b>True</b> if <b>this</b> is a proper subset of the passed-in integers; <b>False</b> otherwise.</returns>
		public virtual bool ProperSubset(params T[] list)
		{
			PascalSet<T> temp = new PascalSet<T>(this.LowerBound, this.UpperBound, list);
			//temp.IsCardinalityCalculated = false;
			return ProperSubset(temp);
		}

		/// <summary>
		/// Determins if this set is a proper subset of the passed-in PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet<int> that is "similar" to <b>this</b>.</param>
		/// <returns><b>True</b> if <b>this</b> is a proper subset of <b>pascalSet</b>; <b>False</b> otherwise.</returns>
		public virtual bool ProperSubset(PascalSet<T> pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to compare two dissimilar sets. Subset comparisons can only occur between two sets with the same universe.", "pascalSet");

			return Subset(pascalSet) && !pascalSet.Subset(this);
		}
		#endregion

		#region Superset
		/// <summary>
		/// Determins if this set is a superset of the integers passed-in.
		/// </summary>
		/// <param name="list">A variable number of integers.</param>
		/// <returns><b>True</b> if <b>this</b> is a superset of the passed-in integers; <b>False</b> otherwise.</returns>
		public virtual bool Superset(params T[] list)
		{
			PascalSet<T> temp = new PascalSet<T>(this.LowerBound, this.UpperBound, list);
			//temp.IsCardinalityCalculated = false;
			return Superset(temp);
		}

		/// <summary>
		/// Determins if this set is a superset of the passed-in PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet<int> that is "similar" to <b>this</b>.</param>
		/// <returns><b>True</b> if <b>this</b> is a superset of <b>pascalSet</b>; <b>False</b> otherwise.</returns>
		public virtual bool Superset(PascalSet<T> pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to compare two dissimilar sets.  Superset comparisons can only occur between two sets with the same universe.", "pascalSet");

			return pascalSet.Subset(this);
		}

		/// <summary>
		/// Determins if this set is a proper superset of the integers passed-in.
		/// </summary>
		/// <param name="list">A variable number of integers.</param>
		/// <returns><b>True</b> if <b>this</b> is a proper superset of the passed-in integers; <b>False</b> otherwise.</returns>
		public virtual bool ProperSuperset(params T[] list)
		{
			PascalSet<T> temp = new PascalSet<T>(this.LowerBound, this.UpperBound, list);
			//temp.IsCardinalityCalculated = false;
			return ProperSuperset(temp);
		}

		/// <summary>
		/// Determins if this set is a proper superset of the passed-in PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet<int> that is "similar" to <b>this</b>.</param>
		/// <returns><b>True</b> if <b>this</b> is a proper superset of <b>pascalSet</b>; <b>False</b> otherwise.</returns>
		public virtual bool ProperSuperset(PascalSet<T> pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to compare two dissimilar sets.  Superset comparisons can only occur between two sets with the same universe.", "pascalSet");

			return Superset(pascalSet) && !pascalSet.Superset(this);
		}
		#endregion

		public T[] ToArray()
		{
			int size = this.Count;
			T[] array = new T[size];
			if (size == 0) return array;            
			int j = 0;
			for (int i = 0; i < this.Data.Length; i++)
			{
				if (this.Data.Get(i))
					array[j++] = Operator.AddAlternative<T, int>(this.LowerBound, i);
			}
			return array;
		}

		#endregion

		#region ICloneable Members
		/// <summary>
		/// Clones the PascalSet, performing a deep copy.
		/// </summary>
		/// <returns>A new instance of a PascalSet, using a deep copy.</returns>
		public virtual object Clone()
		{
			PascalSet<T> p = new PascalSet<T>(this.LowerBound, this.UpperBound);
			p.Data = new BitArray(this.Data);
			//p.IsCardinalityCalculated = false;
			return p;
		}
		#endregion

		#region ICollection Members

		/// <summary>
		/// Returns a key indicating whether access to the ICollection is synchronized (thread-safe).
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Provides the cardinality of the set.
		/// </summary>
		public int Count
		{
			get
			{
				return this.GetCardinality();
				
				/*
				if (this.IsCardinalityCalculated == false)
				{
					//int elements = 0;
					//for (int i = 0; i < data.Length; i++)
					//	if (data.Get(i)) elements++;
					//cardinality = elements;
					cardinality = this.GetCardinality();
					this.IsCardinalityCalculated = true;
				}

				return cardinality;
				*/
			}
		}

		/// <summary>
		/// Copies the elements of the ICollection to an Array, starting at a particular Array index.
		/// </summary>
		public void CopyTo(Array array, int index)
		{
			Data.CopyTo(array, index);
		}

		// Provide the strongly typed member for ICollection.
		public void CopyTo(T[] array, int index)
		{
			this.ToArray().CopyTo(array, index);
		}

		/// <summary>
		/// Returns an object that can be used to synchronize access to the ICollection.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an IEnumerator to enumerate through the set.
		/// </summary>
		/// <returns>An IEnumerator instance.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			int totalElements = Count;
			int itemsReturned = 0;
			for (int i = 0; i < this.Data.Length; i++)
			{
				if (itemsReturned >= totalElements)
					break;
				else if (this.Data.Get(i))
					//yield return i + this.LowerBound;
					yield return Operator<T>.Add((T)Convert.ChangeType(i, typeof(T)), LowerBound);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)GetEnumerator();
		}

		#endregion

		#region System.Object Methods

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < this.Data.Length; i++)
				if (this.Data.Get(i))
					sb.Append(Operator<T>.Add(
						(T)Convert.ChangeType(i, typeof(T)), 
						LowerBound)).Append(' ');
			sb.Remove(sb.Length - 1, 1);
			return sb.ToString();
		}

		public override int GetHashCode()
		{
			//return HashHelper.GetHashCode<int>(Data.GetInternalValues());
			return HashHelper.GetHashCode(this.Data);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			PascalSet<T> pascalSet = obj as PascalSet<T>;
			if (pascalSet == null)
				return false;

			if (!this.LowerBound.Equals(pascalSet.LowerBound))
				return false;

			if (!this.UpperBound.Equals(pascalSet.UpperBound))
				return false;

			for (int i = 0; i < this.Data.Length; i++)
				if (this.Data[i] != pascalSet.Data[i])
					return false;

			return true;
		}

		#endregion
	}
}
