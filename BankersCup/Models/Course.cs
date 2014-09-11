using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BankersCup.Models
{
    public class Course
    {
        public string Name { get; set; }

        public int Par { get { return Holes.Sum(h => h.Par); } }
        public int Distance { get { return Holes.Sum(h => h.Distance); } }

        public List<HoleInfo> Holes { get; set; }

        public Course()
        {
            Holes = new List<HoleInfo>();
        }
    }
}
