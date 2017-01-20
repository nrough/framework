using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Raccoon.Math
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
    public class DistanceMatrix : IDistanceMatrix, IEnumerable<KeyValuePair<SymetricPair<int, int>, double>>
    {
        private Dictionary<SymetricPair<int, int>, double> matrix;
        private ReadOnlyDictionary<SymetricPair<int, int>, double> readOnlyMatrix;
        private Func<double[], double[], double> distance;
        private int numberOfInstances;

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

        public ReadOnlyDictionary<SymetricPair<int, int>, double> ReadOnlyMatrix
        {
            get { return this.readOnlyMatrix; }
        }

        public double this[SymetricPair<int, int> key]
        {
            get { return this.matrix[key]; }
            set { this.matrix[key] = value; }
        }

        public double this[int x, int y]
        {
            get { return this.GetDistance(x, y); }
            set
            {
                SymetricPair<int, int> key = new SymetricPair<int, int>(x, y);
                if (this.matrix.ContainsKey(key))
                {
                    this.matrix[key] = value;
                }
                else
                {
                    this.matrix.Add(key, value);
                }
            }
        }

        public int NumberOfInstances
        {
            get { return this.numberOfInstances; }
            set { this.numberOfInstances = value; }
        }

        public bool ReverseDistanceFunction
        {
            get;
            set;
        }

        public DistanceMatrix()
        {
            matrix = new Dictionary<SymetricPair<int, int>, double>();
            readOnlyMatrix = new ReadOnlyDictionary<SymetricPair<int, int>, double>(matrix);
        }

        public DistanceMatrix(Func<double[], double[], double> distance)
        {
            this.Distance = distance;
            matrix = new Dictionary<SymetricPair<int, int>, double>();
            readOnlyMatrix = new ReadOnlyDictionary<SymetricPair<int, int>, double>(matrix);
        }

        public DistanceMatrix(int size, Func<double[], double[], double> distance)
        {
            this.Distance = distance;
            matrix = new Dictionary<SymetricPair<int, int>, double>(size);
            readOnlyMatrix = new ReadOnlyDictionary<SymetricPair<int, int>, double>(matrix);
        }

        public DistanceMatrix(DistanceMatrix distanceMatrix)
        {
            this.matrix = new Dictionary<SymetricPair<int, int>, double>(distanceMatrix.matrix);
            this.readOnlyMatrix = new ReadOnlyDictionary<SymetricPair<int, int>, double>(this.matrix);
            this.distance = distanceMatrix.Distance;
            this.ReverseDistanceFunction = distanceMatrix.ReverseDistanceFunction;
        }

        public void Initialize(double[][] points)
        {
            int size = points.Length * (points.Length - 1) / 2;
            matrix = new Dictionary<SymetricPair<int, int>, double>(size);
            readOnlyMatrix = new ReadOnlyDictionary<SymetricPair<int, int>, double>(matrix);
            this.numberOfInstances = points.Length;

            //calculate initial distance matrix
            for (int i = 0; i < points.Length; i++)
            {
                for (int j = i + 1; j < points.Length; j++)
                {
                    SymetricPair<int, int> key = new SymetricPair<int, int>(i, j);
                    double distance = this.Distance(points[i], points[j]);
                    if (this.ReverseDistanceFunction)
                    {
                        distance = System.Math.Exp(-distance);
                    }

                    matrix.Add(key, distance);
                }
            }
        }

        public double GetDistance(int x, int y)
        {
            SymetricPair<int, int> key = new SymetricPair<int, int>(x, y);
            if (!matrix.ContainsKey(new SymetricPair<int, int>(x, y)))
                key = new SymetricPair<int, int>(y, x);
            return matrix[key];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<SymetricPair<int, int>, double> kvp in matrix)
            {
                sb.AppendLine(String.Format("d({0}, {1}) = {2}", kvp.Key.Item1, kvp.Key.Item2, kvp.Value));
            }
            return sb.ToString();
        }

        public void Add(SymetricPair<int, int> key, double distance)
        {
            matrix.Add(key, distance);
        }

        public void Remove(SymetricPair<int, int> key)
        {
            matrix.Remove(key);
        }

        public bool ContainsKey(SymetricPair<int, int> key)
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
        public IEnumerator<KeyValuePair<SymetricPair<int, int>, double>> GetEnumerator()
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