using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.DataContracts
{
    public class Stat : GameEvent
    {
        public Guid PossessionId { get; set; }
        public Guid LineupId { get; set; }
        public string StatName { get; set; }
        public Guid StatEventLink { get; set; }
    }
}
