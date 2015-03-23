using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts.Db
{
    public class DbStat : DbGameEvent
    {
        public Guid PossessionId { get; set; }
        public Guid LineupId { get; set; }
        public Guid TeamId { get; set; }
        public Guid? PlayerId { get; set; }
        public string StatName { get; set; }
        public Guid StatEventLink { get; set; }
    }
}
