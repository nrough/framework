using System;
using System.Collections.Generic;
using Infovision.Utils;

namespace Infovision.Data
{
    [Flags]
    public enum FieldTypes
    {
        None = 0,
        Standard = 1,
        Identifier = 2,
        Decision = 4,
        Technical = 8,

        All = 15
    }

    public static class FieldTypesHelper
    {
        public static List<FieldTypes> basicTypes = null;
        public static object syncRoot = new object();
        
        public static IEnumerable<FieldTypes> BasicFieldTypes
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

                                if (InfovisionHelper.IsPowerOfTwo((ulong)ft))
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