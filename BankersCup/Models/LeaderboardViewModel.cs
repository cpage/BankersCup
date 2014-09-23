using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class LeaderboardViewModel
    {
        public List<LeaderboardEntryViewModel> Teams { get; set; }
        public int HolesPlayed { get; set; }
        public Team CurrentTeam { get; set; }
    }
}