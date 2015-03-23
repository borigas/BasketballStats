using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts.Db
{
    public class DbShot : DbStat
    {
        public bool IsMake { get; set; }
        public ShotType ShotType { get; set; }
        public double LocationX { get; set; }
        public double LocationY { get; set; }
        public double Distance { get; set; }
    }
}
