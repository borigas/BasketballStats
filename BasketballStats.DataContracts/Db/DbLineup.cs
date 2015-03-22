using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.DataContracts
{
    public class DbLineup : DbGameEvent
    {
        public List<Guid> PlayerIds { get; set; }
    }
}
