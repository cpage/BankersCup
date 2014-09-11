using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class Game
    {
        public int Id { get; set; }

        public DateTime GameDate { get; set; }

        public string Name { get; set; }

        public Course GameCourse { get; set; }

        public List<Team> RegisteredTeams { get; set; }

        public List<TeamHoleScore> Scores { get; set; }

    }
}