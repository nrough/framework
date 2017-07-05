// 
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

using System;
using System.Collections.Generic;
using System.Linq;
using NRough.Core;
using NRough.Core.Helpers;

namespace NRough.Data
{
    [Serializable]
    public class DataRecordInternal
    {
        #region Members

        private Dictionary<int, long> fields;

        #endregion Members

        #region Properties

        public long ObjectId { get; set; }
        public int ObjectIdx { get; set; }

        public long this[int fieldId]
        {
            get { return fields[fieldId]; }
            set { fields[fieldId] = value; }
        }

        public int Length
        {
            get { return fields.Count; }
        }

        #endregion Properties

        #region Constructors

        public DataRecordInternal(int[] fieldIds, long[] fieldValues)
        {
            this.fields = new Dictionary<int, long>(fieldIds.Length);

            if (fieldIds.Length != fieldValues.Length)
                throw new InvalidOperationException("Field and key lists must have the same length");

            for (int i = 0; i < fieldIds.Length; i++)
                fields.Add(fieldIds[i], fieldValues[i]);
        }

        public DataRecordInternal(DataRecordInternal otherDataRecordInternal)
        {
            fields = new Dictionary<int, long>(otherDataRecordInternal.fields);
        }

        public DataRecordInternal(Dictionary<int, long> valueMap)
        {
            this.fields = valueMap;
        }

        #endregion Constructors

        #region Methods

        public IEnumerable<int> GetFields()
        {
            return fields.Keys;
        }

        public IEnumerable<long> GetValues()
        {
            return fields.Values;
        }

        #region System.Object Methods

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode<long>(fields.Values.Concat(fields.Keys.Select(i => (long)i)));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            DataRecordInternal p = obj as DataRecordInternal;
            if (p == null)
                return false;

            if (p.Length != this.Length)
                return false;

            try
            {
                foreach (KeyValuePair<int, long> kvp in this.fields)
                {
                    if (p[kvp.Key] != kvp.Value)
                        return false;
                }
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        #endregion System.Object Methods

        #endregion Methods
    }
}