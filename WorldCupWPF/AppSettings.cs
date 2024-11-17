using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WorldCupWPF
{
    public class AppSettings
    {
        public string Gender { get; set; }
        public string Language { get; set; }
        public string DataSource { get; set; }

        public static AppSettings LoadSettings()
        {
            if (!File.Exists("settings.txt")) return null;

            var settingsData = File.ReadAllText("settings.txt");
            var parts = settingsData.Split(';');
            return new AppSettings
            {
                Gender = parts[0],
                Language = parts[1],
                DataSource = parts.Length > 2 ? parts[2] : "API" // Ensure a default value is provided
            };
        }
        public static void SaveSettings(AppSettings settings)
        {
            var settingsData = $"{settings.Gender};{settings.Language};{settings.DataSource}";
            File.WriteAllText("settings.txt", settingsData);
        }
    }
}
 