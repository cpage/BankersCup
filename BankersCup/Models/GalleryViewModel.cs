using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class GalleryViewModel
    {
        public GalleryViewModel()
        {
            Images = new List<Models.GalleryImageViewModel>();
        }

        public List<GalleryImageViewModel> Images { get; set; }
        public HttpPostedFileBase UploadedImage { get; set; }
    }
}