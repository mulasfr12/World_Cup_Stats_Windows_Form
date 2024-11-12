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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorldCupWPF
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        public PlayerControl(string name, int shirtNumber, string imageUri)
        {
            InitializeComponent();
            DataContext = this;
            Name = name;
            ShirtNumber = shirtNumber;
            Image = new BitmapImage(new Uri(imageUri, UriKind.RelativeOrAbsolute));
        }

        public string Name { get; set; }
        public int ShirtNumber { get; set; }
        public ImageSource Image { get; set; }
    }


}
