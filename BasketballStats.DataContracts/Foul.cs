using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.DataContracts
{
    public class Foul : DbBase
    {
        public Guid PlayerId { get; set; }
        public bool IsOffensiveFoul { get; set; }
        public bool IsTechnicalFoul { get; set; }
    }
}
