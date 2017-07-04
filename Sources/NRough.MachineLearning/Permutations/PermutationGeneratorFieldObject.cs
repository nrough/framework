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
using System.Linq;
using NRough.Data;
using NRough.Core;
using NRough.Core.Random;

namespace NRough.MachineLearning.Permutations
{
    public class PermutationAttributeObjectGenerator : PermutationGenerator
    {
        private double fieldSelectionRatio = 0.5;
        private int numberOfFields = 0;

        #region Constructors

        public PermutationAttributeObjectGenerator(int[] objects, int[] fields, double fieldSelectionRatio)
        {
            this.elements = new int[objects.Length + fields.Length];
            for (int i = 0; i < objects.Length; i++)
                this.elements[i] = -objects[i];
            Buffer.BlockCopy(fields, 0, this.elements, objects.Length * sizeof(int), fields.Length * sizeof(int));
            this.fieldSelectionRatio = fieldSelectionRatio;
            this.numberOfFields = fields.Length;
        }

        public PermutationAttributeObjectGenerator(int[] objects, int[] fields)
            : this(objects, fields, (double)fields.Length / (double)(fields.Length + objects.Length))
        {
        }

        public PermutationAttributeObjectGenerator(DataStore dataSet)
            : this(Enumerable.Range(0, dataSet.NumberOfRecords).ToArray(), dataSet.GetStandardFields())
        {
        }

        public PermutationAttributeObjectGenerator(DataStore dataSet, double fieldSelectionRatio)
            : this(Enumerable.Range(0, dataSet.NumberOfRecords).ToArray(), dataSet.GetStandardFields(), fieldSelectionRatio)
        {
        }

        #endregion Constructors

        #region Properties

        protected int NumberOfObjects
        {
            get { return this.elements.Length - this.numberOfFields; }
        }

        protected int NumberOfFields
        {
            get { return this.numberOfFields; }
        }

        protected double FieldSelectionRatio
        {
            get { return this.fieldSelectionRatio; }
        }

        #endregion Properties

        #region Methods

        protected override Permutation CreatePermutation()
        {
            List<int> fieldList = new List<int>(this.GetFieldsArray());
            List<int> objectList = new List<int>(this.GetObjectArray());

            int[] localElements = new int[this.elements.Length];

            for (int j = 0; j < this.elements.Length; j++)
            {
                int element = 0;

                if (fieldList.Count > 0 && objectList.Count > 0)
                {
                    if (this.fieldSelectionRatio >= 1.0
                        || RandomSingleton.Random.NextDouble() < (double)this.fieldSelectionRatio)
                    {
                        element = this.GetAndRemoveListElement<int>(fieldList);
                    }
                    else
                    {
                        element = this.GetAndRemoveListElement<int>(objectList);
                    }
                }
                else if (fieldList.Count > 0)
                {
                    element = this.GetAndRemoveListElement<int>(fieldList);
                }
                else
                {
                    element = this.GetAndRemoveListElement<int>(objectList);
                }

                localElements[j] = element;
            }

            Permutation permutation = new Permutation(localElements);
            return permutation;
        }

        protected virtual int[] GetFieldsArray()
        {
            int[] fieldArray = new int[this.NumberOfFields];
            for (int i = this.NumberOfObjects; i < this.NumberOfFields + this.NumberOfObjects; i++)
            {
                fieldArray[i - this.NumberOfObjects] = this.elements[i];
            }
            return fieldArray;
        }

        protected virtual int[] GetObjectArray()
        {
            int[] objectArray = new int[this.NumberOfObjects];
            for (int i = 0; i < this.NumberOfObjects; i++)
            {
                objectArray[i] = this.elements[i];
            }
            return objectArray;
        }

        protected T GetAndRemoveListElement<T>(List<T> list)
        {
            int k = RandomSingleton.Random.Next() % (list.Count);
            T element = list[k];
            list.RemoveAt(k);
            return element;
        }

        public static double CalcSelectionRatio(int numberOfFields, int numberOfObjects, double fieldSelectionRatio)
        {
            if (numberOfFields <= 0)
                throw new System.ArgumentOutOfRangeException("numberOfFields", "Should be greater than zero");
            if (numberOfObjects <= 0)
                throw new System.ArgumentOutOfRangeException("numberOfObjects", "Should be greater than zero");

            if (numberOfFields < numberOfObjects)
                return 2 * fieldSelectionRatio * ((double)numberOfFields / ((double)numberOfFields + (double)numberOfObjects));

            return fieldSelectionRatio * ((double)numberOfObjects / ((double)numberOfFields + (double)numberOfObjects));
        }

        #endregion Methods
    }
}