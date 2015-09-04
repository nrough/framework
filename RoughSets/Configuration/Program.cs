using System;
using System.Windows.Forms;

namespace Configuration
{
    enum DatabaseType : short
    {
        MSSQLServer = 1,
        MySQL       = 2
    }

    enum AuthenticationType : short
    {
        WindowsBased = 1,
        SQLBased     = 2
    }
    
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Configuration());
        }
    }
}
