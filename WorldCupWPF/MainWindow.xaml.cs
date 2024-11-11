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

         private void TeamCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            matchResultTB.Text = "--";
            LoadOpponentTeams(); // Populate OpponentTeamCB based on the selected team
        }

        private void OpponentTeamCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Display match result when opponent team is selected
            DisplayMatchResult();
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

        private Point GetPositionOnField(string position, bool isHomeTeam)
        {
            switch (position)
            {
                case "Goalie":
                    return isHomeTeam ? new Point(50, 200) : new Point(450, 200);
                case "Defender":
                    return isHomeTeam ? new Point(100, 150) : new Point(400, 150);
                case "Midfield":
                    return isHomeTeam ? new Point(200, 200) : new Point(300, 200);
                case "Forward":
                    return isHomeTeam ? new Point(300, 250) : new Point(200, 250);
                default:
                    return new Point(0, 0);
            }
        }
        //private void DisplayStartingEleven(List<Country.StartingEleven> startingEleven, bool isHomeTeam)
        //{
        //    foreach (var player in startingEleven)
        //    {
        //        PlayerControl playerControl = new PlayerControl
        //        {
        //            PlayerName = player.name,
        //            ShirtNumber = player.shirt_number,
        //            // Assuming you have a method to get player images
        //            PositionIconSource = GetPlayerImage(player.name)
        //        };

        //        Point position = GetPositionOnField(player.position, isHomeTeam);
        //        Canvas.SetLeft(playerControl, position.X);
        //        Canvas.SetTop(playerControl, position.Y);

        //        FieldCanvas.Children.Add(playerControl);
        //    }
        //}
        //List<Country.StartingEleven> homeStartingEleven = homeTeamStats.starting_eleven;
        //List<Country.StartingEleven> awayStartingEleven = awayTeamStats.starting_eleven;

        //private void ShowStartingLineups()
        //{
        //    DisplayStartingEleven(homeStartingEleven, isHomeTeam: true);
        //    DisplayStartingEleven(awayStartingEleven, isHomeTeam: false);
        //}

    }
}
