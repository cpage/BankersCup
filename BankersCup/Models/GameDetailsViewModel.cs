using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class GameDetailsViewModel
    {
        public int GameId { get; set; }
        public Course GameCourse { get; set; }

        public Team CurrentTeam { get; set; }
        public int HolesPlayed { get; set; }

        public List<TeamHoleScore> CurrentTeamScores { get; set; }
        public List<LeaderboardEntryViewModel> Leaderboard { get; set; }
    }
}