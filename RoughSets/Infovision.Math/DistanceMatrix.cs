﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Infovision.Math
{
    public interface IDistanceMatrix
    {
        Func<double[], double[], double> Distance
        {
            get;
            set;
        }
    }

    [Serializable]
    public class DistanceMatrix : IDistanceMatrix, IEnumerable<KeyValuePair<MatrixKey, double>>
    {
        private Dictionary<MatrixKey, double> matrix;
        private ReadOnlyDictionary<MatrixKey, double> readOnlyMatrix;
        private Func<double[], double[], double> distance;

        public DistanceMatrix()
        {            
            matrix = new Dictionary<MatrixKey, double>();
            readOnlyMatrix = new ReadOnlyDictionary<MatrixKey, double>(matrix);
        }
        
        public DistanceMatrix(Func<double[], double[], double> distance)
        {
            this.Distance = distance;
            matrix = new Dictionary<MatrixKey, double>();
            readOnlyMatrix = new ReadOnlyDictionary<MatrixKey, double>(matrix);
        }

        public DistanceMatrix(int size, Func<double[], double[], double> distance)            
        {
            this.Distance = distance;
            matrix = new Dictionary<MatrixKey, double>(size);
            readOnlyMatrix = new ReadOnlyDictionary<MatrixKey, double>(matrix);
        }
                
        /// <summary>
        ///   Gets or sets the distance function used
        ///   as a distance metric between data points.
        /// </summary>
        /// 
        public Func<double[], double[], double> Distance
        {
            get { return this.distance; }
            set { this.distance = value; }
        }

        public ReadOnlyDictionary<MatrixKey, double> ReadOnlyMatrix
        {
            get { return this.readOnlyMatrix; }
        }

        public double this[MatrixKey key]
        {
            get { return this.matrix[key]; }
            set { this.matrix[key] = value; }
        }

        public double this[int x, int y]
        {
            get { return this.GetDistance(x, y); }
        }


        public void Initialize(double[][] points)
        {
            int size = points.Length * (points.Length - 1) / 2;
            matrix = new Dictionary<MatrixKey, double>(size);
            readOnlyMatrix = new ReadOnlyDictionary<MatrixKey, double>(matrix);
            
            //calculate initial distance matrix
            for (int i = 0; i < points.Length; i++)
            {
                for (int j = i + 1; j < points.Length; j++)
                {
                    MatrixKey key = new MatrixKey(i, j);
                    matrix.Add(key, this.Distance(points[i], points[j]));
                }
            }            
        }

        public MatrixKey FindMinimumDistance()
        {
            int[] result = new int[2] { -1, -1 };
            double minDistance = Double.MaxValue;

            foreach(KeyValuePair<MatrixKey, double> kvp in matrix)
            {                
                if (kvp.Value < minDistance)
                {
                    result[0] = kvp.Key.X;
                    result[1] = kvp.Key.Y;
                    minDistance = kvp.Value;
                }
            }           
            
            return new MatrixKey(result[0], result[1]);
        }        

        public double GetDistance(int x, int y)
        {
            MatrixKey key = new MatrixKey(x, y);
            if (!matrix.ContainsKey(new MatrixKey(x, y)))
                key = new MatrixKey(y, x);
            return matrix[key];
        }        

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<MatrixKey, double> kvp in matrix)
            {
                sb.AppendLine(String.Format("d({0}, {1}) = {2}", kvp.Key.X, kvp.Key.Y, kvp.Value));
            }
            return sb.ToString();
        }

        public void Add(MatrixKey key, double distance)
        {            
            matrix.Add(key, distance);
        }

        public bool ContainsKey(MatrixKey key)
        {
            return matrix.ContainsKey(key);
        }

        /// <summary>
        ///   Returns an enumerator that iterates through a collection.
        /// </summary>
        /// 
        /// <returns>
        ///   An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// 
        public IEnumerator<KeyValuePair<MatrixKey, double>> GetEnumerator()
        {
            return this.ReadOnlyMatrix.GetEnumerator();
        }

        /// <summary>
        ///   Returns an enumerator that iterates through a collection.
        /// </summary>
        /// 
        /// <returns>
        ///   An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// 
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.ReadOnlyMatrix.GetEnumerator();
        }
    }
}
