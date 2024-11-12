using DataLayer.Models;
using DataLayer.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WorldCupWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataService _dataService;
        private List<Team> _teams;
        private bool _isMen;
        private bool _useApi;
        private string _language;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            InitializeDataService();
            LoadTeams();
        }
        private void InitializeDataService()
        {
            _dataService = new DataService(_useApi);
        }

        private async void LoadTeams()
        {
            try
            {
                // Use the correct URL based on gender selection
                string endpointOrPath = _useApi
                    ? (_isMen ? "https://worldcup-vua.nullbit.hr/men/teams/results"
                               : "https://worldcup-vua.nullbit.hr/women/teams/results")
                    : string.Empty;

                _teams = await _dataService.GetTeamsData(endpointOrPath, _isMen); // Store teams

                TeamCB.ItemsSource = _teams.Select(t => $"{t.Country} ({t.Fifa_Code})").ToList();
                OpponentTeamCB.ItemsSource = null; // Don't populate OpponentTeamCB here
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading teams: {ex.Message}");
            }
        }


        private async void LoadOpponentTeams()
        {
            // Choose the correct endpoint for matches based on gender
            string endpointOrPath = _useApi
                ? (_isMen ? "https://worldcup-vua.nullbit.hr/men/matches"
                          : "https://worldcup-vua.nullbit.hr/women/matches")
                : string.Empty;

            OpponentTeamCB.Items.Clear();

            if (!(TeamCB.SelectedItem is string selectedTeamText))
                return;

            var selectedTeamName = selectedTeamText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

            try
            {
                var matches = await _dataService.GetMatchesData(endpointOrPath, _isMen);

                if (matches == null || !matches.Any())
                {
                    MessageBox.Show("No match data available.");
                    return;
                }

                var opponents = matches
                    .Where(m => m.home_team_country.Equals(selectedTeamName, StringComparison.OrdinalIgnoreCase) ||
                                 m.away_team_country.Equals(selectedTeamName, StringComparison.OrdinalIgnoreCase))
                    .Select(m => m.home_team_country.Equals(selectedTeamName, StringComparison.OrdinalIgnoreCase)
                        ? m.away_team_country
                        : m.home_team_country)
                    .Distinct()
                    .ToList();

                if (opponents.Count == 0)
                {
                    MessageBox.Show("No opponents found for the selected team.");
                    return;
                }

                foreach (var opponent in opponents)
                {
                    var opponentTeam = _teams.FirstOrDefault(t => t.Country.Equals(opponent));
                    if (opponentTeam != null)
                    {
                        OpponentTeamCB.Items.Add($"{opponent} ({opponentTeam.Fifa_Code})");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading opponent teams: {ex.Message}");
            }
        }

        //private async void ButtonShowResult_Click(object sender, RoutedEventArgs e)
        //{
        //    if (TeamCB.SelectedItem is string team1Text && OpponentTeamCB.SelectedItem is string team2Text)
        //    {
        //        var team1Name = team1Text.Split(' ')[0];
        //        var team2Name = team2Text.Split(' ')[0];

        //        var matchResult = await GetMatchResult(team1Name, team2Name);
        //        matchResultTB.Text = matchResult;

        //        // Load team flags
        //        firstTeamFlag.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{team1Name}.png"));
        //        secondTeamFlag.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{team2Name}.png"));
        //    }
        //}

        private async void DisplayMatchResult()
        {
            if (TeamCB.SelectedItem is string team1Text && OpponentTeamCB.SelectedItem is string team2Text)
            {
                var team1Name = team1Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                var team2Name = team2Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

                try
                {
                    // Select the correct match endpoint based on gender
                    string endpointOrPath = _useApi
                        ? (_isMen ? "https://worldcup-vua.nullbit.hr/men/matches"
                                  : "https://worldcup-vua.nullbit.hr/women/matches")
                        : string.Empty;

                    var matches = await _dataService.GetMatchesData(endpointOrPath, _isMen);

                    if (matches == null || !matches.Any())
                    {
                        matchResultTB.Text = _language == "hr" ? "Nema podataka o utakmici" : "No match data available.";
                        return;
                    }

                    // Find the match where team1 and team2 played against each other
                    var match = matches.FirstOrDefault(m =>
                        (m.home_team?.country?.Equals(team1Name, StringComparison.OrdinalIgnoreCase) == true &&
                         m.away_team?.country?.Equals(team2Name, StringComparison.OrdinalIgnoreCase) == true) ||
                        (m.home_team?.country?.Equals(team2Name, StringComparison.OrdinalIgnoreCase) == true &&
                         m.away_team?.country?.Equals(team1Name, StringComparison.OrdinalIgnoreCase) == true));

                    if (match != null)
                    {
                        string result;

                        // Determine if team1 is home or away in the found match
                        bool isTeam1Home = match.home_team?.country?.Equals(team1Name, StringComparison.OrdinalIgnoreCase) == true;

                        if (isTeam1Home)
                        {
                            // team1 is the home team in the match
                            result = $"{match.home_team?.goals} : {match.away_team?.goals}";
                        }
                        else
                        {
                            // team1 is the away team, so reverse the result
                            result = $"{match.away_team?.goals} : {match.home_team?.goals}";
                        }

                        matchResultTB.Text = result;
                    }
                    else
                    {
                        matchResultTB.Text = _language == "hr" ? "Nije pronađeno" : "Not Found";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching match result: {ex.Message}");
                }
            }
        }



        //private async Task<string> GetMatchResult(string team1, string team2)
        //{
        //    // Implement logic to retrieve and return the match result based on your data layer
        //    // For demonstration purposes, return a static result
        //    return await Task.FromResult("2 : 1");
        //}

        private void LoadSettings()
        {
            try
            {
                var settings = File.ReadAllText("settings.txt").Split(';');

                if (settings.Length == 3)
                {
                    // Read Gender setting
                    _isMen = settings[0].Trim().Equals("Male", StringComparison.OrdinalIgnoreCase);

                    // Read Language setting
                    _language = settings[1].Trim().Equals("Croatian", StringComparison.OrdinalIgnoreCase) ? "hr" : "en";

                    // Read DataSource setting
                    _useApi = settings[2].Trim().Equals("API", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    MessageBox.Show("Settings file format is incorrect. Please use the format: Male;English;API", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}");
                // Set defaults if loading fails
                _isMen = true;       // Default to male teams
                _language = "en";     // Default to English
                _useApi = true;       // Default to API
            }
        }

         private async void TeamCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            matchResultTB.Text = "--";
            LoadOpponentTeams(); // Populate OpponentTeamCB based on the selected team
            await UpdateLineups();
        }

        private async void OpponentTeamCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Display match result when opponent team is selected
            DisplayMatchResult();
            await UpdateLineups();
        }

        private async Task UpdateLineups()
        {
            if (!(TeamCB.SelectedItem is string homeTeamText) || !(OpponentTeamCB.SelectedItem is string awayTeamText))
                return;

            var homeTeamCode = ExtractFifaCode(homeTeamText);
            var awayTeamCode = ExtractFifaCode(awayTeamText);

            string baseEndpoint = "https://worldcup-vua.nullbit.hr";
            string genderEndpoint = _isMen ? $"{baseEndpoint}/men/matches" : $"{baseEndpoint}/women/matches";

            var matches = await _dataService.GetMatchesData(genderEndpoint, _isMen);

            if (matches == null || !matches.Any())
            {
                MessageBox.Show("No match data available.");
                return;
            }

            // Find the match based on the selected teams
            var match = matches.FirstOrDefault(m =>
                m.home_team?.code?.Equals(homeTeamCode, StringComparison.OrdinalIgnoreCase) == true &&
                m.away_team?.code?.Equals(awayTeamCode, StringComparison.OrdinalIgnoreCase) == true);

            if (match != null)
            {
                if (match.home_team_statistics?.starting_eleven != null &&
                    match.away_team_statistics?.starting_eleven != null)
                {
                    // Display the starting lineups
                    DisplayLineups(match.home_team_statistics.starting_eleven, match.away_team_statistics.starting_eleven);
                }
                else
                {
                    MessageBox.Show("Starting lineup data is missing for one or both teams.");
                }
            }
            else
            {
                MessageBox.Show("No match found for the selected teams.");
            }
        }


        private async void TeamInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (TeamCB.SelectedItem is string selectedTeamText)
            {
                var fifaCode = ExtractFifaCode(selectedTeamText);
                MessageBox.Show($"Team Info button clicked. FIFA Code: {fifaCode}");

                // Show loading animation
                ShowLoadingOverlay();

                // Wait 0.5 seconds before continuing
                await Task.Delay(500);

                // Open Team Details after delay
                ShowTeamInfo(fifaCode, _isMen);

                // Hide loading animation
                HideLoadingOverlay();
            }
        }

        private async void OpponentInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpponentTeamCB.SelectedItem is string selectedOpponentText)
            {
                var fifaCode = ExtractFifaCode(selectedOpponentText);

                // Show loading animation
                ShowLoadingOverlay();

                // Wait 0.5 seconds before continuing
                await Task.Delay(500);

                // Open Team Details after delay
                ShowTeamInfo(fifaCode, _isMen);

                // Hide loading animation
                HideLoadingOverlay();
            }
        }

        // Methods to show and hide the loading overlay
        private void ShowLoadingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
        }

        private void HideLoadingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }


        // Helper method to extract FIFA code from the combo box item
        private string ExtractFifaCode(string itemText)
        {
            var startIndex = itemText.IndexOf('(') + 1;
            var endIndex = itemText.IndexOf(')');
            return itemText.Substring(startIndex, endIndex - startIndex);
        }

        private async void ShowTeamInfo(string fifaCode, bool isMen)
        {
            // Choose the correct endpoint based on gender and data source
            string endpoint = _useApi
                ? (isMen ? "https://worldcup-vua.nullbit.hr/men/teams/results"
                         : "https://worldcup-vua.nullbit.hr/women/teams/results")
                : null;

            var results = await _dataService.GetResultsData(fifaCode, isMen, endpoint);

            if (results == null || !results.Any())
            {
                MessageBox.Show("No results data found for the selected team.");
                return;
            }

            var teamDetailsWindow = new TeamDetailsWindow(results);
            teamDetailsWindow.Show();
        }
        private void DisplayLineups(List<Country.StartingEleven> homeTeam, List<Country.StartingEleven> awayTeam)
        {
            fieldCanvas.Children.Clear(); // Clear previous lineups

            double canvasWidth = fieldCanvas.ActualWidth;
            double canvasHeight = fieldCanvas.ActualHeight;

            // Define base positions as percentages of the canvas dimensions
            var homePositions = new Dictionary<string, (double X, double Y)>
    {
        { "Goalie", (0.42 * canvasWidth, 0.03 * canvasHeight) },
        { "Defender", (0.08 * canvasWidth, 0.18 * canvasHeight) },
        { "Midfield", (0.5 * canvasWidth, 0.5 * canvasHeight) },
        { "Forward", (0.7 * canvasWidth, 0.7 * canvasHeight) }
    };

            var awayPositions = new Dictionary<string, (double X, double Y)>
    {
        { "Goalie", (0.9 * canvasWidth, 0.5 * canvasHeight) },
        { "Defender", (0.7 * canvasWidth, 0.3 * canvasHeight) },
        { "Midfield", (0.5 * canvasWidth, 0.5 * canvasHeight) },
        { "Forward", (0.3 * canvasWidth, 0.7 * canvasHeight) }
    };

            // Spacing for multiple players in the same position
            const double spacing = 80;

            // Render players
            RenderPlayers(homeTeam, homePositions, spacing, isHomeTeam: true);
            RenderPlayers(awayTeam, awayPositions, spacing, isHomeTeam: false);
        }

        private void RenderPlayers(List<Country.StartingEleven> players, Dictionary<string, (double X, double Y)> positions, double spacing, bool isHomeTeam)
        {
            var playerCountByPosition = new Dictionary<string, int>();

            foreach (var player in players)
            {
                if (positions.TryGetValue(player.position, out var basePosition))
                {
                    if (!playerCountByPosition.ContainsKey(player.position))
                        playerCountByPosition[player.position] = 0;

                    // Calculate position with horizontal spacing for overlapping players
                    var offset = playerCountByPosition[player.position] * spacing;
                    var x = basePosition.X + offset; // Offset horizontally for multiple players
                    var y = basePosition.Y; // Keep vertical position fixed

                    // Create player control and position it
                    var playerControl = new PlayerControl(player.name, player.shirt_number, "Images/PlayerIcon.png");
                    Canvas.SetLeft(playerControl, x);
                    Canvas.SetTop(playerControl, y);
                    fieldCanvas.Children.Add(playerControl);

                    playerCountByPosition[player.position]++;
                }
            }
        }


    }
}
