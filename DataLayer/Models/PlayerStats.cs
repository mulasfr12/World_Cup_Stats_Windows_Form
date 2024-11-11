using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DataLayer.Models
{
    public class PlayerStats
    {
        public string FullName { get; set; }
        public int Goals { get; set; }
        public int YellowCards { get; set; }
        public int Appearances
        {
            get; set;
        }
    }
}
