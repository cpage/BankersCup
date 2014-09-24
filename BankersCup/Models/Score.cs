using System;
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

        public override string ToString()
        {
            return string.Format("Team: {0}, Hole: {1}, Score {2}, AgainstPar: {3}", TeamId, HoleNumber, Score, AgainstPar);
        }
    }
}
