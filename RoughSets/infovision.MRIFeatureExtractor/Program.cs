﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CommandLine;
using CommandLine.Text;

namespace NRough.MRI.UI
{
    internal static class Program
    {
        private static readonly HeadingInfo _headingInfo = new HeadingInfo("NRough MRI", "1.8");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var options = new CommandLineOptions();
            var parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));

            string[] args = Environment.GetCommandLineArgs();
            if (!parser.ParseArguments(args, options))
            {
                Environment.Exit(1);
            }

            Run(options);

            Environment.Exit(0);
        }

        private static void Run(CommandLineOptions options)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MRIApplication appl = MRIApplication.Default;

            if (!String.IsNullOrEmpty(options.ProjectFileName)
                    && File.Exists(options.ProjectFileName))
            {
                appl.LoadProject(options.ProjectFileName);
            }

            MainForm mainWindow = new MainForm();
            Application.Run(mainWindow);
        }
    }
}