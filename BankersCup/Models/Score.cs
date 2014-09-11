﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BankersCup.Models
{
    public class TeamHoleScore
    {
        public int HoleNumber { get; set; }
        public int TeamId { get; set; }
        public int Score { get; set; }
        public int AgainstPar { get; set; }
    }
}
