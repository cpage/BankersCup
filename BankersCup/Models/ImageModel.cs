using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class Image
    {
        public int GameId { get; set; }
        public int HoleNumber { get; set; }
        public string ImageUrl { get; set; }
        public string UploadedById { get; set; }
        public int UploadedByTeamId { get; set; }
        DateTime UploadedOn { get; set; }
        public string Caption { get; set; }
    }
}