using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
	public class ReductCrisp : ReductWeights
	{
		private HashSet<int> removedAttributes;
		private bool isEqMapCreated;
		
		#region Constructors
		
		public ReductCrisp(DataStore dataStore)
			: base(dataStore, 0.0)
		{
			this.Init();
		}

		public ReductCrisp(DataStore dataStore, double epsilon)
			: base(dataStore, epsilon)
		{
			this.Init();
		}

		public ReductCrisp(DataStore dataStore, int[] fieldIds, double epsilon)
			: base(dataStore, fieldIds, epsilon)
		{
			this.Init();
		}

		public ReductCrisp(DataStore dataStore, int[] fieldIds, double[] weights, double epsilon)
			: base(dataStore, fieldIds, weights, epsilon)
		{
			this.Init();
		}        

		public ReductCrisp(ReductCrisp reduct)
			: base(reduct as ReductWeights)
		{
			//TODO Casting Error:
			/*
			Test 'Infovision.Datamining.Roughset.UnitTests.ReductGeneralDecisionGeneratorTest.GenerateTest(System.Collections.Generic.Dictionary`2[System.String,System.Object])' failed:
				System.InvalidCastException : Nie można rzutować obiektu typu 'Infovision.Datamining.Roughset.EquivalenceClassCollection' na typ 'Infovision.Datamining.Roughset.EquivalenceClassSortedMap'.
				w Infovision.Datamining.Roughset.ReductCrisp..ctor(ReductCrisp reduct) w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductCrisp.cs:wiersz 38
				w Infovision.Datamining.Roughset.ReductCrisp.Clone() w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductCrisp.cs:wiersz 105
				w Infovision.Datamining.Roughset.ReductStore..ctor(ReductStore reductStore) w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductStore.cs:wiersz 121
				w Infovision.Datamining.Roughset.ReductStore.RemoveDuplicates() w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductStore.cs:wiersz 157
				w Infovision.Datamining.Roughset.ReductGeneralDecisionGenerator.Generate() w f:\Projects\Infovision\Infovision.Datamining.RoughSet\ReductGeneralDecisionGenerator.cs:wiersz 56
				w Infovision.Datamining.Roughset.UnitTests.ReductGeneralDecisionGeneratorTest.GenerateTest(Dictionary`2 args) w f:\Projects\Infovision\Infovision.Datamining.Roughset.UnitTests\ReductGeneralDecisionGeneratorTest.cs:wiersz 87
			*/

			//this.EquivalenceClassCollection = (EquivalenceClassSortedMap) reduct.EquivalenceClassCollection.Clone();

			//TODO Temporary fix : This will cause EQ map to be recalculated on next call
			this.EquivalenceClasses = null;
			this.removedAttributes = new HashSet<int>(reduct.removedAttributes);
		}

		#endregion

		#region Methods

		private void Init()
		{
			this.removedAttributes = new HashSet<int>();
			this.isEqMapCreated = false;
		}

		protected override void InitEquivalenceMap()
		{
			if (isEqMapCreated == false)
				this.EquivalenceClasses = new EquivalenceClassSortedMap(this.DataStore);
			else
				throw new InvalidOperationException("EquicalenceClassMap can only be initialized once.");
		}

		public override void BuildEquivalenceMap()
		{
			//base.BuildEquivalenceMap();
			if (isEqMapCreated == false)
			{ 
				this.InitEquivalenceMap();
				this.EquivalenceClasses.Calc(this.Attributes, this.DataStore, this.Weights);
				
				this.isEqMapCreated = true;
			}
		}

		/// <summary>
		/// Method tries to remove attributes from current reduct. Attributes are removed in order passed in attributeOrder array.
		/// </summary>
		/// <param name="attributeOrder">Attributes to be tried to be removed in given order.</param>
		public virtual void Reduce(int[] attributeOrder, int minimumLength)
		{
			if (minimumLength == this.DataStore.DataStoreInfo.GetNumberOfFields(FieldTypes.Standard))
				return;
			
			foreach (EquivalenceClass eq in this.EquivalenceClasses)
				eq.RemoveObjectsWithMinorDecisions();
			
			bool isReduced = false;
			int len = attributeOrder.Length;
			int reduced = 0;
			for (int i = len - 1; (i >= 0) && (len - minimumLength - reduced > 0); i--)
			{
				if (this.TryRemoveAttribute(attributeOrder[i]))
				{
					reduced++;
					isReduced = true;
				}
			}

			if (isReduced)
			{
				this.EquivalenceClasses.Calc(this.Attributes, this.DataStore, this.Weights);

				/*
				EquivalenceClassSortedMap newEqMap = new EquivalenceClassSortedMap(this.DataStore);
				foreach (EquivalenceClass eq in this.EquivalenceClassCollection)
				{                    
					AttributeValueVector instance = eq.Instance;
					foreach (int removedAttribute in this.removedAttributes)
						instance = instance.RemoveAttribute(removedAttribute);

					EquivalenceClass existingEqClass = newEqMap.GetEquivalenceClass(instance);

					if (existingEqClass == null)
					{
						existingEqClass = new EquivalenceClass(instance, this.DataStore);
						newEqMap.Partitions.Add(instance, existingEqClass);
					}

					existingEqClass.Merge(eq);					
				}

				this.EquivalenceClassCollection = newEqMap;
				*/

				this.removedAttributes = new HashSet<int>();
			}
		}

		
		/// <summary>
		/// Method tries to remove attributes from current reduct
		/// </summary>
		/// <remarks>
		/// this method always reduce attributes in increasing order, consider using Reduce(int[] attributes)
		/// </remarks>
		public virtual void Reduce()
		{
			this.Reduce(this.Attributes.ToArray(), 0);
		}

		public override bool TryRemoveAttribute(int attributeId)
		{
			//base.TryRemoveAttribute(attributeId);
			if (this.CheckRemoveAttribute(attributeId))
			{
				this.Attributes.RemoveElement(attributeId);
				this.removedAttributes.Add(attributeId);								
				return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if an attribute can be removed from super-reduct.
		/// </summary>
		/// <param name="attributeId">Attribute Id to remove</param>
		/// <returns></returns>
		protected override bool CheckRemoveAttribute(int attributeId)
		{
			//checks if attribute exists in current reduct
			bool ret = base.CheckRemoveAttribute(attributeId);

			if (ret == false)
				return false;
						
			//duplicate current attribute set and remove selected attribute
			FieldSet newFieldSet = (FieldSet) (this.Attributes - attributeId);
					
			//new temporary equivalence class map with decision set intersection  (data vector --> generalized decision)
			Dictionary<AttributeValueVector, PascalSet> generalDecisionMap = new Dictionary<AttributeValueVector, PascalSet>();                                                            
			DataFieldInfo decisionFieldInfo = this.DataStore.DataStoreInfo.GetDecisionFieldInfo();
				
			foreach (EquivalenceClass eq in this.EquivalenceClasses)
			{
				//instance of a record belonging to equivalence class
				AttributeValueVector instance = eq.Instance.RemoveAttribute(attributeId);
				foreach (int removedAttribute in this.removedAttributes)
					instance = instance.RemoveAttribute(removedAttribute);

				//add EQ class to map and calculate intersection of decisions
				PascalSet existingGeneralDecisions = null;

				if (generalDecisionMap.TryGetValue(instance, out existingGeneralDecisions))
				{
					existingGeneralDecisions = existingGeneralDecisions.Intersection(eq.DecisionSet);
				}
				else
				{
					existingGeneralDecisions = eq.DecisionSet;
				}
				generalDecisionMap[instance] = existingGeneralDecisions;

				//empty intersection => we cannot remove the attribute
				if (existingGeneralDecisions.GetCardinality() == 0)
				{
					return false;
				}
			}
			
			return true;            
		}

		#region ICloneable Members
		/// <summary>
		/// Clones the Reduct, performing a deep copy.
		/// </summary>
		/// <returns>A new instance of a FieldSet, using a deep copy.</returns>
		public override object Clone()
		{
			return new ReductCrisp(this);
		}
		#endregion        

		#region System.Object Methods                

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
		   
			ReductCrisp reduct = obj as ReductCrisp;
			if (reduct == null)
				return false;

			return base.Equals(reduct);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		#endregion        
	}
}
