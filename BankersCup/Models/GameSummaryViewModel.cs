using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class GameSummaryViewModel
    {
        public int GameId { get; set; }
        public string EventName { get; set; }
        public string CourseName { get; set; }
        public DateTime GameDate { get; set; }
        public int NumberOfTeams { get; set; }
    }
}