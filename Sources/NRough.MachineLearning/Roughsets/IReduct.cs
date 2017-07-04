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
using NRough.Data;
using System.Collections.Generic;

namespace NRough.MachineLearning.Roughsets
{
    public interface IReduct : ICloneable, IComparable
    {        
        DataStore DataStore { get; }
        HashSet<int> Attributes { get; }
        bool IsException { get; }
        
        //TODO Move to Bireduct Interface? (Exceptions?)
        //TODO This should be based on Equivalence Class Collection
        HashSet<int> SupportedObjects { get; }
        
        double[] Weights { get; }

        double Epsilon { get; }
        string Id { get; set; }

        EquivalenceClassCollection EquivalenceClasses { get; }
        bool IsEquivalenceClassCollectionCalculated { get; }

        bool AddAttribute(int attributeId);                
        bool TryRemoveAttribute(int attributeId);
        
        void SetEquivalenceClassCollection(EquivalenceClassCollection equivalenceClasses);

        int GetHashCode();
        bool Equals(object obj);
    }
}