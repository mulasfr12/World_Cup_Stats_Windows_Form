using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!File.Exists("settings.txt"))
            {
                // Show settings form if settings are missing
                Application.Run(new SettingsForm());
            }
            else
            {
                // Load settings and proceed to the main form
                var settings = File.ReadAllText("settings.txt").Split(';');
                string gender = settings[0];
                string language = settings[1];
                string dataSource = settings[2];

                // Use the settings for initialization
                Application.Run(new MainForm(gender, language, dataSource));
            }
        }
    }
}
