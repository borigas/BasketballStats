using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.DataContracts.Db;

namespace BasketballStats.Shared.DataContracts
{
    public class Game : DbGame
    {
        public Season Season { get; set; }
        public TeamGame HomeTeam { get; set; }
        public TeamGame AwayTeam { get; set; }
        public List<Possession> Possessions { get; set; }
        public List<Shot> Shots { get; set; }
        public List<Stat> Stats { get; set; }
    }
}
