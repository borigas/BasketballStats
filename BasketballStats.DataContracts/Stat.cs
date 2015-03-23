using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.DataContracts
{
    public class Stat : DbStat
    {
        public Possession Possession { get; set; }
        public Lineup Lineup { get; set; }
        public Player Player { get; set; }
    }
}
