using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class LeaderboardEntryViewModel
    {
        public string TeamName { get; set; }
        public int TotalScore { get; set; }
        public int AgainstPar { get; set; }
    }
}