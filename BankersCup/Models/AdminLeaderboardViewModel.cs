using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class AdminLeaderboardViewModel
    {
        public List<LeaderboardEntryViewModel> ByTeams { get; set; }
        public List<LeaderboardEntryViewModel> ByInstitutions { get; set; }

    }
}