using NRough.Doc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.Core
{
    [AssemblyTreeVisible(false)]
    public class ConfigManager
    {
#if DEBUG
        private static int DEBUG_MAX_DOP = 1; // Environment.ProcessorCount;
#endif
        private static int maxDegreeOfParallelism = -1;

        public static int MaxDegreeOfParallelism
        {
            get
            {
                if (maxDegreeOfParallelism != -1)
                {
                    return maxDegreeOfParallelism;
                }
                else
                {
                    string result = ConfigurationManager.AppSettings["MaxDegreeOfParallelism"];
                    if (String.IsNullOrEmpty(result))
                    {
#if DEBUG
                        maxDegreeOfParallelism = DEBUG_MAX_DOP;
#else
                        maxDegreeOfParallelism = Environment.ProcessorCount;
#endif
                        return maxDegreeOfParallelism;
                    }
                    else
                    {
                        if (!Int32.TryParse(result, out maxDegreeOfParallelism))
                        {
#if DEBUG
                            maxDegreeOfParallelism = 1;
#else
                            maxDegreeOfParallelism = Environment.ProcessorCount;
#endif

                            return maxDegreeOfParallelism;
                        }
                        else
                        {
                            return maxDegreeOfParallelism;
                        }
                    }

                }
            }

            set
            {
                maxDegreeOfParallelism = value;
            }
        }
    }
}
