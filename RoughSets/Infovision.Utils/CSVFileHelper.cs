using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infovision.Utils
{
    public static class CSVFileHelper
    {        
        public static string GetRecord(char separator, params object[] values)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= values.Length; i++)
            {
                sb.Append(values[i-1].ToString());
                if(i != values.Length)
                    sb.Append(separator);
            }   
            return sb.ToString();
        }
    }
}
