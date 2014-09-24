using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class AddHoleScoreViewModel
    {
        public int GameId { get; set; }
        public int HoleNumber { get; set; }
        public int Par { get; set; }
        public int Distance { get; set; }
        public int TeamScore { get; set; }
        public int TeamId { get; set; }
        public double AverageScore { get; set; }
        public bool AllHoleScoresEntered { get; set; }
        public bool NextHoleAfterSave { get; set; }
        public bool PreviousHoleAfterSave { get; set; }
    }
}