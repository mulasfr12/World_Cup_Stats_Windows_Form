using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp
{
   public partial class SettingsForm : Form
{
        private string _language; 
    public SettingsForm()
    {
            
            InitializeComponent();
           
        }
        private void saveButton_Click(object sender, EventArgs e)
        {
            ConfirmAndSaveSettings();
        }

        private void ConfirmAndSaveSettings()
        {
            var confirmation = MessageBox.Show("Do you want to save the changes?", "Confirm Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmation == DialogResult.Yes)
            {
                _language = genderComboBox.SelectedItem.ToString();
                var selectedLanguage = languageComboBox.SelectedItem.ToString();
                var useApi = apiCheckBox.Checked ? "API" : "JSON";

                // Save settings to file
                var settings = $"{_language};{selectedLanguage};{useApi}";
                File.WriteAllText("settings.txt", settings);

                MessageBox.Show("Settings saved successfully.");
                
                this.DialogResult = DialogResult.OK;
                this.Close();
                
            }
           
        }
        
    }

}
