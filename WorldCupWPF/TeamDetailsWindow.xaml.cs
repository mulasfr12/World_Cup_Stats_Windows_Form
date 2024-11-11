using DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WorldCupWPF
{
    /// <summary>
    /// Interaction logic for TeamDetailsWindow.xaml
    /// </summary>
    public partial class TeamDetailsWindow : Window
    {
        public TeamDetailsWindow(List<Results> teamResults)
        {
            InitializeComponent();
            DisplayTeamInfo(teamResults);
        }

        private void DisplayTeamInfo(List<Results> teamResults)
        {
            // Assuming teamResults contains multiple entries for different matches or summaries for the same team
            if (teamResults == null || !teamResults.Any())
            {
                MessageBox.Show("No data available for this team.");
                return;
            }

            // Display general information based on the first result
            var team = teamResults.First();
            CountryNameText.Text = $"Country: {team.Country}";
            FifaCodeText.Text = $"FIFA Code: {team.Fifa_Code}";

            // Aggregate or display individual match data
            GamesPlayedText.Text = $"Games Played: {teamResults.Sum(r => r.games_played)}";
            WinsText.Text = $"Wins: {teamResults.Sum(r => r.wins)}";
            DrawsText.Text = $"Draws: {teamResults.Sum(r => r.draws)}";
            LossesText.Text = $"Losses: {teamResults.Sum(r => r.losses)}";
            GoalsForText.Text = $"Goals Scored: {teamResults.Sum(r => r.goals_for)}";
            GoalsAgainstText.Text = $"Goals Against: {teamResults.Sum(r => r.goals_against)}";
            GoalDifferenceText.Text = $"Goal Difference: {teamResults.Sum(r => r.goal_differential)}";
        }
    }
}
