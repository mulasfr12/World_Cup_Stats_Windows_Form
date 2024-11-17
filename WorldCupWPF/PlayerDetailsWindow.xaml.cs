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
    /// Interaction logic for PlayerDetailsWindow.xaml
    /// </summary>
    public partial class PlayerDetailsWindow : Window
    {
        public PlayerDetailsWindow(Country.StartingEleven player, PlayerStats stats)
        {
            InitializeComponent();
            PlayerName.Text = $"Name: {player.name}";
            PlayerNumber.Text = $"Number: {player.shirt_number}";
            PlayerPosition.Text = $"Position: {player.position}";
            PlayerCaptain.Text = player.captain ? "Captain: Yes" : "Captain: No";
            PlayerGoals.Text = $"Goals Scored: {stats.Goals}";
            PlayerYellowCards.Text = $"Yellow Cards: {stats.YellowCards}";
            PlayerImage.Source = new BitmapImage(new Uri("Images/PlayerIcon.png", UriKind.Relative));
        }
    }
}
