﻿using System;
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
        public bool AllHoleScoresEntered { get; set; }
    }
}