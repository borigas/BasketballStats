using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts.Db
{
    public class DbStatType : DbBase
    {
        public string StatName { get; set; }
        public string StatAbbreviation { get; set; }
        public bool WillEndPossession { get; set; }
        public bool RequiresParentStat { get; set; }
        public bool IsEnabled { get; set; }
    }
}
