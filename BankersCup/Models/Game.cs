using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class Game
    {
        [JsonProperty("_self")]
        public string SelfLink { get; set; }

        [JsonProperty("id")]
        public string DocumentId { get; set; }

        public int GameId { get; set; }

        
        public DateTime GameDate { get; set; }

        public string Name { get; set; }

        public Course GameCourse { get; set; }

        public List<Team> RegisteredTeams { get; set; }

        public List<TeamHoleScore> Scores { get; set; }

        public List<GameComment> Comments { get; set; }

        public List<Image> Images { get; set; }

        public bool IsDeleted { get; set; }

        public int GetNextTeamId()
        {
            if(RegisteredTeams == null || RegisteredTeams.Count == 0)
            {
                return 1;
            }

            return RegisteredTeams.Max(t => t.TeamId) + 1;
        }
    }
}