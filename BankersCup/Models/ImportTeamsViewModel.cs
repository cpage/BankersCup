using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class ImportTeamsViewModel
    {
        public int GameId { get; set; }
        public string Message { get; set; }
        public HttpPostedFileBase TeamsFile { get; set; }
        public bool RemoveExistingTeams { get; set; }
    }
}