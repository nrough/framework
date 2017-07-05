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
using System.Text;
using System.Threading.Tasks;

namespace NRough.Data.Filters
{
    public class KeepColumns : FilterBase
    {
        private IEnumerable<int> toKeep;
        public KeepColumns(IEnumerable<int> columnsToKeep)
            : base()
        {
            this.toKeep = columnsToKeep.ToArray();
        }        

        public override DataStore Apply(DataStore data)
        {            
            if (data == null) throw new ArgumentNullException("data");            
            var columns = new HashSet<int>(data.DataStoreInfo.SelectAttributeIds());
            foreach (int k in toKeep)
                columns.Remove(k);
            return new RemoveColumns(columns).Apply(data);
        }
    }
}
