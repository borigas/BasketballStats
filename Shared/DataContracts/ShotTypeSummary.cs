using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts
{
    public class ShotTypeSummary
    {
        public ShotType ShotType { get; set; }
        public int Attempts { get; set; }
        public int Makes { get; set; }
        public int Points { get; set; }
    }
}
