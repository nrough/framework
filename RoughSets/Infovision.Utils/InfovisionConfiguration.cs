﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{
    public class InfovisionConfiguration
    {
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
                        maxDegreeOfParallelism = 1;
                        return 1;
#endif
                        maxDegreeOfParallelism = Environment.ProcessorCount;
                        return maxDegreeOfParallelism;
                    }
                    else
                    {
                        if (!Int32.TryParse(result, out maxDegreeOfParallelism))
                        {
#if DEBUG
                        maxDegreeOfParallelism = 1;
                        return 1;
#endif
                            maxDegreeOfParallelism = Environment.ProcessorCount;
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