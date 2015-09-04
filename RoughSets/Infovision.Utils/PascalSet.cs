using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Infovision.Utils
{
	[Serializable]
	public class PascalSet : ICloneable, ICollection, IEnumerable<int>
	{
		// Private member variables
		private int lowerBound, upperBound;
		private BitArray data;
		private int cardinality;

		#region Constructors
		/// <summary>
		/// Creates a new PascalSet instance with a specified lower and upper bound.
		/// </summary>
		/// <param name="lowerBound">The lower bound for the set.  Can be any legal integer value.</param>
		/// <param name="upperBound">The upper bound for the set.  Can be any legal integer value.</param>
		public PascalSet(int lowerBound, int upperBound)
		{
			//cardinality is not yet calculated
			cardinality = -1;
			
			// make sure lowerbound is less than or equal to upperbound
			if (lowerBound > upperBound)
				throw new ArgumentException("The set's lower bound cannot be greater than its upper bound.");

			this.lowerBound = lowerBound;
			this.upperBound = upperBound;

			// Create the BitArray
			int size = upperBound - lowerBound + 1;
			data = new BitArray(size);
		}

		/// <summary>
		/// Creates a new PascalSet instance whose initial values are assigned from an integer array.
		/// </summary>
		/// <param name="lowerBound">The lower bound for the set.  Can be any legal integer value.</param>
		/// <param name="upperBound">The upper bound for the set.  Can be any legal integer value.</param>
		/// <param name="initialData">An integer array that is used as the initial values of the array.</param>
		public PascalSet(int lowerBound, int upperBound, int[] initialData)
		{
			//cardinality is not yet calculated
			cardinality = -1;
			
			// make sure lowerbound is less than or equal to upperbound
			if (lowerBound > upperBound)
				throw new ArgumentException("The set's lower bound cannot be greater than its upper bound.");

			this.lowerBound = lowerBound;
			this.upperBound = upperBound;

			// Create the BitArray
			int size = upperBound - lowerBound + 1;
			data = new BitArray(size);
			
			// Populuate the BitArray with the passed-in initialData array.
			for (int i = 0; i < initialData.LongLength; i++)
			{
				int val = initialData[i];
				if (val >= this.lowerBound && val <= this.upperBound)
				{
					int index = val - this.lowerBound;
					data.Set(index, true);
					cardinality++;
				}
				else
					throw new ArgumentException("Attempting to add an element with value " 
												+ val.ToString(CultureInfo.InvariantCulture) 
												+ " that is outside of the set's universe.  Value must be between "
												+ this.lowerBound.ToString(CultureInfo.InvariantCulture) 
												+ " and "
												+ this.upperBound.ToString(CultureInfo.InvariantCulture));
			}

			if (cardinality > -1)
				cardinality++;
		}

		public PascalSet(int lowerBound, int upperBound, BitArray data)
		{
			//cardinality is not yet calculated
			cardinality = -1;
			
			// make sure lowerbound is less than or equal to upperbound
			if (lowerBound > upperBound)
				throw new ArgumentException("The set's lower bound cannot be greater than its upper bound.");

			this.lowerBound = lowerBound;
			this.upperBound = upperBound;

			// Create the BitArray
			int size = upperBound - lowerBound + 1;
			this.data = new BitArray(size);
			if (this.data.Length != data.Length)
				throw new ArgumentException("data length does not match the upper and lower bound settings");            

			// Populuate the BitArray with the passed-in data array.
			for (int i = 0; i < data.Length; i++)
			{
				this.data[i] = data[i];
			}
		}

		/// <summary>
		/// Creates a new PascalSet instance with a specified lower and upper bound.
		/// </summary>
		/// <param name="lowerBound">The lower bound for the set.  Can be any legal character value.</param>
		/// <param name="upperBound">The upper bound for the set.  Can be any legal character value.</param>
		public PascalSet(char lowerBound, char upperBound) : this((int)lowerBound, (int)upperBound) { }

		#endregion

		#region Methods

        public int GetCardinality()
        {
            int[] ints = new int[(this.data.Count >> 5) + 1];
            this.data.CopyTo(ints, 0);
            int count = 0;

            // fix for not truncated bits in last integer that may have been set to true with SetAll()
            ints[ints.Length - 1] &= ~(-1 << (this.data.Count % 32));

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
		protected virtual bool AreSimilar(PascalSet pascalSet)
		{
			return this.lowerBound == pascalSet.lowerBound && this.upperBound == pascalSet.upperBound;
		}

		#region Union
		/// <summary>
		/// Unions a set of integers with the current PascalSet.
		/// </summary>
		/// <param name="list">An variable number of integers.</param>
		/// <returns>A new PascalSet, which is the union of the <b>this</b> PascalSet and the passed-in integers.</returns>
		public virtual PascalSet Union(params int[] list)
		{
			// create a deep copy of this
			PascalSet result = (PascalSet)Clone();

			// For each integer passed in, if it's within the bounds add it to the results's BitArray.
			for (int i = 0; i < list.LongLength; i++)
			{
				int val = list[i];
				if (val >= this.lowerBound && val <= this.upperBound)
				{
					int index = val - this.lowerBound;
					result.data.Set(index, true);
				}
				else
				{
					throw new ArgumentException("Attempting to add an element with value " 
												+ val.ToString(CultureInfo.InvariantCulture) 
												+ " that is outside of the set's universe.  Value must be between "
												+ this.lowerBound.ToString(CultureInfo.InvariantCulture) 
												+ " and "
												+ this.upperBound.ToString(CultureInfo.InvariantCulture));
				}
			}

			return result;		// return the new PascalSet
		}

		/// <summary>
		/// Unions a set of characters with the current PascalSet.
		/// </summary>
		/// <param name="list">A variable number of characters.</param>
		/// <returns>A new PascalSet, which is the union of the <b>this</b> PascalSet and the passed-in characters.</returns>
		public virtual PascalSet Union(params Char[] list)
		{
			int[] intForm = new int[list.Length];
			Array.Copy(list, intForm, list.Length);
			return Union(intForm);
		}

		/// <summary>
		/// Unions a passed-in PascalSet with the current PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet.</param>
		/// <returns>A new PascalSet whose elements are the union of <b>pascalSet</b> and <b>this</b>.</returns>
		/// <remarks><b>pascalSet</b> and <b>this</b> must be "similar" PascalSets.</remarks>
		public virtual PascalSet Union(PascalSet pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to union two dissimilar sets.  Union can only occur between two sets with the same universe.");

			// do a bit-wise OR to union together this.data and s.data
			PascalSet result = (PascalSet)Clone();
			result.data.Or(pascalSet.data);

			return result;
		}

		// Overloaded + operator for union...
		public static PascalSet operator +(PascalSet pascalSet1, PascalSet pascalSet2)
		{
			return pascalSet1.Union(pascalSet2);
		}

		public PascalSet Add(PascalSet pascalSet)
		{
			return this.Union(pascalSet);
		}

		// Overloaded + operator for union with one integer element
		public static PascalSet operator +(PascalSet pascalSet, int element)
		{
			return pascalSet.Union(new int[] { element });
		}

		public PascalSet Add(int element)
		{
			return this.Union(new int[] { element });
		}

		// Overloaded + operator for union with one character element
		public static PascalSet operator +(PascalSet pascalSet, Char element)
		{
			return pascalSet.Union(new int[] { (int)element });
		}

		public PascalSet Add(Char element)
		{
			return this.Union(new int[] { (int)element });
		}

		//Adds integer element to a set. No new object is created
		public virtual void AddElement(int element)
		{
			if (element >= this.lowerBound && element <= this.upperBound)
			{
				int index = element - this.lowerBound;
				this.data.Set(index, true);
			}
			else
			{
				throw new ArgumentException("Attempting to add an element with value " 
											+ element.ToString(CultureInfo.InvariantCulture) 
											+ " that is outside of the set's universe.  Value must be between "
											+ this.lowerBound.ToString(CultureInfo.InvariantCulture) 
											+ " and "
											+ this.upperBound.ToString(CultureInfo.InvariantCulture));
			}
		}

		//Removes integer element to a set. No new object is created
		public virtual void RemoveElement(int element)
		{
			if (element >= this.lowerBound && element <= this.upperBound)
			{
				int index = element - this.lowerBound;
				this.data.Set(index, false);
			}
			else
			{
				throw new ArgumentException("Attempting to remove an element with value " 
											+ element.ToString(CultureInfo.InvariantCulture) 
											+ " that is outside of the set's universe.  Value must be between "
											+ this.lowerBound.ToString(CultureInfo.InvariantCulture) 
											+ " and "
											+ this.upperBound.ToString(CultureInfo.InvariantCulture));
			}
		}

		#endregion

		#region Intersection
		/// <summary>
		/// Intersects a set of integers with the current PascalSet.
		/// </summary>
		/// <param name="list">An variable number of integers.</param>
		/// <returns>A new PascalSet, which is the intersection of the <b>this</b> PascalSet and the passed-in integers.</returns>
		public virtual PascalSet Intersection(params int[] list)
		{
			PascalSet result = new PascalSet(this.lowerBound, this.upperBound);

			for (int i = 0; i < list.Length; i++)
			{
				// only add the element to result if its in this.data
				int val = list[i];
				if (val >= this.lowerBound && val <= this.upperBound)
				{
					int index = val - this.lowerBound;
					if (this.data.Get(index))
					{
						result.data.Set(index, true);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Intersects a set of characters with the current PascalSet.
		/// </summary>
		/// <param name="list">A variable number of characters.</param>
		/// <returns>A new PascalSet, which is the intersection of the <b>this</b> PascalSet and the passed-in characters.</returns>
		public virtual PascalSet Intersection(params char[] list)
		{
			int[] intForm = new int[list.Length];
			Array.Copy(list, intForm, list.Length);
			return Intersection(intForm);
		}

		/// <summary>
		/// Intersects a passed-in PascalSet with the current PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet.</param>
		/// <returns>A new PascalSet whose elements are the intersection of <b>pascalSet</b> and <b>this</b>.</returns>
		/// <remarks><b>pascalSet</b> and <b>this</b> must be "similar" PascalSets.</remarks>
		public virtual PascalSet Intersection(PascalSet pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to intersect two dissimilar sets.  Intersection can only occur between two sets with the same universe.");

			// do a bit-wise AND to intersect this.data and s.data
			PascalSet result = (PascalSet)Clone();
			result.data.And(pascalSet.data);

			return result;
		}

		// Overloaded * operator for intersection
		public static PascalSet operator *(PascalSet pascalSet1, PascalSet pascalSet2)
		{
			return pascalSet1.Intersection(pascalSet2);
		}

		public PascalSet Multiply(PascalSet pascalSet)
		{
			return this.Intersection(pascalSet);
		}
		#endregion

		#region Difference
		/// <summary>
		/// Set differences a set of integers with the current PascalSet.
		/// </summary>
		/// <param name="list">An variable number of integers.</param>
		/// <returns>A new PascalSet, which is the set difference of the <b>this</b> PascalSet and the passed-in integers.</returns>
		public virtual PascalSet Difference(params int[] list)
		{
			PascalSet result = new PascalSet(this.lowerBound, this.upperBound, list);
			return Difference(result);
		}

		/// <summary>
		/// Set differences a set of characters with the current PascalSet.
		/// </summary>
		/// <param name="list">A variable number of characters.</param>
		/// <returns>A new PascalSet, which is the set difference of the <b>this</b> PascalSet and the passed-in characters.</returns>
		public virtual PascalSet Difference(params char[] list)
		{
			int[] intForm = new int[list.Length];
			Array.Copy(list, intForm, list.Length);
			return Difference(intForm);
		}

		/// <summary>
		/// Set differences a passed-in PascalSet with the current PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet.</param>
		/// <returns>A new PascalSet whose elements are the set difference of <b>pascalSet</b> and <b>this</b>.</returns>
		/// <remarks><b>pascalSet</b> and <b>this</b> must be "similar" PascalSets.</remarks>
		public virtual PascalSet Difference(PascalSet pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to apply set difference to two dissimilar sets.  Set difference can only occur between two sets with the same universe.");

			// do a bit-wise XOR and then an AND to achieve set difference
			PascalSet result = (PascalSet)Clone();
			result.data.Xor(pascalSet.data).And(this.data);

			return result;
		}

		// Overloaded - operator for set difference
		public static PascalSet operator -(PascalSet pascalSet1, PascalSet pascalSet2)
		{
			return pascalSet1.Difference(pascalSet2);
		}

		public PascalSet Subtract(PascalSet pascalSet)
		{
			return this.Difference(pascalSet);
		}

		// Overloaded - operator for set difference - substraction of one integer element
		public static PascalSet operator -(PascalSet pascalSet, int element)
		{
			return pascalSet.Difference(new int[] { element });
		}

		public PascalSet Subtract(int element)
		{
			return this.Difference(new int[] { element });
		}
		
		// Overloaded - operator for set difference - substraction of one character element
		public static PascalSet operator -(PascalSet pascalSet, char element)
		{
			return pascalSet.Difference(new int[] { (int)element });
		}

		public PascalSet Subtract(char element)
		{
			return this.Difference(new int[] { (int)element });
		}
		#endregion

		#region Complement
		/// <summary>
		/// Complements a PascalSet.
		/// </summary>
		/// <returns>A new PascalSet that is the complement of <b>this</b>.</returns>
		public virtual PascalSet Complement()
		{
			PascalSet result = (PascalSet)Clone();
			result.data.Not();
			return result;
		}
		#endregion

		#region Element Of
		/// <summary>
		/// Determines if a passed-in value is an element of the PascalSet.
		/// </summary>
		/// <param name="element">The integer to check if it exists in the set.</param>
		/// <returns><b>True</b> is <b>element</b> is in the set, <b>False</b> otherwise</returns>
		public virtual bool ContainsElement(int element)
		{
			if (element < this.lowerBound || element > this.upperBound)
				return false;

			int index = element - this.lowerBound;
			return this.data.Get(index);
		}

		/// <summary>
		/// Determines if a passed-in value is an element of the PascalSet.
		/// </summary>
		/// <param name="element">The character to check if it exists in the set.</param>
		/// <returns><b>True</b> is <b>element</b> is in the set, <b>False</b> otherwise</returns>
		public virtual bool ContainsElement(char element)
		{
			return ContainsElement((int)element);
		}

		#endregion

		#region Subset
		/// <summary>
		/// Determins if this set is a subset of the integers passed-in.
		/// </summary>
		/// <param name="list">A variable number of integers.</param>
		/// <returns><b>True</b> if <b>this</b> is a subset of the passed-in integers; <b>False</b> otherwise.</returns>
		public virtual bool Subset(params int[] list)
		{
			PascalSet temp = new PascalSet(this.lowerBound, this.upperBound, list);
			return Subset(temp);
		}

		/// <summary>
		/// Determins if this set is a subset of the characters passed-in.
		/// </summary>
		/// <param name="list">A variable number of characters.</param>
		/// <returns><b>True</b> if <b>this</b> is a subset of the passed-in characters; <b>False</b> otherwise.</returns>
		public virtual bool Subset(params char[] list)
		{
			int[] intForm = new int[list.Length];
			Array.Copy(list, intForm, list.Length);
			return Subset(intForm);
		}

		/// <summary>
		/// Determins if this set is a subset of the passed-in PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet that is "similar" to <b>this</b>.</param>
		/// <returns><b>True</b> if <b>this</b> is a subset of <b>pascalSet</b>; <b>False</b> otherwise.</returns>
		public virtual bool Subset(PascalSet pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to compare two dissimilar sets.  Subset comparisons can only occur between two sets with the same universe.");

			// Get the BitArray's underlying array
			const int INT_SIZE = 32;
			int arraySize = (data.Length + INT_SIZE - 1) / INT_SIZE;
			int[] thisBits = new int[arraySize];
			int[] sBits = new int[arraySize];
			data.CopyTo(thisBits, 0);
			pascalSet.data.CopyTo(sBits, 0);

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
		public virtual bool ProperSubset(params int[] list)
		{
			PascalSet temp = new PascalSet(this.lowerBound, this.upperBound, list);
			return ProperSubset(temp);
		}

		/// <summary>
		/// Determins if this set is a proper subset of the characters passed-in.
		/// </summary>
		/// <param name="list">A variable number of characters.</param>
		/// <returns><b>True</b> if <b>this</b> is a proper subset of the passed-in characters; <b>False</b> otherwise.</returns>
		public virtual bool ProperSubset(params char[] list)
		{
			int[] intForm = new int[list.Length];
			Array.Copy(list, intForm, list.Length);
			return ProperSubset(intForm);
		}

		/// <summary>
		/// Determins if this set is a proper subset of the passed-in PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet that is "similar" to <b>this</b>.</param>
		/// <returns><b>True</b> if <b>this</b> is a proper subset of <b>pascalSet</b>; <b>False</b> otherwise.</returns>
		public virtual bool ProperSubset(PascalSet pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to compare two dissimilar sets.  Subset comparisons can only occur between two sets with the same universe.");

			return Subset(pascalSet) && !pascalSet.Subset(this);
		}
		#endregion

		#region Superset
		/// <summary>
		/// Determins if this set is a superset of the integers passed-in.
		/// </summary>
		/// <param name="list">A variable number of integers.</param>
		/// <returns><b>True</b> if <b>this</b> is a superset of the passed-in integers; <b>False</b> otherwise.</returns>
		public virtual bool Superset(params int[] list)
		{
			PascalSet temp = new PascalSet(this.lowerBound, this.upperBound, list);
			return Superset(temp);
		}

		/// <summary>
		/// Determins if this set is a superset of the characters passed-in.
		/// </summary>
		/// <param name="list">A variable number of characters.</param>
		/// <returns><b>True</b> if <b>this</b> is a superset of the passed-in characters; <b>False</b> otherwise.</returns>
		public virtual bool Superset(params char[] list)
		{
			int[] intForm = new int[list.Length];
			Array.Copy(list, intForm, list.Length);
			return Superset(intForm);
		}

		/// <summary>
		/// Determins if this set is a superset of the passed-in PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet that is "similar" to <b>this</b>.</param>
		/// <returns><b>True</b> if <b>this</b> is a superset of <b>pascalSet</b>; <b>False</b> otherwise.</returns>
		public virtual bool Superset(PascalSet pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to compare two dissimilar sets.  Superset comparisons can only occur between two sets with the same universe.");

			return pascalSet.Subset(this);
		}

		/// <summary>
		/// Determins if this set is a proper superset of the integers passed-in.
		/// </summary>
		/// <param name="list">A variable number of integers.</param>
		/// <returns><b>True</b> if <b>this</b> is a proper superset of the passed-in integers; <b>False</b> otherwise.</returns>
		public virtual bool ProperSuperset(params int[] list)
		{
			PascalSet temp = new PascalSet(this.lowerBound, this.upperBound, list);
			return ProperSuperset(temp);
		}

		/// <summary>
		/// Determins if this set is a proper superset of the characters passed-in.
		/// </summary>
		/// <param name="list">A variable number of characters.</param>
		/// <returns><b>True</b> if <b>this</b> is a proper superset of the passed-in characters; <b>False</b> otherwise.</returns>
		public virtual bool ProperSuperset(params char[] list)
		{
			int[] intForm = new int[list.Length];
			Array.Copy(list, intForm, list.Length);
			return ProperSuperset(intForm);
		}

		/// <summary>
		/// Determins if this set is a proper superset of the passed-in PascalSet.
		/// </summary>
		/// <param name="pascalSet">A PascalSet that is "similar" to <b>this</b>.</param>
		/// <returns><b>True</b> if <b>this</b> is a proper superset of <b>pascalSet</b>; <b>False</b> otherwise.</returns>
		public virtual bool ProperSuperset(PascalSet pascalSet)
		{
			if (!AreSimilar(pascalSet))
				throw new ArgumentException("Attempting to compare two dissimilar sets.  Superset comparisons can only occur between two sets with the same universe.");

			return Superset(pascalSet) && !pascalSet.Superset(this);
		}
		#endregion

		public int[] ToArray()
		{
			int size = this.Count;
			int[] array = new int[size];

			if (size == 0)
				return array;

			int j = 0;
			for(int i = 0; i < data.Length; i++)
			{
				if (data.Get(i))
				{
					array[j++] =  i + this.lowerBound;
				}
			}

			return array;
		}

		#endregion

		#region PascalSet Properties
		/// <summary>
		/// Returns the lower bound of the set.
		/// </summary>
		public virtual int LowerBound
		{
			get
			{
				return this.lowerBound;
			}
		}

		/// <summary>
		/// Returns the upper bound of the set.
		/// </summary>
		public virtual int UpperBound
		{
			get
			{
				return this.upperBound;
			}
		}

		public BitArray Data
		{
			get
			{
				return data;
			}
		}

		#endregion

		#region ICloneable Members
		/// <summary>
		/// Clones the PascalSet, performing a deep copy.
		/// </summary>
		/// <returns>A new instance of a PascalSet, using a deep copy.</returns>
		public virtual object Clone()
		{
			PascalSet p = new PascalSet(lowerBound, upperBound);
			p.data = new BitArray(this.data);
			return p;
		}
		#endregion

		#region ICollection Members

		/// <summary>
		/// Returns a value indicating whether access to the ICollection is synchronized (thread-safe).
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
				if (cardinality == -1)
				{
					//int elements = 0;
					//for (int i = 0; i < data.Length; i++)
					//	if (data.Get(i)) elements++;
					//cardinality = elements;
                    cardinality = this.GetCardinality();
				}

				return cardinality;
			}
		}

		/// <summary>
		/// Copies the elements of the ICollection to an Array, starting at a particular Array index.
		/// </summary>
		public void CopyTo(Array array, int index)
		{
			data.CopyTo(array, index);
		}

		// Provide the strongly typed member for ICollection.
		public void CopyTo(int[] array, int index)
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
		public IEnumerator<int> GetEnumerator()
		{
			int totalElements = Count;
			int itemsReturned = 0;
			for (int i = 0; i < this.data.Length; i++)
			{
				if (itemsReturned >= totalElements)
					break;
				else if (this.data.Get(i))
					yield return i + this.lowerBound;
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
			for (int i = 0; i < this.data.Length; i++)
			{
				if (this.data.Get(i))
				{
					sb.Digits(i + this.lowerBound).Append(' ');  
				}
			}

			return sb.ToString();
		}

		public override int GetHashCode()
		{
			return HashHelper.GetHashCode<int>(data.GetInternalValues());
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			PascalSet pascalSet = obj as PascalSet;
			if (pascalSet == null)
				return false;

			if (this.LowerBound != pascalSet.LowerBound
				|| this.UpperBound != pascalSet.UpperBound)
				return false;

			for (int i = 0; i < this.data.Length; i++)
				if (this.Data[i] != pascalSet.Data[i])
					return false;

			return true;
		}

		#endregion
	}
}
