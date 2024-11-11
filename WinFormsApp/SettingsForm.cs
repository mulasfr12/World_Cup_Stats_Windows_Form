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
                var selectedGender = genderComboBox.SelectedItem.ToString();
                var selectedLanguage = languageComboBox.SelectedItem.ToString();
                var useApi = apiCheckBox.Checked ? "API" : "JSON";

                // Save settings to file
                var settings = $"{selectedGender};{selectedLanguage};{useApi}";
                File.WriteAllText("settings.txt", settings);

                MessageBox.Show("Settings saved successfully.");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConfirmAndSaveSettings();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }

}
