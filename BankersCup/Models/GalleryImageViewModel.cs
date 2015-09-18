using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class GalleryImageViewModel
    {
        public string Url { get; set; }
        public int HoleNumber { get; set; }
        public string Caption { get; set; }
        public int TakenBy { get; set; }
    }
}