using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class MatchStats
    {
            public string FifaId { get; set; }
            public string Location { get; set; }
            public string NumberOfVisitors { get; set; }
            public string HostTeam { get; set; }
            public string GuestTeam { get; set; }
    }
}
