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
                // Apply the gender-specific endpoint based on the saved selection
                string endpoint = (_gender == "Male")
                    ? "teams/results" // Men’s endpoint for teams or matches
                    : "teams/results"; // Women’s endpoint for teams or matches

                // Apply language localization
                ApplyLanguage(_language);

                // Fetch and display data based on the data source (API or JSON)
                FetchData(endpoint,_gender);

                // Show a confirmation message
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
                // Pass in gender-specific endpoint or determine if fetching via API/JSON.
                bool isMen = gender.ToLower() == "male";
                return await _dataService.GetTeamsData(endpoint, isMen);
            }
            return new List<Team>();
        }

        private async void LoadTeams(string gender, string dataSource)
        {
            try
            {
                // Determine if data should be fetched from the API or JSON based on the existing DataService
                bool isMen = gender.ToLower() == "male";
                string endpointOrPath = "";

                // Set the appropriate endpoint or JSON path based on the gender and data source
                if (_dataService != null)
                {
                    if (_dataSource == "API")
                    {
                        // Use API endpoint based on gender selection
                        endpointOrPath = isMen
                            ? "https://worldcup-vua.nullbit.hr/men/teams/results"
                            : "https://worldcup-vua.nullbit.hr/women/teams/results";
                    }
                    else
                    {
                        // Use JSON file path based on gender selection
                        endpointOrPath = isMen
                            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Men", "teams.json")
                            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Women", "teams.json");
                    }
                }

                // Fetch teams using the DataService instance
                List<Team> teams = await _dataService.GetTeamsData(endpointOrPath, isMen);

                // Clear the ComboBox items to avoid duplication
                cbFavoriteTeam.Items.Clear();

                // Check if teams were successfully fetched
                if (teams != null && teams.Count > 0)
                {
                    // Load teams into ComboBox in the format "NAME (FIFA_CODE)"
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
        }

        private async void LoadPlayers(string gender, string dataSource, string favoriteTeam)
        {
            try
            {
                // Determine if data should be fetched from the API or JSON based on the existing DataService
                bool isMen = gender.ToLower() == "male";
                string endpoint = "";

                // Set the appropriate endpoint or JSON path based on the gender and data source
                if (_dataService != null)
                {
                    if (_dataSource == "API")
                    {
                        // Use API endpoint based on gender selection
                        endpoint = isMen
                            ? "https://worldcup-vua.nullbit.hr/men/matches"
                            : "https://worldcup-vua.nullbit.hr/women/matches";
                    }
                    else
                    {
                        // Use JSON file path based on gender selection
                        endpoint = isMen
                            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Men", "matches.json")
                            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonFiles", "Women", "matches.json");
                    }
                }

                // Fetch matches data
                var matches = await _dataService.GetMatchesData(endpoint, gender == "Male");
               // var matches = await _dataService.GetMatchesDataForCountry(favoriteTeam, isMen);


                // Ensure data was fetched
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
                
                // Filter players only from the favorite team
                var players = new List<object>(); // Using object to accommodate different player types
                if (firstMatch.home_team_statistics.country.Equals(favoriteTeamName, StringComparison.OrdinalIgnoreCase))
                {
                    // Load players from the home team
                    players.AddRange(firstMatch.home_team_statistics.starting_eleven);
                    players.AddRange(firstMatch.home_team_statistics.substitutes);
                }
                else if (firstMatch.away_team_statistics.country.Equals(favoriteTeamName, StringComparison.OrdinalIgnoreCase))
                {
                    // Load players from the away team
                    players.AddRange(firstMatch.away_team_statistics.starting_eleven);
                    players.AddRange(firstMatch.away_team_statistics.substitutes);
                }

                // If no players found for the favorite team
                if (!players.Any())
                {
                    MessageBox.Show("No players found for the selected team.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Load the players into the UI panels
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

            // Load previously saved favorite players
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
                    flowPanelFavorites.Controls.Add(playerControl);  // Use flowPanelFavorites
                    Console.WriteLine($"Added favorite player: {playerControl.Player.name}");
                }
                else
                {
                    playerControl.UpdateFavoriteStatus(false);
                    flowPanelOthers.Controls.Add(playerControl);  // Use flowPanelOthers
                    Console.WriteLine($"Added other player: {playerControl.Player.name}");
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
            // Ensure left mouse button is pressed to start drag
            if (e.Button == MouseButtons.Left)
            {
                PlayerControl playerControl = sender as PlayerControl;

                // Begin dragging the control
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
                control.UpdateFavoriteStatus(true); // Ensure the star is shown
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
            // Create a list to hold the favorite players' names
            List<string> favoritePlayerNames = new List<string>();

            // Loop through the controls in the flowPanelFavorites (assuming it holds PlayerControl instances)
            foreach (Control control in flowPanelFavorites.Controls)
            {
                if (control is PlayerControl playerControl)
                {
                    // Add the player's name to the list
                    favoritePlayerNames.Add(playerControl.Player.name);
                }
            }
            // Write the player names to a text file
            File.WriteAllLines("favorite_players.txt", favoritePlayerNames);
        }


        //private void MarkAsFavorite(PlayerControl playerControl)
        //{
        //    // Set the player as a favorite
        //    playerControl.UpdateFavoriteStatus(true);

        //    // Add logic to move the player control to the favorite panel if necessary
        //    panelFavorites.Controls.Add(playerControl);
        //    panelOthers.Controls.Remove(playerControl);
        //}
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
            // Clear the current DataGridView rows to refresh the rankings
            dataGridViewPlayerRankings.Rows.Clear();

            // Get the data for the favorite team stored previously (e.g., from a text file or settings)
            var favoriteTeamCode = LoadFavoriteTeam(); // Assumes FIFA code is stored
            MessageBox.Show($"Favorite Team FIFA Code: {favoriteTeamCode}"); // Check if the correct code is loaded
            // Get the match data (either from JSON or API) based on the favorite team
            var matches = await _dataService.GetMatchesData("https://worldcup-vua.nullbit.hr/men/matches", true); // Await the asynchronous c

            var filteredMatches = matches.Where(match =>
            favoriteTeamCode.Contains(match.home_team_country) || favoriteTeamCode.Contains(match.away_team_country) ).ToList();

            if (filteredMatches.Count == 0)
            {
                MessageBox.Show("No matches found for the favorite team.");
                return;
            }

            MessageBox.Show($"Total Matches for Favorite Team: {filteredMatches.Count}");
            // Calculate the player stats based on the match data
            var playerStats = CalculatePlayerStats(filteredMatches);

            // Sort players based on goals, then by yellow cards (descending order)
            var sortedPlayerStats = playerStats
                .OrderByDescending(p => p.Goals)
                .ThenByDescending(p => p.YellowCards)
                .ToList();

            if (sortedPlayerStats.Count == 0)
            {
                MessageBox.Show("No player stats available for the selected team.");
                return;
            }

            // Add sorted players to the DataGridView
            foreach (var player in sortedPlayerStats)
            {
                var playerPicture = Resources.defaultPlayer; // Use default image if picture is null

                dataGridViewPlayerRankings.Rows.Add(player.FullName, player.Goals, player.YellowCards, player.Appearances, playerPicture);
            }
        }
        

        private List<PlayerStats> CalculatePlayerStats(List<Country.Root> matches)
        {
            var playerStatsDict = new Dictionary<string, PlayerStats>(); // Dictionary to accumulate player stats

            foreach (var match in matches)
            {
                // Process the home team players
                var homeTeam = match.home_team_statistics;
                if (homeTeam != null)
                {
                    ProcessTeamPlayers(homeTeam.starting_eleven, homeTeam.substitutes, playerStatsDict);
                }

                // Process the away team players
                var awayTeam = match.away_team_statistics;
                if (awayTeam != null)
                {
                    ProcessTeamPlayers(awayTeam.starting_eleven, awayTeam.substitutes, playerStatsDict);
                }

                // Process home team match events
                ProcessHomeTeamEvents(match.home_team_events, playerStatsDict);

                // Process away team match events
                ProcessAwayTeamEvents(match.away_team_events, playerStatsDict);
            }
            // Debugging: Check if playerStatsDict has any players
            if (playerStatsDict.Count == 0)
            {
                MessageBox.Show("No player statistics found.");
            }
            // Return the list of PlayerStats accumulated
            return playerStatsDict.Values.ToList();
        }

        private void ProcessHomeTeamEvents(List<Country.HomeTeamEvent> homeTeamEvents, Dictionary<string, PlayerStats> playerStatsDict)
        {
            foreach (var teamEvent in homeTeamEvents)
            {
                // Process goals and yellow cards for home team events
                if (teamEvent.type_of_event == "goal" || teamEvent.type_of_event == "goal-penalty")
                {
                    // Increment goals for the player if they exist in the dictionary
                    if (playerStatsDict.TryGetValue(teamEvent.player, out var playerStats))
                    {
                        playerStats.Goals++;
                    }
                }
                else if (teamEvent.type_of_event == "yellow-card")
                {
                    // Increment yellow cards for the player if they exist in the dictionary
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
                // Process goals and yellow cards for away team events
                if (teamEvent.type_of_event == "goal" || teamEvent.type_of_event == "goal-penalty")
                {
                    // Increment goals for the player if they exist in the dictionary
                    if (playerStatsDict.TryGetValue(teamEvent.player, out var playerStats))
                    {
                        playerStats.Goals++;
                    }
                }
                else if (teamEvent.type_of_event == "yellow-card")
                {
                    // Increment yellow cards for the player if they exist in the dictionary
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
        }
        private void UpdatePlayerStats(dynamic player, Dictionary<string, PlayerStats> playerStatsDict)
        {
            // If the player already exists in the dictionary, update their stats
            if (!playerStatsDict.TryGetValue(player.name, out PlayerStats playerStats)) // Explicitly define the type
            {
                playerStats = new PlayerStats { FullName = player.name, Appearances = 0, Goals = 0, YellowCards = 0 };
                playerStatsDict[player.name] = playerStats;
            }

            // Increment appearances
            playerStats.Appearances++;
        }      
        //private Image GetPlayerImage(string playerName)
        //{
        //    // You can modify this to load from your stored images
        //    var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PlayerImages", $"{playerName}.jpg");
        //    if (File.Exists(imagePath))
        //        return Image.FromFile(imagePath);

        //    return Properties.Resources.defaultPlayer; // Use a default image if none found
        //}

        private async void btnShowMatchRankings_Click(object sender, EventArgs e)
        {
            // Clear the current DataGridView rows to refresh the rankings
            dataGridViewMatchRankings.Rows.Clear();

            // Fetch matches for the selected team (either from JSON or API)
            var favoriteTeamCode = LoadFavoriteTeam(); // Load favorite team FIFA code
         
            var matches = await _dataService.GetMatchesData("https://worldcup-vua.nullbit.hr/men/matches", true); // Await the asynchronous c

            var filteredMatches = matches.Where(match =>
            favoriteTeamCode.Contains(match.home_team_country) || favoriteTeamCode.Contains(match.away_team_country)).ToList();

            if (filteredMatches.Count == 0)
            {
                MessageBox.Show("No matches found for the favorite team.");
                return;
            }

            MessageBox.Show($"Total Matches for Favorite Team: {filteredMatches.Count}");

            // Calculate the player stats based on the match data
            var playerStats = CalculatePlayerStats(filteredMatches);
            // Calculate match stats
            var matchStats = CalculateMatchStats(matches);

            // Sort matches by the number of visitors (attendance) in descending order
            var sortedMatchStats = matchStats.OrderByDescending(m => m.NumberOfVisitors).ToList();

            // Add sorted matches to the DataGridView
            foreach (var match in sortedMatchStats)
            {
                dataGridViewMatchRankings.Rows.Add(match.Location, match.NumberOfVisitors, match.HostTeam, match.GuestTeam);
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
                    FifaId = match.fifa_id, // Assuming there's a 'fifa_id' property
                    Location = match.location,
                    NumberOfVisitors = match.attendance,
                    HostTeam = match.home_team_country,
                    GuestTeam = match.away_team_country
                };

                matchStatsList.Add(matchStats);
            }

            // Return the list of MatchStats
            return matchStatsList;
        }
        private void ExportRankingsToPdf()
        {
            // Define the PDF document
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
                // Close the document
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

        private void PrintPageEvent(object sender, PrintPageEventArgs e)
        {
            // Fonts for printing
            System.Drawing.Font headerFont = new System.Drawing.Font("Arial", 14, FontStyle.Bold);
            System.Drawing.Font bodyFont = new System.Drawing.Font("Arial", 10);

            // Starting coordinates for printing
            int yPos = 100;
            int margin = 50; // Margin between sections

            // Print Player Rankings Header
            e.Graphics.DrawString("Player Rankings", headerFont, Brushes.Black, new Point(100, yPos));
            yPos += margin;

            // Loop through DataGridView rows and print player rankings
            foreach (DataGridViewRow row in dataGridViewPlayerRankings.Rows)
            {
                if (!row.IsNewRow)
                {
                    string playerText = $"{row.Cells[0].Value} - Goals: {row.Cells[1].Value}, Yellow Cards: {row.Cells[2].Value}, Appearances: {row.Cells[3].Value}";
                    e.Graphics.DrawString(playerText, bodyFont, Brushes.Black, new Point(100, yPos));
                    yPos += 30;
                }
            }

            // Add some space between sections
            yPos += margin;

            // Print Match Rankings Header
            e.Graphics.DrawString("Match Rankings", headerFont, Brushes.Black, new Point(100, yPos));
            yPos += margin;

            // Loop through DataGridView rows and print match rankings
            foreach (DataGridViewRow row in dataGridViewMatchRankings.Rows) // Assuming a separate DataGridView for match rankings
            {
                if (!row.IsNewRow)
                {
                    string matchText = $"Location: {row.Cells[0].Value}, Visitors: {row.Cells[1].Value}, Home Team: {row.Cells[2].Value}, Away Team: {row.Cells[3].Value}";
                    e.Graphics.DrawString(matchText, bodyFont, Brushes.Black, new Point(100, yPos));
                    yPos += 30;
                }
            }

            // Check if more pages are required (based on content length)
            if (yPos > e.MarginBounds.Bottom)
            {
                e.HasMorePages = true;
                yPos = 100; // Reset yPos for next page
            }
            else
            {
                e.HasMorePages = false;
            }
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
