using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRough.MachineLearning.Classification
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// IFormattable result = new ClassificationResult();
    /// Console.WriteLine(result.ToString("G", null));
    /// </example>
    class ClassificationResultFormatter : IFormatProvider, ICustomFormatter
    {
        public char OutputSepartor { get; set; }
        protected readonly static char[] FormatSeparators = new char[] { ';' };

        public ClassificationResultFormatter()
        {
            this.OutputSepartor = '|';
        }

        public ClassificationResultFormatter(char outputSeparator)
            : this()
        {
            this.OutputSepartor = outputSeparator;
        }

        public object GetFormat(Type formatType)
        {
            return (formatType == typeof(ClassificationResult)) ? this : null;
        }

        public string Format(string format, object args, IFormatProvider formatProvider)
        {
            ClassificationResult result = args as ClassificationResult;
            if (result == null)
                return args.ToString();

            if(String.IsNullOrEmpty(format))
                return result.ToString();

            List<string> tokens = format.Split(FormatSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
            Dictionary<string, string> formatParams = new Dictionary<string, string>(tokens.Count);
            foreach (var token in tokens)
            {
                string[] parm = token.Split('=');
                if (!formatParams.ContainsKey(parm[0].Trim()))
                    formatParams.Add(parm[0].Trim(), parm.Length > 1 && !String.IsNullOrEmpty(parm[1]) ? parm[1] : String.Empty);
            }

            char outputSeparator = formatParams.ContainsKey("sep") ? formatParams["sep"].ToCharArray()[0] : this.OutputSepartor;
            if (formatParams.ContainsKey("sep")) formatParams.Remove("sep");

            bool showHeader = formatParams.ContainsKey("H");
            if (formatParams.ContainsKey("H")) formatParams.Remove("H");            

            StringBuilder sb = new StringBuilder();
            int i = 0, len = formatParams.Count;
            foreach (var kvp in formatParams)
            {
                i++;
                if (showHeader)
                {
                    switch (kvp.Key)
                    {
                        case "ds": sb.AppendFormat("{0,20}", "ds"); break;
                        case "model": sb.AppendFormat("{0,20}", "model"); break;
                        case "t": sb.AppendFormat("{0,2}", "t"); break;
                        case "f": sb.AppendFormat("{0,2}", "f"); break;
                        case "eps": sb.AppendFormat("{0,5}", "eps"); break;
                        case "ens": sb.AppendFormat("ens"); break;

                        case "cls": sb.AppendFormat("{0,4}", "cls"); break;
                        case "mcls": sb.AppendFormat("{0,4}", "mis"); break;
                        case "ucls": sb.AppendFormat("{0,4}", "ucls"); break;

                        case "wcls": sb.AppendFormat("{0,6}", "wcls"); break;
                        case "wmcls": sb.AppendFormat("{0,6}", "wmis"); break;
                        case "wucls": sb.AppendFormat("{0,6}", "wucls"); break;

                        case "acc": sb.AppendFormat("{0,6}", "acc"); break;
                        case "bal": sb.AppendFormat("{0,6}", "bal"); break;
                        case "conf": sb.AppendFormat("{0,6}", "conf"); break;
                        case "cov": sb.AppendFormat("{0,6}", "cov"); break;

                        case "mtime": sb.AppendFormat("mtime"); break;
                        case "ctime": sb.AppendFormat("ctime"); break;

                        case "erulhit": sb.AppendFormat("erulhit"); break;
                        case "erullen": sb.AppendFormat("erullen"); break;
                        case "srulhit": sb.AppendFormat("srulhit"); break;
                        case "srullen": sb.AppendFormat("srullen"); break;

                        case "attr": sb.AppendFormat("{0,6}", "attr"); break;
                        case "numrul": sb.AppendFormat("{0,7}", "numrul"); break;
                        case "numexrul": sb.AppendFormat("{0,7}", "numexrul"); break;
                        case "dthm": sb.AppendFormat("{0,5}", "dthm"); break;
                        case "dtha": sb.AppendFormat("{0,5}", "dtha"); break;

                        case "gamma": sb.AppendFormat("{0,7}", "gamma"); break;
                        case "alpha": sb.AppendFormat("{0,7}", "alpha"); break;
                        case "beta": sb.AppendFormat("{0,7}", "beta"); break;

                        case "desc": sb.AppendFormat("description"); break;

                        case "precisionmicro": sb.AppendFormat("{0,6}", "precisionmicro"); break;
                        case "precisionmacro": sb.AppendFormat("{0,6}", "precisionmacro"); break;
                        case "recallmicro": sb.AppendFormat("{0,6}", "recallmicro"); break;
                        case "recallmacro": sb.AppendFormat("{0,6}", "recallmacro"); break;
                        case "f1scoremicro": sb.AppendFormat("{0,6}", "f1scoremicro"); break;
                        case "f1scoremacro": sb.AppendFormat("{0,6}", "f1scoremacro"); break;
                    }
                }
                else
                {
                    switch (kvp.Key)
                    {
                        case "ds": sb.AppendFormat("{0,20}", String.IsNullOrEmpty(result.DatasetName) ? "" : result.DatasetName); break;
                        case "model": sb.AppendFormat("{0,20}", String.IsNullOrEmpty(result.ModelName) ? "" : result.ModelName); break;
                        case "t": sb.AppendFormat("{0,2}", result.TestNum); break;
                        case "f": sb.AppendFormat("{0,2}", result.Fold); break;
                        case "eps": sb.AppendFormat("{0,5:0.00}", result.Epsilon); break;
                        case "ens": sb.AppendFormat("{0,3}", result.EnsembleSize); break;

                        case "cls": sb.AppendFormat("{0,4}", result.Classified); break;
                        case "mcls": sb.AppendFormat("{0,4}", result.Misclassified); break;
                        case "ucls": sb.AppendFormat("{0,4}", result.Unclassified); break;

                        case "wcls": sb.AppendFormat("{0:0.0000}", result.WeightClassified); break;
                        case "wmcls": sb.AppendFormat("{0:0.0000}", result.WeightMisclassified); break;
                        case "wucls": sb.AppendFormat("{0:0.0000}", result.WeightUnclassified); break;

                        case "acc": sb.AppendFormat("{0:0.0000}", result.Accuracy); break;
                        case "bal": sb.AppendFormat("{0:0.0000}", result.BalancedAccuracy); break;
                        case "conf": sb.AppendFormat("{0:0.0000}", result.Confidence); break;
                        case "cov": sb.AppendFormat("{0:0.0000}", result.Coverage); break;
                        
                        case "mtime": sb.AppendFormat("{0,6}", result.ModelCreationTime); break;
                        case "clstime": sb.AppendFormat("{0,6}", result.ClassificationTime); break;

                        case "erulhit": sb.AppendFormat("{0,5}", result.ExceptionRuleHitCounter); break;
                        case "erullen": sb.AppendFormat("{0,7}", result.ExceptionRuleLengthSum); break;
                        case "srulhit": sb.AppendFormat("{0,5}", result.StandardRuleHitCounter); break;
                        case "srullen": sb.AppendFormat("{0,7}", result.StandardRuleLengthSum); break;

                        case "attr": sb.AppendFormat("{0,6:0.00}", result.AvgNumberOfAttributes); break;
                        case "numrul": sb.AppendFormat("{0,7:0.00}", result.NumberOfRules); break;
                        case "numexrul": sb.AppendFormat("{0,7:0.00}", result.NumberOfExceptionRules); break;
                        case "dthm": sb.AppendFormat("{0,5:0.00}", result.MaxTreeHeight); break;
                        case "dtha": sb.AppendFormat("{0,5:0.00}", result.AvgTreeHeight); break;

                        case "gamma": sb.AppendFormat("{0,7:0.00}", result.Gamma); break;
                        case "alpha": sb.AppendFormat("{0,7:0.00}", result.Alpha); break;
                        case "beta": sb.AppendFormat("{0,7:0.00}", result.Beta); break;
                        case "desc": sb.AppendFormat(String.IsNullOrEmpty(result.Description) ? "" : result.Description); break;
                        
                        case "precisionmicro": sb.AppendFormat("{0:0.0000}", result.PrecisionMicro); break;
                        case "precisionmacro": sb.AppendFormat("{0:0.0000}", result.PrecisionMacro); break;                        
                        case "recallmicro": sb.AppendFormat("{0:0.0000}", result.RecallMicro); break;
                        case "recallmacro": sb.AppendFormat("{0:0.0000}", result.RecallMacro); break;
                        case "f1scoremicro": sb.AppendFormat("{0:0.0000}", result.F1scoreMicro); break;
                        case "f1scoremacro": sb.AppendFormat("{0:0.0000}", result.F1scoreMacro); break;

                    }
                }

                if (i != len)
                    sb.Append(outputSeparator);
            }

            return sb.ToString();
        }        
    }
}
