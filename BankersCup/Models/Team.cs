using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BankersCup.Models
{
    public class Team
    {
        public int TeamId { get; set; }
        public string RegistrationCode { get; set; }
        public string TeamName { get; set; }
        public int StartingHole { get; set; }
        public DateTime TeeTime { get; set; }
        public List<Player> Players { get; set; }

    }
}
