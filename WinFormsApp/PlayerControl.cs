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
using static DataLayer.Models.Country;

namespace WinFormsApp
{
    public partial class PlayerControl : UserControl
    {
        private const string PlayerImagesFolder = "PlayerImages";  // Folder within the solution
        private const string DefaultImage = "default_player_image.png";  // Name of the default image file
        public bool IsFavorite { get; set; }
        public StartingEleven Player { get; private set; }
        public PlayerControl(StartingEleven player)
        {
            InitializeComponent();
            Player = player;
           
            // Update UI components with player data
            label1.Text = player.name;              // Set player's name
            if (Player.captain)
            {
                label1.Text += " (C)";  // Append "(C)" to the player's name if they are the captain
                this.BackColor = Color.LightBlue; // Change background color for captains (optional)
            }
            label2.Text = $"Number: {player.shirt_number}";  // Set player's shirt number
            label3.Text = $"Position: {player.position}";    // Set player's position
            LoadPlayerImage();
        }
        public PlayerControl(Substitute substitute) : this(new StartingEleven(substitute.name, substitute.captain, substitute.shirt_number, substitute.position))
        {
            // This constructor delegates to the StartingEleven constructor above
            LoadPlayerImage();

        }
        private void LoadPlayerImage()
        {
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PlayerImagesFolder, $"{Player.name}.png");

            if (File.Exists(imagePath))
            {
                playerImage.Image = Image.FromFile(imagePath);
            }
            else
            {
                playerImage.Image = Properties.Resources.defaultPlayer;  // Use the default image
            }
        }
        public void ToggleFavorite()
        {
            IsFavorite = !IsFavorite;
        }
        public string GetPlayerDetails()
        {
            return Player.ToString(); // You can override ToString() in StartingEleven for detailed info
        }

        public void UpdateFavoriteStatus(bool isFavorite)
        {
            IsFavorite = isFavorite;

            // Update PictureBox based on favorite status
            if (IsFavorite)
            {
                // Set the star image for the player when they are a favorite
                this.pictureBox1.Image = global::WinFormsApp.Properties.Resources.starcap; // Make sure to have a star image in resources
            }
            else
            {
                // Reset to the default image when not a favorite
                this.pictureBox1.Image = global::WinFormsApp.Properties.Resources.download; // Reset to default image
                this.BackColor = SystemColors.Control;  // Reset background
            }
        }

        public PlayerControl()
        {
            InitializeComponent();
            LoadPlayerImage();
        }

    }
}
