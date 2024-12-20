﻿using DataLayer.Models;
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
        private PlayerDetailsWindow _playerDetailsWindow;
        private SettingsWindow _settingsWindow;

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

                _teams = await _dataService.GetTeamsData(endpointOrPath, _isMen); 

                TeamCB.ItemsSource = _teams.Select(t => $"{t.Country} ({t.Fifa_Code})").ToList();
                OpponentTeamCB.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading teams: {ex.Message}");
            }
        }
        private async void LoadOpponentTeams()
        {
            
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
        private async void DisplayMatchResult()
        {
            if (TeamCB.SelectedItem is string team1Text && OpponentTeamCB.SelectedItem is string team2Text)
            {
                var team1Name = team1Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                var team2Name = team2Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

                try
                {
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

                    var match = matches.FirstOrDefault(m =>
                        (m.home_team?.country?.Equals(team1Name, StringComparison.OrdinalIgnoreCase) == true &&
                         m.away_team?.country?.Equals(team2Name, StringComparison.OrdinalIgnoreCase) == true) ||
                        (m.home_team?.country?.Equals(team2Name, StringComparison.OrdinalIgnoreCase) == true &&
                         m.away_team?.country?.Equals(team1Name, StringComparison.OrdinalIgnoreCase) == true));

                    if (match != null)
                    {
                        string result;

                        bool isTeam1Home = match.home_team?.country?.Equals(team1Name, StringComparison.OrdinalIgnoreCase) == true;

                        if (isTeam1Home)
                        {
                            result = $"{match.home_team?.goals} : {match.away_team?.goals}";
                        }
                        else
                        {
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
        private void LoadSettings()
        {
            try
            {
                var settings = File.ReadAllText("settings.txt").Split(';');

                if (settings.Length == 3)
                {
                    _isMen = settings[0].Trim().Equals("Male", StringComparison.OrdinalIgnoreCase);

                    _language = settings[1].Trim().Equals("Croatian", StringComparison.OrdinalIgnoreCase) ? "hr" : "en";

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
                
                _isMen = true; 
                _language = "en";     
                _useApi = true;      
            }
            if(_language == "hr")
            {
                Label_Team.Content = "Timu";
                Label_Opponent.Content = "Protivnik";
                TeamInfoButton.Content = "Info o Timu";
                OpponentInfoButton.Content = "Info o Timu";
                SettingsButton.Content = "Postavke";
                Home_Team.Text = "Domaci tim";
                Away_Team.Text = "gostujuci tim";              
            }
        }
        private async void TeamCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            matchResultTB.Text = "--";
            LoadOpponentTeams(); 
            await UpdateLineups();
        }
        private async void OpponentTeamCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayMatchResult();
            await UpdateLineups();
        }
        private async Task UpdateLineups()
            {
                if (!(TeamCB.SelectedItem is string homeTeamText) || !(OpponentTeamCB.SelectedItem is string awayTeamText))
                    return;

                var homeTeamCode = ExtractFifaCode(homeTeamText);
                var awayTeamCode = ExtractFifaCode(awayTeamText);

            string genderEndpoint = _useApi
                        ? (_isMen ? "https://worldcup-vua.nullbit.hr/men/matches"
                                  : "https://worldcup-vua.nullbit.hr/women/matches")
                        : string.Empty;

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
                        DisplayLineups(match.home_team_statistics?.starting_eleven,
                        match.home_team_statistics?.substitutes,
                        match.away_team_statistics?.starting_eleven,
                        match.away_team_statistics?.substitutes, match.home_team_events,
                    match.away_team_events);
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
              
                // Show loading animation
                ShowLoadingOverlay();

                await Task.Delay(500);
 
                ShowTeamInfo(fifaCode, _isMen);
                HideLoadingOverlay();
            }
        }
        private async void OpponentInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpponentTeamCB.SelectedItem is string selectedOpponentText)
            {
                var fifaCode = ExtractFifaCode(selectedOpponentText);

                ShowLoadingOverlay();
                await Task.Delay(500);
                ShowTeamInfo(fifaCode, _isMen);
                HideLoadingOverlay();
            }
        }
        
        private void ShowLoadingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
        }
        private void HideLoadingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        private string ExtractFifaCode(string itemText)
        {
            var startIndex = itemText.IndexOf('(') + 1;
            var endIndex = itemText.IndexOf(')');
            return itemText.Substring(startIndex, endIndex - startIndex);
        }

        private async void ShowTeamInfo(string fifaCode, bool isMen)
        {
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
        private void DisplayLineups(
         List<Country.StartingEleven> homeTeam,
         List<Country.Substitute> homeSubstitutes,
         List<Country.StartingEleven> awayTeam,
         List<Country.Substitute> awaySubstitutes,
         List<Country.HomeTeamEvent> homeEvents,
         List<Country.AwayTeamEvent> awayEvents
            )
        {
            fieldCanvas.Children.Clear();

            double canvasWidth = fieldCanvas.ActualWidth;
            double canvasHeight = fieldCanvas.ActualHeight;

            var homePositions = new Dictionary<string, (double X, double Y)>
            {
                { "Goalie", (0.465 * canvasWidth, 0.03 * canvasHeight) },
                { "Defender", (0.1 * canvasWidth, 0.18 * canvasHeight) },
                { "Midfield", (0.1 * canvasWidth, 0.31 * canvasHeight) },
                { "Forward", (0.35 * canvasWidth, 0.42 * canvasHeight) }
            };

                    var awayPositions = new Dictionary<string, (double X, double Y)>
            {
                { "Goalie", (0.465 * canvasWidth, 0.95 * canvasHeight) },
                { "Defender", (0.1 * canvasWidth, 0.8 * canvasHeight) },
                { "Midfield", (0.1 * canvasWidth, 0.66 * canvasHeight) },
                { "Forward", (0.35 * canvasWidth, 0.55 * canvasHeight) }
            };

            const double spacing = 75;

            RenderPlayers(homeTeam, homePositions, spacing, true, homeEvents, awayEvents);
            RenderPlayers(awayTeam, awayPositions, spacing, false, homeEvents, awayEvents);
            RenderLineup(HomeTeamLineup, homeTeam, homeSubstitutes, "Home Team");
            RenderLineup(AwayTeamLineup, awayTeam, awaySubstitutes, "Away Team");

        }

        private void RenderPlayers(List<Country.StartingEleven> players,
                           Dictionary<string, (double X, double Y)> positions,
                           double spacing,
                           bool isHomeTeam,
                           List<Country.HomeTeamEvent> homeEvents,
                           List<Country.AwayTeamEvent> awayEvents)
        {
            var playerCountByPosition = new Dictionary<string, int>();

            foreach (var player in players)
            {
                if (positions.TryGetValue(player.position, out var basePosition))
                {
                    if (!playerCountByPosition.ContainsKey(player.position))
                        playerCountByPosition[player.position] = 0;

                    var offset = playerCountByPosition[player.position] * spacing;
                    var x = basePosition.X + offset;
                    var y = basePosition.Y;

                    var playerControl = new PlayerControl(player.shirt_number, "Images/PlayerIcon.png");
                    Canvas.SetLeft(playerControl, x);
                    Canvas.SetTop(playerControl, y);
                    fieldCanvas.Children.Add(playerControl);

                    // Attach click event to open player details
                    playerControl.MouseDown += (s, e) =>
                    {
                        var stats = GetPlayerStats(player, homeEvents, awayEvents);
                        ShowPlayerDetails(player, stats);
                    };

                    playerCountByPosition[player.position]++;
                }
            }
        }
        private PlayerStats GetPlayerStats(Country.StartingEleven player, List<Country.HomeTeamEvent> homeEvents, List<Country.AwayTeamEvent> awayEvents)
        {
            return new PlayerStats
            {
                FullName = player.name,
                Goals = GetPlayerGoals(player.name, homeEvents, awayEvents),
                YellowCards = GetPlayerYellowCards(player.name, homeEvents, awayEvents),
            };
        }
        private int GetPlayerGoals(string playerName, List<Country.HomeTeamEvent> homeEvents, List<Country.AwayTeamEvent> awayEvents)
        {
            return homeEvents.Count(e => e.player == playerName && e.type_of_event == "goal") +
                   awayEvents.Count(e => e.player == playerName && e.type_of_event == "goal");
        }
        private int GetPlayerYellowCards(string playerName, List<Country.HomeTeamEvent> homeEvents, List<Country.AwayTeamEvent> awayEvents)
        {
            return homeEvents.Count(e => e.player == playerName && e.type_of_event == "yellow-card") +
                   awayEvents.Count(e => e.player == playerName && e.type_of_event == "yellow-card");
        }

        private void ShowPlayerDetails(Country.StartingEleven player, PlayerStats stats)
        {
            if (_playerDetailsWindow != null)
            {
                _playerDetailsWindow.Close();
                _playerDetailsWindow = null;
            }

            _playerDetailsWindow = new PlayerDetailsWindow(player, stats);

            var animation = new System.Windows.Media.Animation.DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.3)));
            _playerDetailsWindow.BeginAnimation(UIElement.OpacityProperty, animation);

            _playerDetailsWindow.Show();
        }
       
        private void RenderLineup(StackPanel lineupPanel, List<Country.StartingEleven> players, List<Country.Substitute> substitutes, string teamName)
        {
            lineupPanel.Children.Clear();
            
            var teamHeader = new TextBlock
            {
                Text = $"{teamName} Lineup",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            lineupPanel.Children.Add(teamHeader);
 
            foreach (var player in players)
            {
                var playerInfo = new TextBlock
                {
                    Text = $"{player.name} ({player.shirt_number})",
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                lineupPanel.Children.Add(playerInfo);
            }
         
            var separator = new TextBlock
            {
                Text = "Substitutes",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            };
            lineupPanel.Children.Add(separator);

            foreach (var substitute in substitutes)
            {
                var substituteInfo = new TextBlock
                {
                    Text = $"{substitute.name} ({substitute.shirt_number})",
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                lineupPanel.Children.Add(substituteInfo);
            }

        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true) 
            {
                MessageBox.Show("Settings have been updated!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSettings(); 
            }
        }
    }
}
