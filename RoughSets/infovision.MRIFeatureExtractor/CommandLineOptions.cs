using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace Infovision.MRI.UI
{
    public sealed class CommandLineOptions
    {
        [Option("p", "project", HelpText = "Project to be loaded on startup")]
        public string ProjectFileName { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("MRI Infovision 1.0");
            usage.AppendLine("Read user manual for usage instructions...");
            return usage.ToString();
        }
    }
}
