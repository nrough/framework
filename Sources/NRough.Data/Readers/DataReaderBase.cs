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
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Readers
{
    public abstract class DataReaderBase : IDataReader
    {
        public string DataName { get; set; }
        public int DecisionId { get; set; }
        public int ExpectedColumns { get; set; }
        public int ExpectedRows { get; set; }
        public bool HandleMissingData { get; set; }
        public string MissingValue { get; set; }        
        public DataStoreInfo ReferenceDataStoreInfo { get; set; }                

        public DataReaderBase()
        {
            DataName = String.Empty;
            DecisionId = -1;
            ExpectedColumns = -1;
            ExpectedRows = -1;
            HandleMissingData = true;
            MissingValue = "?";
            ReferenceDataStoreInfo = null;
        }
        

        public abstract DataStore Read();
    }
}
