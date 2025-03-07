﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Input;

namespace WorldCupWPF
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            genderComboBox.Items.Add("Male");
            genderComboBox.Items.Add("Female");

            languageComboBox.Items.Add("English");
            languageComboBox.Items.Add("Croatian");

            dataSourceComboBox.Items.Add("API");
            dataSourceComboBox.Items.Add("JSON");

            LoadSavedSettings();  // Load saved settings on window load
        }

        public void LoadSavedSettings()
        {
            var settings = AppSettings.LoadSettings();

            if (settings == null)
            {
                // Create a new settings instance with default values if none exist
                settings = new AppSettings
                {
                    Gender = "Male",
                    Language = "English",
                    DataSource = "API"
                };

                // Save the default settings to avoid future null issues
                App.SaveSettings(settings);
            }

            // Set ComboBox selected items based on loaded settings
            genderComboBox.SelectedItem = settings.Gender;
            languageComboBox.SelectedItem = settings.Language;
            dataSourceComboBox.SelectedItem = settings.DataSource;

            // Update UI labels if the language is Croatian
            if (settings.Language == "Croatian")
            {
                Genderlbl.Content = "Spol";
                Languagelbl.Content = "Jezik";
                DataSourcelbl.Content = "Izvor podataka";
                buttonConfirm.Content = "Ok";
                buttonCancel.Content = "Otkazati";
            }
        }


        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            var selectedGender = genderComboBox.SelectedItem?.ToString();
            var selectedLanguage = languageComboBox.SelectedItem?.ToString();
            var selectedDataSource = dataSourceComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedGender) || string.IsNullOrEmpty(selectedLanguage) || string.IsNullOrEmpty(selectedDataSource))
            {
                MessageBox.Show("Please select gender, language, and data source.", "Incomplete Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create and save settings in the new format
            var settings = new AppSettings
            {
                Gender = selectedGender,
                Language = selectedLanguage,
                DataSource = selectedDataSource
            };

            App.SaveSettings(settings);  // Save using a method from App class to match new format
            //ApplyLocalization(selectedLanguage);
            MessageBox.Show("Settings saved successfully.");
            this.DialogResult = true;
            this.Close();
        }

        //private void ApplyLocalization(string language)
        //{
        //    var cultureCode = language == "Croatian" ? "hr" : "en";
        //    var culture = new CultureInfo(cultureCode);
        //    System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        //    System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

        //    var dictionary = new ResourceDictionary
        //    {
        //        Source = new Uri($"Resources.{cultureCode}.xaml", UriKind.Relative)
        //    };
        //    Application.Current.Resources.MergedDictionaries.Clear();
        //    Application.Current.Resources.MergedDictionaries.Add(dictionary);
        //}
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;  // Close without saving
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Enter) // Confirm
            {
                buttonConfirm_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape) // Cancel
            {
                buttonCancel_Click(this, new RoutedEventArgs());
            }
        }

    }
}
