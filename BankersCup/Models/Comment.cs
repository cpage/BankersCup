using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class Comment
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int HoleNumber { get; set; }
        public string Content { get; set; }
        public DateTime DateEntered { get; set; }
    }
}