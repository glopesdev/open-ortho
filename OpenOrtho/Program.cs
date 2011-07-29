using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OpenOrtho
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainForm = new MainForm();
            mainForm.DesktopBounds = Screen.PrimaryScreen.Bounds;
            args = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData ?? args;
            if (args.Length > 0 && System.IO.File.Exists(args[0]))
            {
                mainForm.StartupProject = args[0];
            }

            Application.Run(mainForm);
        }
    }
}
