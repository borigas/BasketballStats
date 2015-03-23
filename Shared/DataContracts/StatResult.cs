using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.DataContracts.Db;

namespace BasketballStats.Shared.DataContracts
{
    public class StatResult<T> where T : DbStat
    {
        public T Stat { get; set; }
        public List<StatType> DependentStats { get; set; }
    }
}
