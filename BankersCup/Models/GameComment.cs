using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class GameComment
    {
        public GameComment()
        {
            this.CommentId = Guid.NewGuid();
        }

        public int GameId { get; set; }
        public Guid CommentId { get; private set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int HoleNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CommentText { get; set; }
    }
}