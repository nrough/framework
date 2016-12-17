using System;
using System.Collections.Generic;
using Infovision.Core;

namespace Infovision.Data
{
    [Flags]
    public enum FieldTypes
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
        public static List<FieldTypes> basicTypes = null;
        public static readonly object syncRoot = new object();

        public static ICollection<FieldTypes> BasicFieldTypes
        {
            get
            {
                if (basicTypes == null)
                {
                    lock (syncRoot)
                    {
                        if (basicTypes == null)
                        {
                            basicTypes = new List<FieldTypes>();
                            foreach (FieldTypes ft in EnumHelper.GetValues<FieldTypes>())
                            {
                                if (ft == FieldTypes.None
                                    || ft == FieldTypes.All)
                                {
                                    continue;
                                }

                                if (InfovisionHelper.IsPowerOfTwo((long)ft))
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