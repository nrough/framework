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

using NRough.Core;
using NRough.Core.DataStructures;
using NRough.MachineLearning.Roughsets;
using NUnit.Framework;

namespace NRough.Tests.MachineLearning.Roughsets
{
    [TestFixture]
    public class MatrixTest
    {
        [Test]
        public void ConstructorTest()
        {
            Matrix<Reduct, int, int> matrix = new Matrix<Reduct, int, int>();
            Assert.IsNotNull(matrix);
        }

        protected void AddElements(Matrix<int, int, int> matrix)
        {
            matrix.AddElement(1, 1, 1);
            matrix.AddElement(1, 2, 1);
            matrix.AddElement(1, 3, 1);
            matrix.AddElement(2, 1, 1);
            matrix.AddElement(2, 2, 1);
            matrix.AddElement(2, 3, 1);
            matrix.AddElement(3, 1, 1);
            matrix.AddElement(3, 2, 1);
            matrix.AddElement(3, 3, 1);
        }

        [Test]
        public void AddElementTest()
        {
            Matrix<int, int, int> matrix = new Matrix<int, int, int>();

            this.AddElements(matrix);

            Assert.AreEqual(3, matrix.NumberOfCols);
            Assert.AreEqual(3, matrix.NumberOfRows);

            Assert.AreEqual(true, matrix.ContainsElement(1, 1));
            Assert.AreEqual(true, matrix.ContainsElement(1, 2));
            Assert.AreEqual(true, matrix.ContainsElement(1, 3));
            Assert.AreEqual(true, matrix.ContainsElement(2, 1));
            Assert.AreEqual(true, matrix.ContainsElement(2, 2));
            Assert.AreEqual(true, matrix.ContainsElement(2, 3));
            Assert.AreEqual(true, matrix.ContainsElement(3, 1));
            Assert.AreEqual(true, matrix.ContainsElement(3, 2));
            Assert.AreEqual(true, matrix.ContainsElement(3, 3));

            Assert.AreEqual(false, matrix.ContainsElement(1, 4));
            Assert.AreEqual(false, matrix.ContainsElement(2, 4));
            Assert.AreEqual(false, matrix.ContainsElement(3, 4));
        }

        [Test]
        public void RemoveElementTest()
        {
            Matrix<int, int, int> matrix = new Matrix<int, int, int>();
            this.AddElements(matrix);

            Assert.AreEqual(3, matrix.NumberOfCols);
            Assert.AreEqual(3, matrix.NumberOfRows);

            matrix.RemoveElement(1, 1);
            matrix.RemoveElement(1, 2);
            matrix.RemoveElement(1, 3);

            Assert.AreEqual(3, matrix.NumberOfCols);
            Assert.AreEqual(2, matrix.NumberOfRows);

            Assert.AreEqual(false, matrix.ContainsElement(1, 1));
            Assert.AreEqual(false, matrix.ContainsElement(1, 2));
            Assert.AreEqual(false, matrix.ContainsElement(1, 3));
            Assert.AreEqual(true, matrix.ContainsElement(2, 1));
            Assert.AreEqual(true, matrix.ContainsElement(2, 2));
            Assert.AreEqual(true, matrix.ContainsElement(2, 3));
            Assert.AreEqual(true, matrix.ContainsElement(3, 1));
            Assert.AreEqual(true, matrix.ContainsElement(3, 2));
            Assert.AreEqual(true, matrix.ContainsElement(3, 3));
        }

        [Test]
        public void RemoveColumnTest()
        {
            Matrix<int, int, int> matrix = new Matrix<int, int, int>();
            this.AddElements(matrix);

            Assert.AreEqual(3, matrix.NumberOfCols);
            Assert.AreEqual(3, matrix.NumberOfRows);

            matrix.RemoveColumn(1);

            Assert.AreEqual(2, matrix.NumberOfCols);
            Assert.AreEqual(3, matrix.NumberOfRows);

            Assert.AreEqual(false, matrix.ContainsElement(1, 1));
            Assert.AreEqual(true, matrix.ContainsElement(1, 2));
            Assert.AreEqual(true, matrix.ContainsElement(1, 3));
            Assert.AreEqual(false, matrix.ContainsElement(2, 1));
            Assert.AreEqual(true, matrix.ContainsElement(2, 2));
            Assert.AreEqual(true, matrix.ContainsElement(2, 3));
            Assert.AreEqual(false, matrix.ContainsElement(3, 1));
            Assert.AreEqual(true, matrix.ContainsElement(3, 2));
            Assert.AreEqual(true, matrix.ContainsElement(3, 3));
        }

