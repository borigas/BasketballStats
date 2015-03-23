using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.DataContracts.Db;

namespace BasketballStats.Shared.DataContracts
{
    public class Shot : DbShot
    {
        public Possession Possession { get; set; }
        public Lineup Lineup { get; set; }
        public Player Player { get; set; }
    }
}
