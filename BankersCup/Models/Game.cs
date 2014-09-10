using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class Game
    {
        public Course GameCourse { get; set; }

        public List<Team> RegisteredTeams { get; set; }

        public List<PlayerScore> Scores { get; set; }

    }
}