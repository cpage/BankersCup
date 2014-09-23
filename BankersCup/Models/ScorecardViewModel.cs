using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class ScorecardViewModel
    {
        public int GameId { get; set; }
        public Team CurrentTeam { get; set; }
        public List<TeamHoleScore> HoleScores { get; set; }
    }
}