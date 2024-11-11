using System.Windows.Forms;

namespace WinFormsApp
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbFavoriteTeam = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSaveFavorite = new System.Windows.Forms.Button();
            this.flowPanelFavorites = new System.Windows.Forms.FlowLayoutPanel();
            this.flowPanelOthers = new System.Windows.Forms.FlowLayoutPanel();
            this.btnShowRankings = new System.Windows.Forms.Button();
            this.dataGridViewPlayerRankings = new System.Windows.Forms.DataGridView();
            this.FullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Goals = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.YellowCards = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Appearances = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PlayerPicture = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewMatchRankings = new System.Windows.Forms.DataGridView();
            this.Locatison = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Attendance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HostTeam = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GuestTeam = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnShowMatchRankings = new System.Windows.Forms.Button();
            this.PrintRankings = new System.Windows.Forms.Button();
            this.ExportPlayerRanking = new System.Windows.Forms.Button();
            this.btnChangeSettings = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPlayerRankings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMatchRankings)).BeginInit();
            this.SuspendLayout();
            // 
            // cbFavoriteTeam
            // 
            this.cbFavoriteTeam.FormattingEnabled = true;
            this.cbFavoriteTeam.Location = new System.Drawing.Point(213, 36);
            this.cbFavoriteTeam.Name = "cbFavoriteTeam";
            this.cbFavoriteTeam.Size = new System.Drawing.Size(286, 24);
            this.cbFavoriteTeam.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select your Favourite team";
            // 
            // btnSaveFavorite
            // 
            this.btnSaveFavorite.Location = new System.Drawing.Point(527, 36);
            this.btnSaveFavorite.Name = "btnSaveFavorite";
            this.btnSaveFavorite.Size = new System.Drawing.Size(75, 23);
            this.btnSaveFavorite.TabIndex = 2;
            this.btnSaveFavorite.Text = "Save";
            this.btnSaveFavorite.UseVisualStyleBackColor = true;
            this.btnSaveFavorite.Click += new System.EventHandler(this.btnSaveFavorite_Click);
            // 
            // flowPanelFavorites
            // 
            this.flowPanelFavorites.AllowDrop = true;
            this.flowPanelFavorites.AutoScroll = true;
            this.flowPanelFavorites.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPanelFavorites.Location = new System.Drawing.Point(46, 100);
            this.flowPanelFavorites.Name = "flowPanelFavorites";
            this.flowPanelFavorites.Size = new System.Drawing.Size(251, 565);
            this.flowPanelFavorites.TabIndex = 3;
            this.flowPanelFavorites.WrapContents = false;
            this.flowPanelFavorites.DragDrop += new System.Windows.Forms.DragEventHandler(this.flowPanelFavorites_DragDrop);
            this.flowPanelFavorites.DragEnter += new System.Windows.Forms.DragEventHandler(this.flowPanelFavorites_DragEnter);
            // 
            // flowPanelOthers
            // 
            this.flowPanelOthers.AllowDrop = true;
            this.flowPanelOthers.AutoScroll = true;
            this.flowPanelOthers.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPanelOthers.Location = new System.Drawing.Point(341, 100);
            this.flowPanelOthers.Name = "flowPanelOthers";
            this.flowPanelOthers.Size = new System.Drawing.Size(261, 565);
            this.flowPanelOthers.TabIndex = 4;
            this.flowPanelOthers.WrapContents = false;
            this.flowPanelOthers.DragDrop += new System.Windows.Forms.DragEventHandler(this.flowPanelOthers_DragDrop);
            this.flowPanelOthers.DragEnter += new System.Windows.Forms.DragEventHandler(this.flowPanelOthers_DragEnter);
            // 
            // btnShowRankings
            // 
            this.btnShowRankings.Location = new System.Drawing.Point(633, 37);
            this.btnShowRankings.Name = "btnShowRankings";
            this.btnShowRankings.Size = new System.Drawing.Size(169, 23);
            this.btnShowRankings.TabIndex = 5;
            this.btnShowRankings.Text = "Show Player Rankings";
            this.btnShowRankings.UseVisualStyleBackColor = true;
            this.btnShowRankings.Click += new System.EventHandler(this.btnShowRankings_Click);
            // 
            // dataGridViewPlayerRankings
            // 
            this.dataGridViewPlayerRankings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPlayerRankings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FullName,
            this.Goals,
            this.YellowCards,
            this.Appearances,
            this.PlayerPicture});
            this.dataGridViewPlayerRankings.Location = new System.Drawing.Point(633, 100);
            this.dataGridViewPlayerRankings.Name = "dataGridViewPlayerRankings";
            this.dataGridViewPlayerRankings.RowHeadersWidth = 51;
            this.dataGridViewPlayerRankings.RowTemplate.Height = 24;
            this.dataGridViewPlayerRankings.Size = new System.Drawing.Size(685, 268);
            this.dataGridViewPlayerRankings.TabIndex = 6;
            // 
            // FullName
            // 
            this.FullName.HeaderText = "Player Name";
            this.FullName.MinimumWidth = 6;
            this.FullName.Name = "FullName";
            this.FullName.Width = 125;
            // 
            // Goals
            // 
            this.Goals.HeaderText = "Goals";
            this.Goals.MinimumWidth = 6;
            this.Goals.Name = "Goals";
            this.Goals.Width = 125;
            // 
            // YellowCards
            // 
            this.YellowCards.HeaderText = "Yellow Cards";
            this.YellowCards.MinimumWidth = 6;
            this.YellowCards.Name = "YellowCards";
            this.YellowCards.Width = 125;
            // 
            // Appearances
            // 
            this.Appearances.HeaderText = "Appearances";
            this.Appearances.MinimumWidth = 6;
            this.Appearances.Name = "Appearances";
            this.Appearances.Width = 125;
            // 
            // PlayerPicture
            // 
            this.PlayerPicture.HeaderText = "Player Image";
            this.PlayerPicture.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.PlayerPicture.MinimumWidth = 6;
            this.PlayerPicture.Name = "PlayerPicture";
            this.PlayerPicture.Width = 125;
            // 
            // dataGridViewMatchRankings
            // 
            this.dataGridViewMatchRankings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMatchRankings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Locatison,
            this.Attendance,
            this.HostTeam,
            this.GuestTeam});
            this.dataGridViewMatchRankings.Location = new System.Drawing.Point(633, 444);
            this.dataGridViewMatchRankings.Name = "dataGridViewMatchRankings";
            this.dataGridViewMatchRankings.RowHeadersWidth = 51;
            this.dataGridViewMatchRankings.RowTemplate.Height = 24;
            this.dataGridViewMatchRankings.Size = new System.Drawing.Size(685, 221);
            this.dataGridViewMatchRankings.TabIndex = 7;
            // 
            // Locatison
            // 
            this.Locatison.HeaderText = "Location";
            this.Locatison.MinimumWidth = 6;
            this.Locatison.Name = "Locatison";
            this.Locatison.Width = 125;
            // 
            // Attendance
            // 
            this.Attendance.HeaderText = "Attendance";
            this.Attendance.MinimumWidth = 6;
            this.Attendance.Name = "Attendance";
            this.Attendance.Width = 125;
            // 
            // HostTeam
            // 
            this.HostTeam.HeaderText = "Host Team";
            this.HostTeam.MinimumWidth = 6;
            this.HostTeam.Name = "HostTeam";
            this.HostTeam.Width = 125;
            // 
            // GuestTeam
            // 
            this.GuestTeam.HeaderText = "GuestTeam";
            this.GuestTeam.MinimumWidth = 6;
            this.GuestTeam.Name = "GuestTeam";
            this.GuestTeam.Width = 125;
            // 
            // btnShowMatchRankings
            // 
            this.btnShowMatchRankings.Location = new System.Drawing.Point(633, 406);
            this.btnShowMatchRankings.Name = "btnShowMatchRankings";
            this.btnShowMatchRankings.Size = new System.Drawing.Size(169, 23);
            this.btnShowMatchRankings.TabIndex = 8;
            this.btnShowMatchRankings.Text = "Show Match Rankings";
            this.btnShowMatchRankings.UseVisualStyleBackColor = true;
            this.btnShowMatchRankings.Click += new System.EventHandler(this.btnShowMatchRankings_Click);
            // 
            // PrintRankings
            // 
            this.PrintRankings.Location = new System.Drawing.Point(848, 36);
            this.PrintRankings.Name = "PrintRankings";
            this.PrintRankings.Size = new System.Drawing.Size(173, 24);
            this.PrintRankings.TabIndex = 9;
            this.PrintRankings.Text = "Print Rankings";
            this.PrintRankings.UseVisualStyleBackColor = true;
            this.PrintRankings.Click += new System.EventHandler(this.PrintRankings_Click);
            // 
            // ExportPlayerRanking
            // 
            this.ExportPlayerRanking.Location = new System.Drawing.Point(1056, 36);
            this.ExportPlayerRanking.Name = "ExportPlayerRanking";
            this.ExportPlayerRanking.Size = new System.Drawing.Size(156, 24);
            this.ExportPlayerRanking.TabIndex = 10;
            this.ExportPlayerRanking.Text = "Export Ranking to PDF";
            this.ExportPlayerRanking.UseVisualStyleBackColor = true;
            this.ExportPlayerRanking.Click += new System.EventHandler(this.ExportPlayerRanking_Click);
            // 
            // btnChangeSettings
            // 
            this.btnChangeSettings.Location = new System.Drawing.Point(1295, 37);
            this.btnChangeSettings.Name = "btnChangeSettings";
            this.btnChangeSettings.Size = new System.Drawing.Size(146, 27);
            this.btnChangeSettings.TabIndex = 11;
            this.btnChangeSettings.Text = "Change Settings";
            this.btnChangeSettings.UseVisualStyleBackColor = true;
            this.btnChangeSettings.Click += new System.EventHandler(this.btnChangeSettings_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1565, 799);
            this.Controls.Add(this.btnChangeSettings);
            this.Controls.Add(this.ExportPlayerRanking);
            this.Controls.Add(this.PrintRankings);
            this.Controls.Add(this.btnShowMatchRankings);
            this.Controls.Add(this.dataGridViewMatchRankings);
            this.Controls.Add(this.dataGridViewPlayerRankings);
            this.Controls.Add(this.btnShowRankings);
            this.Controls.Add(this.flowPanelOthers);
            this.Controls.Add(this.flowPanelFavorites);
            this.Controls.Add(this.btnSaveFavorite);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbFavoriteTeam);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPlayerRankings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMatchRankings)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbFavoriteTeam;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSaveFavorite;
        private System.Windows.Forms.FlowLayoutPanel flowPanelFavorites;
        private System.Windows.Forms.FlowLayoutPanel flowPanelOthers;
        private System.Windows.Forms.Button btnShowRankings;
        private System.Windows.Forms.DataGridView dataGridViewPlayerRankings;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewImageColumn PlayerImage;
        private DataGridViewTextBoxColumn FullName;
        private DataGridViewTextBoxColumn Goals;
        private DataGridViewTextBoxColumn YellowCards;
        private DataGridViewTextBoxColumn Appearances;
        private DataGridViewImageColumn PlayerPicture;
        private DataGridView dataGridViewMatchRankings;
        private Button btnShowMatchRankings;
        private DataGridViewTextBoxColumn Location;
        private DataGridViewTextBoxColumn Attendance;
        private DataGridViewTextBoxColumn HostTeam;
        private DataGridViewTextBoxColumn GuestTeam;
        private DataGridViewTextBoxColumn Locatison;
        private Button PrintRankings;
        private Button ExportPlayerRanking;
        private Button btnChangeSettings;
    }
}

