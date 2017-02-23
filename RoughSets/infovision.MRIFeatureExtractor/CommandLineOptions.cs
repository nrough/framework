using System.Text;
using CommandLine;

namespace NRough.MRI.UI
{
    public sealed class CommandLineOptions
    {
        [Option('p', "project", HelpText = "Project to be loaded on startup")]
        public string ProjectFileName { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("MRI NRough 1.0");
            usage.AppendLine("Read user manual for usage instructions...");
            return usage.ToString();
        }
    }
}