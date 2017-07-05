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
using NRough.Core;
using NRough.Core.Helpers;

namespace NRough.Data
{
    [Flags]
    public enum FieldGroup
    {
        None = 0,
        Standard = 1,
        Id = 2,
        Output = 4,
        Sys = 8,
        Weight = 16,

        All = 31
    }    

    public static class FieldTypesHelper
    {
        public static List<FieldGroup> basicTypes = null;
        public static readonly object syncRoot = new object();

        public static ICollection<FieldGroup> BasicFieldTypes
        {
            get
            {
                if (basicTypes == null)
                {
                    lock (syncRoot)
                    {
                        if (basicTypes == null)
                        {
                            basicTypes = new List<FieldGroup>();
                            foreach (FieldGroup ft in EnumHelper.GetValues<FieldGroup>())
                            {
                                if (ft == FieldGroup.None
                                    || ft == FieldGroup.All)
                                {
                                    continue;
                                }

                                if (MiscHelper.IsPowerOfTwo((long)ft))
                                {
                                    basicTypes.Add(ft);
                                }
                            }
                        }
                    }
                }

                return basicTypes;
            }
        }
    }
}