        [Test]
        public void RemoveRowTest()
        {
            Matrix<int, int, int> matrix = new Matrix<int, int, int>();
            this.AddElements(matrix);

            Assert.AreEqual(3, matrix.NumberOfCols);
            Assert.AreEqual(3, matrix.NumberOfRows);

            matrix.RemoveRow(1);

            Assert.AreEqual(3, matrix.NumberOfCols);
            Assert.AreEqual(2, matrix.NumberOfRows);

            Assert.AreEqual(false, matrix.ContainsElement(1, 1));
            Assert.AreEqual(false, matrix.ContainsElement(1, 2));
            Assert.AreEqual(false, matrix.ContainsElement(1, 3));

            Assert.AreEqual(true, matrix.ContainsElement(2, 1));
            Assert.AreEqual(true, matrix.ContainsElement(2, 2));
            Assert.AreEqual(true, matrix.ContainsElement(2, 3));

            Assert.AreEqual(true, matrix.ContainsElement(3, 1));
            Assert.AreEqual(true, matrix.ContainsElement(3, 2));
            Assert.AreEqual(true, matrix.ContainsElement(3, 3));
        }

        [Test]
        public void MakeMatrixEmptyTest()
        {
            Matrix<int, int, int> matrix = new Matrix<int, int, int>();
            this.AddElements(matrix);

            Assert.AreEqual(3, matrix.NumberOfCols);
            Assert.AreEqual(3, matrix.NumberOfRows);

            matrix.RemoveColumn(1);
            matrix.RemoveColumn(2);
            matrix.RemoveColumn(3);

            Assert.AreEqual(0, matrix.NumberOfCols);
            Assert.AreEqual(0, matrix.NumberOfRows);
        }

        [Test]
        public void GetValueTest()
        {
            Matrix<int, int, int> matrix = new Matrix<int, int, int>();
            this.AddElements(matrix);

            Assert.AreEqual(1, matrix.GetValue(1, 1));
            Assert.AreEqual(1, matrix.GetValue(1, 2));
            Assert.AreEqual(1, matrix.GetValue(1, 3));
            Assert.AreEqual(1, matrix.GetValue(2, 1));
            Assert.AreEqual(1, matrix.GetValue(2, 2));
            Assert.AreEqual(1, matrix.GetValue(2, 3));
            Assert.AreEqual(1, matrix.GetValue(3, 1));
            Assert.AreEqual(1, matrix.GetValue(3, 2));
            Assert.AreEqual(1, matrix.GetValue(3, 3));
        }

        [Test]
        public void SetValueTest()
        {
            Matrix<int, int, int> matrix = new Matrix<int, int, int>();
            this.AddElements(matrix);

            Assert.AreEqual(1, matrix.GetValue(1, 1));
            matrix.SetValue(1, 1, 2);
            Assert.AreEqual(2, matrix.GetValue(1, 1));
        }

        [Test]
        public void SetNotExistingValueTest()
        {
            Matrix<int, int, int> matrix = new Matrix<int, int, int>();
            this.AddElements(matrix);

            matrix.SetValue(1, 4, 2);
            matrix.SetValue(2, 4, 2);
            matrix.SetValue(3, 4, 2);

            Assert.AreEqual(2, matrix.GetValue(1, 4));

            Assert.AreEqual(3, matrix.NumberOfRows);
            Assert.AreEqual(4, matrix.NumberOfCols);
        }

        [Test]
        public void SumColumnTest()
        {
            MatrixInt<int, int> matrix = new MatrixInt<int, int>();
            this.AddElements(matrix);

            Assert.AreEqual(3, matrix.SumColumn(1));
            matrix.SetValue(1, 1, 5);
            Assert.AreEqual(7, matrix.SumColumn(1));
            Assert.AreEqual(3, matrix.SumColumn(2));
            Assert.AreEqual(3, matrix.SumColumn(3));
        }

        [Test]
        public void SumRowTest()
        {
            MatrixInt<int, int> matrix = new MatrixInt<int, int>();
            this.AddElements(matrix);
            Assert.AreEqual(3, matrix.SumRow(1));
            matrix.SetValue(1, 1, 5);
            Assert.AreEqual(7, matrix.SumRow(1));
            Assert.AreEqual(3, matrix.SumRow(2));
            Assert.AreEqual(3, matrix.SumRow(3));
        }
    }
}