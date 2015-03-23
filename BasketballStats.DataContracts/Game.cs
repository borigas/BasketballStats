using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.DataContracts
{
    public class Game : DbGame
    {
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public List<Lineup> Lineups { get; set; }
        public List<Possession> Possessions { get; set; }
        public List<Shot> Shots { get; set; }
        public List<Stat> Stats { get; set; }
    }
}
