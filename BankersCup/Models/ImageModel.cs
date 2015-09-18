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
        public int UploadedById { get; set; }
        public int UploadedByTeamId { get; set; }
        public DateTime UploadedOn { get; set; }
        public string Caption { get; set; }
    }
}