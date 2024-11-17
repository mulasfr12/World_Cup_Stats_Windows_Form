using DataLayer.Models;
using DataLayer.Repositories;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsApp.Properties;
using static DataLayer.Models.Country;
using System.Drawing.Printing;
using System.Reflection.Emit;

namespace WinFormsApp
{
    public partial class MainForm : Form
    {
        private string _gender;
        private string _language;
        private string _dataSource;
        private DataService _dataService;
       

        public MainForm(string gender, string language, string dataSource)
        {
            _gender = gender;
            _language = language;
            _dataSource = dataSource;

            // Initialize the DataService based on the data source (API or JSON)
            bool useApi = _dataSource == "API";
            _dataService = new DataService(useApi);

            InitializeComponent();
            ApplySettings();
            LoadTeams(gender, dataSource);
        }
      
        private void ApplySettings()
        {
            try
            {
                string endpoint = (_gender == "Male")
                    ? "teams/results" 
                    : "teams/results"; 

                ApplyLanguage(_language);

                FetchData(endpoint,_gender);
               
                MessageBox.Show("Settings applied successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }           
        }

        public async Task<List<Team>> FetchData(string endpoint, string gender)
        {
            if (_dataService != null)
            {
                //Passing in gender-specific endpoint or determine if fetching via API/JSON.
                bool isMen = gender.ToLower() == "male";
                return await _dataService.GetTeamsData(endpoint, isMen);
            }
            return new List<Team>();
        }

        private async void LoadTeams(string gender, string dataSource)
        {
            try
            {
                bool isMen = gender.ToLower() == "male";
                string endpointOrPath = "";

                if (_dataService != null)
                {
                    if (_dataSource == "API")
                    {
                        endpointOrPath = isMen
                            ? "https://worldcup-vua.nullbit.hr/men/teams/results"
                            : "https://worldcup-vua.nullbit.hr/women/teams/results";
                    }
                    else
                    {
                        endpointOrPath = isMen
                            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Men", "teams.json")
                            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Women", "teams.json");
                    }
                }

                List<Team> teams = await _dataService.GetTeamsData(endpointOrPath, isMen);
                cbFavoriteTeam.Items.Clear();

                if (teams != null && teams.Count > 0)
                {
                    foreach (var team in teams)
                    {
                        cbFavoriteTeam.Items.Add($"{team.Country} ({team.Fifa_Code})");
                    }

                    // Check if a favorite team is saved and pre-select it
                    string savedTeam = LoadFavoriteTeam();
                    if (!string.IsNullOrEmpty(savedTeam) && cbFavoriteTeam.Items.Contains(savedTeam))
                    {
                        cbFavoriteTeam.SelectedItem = savedTeam;
                    }
                }
                else
                {
                    MessageBox.Show("No teams found. Please check your data source.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading teams: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyLanguage(string language)
        {
            try
            {
                // Apply localization based on selected language
                if (language == "Croatian")
                {
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("hr");
                }
                else
                {
                    // Default to English
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
                }

                // Reload resources and refresh UI text
                ApplyResource();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying language: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if(language == "hr")
            {
                lblSelectTeam.Text = "Odeberite omiljeni tim";
                btnSaveFavorite.Text = "Ustedjeti";
                btnShowRankings.Text = "Pokazati poredak igraca";
                PrintRankings.Text = "Poredak ispisa";
                ExportPlayerRanking.Text = "Izvezi rangiranje u pdf";
                btnChangeSettings.Text = "Promijeniti postavke";
                btnShowMatchRankings.Text = "Pokazati poredak utakmica";
                FullName.HeaderText = "Ime graca";
                Goals.HeaderText = "Ciljevi";
                YellowCards.HeaderText = "Zuti kartoni";
                Appearances.HeaderText = "Pojave";
                PlayerPicture.HeaderText = "Silka igraca";

                Locatison.HeaderText = "Mjesto";
                Attendance.HeaderText = "Pohadanje";
                HostTeam.HeaderText = "Ekipa domacina";
                GuestTeam.HeaderText = "Gostujuca ekipa";

            }
        }

        private async void LoadPlayers(string gender, string dataSource, string favoriteTeam)
        {
            try
            {
                bool isMen = gender.ToLower() == "male";
                string endpoint = "";

                if (_dataService != null)
                {
                    if (_dataSource == "API")
                    {
                        endpoint = isMen
                            ? "https://worldcup-vua.nullbit.hr/men/matches"
                            : "https://worldcup-vua.nullbit.hr/women/matches";
                    }
                    else
                    {
                        endpoint = isMen
                            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Men", "matches.json")
                            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Women", "matches.json");
                    }
                }
                var matches = await _dataService.GetMatchesData(endpoint, gender == "Male");
              
                if (matches == null || !matches.Any())
                {
                    MessageBox.Show("No matches were fetched from the data source.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Extract team name (removing FIFA code) for comparison
                string favoriteTeamName = favoriteTeam.Split('(')[0].Trim();

                // Find the first match involving the favorite team
                var firstMatch = matches.FirstOrDefault(m =>
                    m.home_team_statistics.country.Equals(favoriteTeamName, StringComparison.OrdinalIgnoreCase) ||
                    m.away_team_statistics.country.Equals(favoriteTeamName, StringComparison.OrdinalIgnoreCase));

                if (firstMatch == null)
                {
                    MessageBox.Show("No matches found for the selected team.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var players = new List<object>(); // Using object to accommodate different player types
                if (firstMatch.home_team_statistics.country.Equals(favoriteTeamName, StringComparison.OrdinalIgnoreCase))
                {
                    // Loading players from the home team
                    players.AddRange(firstMatch.home_team_statistics.starting_eleven);
                    players.AddRange(firstMatch.home_team_statistics.substitutes);
                }
                else if (firstMatch.away_team_statistics.country.Equals(favoriteTeamName, StringComparison.OrdinalIgnoreCase))
                {
                    // Loading players from the away team
                    players.AddRange(firstMatch.away_team_statistics.starting_eleven);
                    players.AddRange(firstMatch.away_team_statistics.substitutes);
                }

                if (!players.Any())
                {
                    MessageBox.Show("No players found for the selected team.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                LoadPlayersIntoPanels(players);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading players: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void LoadPlayersIntoPanels(List<object> players)
        {
            flowPanelFavorites.Controls.Clear();
            flowPanelOthers.Controls.Clear();
  
            var favoritePlayers = LoadFavoritePlayers();

            foreach (var player in players)
            {
                PlayerControl playerControl;

                if (player is StartingEleven startingElevenPlayer)
                {
                    playerControl = new PlayerControl(startingElevenPlayer);
                }
                else if (player is Substitute substitutePlayer)
                {
                    playerControl = new PlayerControl(substitutePlayer);
                }
                else
                {
                    continue;
                }

                playerControl.ContextMenuStrip = CreateContextMenu(playerControl);

                if (favoritePlayers.Contains(playerControl.Player.name))
                {
                    playerControl.IsFavorite = true;
                    playerControl.UpdateFavoriteStatus(true);
                    flowPanelFavorites.Controls.Add(playerControl); 
                    
                }
                else
                {
                    playerControl.UpdateFavoriteStatus(false);
                    flowPanelOthers.Controls.Add(playerControl);
                    
                }

                // Attach drag-and-drop event handlers
                playerControl.MouseDown += PlayerControl_MouseDown;
                playerControl.MouseMove += PlayerControl_MouseMove;
            }

            flowPanelFavorites.Refresh();
            flowPanelOthers.Refresh();
        }

        private void PlayerControl_MouseDown(object sender, MouseEventArgs e)
        {          
            if (e.Button == MouseButtons.Left)
            {
                PlayerControl playerControl = sender as PlayerControl;

                if (playerControl != null)
                {
                    DoDragDrop(playerControl, DragDropEffects.Move);
                }
            }
        }
        private void PlayerControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var playerControl = sender as PlayerControl;
                if (playerControl != null)
                {
                    playerControl.DoDragDrop(playerControl, DragDropEffects.Move);
                }
            }
        }

        private void flowPanelFavorites_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void flowPanelFavorites_DragDrop(object sender, DragEventArgs e)
        {
            var control = e.Data.GetData(typeof(PlayerControl)) as PlayerControl;
            if (control != null)
            {
                // Update favorite status and add to favorites panel
                control.IsFavorite = true;
                control.UpdateFavoriteStatus(true); 
                flowPanelFavorites.Controls.Add(control);
                SaveFavoritePlayers();
            }
        }
       

        private void flowPanelOthers_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void flowPanelOthers_DragDrop(object sender, DragEventArgs e)
        {
            var control = e.Data.GetData(typeof(PlayerControl)) as PlayerControl;
            if (control != null)
            {
                flowPanelOthers.Controls.Add(control);
                control.IsFavorite = false;
                SaveFavoritePlayers();
            }
        }

        private ContextMenuStrip CreateContextMenu(PlayerControl playerControl)
        {
            var contextMenu = new ContextMenuStrip();

            var markFavorite = new ToolStripMenuItem("Mark as Favorite", null, (s, e) =>
            {
                flowPanelFavorites.Controls.Add(playerControl);
                playerControl.IsFavorite = true;
                SaveFavoritePlayers();
            });

            var removeFavorite = new ToolStripMenuItem("Remove Favorite", null, (s, e) =>
            {
                flowPanelOthers.Controls.Add(playerControl);
                playerControl.IsFavorite = false;
                SaveFavoritePlayers();
            });

            contextMenu.Items.Add(markFavorite);
            contextMenu.Items.Add(removeFavorite);

            return contextMenu;
        }

        private void SaveFavoritePlayers()
        {
            List<string> favoritePlayerNames = new List<string>();

            foreach (Control control in flowPanelFavorites.Controls)
            {
                if (control is PlayerControl playerControl)
                {               
                    favoritePlayerNames.Add(playerControl.Player.name);
                }
            }
            File.WriteAllLines("favorite_players.txt", favoritePlayerNames);
        }

        private List<string> LoadFavoritePlayers()
        {
            // Check if the file exists
            if (File.Exists("favorite_players.txt"))
            {
                // Read all lines (player names) from the file
                return File.ReadAllLines("favorite_players.txt").ToList();
            }

            return new List<string>(); // Return an empty list if the file does not exist
        }

        private void ApplyResource()
        {
            // Loop through all controls and apply the appropriate text from resources
            foreach (Control control in this.Controls)
            {
                var resText = Properties.Resources.ResourceManager.GetString(control.Name);
                if (!string.IsNullOrEmpty(resText))
                {
                    control.Text = resText;
                }
            }
        }

         private void btnSaveFavorite_Click(object sender, EventArgs e)
        {
            string selectedTeam = cbFavoriteTeam.SelectedItem.ToString();
            try
            {
                if (cbFavoriteTeam.SelectedItem != null)
                {
                    SaveFavoriteTeam(selectedTeam);
                    MessageBox.Show("Favorite team saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Please select a team.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving favorite team: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadPlayers(_gender, _dataSource, selectedTeam);
        }

        private void SaveFavoriteTeam(string team)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favoriteTeam.txt");
                File.WriteAllText(filePath, team);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving favorite team: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string LoadFavoriteTeam()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favoriteTeam.txt");
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading favorite team: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async void btnShowRankings_Click(object sender, EventArgs e)
        {
            dataGridViewPlayerRankings.Rows.Clear();

            bool isMen = _gender.ToLower() == "male";
            string endpoint = "";

            if (_dataService != null)
            {
                if (_dataSource == "API")
                {
                    endpoint = isMen
                        ? "https://worldcup-vua.nullbit.hr/men/matches"
                        : "https://worldcup-vua.nullbit.hr/women/matches";
                }
                else
                {
                    endpoint = isMen
                        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Men", "matches.json")
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Women", "matches.json");
                }
            }

            var favoriteTeamCode = LoadFavoriteTeam(); 
            MessageBox.Show($"Favorite Team FIFA Code: {favoriteTeamCode}"); 

            try
            {
                var matches = await _dataService.GetMatchesData(endpoint, _gender.Equals("Male", StringComparison.OrdinalIgnoreCase));

                var filteredMatches = matches.Where(match =>
                    favoriteTeamCode.Contains(match.home_team_country) || favoriteTeamCode.Contains(match.away_team_country)).ToList();

                if (filteredMatches.Count == 0)
                {
                    MessageBox.Show("No matches found for the favorite team.");
                    return;
                }

                var playerStats = CalculatePlayerStats(filteredMatches);

                var sortedPlayerStats = playerStats
                    .OrderByDescending(p => p.Goals)
                    .ThenByDescending(p => p.YellowCards)
                    .ToList();

                if (sortedPlayerStats.Count == 0)
                {
                    MessageBox.Show("No player stats available for the selected team.");
                    return;
                }

                foreach (var player in sortedPlayerStats)
                {
                    var playerPicture = Resources.defaultPlayer; 

                    dataGridViewPlayerRankings.Rows.Add(player.FullName, player.Goals, player.YellowCards, player.Appearances, playerPicture);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching player rankings: {ex.Message}");
            }
        }
        private List<PlayerStats> CalculatePlayerStats(List<Country.Root> matches)
        {
            var playerStatsDict = new Dictionary<string, PlayerStats>(); // Dictionary to accumulate player stats

            foreach (var match in matches)
            {
                var homeTeam = match.home_team_statistics;
                if (homeTeam != null)
                {
                    ProcessTeamPlayers(homeTeam.starting_eleven, homeTeam.substitutes, playerStatsDict);
                }

                var awayTeam = match.away_team_statistics;
                if (awayTeam != null)
                {
                    ProcessTeamPlayers(awayTeam.starting_eleven, awayTeam.substitutes, playerStatsDict);
                }

                ProcessHomeTeamEvents(match.home_team_events, playerStatsDict);

                ProcessAwayTeamEvents(match.away_team_events, playerStatsDict);
            }    
            if (playerStatsDict.Count == 0)
            {
                MessageBox.Show("No player statistics found.");
            }
            return playerStatsDict.Values.ToList();
        }
        private void ProcessHomeTeamEvents(List<Country.HomeTeamEvent> homeTeamEvents, Dictionary<string, PlayerStats> playerStatsDict)
        {
            foreach (var teamEvent in homeTeamEvents)
            {           
                if (teamEvent.type_of_event == "goal" || teamEvent.type_of_event == "goal-penalty")
                {
                    if (playerStatsDict.TryGetValue(teamEvent.player, out var playerStats))
                    {
                        playerStats.Goals++;
                    }
                }
                else if (teamEvent.type_of_event == "yellow-card")
                {
                    if (playerStatsDict.TryGetValue(teamEvent.player, out var playerStats))
                    {
                        playerStats.YellowCards++;
                    }
                }
            }
        }

        private void ProcessAwayTeamEvents(List<Country.AwayTeamEvent> awayTeamEvents, Dictionary<string, PlayerStats> playerStatsDict)
        {
            foreach (var teamEvent in awayTeamEvents)
            {
                if (teamEvent.type_of_event == "goal" || teamEvent.type_of_event == "goal-penalty")
                {
                    if (playerStatsDict.TryGetValue(teamEvent.player, out var playerStats))
                    {
                        playerStats.Goals++;
                    }
                }
                else if (teamEvent.type_of_event == "yellow-card")
                {
                    if (playerStatsDict.TryGetValue(teamEvent.player, out var playerStats))
                    {
                        playerStats.YellowCards++;
                    }
                }
            }
        }
   
        private void ProcessTeamPlayers(List<Country.StartingEleven> startingEleven, List<Country.Substitute> substitutes, Dictionary<string, PlayerStats> playerStatsDict)
        {
            foreach (var player in startingEleven)
            {
                UpdatePlayerStats(player, playerStatsDict);
            }

            foreach (var player in substitutes)
            {
                UpdatePlayerStats(player, playerStatsDict);
            }
            if (startingEleven == null || !startingEleven.Any())
            {
                MessageBox.Show("No starting players found for the selected team.");
            }
            if (substitutes == null || !substitutes.Any())
            {
                MessageBox.Show("No substitutes found for the selected team.");
            }
        }

        private void UpdatePlayerStats(dynamic player, Dictionary<string, PlayerStats> playerStatsDict)
        {
            if (!playerStatsDict.TryGetValue(player.name, out PlayerStats playerStats))
            {
                playerStats = new PlayerStats { FullName = player.name, Appearances = 0, Goals = 0, YellowCards = 0 };
                playerStatsDict[player.name] = playerStats;
            }

            playerStats.Appearances++;
        }

        private async void btnShowMatchRankings_Click(object sender, EventArgs e)
        {
            dataGridViewMatchRankings.Rows.Clear();

            bool isMen =_gender.ToLower() == "male";
            string endpoint = "";

            if (_dataService != null)
            {
                if (_dataSource == "API")
                {
                    endpoint = isMen
                        ? "https://worldcup-vua.nullbit.hr/men/matches"
                        : "https://worldcup-vua.nullbit.hr/women/matches";
                }
                else
                {
                    endpoint = isMen
                        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Men", "matches.json")
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Women", "matches.json");
                }
            }

            var favoriteTeamCode = LoadFavoriteTeam(); 

            try
            {
                var matches = await _dataService.GetMatchesData(endpoint, _gender.Equals("Male", StringComparison.OrdinalIgnoreCase));

                var filteredMatches = matches.Where(match =>
                    favoriteTeamCode.Contains(match.home_team_country) || favoriteTeamCode.Contains(match.away_team_country)).ToList();

                if (filteredMatches.Count == 0)
                {
                    MessageBox.Show("No matches found for the favorite team.");
                    return;
                }

                var matchStats = CalculateMatchStats(filteredMatches);

                // Sort matches by the number of visitors (attendance) in descending order
                var sortedMatchStats = matchStats.OrderByDescending(m => m.NumberOfVisitors).ToList();

                foreach (var match in sortedMatchStats)
                {
                    dataGridViewMatchRankings.Rows.Add(match.Location, match.NumberOfVisitors, match.HostTeam, match.GuestTeam);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching match rankings: {ex.Message}");
            }
        }

        private List<MatchStats> CalculateMatchStats(List<Country.Root> matches)
        {
            var matchStatsList = new List<MatchStats>();

            foreach (var match in matches)
            {
                // Create a new MatchStats object for each match
                var matchStats = new MatchStats
                {
                    FifaId = match.fifa_id, 
                    Location = match.location,
                    NumberOfVisitors = match.attendance,
                    HostTeam = match.home_team_country,
                    GuestTeam = match.away_team_country
                };

                matchStatsList.Add(matchStats);
            }

            return matchStatsList;
        }
        private void ExportRankingsToPdf()
        {
            Document document = new Document();
            string path = "RankingsReport.pdf";

            try
            {
                // Create a writer to save the PDF to a file
                PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));

                // Open the document to add content
                document.Open();

                // Add a title with iTextSharp's Font
                iTextSharp.text.Font titleFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 18, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font bodyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12);

                // Add Player Rankings Section
                document.Add(new Paragraph("Player Rankings Report", titleFont));

                // Create a table for Player Rankings (FullName, Goals, YellowCards, Appearances)
                PdfPTable playerTable = new PdfPTable(4); // Four columns: FullName, Goals, YellowCards, Appearances

                // Add headers
                playerTable.AddCell("Full Name");
                playerTable.AddCell("Goals");
                playerTable.AddCell("Yellow Cards");
                playerTable.AddCell("Appearances");

                // Loop through DataGridView rows and add player data
                foreach (DataGridViewRow row in dataGridViewPlayerRankings.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        playerTable.AddCell(row.Cells[0].Value?.ToString());
                        playerTable.AddCell(row.Cells[1].Value?.ToString());
                        playerTable.AddCell(row.Cells[2].Value?.ToString());
                        playerTable.AddCell(row.Cells[3].Value?.ToString());
                    }
                }

                // Add player rankings table to document
                document.Add(playerTable);

                // Add some space between sections
                document.Add(new Paragraph("\n"));

                // Add Match Rankings Section
                document.Add(new Paragraph("Match Rankings Report", titleFont));

                // Create a table for Match Rankings (Location, Visitors, Home Team, Away Team)
                PdfPTable matchTable = new PdfPTable(4); // Four columns: Location, Visitors, Home Team, Away Team

                // Add headers
                matchTable.AddCell("Location");
                matchTable.AddCell("Visitors");
                matchTable.AddCell("Home Team");
                matchTable.AddCell("Away Team");

                // Loop through DataGridView rows and add match data
                foreach (DataGridViewRow row in dataGridViewMatchRankings.Rows) // Assuming you have a separate DataGridView for match rankings
                {
                    if (!row.IsNewRow)
                    {
                        matchTable.AddCell(row.Cells[0].Value?.ToString()); // Location
                        matchTable.AddCell(row.Cells[1].Value?.ToString()); // Visitors
                        matchTable.AddCell(row.Cells[2].Value?.ToString()); // Home Team
                        matchTable.AddCell(row.Cells[3].Value?.ToString()); // Away Team
                    }
                }

                // Add match rankings table to document
                document.Add(matchTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating PDF: " + ex.Message);
            }
            finally
            {      
                document.Close();
            }

            MessageBox.Show("PDF created successfully!");
        }

        private void PrintRanking()
        {
            PrintDocument printDoc = new PrintDocument();
            PrintDialog printDialog = new PrintDialog();

            printDialog.Document = printDoc;

            // Assign event for printing
            printDoc.PrintPage += new PrintPageEventHandler(PrintPageEvent);

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print();
            }
        }
        private int playerRowIndex = 0;
        private int matchRowIndex = 0;
        private bool printingPlayers = true;

        private void PrintPageEvent(object sender, PrintPageEventArgs e)
        {
            // Fonts
            System.Drawing.Font headerFont = new System.Drawing.Font("Arial", 14, FontStyle.Bold);
            System.Drawing.Font bodyFont = new System.Drawing.Font("Arial", 10);

            // Margins and starting position
            int yPos = e.MarginBounds.Top;
            int margin = 50;

            // Handle Player Rankings
            if (printingPlayers)
            {
                yPos = PrintSection(e, "Player Rankings", dataGridViewPlayerRankings, ref playerRowIndex, headerFont, bodyFont, yPos, margin);

                // If there's more content, return for next page
                if (e.HasMorePages) return;

                // Switch to Match Rankings
                printingPlayers = false;
            }

            // Handle Match Rankings
            yPos = PrintSection(e, "Match Rankings", dataGridViewMatchRankings, ref matchRowIndex, headerFont, bodyFont, yPos, margin);

            // Mark the end of printing if all rows are done
            e.HasMorePages = matchRowIndex < dataGridViewMatchRankings.Rows.Count;
        }

        private int PrintSection(PrintPageEventArgs e, string title, DataGridView dataGridView, ref int rowIndex, System.Drawing.Font headerFont, System.Drawing.Font bodyFont, int yPos, int margin)
        {
            // Print section title
            e.Graphics.DrawString(title, headerFont, Brushes.Black, new Point(e.MarginBounds.Left, yPos));
            yPos += margin;

            // Print rows
            for (; rowIndex < dataGridView.Rows.Count; rowIndex++)
            {
                var row = dataGridView.Rows[rowIndex];
                if (!row.IsNewRow)
                {
                    string text = string.Join(", ", row.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString() ?? ""));
                    e.Graphics.DrawString(text, bodyFont, Brushes.Black, new Point(e.MarginBounds.Left, yPos));
                    yPos += 30;

                    // Check if we've reached the page bottom
                    if (yPos > e.MarginBounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return yPos;
                    }
                }
            }

            e.HasMorePages = false;
            return yPos;
        }

        private void PrintRankings_Click(object sender, EventArgs e)
        {
            PrintRanking();
        }

        private void ExportPlayerRanking_Click(object sender, EventArgs e)
        {
            ExportRankingsToPdf();
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var confirmation = MessageBox.Show("Do you really want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmation == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void btnChangeSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    ApplySettings();
                }
            }
        }
    }
}
