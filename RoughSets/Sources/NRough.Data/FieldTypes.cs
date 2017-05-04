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