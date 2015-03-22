using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.DataContracts
{
    public class Team : DbBase
    {
        public string Name { get; set; }
        public string HomeColor { get; set; }
        public string AwayColor { get; set; }
    }
}
