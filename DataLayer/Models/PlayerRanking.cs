using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class PlayerRanking
    {
        public string FullName { get; set; }
        public int Goals { get; set; }
        public int YellowCards { get; set; }
        public int Appearances { get; set; }
        public string ImagePath { get; set; } // Path to the player's image
    }
}
