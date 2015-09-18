using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class GameCommentViewModel
    {
        public string PlayerName { get; set; }
        public string TeamName { get; set; }

        public int HoleNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Comment { get; set; }

    }
}