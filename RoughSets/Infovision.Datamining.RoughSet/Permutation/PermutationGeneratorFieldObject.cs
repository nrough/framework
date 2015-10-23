﻿using System;
using System.Collections.Generic;
using Infovision.Data;
using Infovision.Utils;

namespace Infovision.Datamining.Roughset
{
    public class PermutationGeneratorFieldObject : PermutationGenerator
    {
        private decimal fieldSelectionRatio = 0.5M;
        private int numberOfFields = 0;

        #region Constructors

        public PermutationGeneratorFieldObject(int[] objects, int[] fields, decimal fieldSelectionRatio)
        {
            this.elements = new int[objects.Length + fields.Length];
            for (int i = 0; i < objects.Length; i++)
                this.elements[i] = -objects[i];
            Buffer.BlockCopy(fields, 0, this.elements, objects.Length * sizeof(int), fields.Length * sizeof(int));
            this.fieldSelectionRatio = fieldSelectionRatio;
            this.numberOfFields = fields.Length;
        }

        public PermutationGeneratorFieldObject(int[] objects, int[] fields)
            : this(objects, fields, (decimal)fields.Length / (decimal)(fields.Length + objects.Length))
        {
        }

        public PermutationGeneratorFieldObject(DataStore dataSet)
            : this(dataSet.GetObjectIndexes(), dataSet.DataStoreInfo.GetFieldIds(FieldTypes.Standard))
        {
        }

        public PermutationGeneratorFieldObject(DataStore dataSet, decimal fieldSelectionRatio)
            : this(dataSet.GetObjectIndexes(), dataSet.DataStoreInfo.GetFieldIds(FieldTypes.Standard), fieldSelectionRatio)
        {
        }

        #endregion

        #region Properties

        protected int NumberOfObjects
        {
            get { return this.elements.Length - this.numberOfFields; }
        }

        protected int NumberOfFields
        {
            get { return this.numberOfFields; }
        }

        protected decimal FieldSelectionRatio
        {
            get { return this.fieldSelectionRatio; }
        }

        #endregion

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
                    if (this.fieldSelectionRatio >= 1.0M
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

        public static decimal CalcSelectionRatio(int numberOfFields, int numberOfObjects, decimal fieldSelectionRatio)
        {
            if (numberOfFields <= 0)
                throw new System.ArgumentOutOfRangeException("numberOfFields", "Should be greater than zero");
            if (numberOfObjects <= 0)
                throw new System.ArgumentOutOfRangeException("numberOfObjects", "Should be greater than zero");

            if(numberOfFields < numberOfObjects)
                return 2 * fieldSelectionRatio * ((decimal)numberOfFields / ((decimal)numberOfFields + (decimal)numberOfObjects));

            return fieldSelectionRatio * ((decimal)numberOfObjects / ((decimal)numberOfFields + (decimal)numberOfObjects));
        }

        #endregion
    }
}