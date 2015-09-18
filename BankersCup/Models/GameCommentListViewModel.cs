using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class GameCommentListViewModel
    {
        public string NewComment { get; set; }

        public List<GameCommentViewModel> ExistingComments { get; set; }
    }
}