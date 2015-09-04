using System;
using System.Collections.Generic;
using System.Linq;

namespace Infovision.Utils
{
    /*
    internal class MatrixValue<T>
        where T : IComparable
    {
        private T val;
        
        public MatrixValue(T val)
        {
            this.val = val;
        }

        public T Value
        {
            get { return this.val; }
            set { this.val = value; }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(this.val);
            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            MatrixValue<T> element = obj as MatrixValue<T>;
            if ((System.Object)element == null)
                return false;

            return element.val.CompareTo(this.val) == 0;
        }

        public override int GetHashCode()
        {
            return this.val.GetHashCode();
        }
    }
    
    public class MatrixElement<R,C>
    {
        private R rowElement;
        private C columnElement;
        
        public MatrixElement()
        {
        }

        public MatrixElement(R rowElement, C columnElement)
        {
            this.columnElement = columnElement;
            this.rowElement = rowElement;
        }

        public C ColumnElement
        {
            get { return this.columnElement; }
            protected set { this.columnElement = value; }
        }

        public R RowElement
        {
            get { return this.rowElement; }
            protected set { this.rowElement = value; }
        }

        public override int GetHashCode()
        {
            return rowElement.GetHashCode() ^ columnElement.GetHashCode();
        }

        public override bool Equals(object obj)
        {
 	         if (obj == null)
                return false;

            MatrixElement<R,C> element = obj as MatrixElement<R,C>;
            if ((System.Object)element == null)
                return false;

            return rowElement.Equals(element.RowElement) 
                && columnElement.Equals(element.ColumnElement);
        }
    }
    */

    [Serializable]
    public class Matrix<R, C, V>
        where V : IComparable
    {
        #region Globals
        private Dictionary<R, Dictionary<C, V>> rows;
        private Dictionary<C, Dictionary<R, V>> cols;
        #endregion

        #region Constructors
        public Matrix()
        {
            rows = new Dictionary<R, Dictionary<C, V>>();
            cols = new Dictionary<C, Dictionary<R, V>>();
        }

        public Matrix(int numberOfRows, int numberOfCols)
        {
            rows = new Dictionary<R, Dictionary<C, V>>(numberOfRows);
            cols = new Dictionary<C, Dictionary<R, V>>(numberOfCols);
        }
        #endregion

        #region Properties
        public int NumberOfRows
        {
            get { return rows.Count; }
        }

        public int NumberOfCols
        {
            get { return cols.Count; }
        }

        public ICollection<R> Rows
        {
            get { return rows.Keys; }
        }

        public ICollection<C> Columns
        {
            get { return cols.Keys; }
        }
        #endregion

        #region Methods

        public void AddElement(R rowElement, C colElement, V value)
        {
            Dictionary<C, V> localCols = null;
            if (rows.TryGetValue(rowElement, out localCols))
            {
                localCols.Add(colElement, value);
            }
            else
            {
                localCols = new Dictionary<C, V>();
                localCols.Add(colElement, value);
                rows[rowElement] = localCols;
            }

            Dictionary<R, V> localRows = null;
            if (cols.TryGetValue(colElement, out localRows))
            {
                localRows.Add(rowElement, value);
            }
            else
            {
                localRows = new Dictionary<R, V>();
                localRows.Add(rowElement, value);
                cols[colElement] = localRows;
            }
            
        }

        public void RemoveElement(R rowElement, C colElement)
        {
            Dictionary<C, V> localCols = null;
            if (rows.TryGetValue(rowElement, out localCols))
            {
                localCols.Remove(colElement);
                if (localCols.Count == 0)
                {
                    rows.Remove(rowElement);
                }
            }

            Dictionary<R, V> localRows = null;
            if (cols.TryGetValue(colElement, out localRows))
            {
                localRows.Remove(rowElement);
                if (localRows.Count == 0)
                {
                    cols.Remove(colElement);
                }
            }
        }

        public bool ContainsElement(R rowElement, C colElement)
        {
            if (rows.ContainsKey(rowElement))
            {
                Dictionary<C, V> localCols;
                if (rows.TryGetValue(rowElement, out localCols))
                {
                    return localCols.ContainsKey(colElement);
                }
            }

            return false;
        } 

        public void RemoveColumn(C columnElement)
        {
            cols.Remove(columnElement);

            List<R> rowToDelete = new List<R>();
            Dictionary<C, V> localCols;
            foreach (KeyValuePair<R, Dictionary<C, V>> rowElement in rows)
            {
                localCols = null;
                if (rows.TryGetValue(rowElement.Key, out localCols))
                {
                    localCols.Remove(columnElement);
                    if (localCols.Count == 0)
                    {
                        rowToDelete.Add(rowElement.Key);
                    }
                }
            }

            foreach (R rowElement in rowToDelete)
            {
                rows.Remove(rowElement);
            }
        }

        public void RemoveRow(R rowElement)
        {
            rows.Remove(rowElement);

            List<C> colToDelete = new List<C>();
            Dictionary<R, V> localRows;
            foreach (KeyValuePair<C, Dictionary<R, V>> colElement in cols)
            {
                localRows = null;
                if (cols.TryGetValue(colElement.Key, out localRows))
                {
                    localRows.Remove(rowElement);
                    if (localRows.Count == 0)
                    {
                        colToDelete.Add(colElement.Key);
                    }
                }
            }

            foreach (C colElement in colToDelete)
            {
                cols.Remove(colElement);
            }
        }

        public V GetValue(R rowElement, C colElement)
        {
            return rows[rowElement][colElement];
        }

        public void SetValue(R rowElement, C colElement, V value)
        {
            if (rows.ContainsKey(rowElement)
                    && cols.ContainsKey(colElement))
            {
                rows[rowElement][colElement] = value;
                cols[colElement][rowElement] = value;
                return;
            }

            this.AddElement(rowElement, colElement, value);
        }

        public void SetColumnValue(C colElement, V value)
        {
            Dictionary<R, V> localRows = null;
            if (cols.TryGetValue(colElement, out localRows))
            {
                foreach (KeyValuePair<R, V> kvp in localRows)
                {
                    localRows[kvp.Key] = value;
                }
            }
        }

        public void SetRowValue(R rowElement, V value)
        {
            Dictionary<C, V> localCols = null;
            if (rows.TryGetValue(rowElement, out localCols))
            {
                foreach (KeyValuePair<C, V> kvp in localCols)
                {
                    localCols[kvp.Key] = value;
                }
            }
        }

        public IEnumerable<V> GetRowValues(R rowElement)
        {
            return rows[rowElement].Values;
        }

        public IEnumerable<V> GetColValues(C colElement)
        {
            return cols[colElement].Values;
        }

        #endregion
    }

    [Serializable]
    public class MatrixInt<R, C> : Matrix<R, C, Int32>
    {
        public MatrixInt()
            : base()
        { 
        }

        public int SumColumn(C colElement)
        {
            return this.GetColValues(colElement).Sum();    
        }

        public int SumRow(R rowElement)
        {
            return this.GetRowValues(rowElement).Sum();
        }
    }
}
