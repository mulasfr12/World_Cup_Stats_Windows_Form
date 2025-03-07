﻿using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;

namespace WorldCupWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Check if settings.txt exists, otherwise open SettingsWindow
            if (!File.Exists("settings.txt"))
            {
                var settingsWindow = new SettingsWindow();
                if (settingsWindow.ShowDialog() != true) // If the user cancels the settings window
                {
                    Application.Current.Shutdown(); // Exit the application
                    return;
                }
            }

            // Load settings and set culture
            var settings = AppSettings.LoadSettings();
            if (settings != null)
            {
                SetCulture(settings.Language == "Croatian" ? "hr" : "en");
            }

            // Launch the main window
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }


        private void SetCulture(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private AppSettings PromptForSettings()
        {
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                var settings = LoadSettings();
                if (settings != null)
                {
                    SaveSettings(settings);  // Save settings after confirmation
                    return settings;
                }
            }
            return null;
        }

        public static AppSettings LoadSettings()
        {
            if (!File.Exists("settings.txt")) return null;

            var settingsData = File.ReadAllText("settings.txt").Split(';');
            if (settingsData.Length < 3) return null; // Check if all three settings are available

            return new AppSettings
            {
                Gender = settingsData[0] ?? "Male",       // Default to "Male" if null
                Language = settingsData[1] ?? "English",  // Default to "English" if null
                DataSource = settingsData[2] ?? "API"     // Default to "API" if null
            };
        }

        public static void SaveSettings(AppSettings settings)
        {
            var settingsData = $"{settings.Gender};{settings.Language};{settings.DataSource}";
            File.WriteAllText("settings.txt", settingsData);
        }
    }
}
