using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts.Db
{
    public class StatTypeLink : DbBase
    {
        public Guid ParentStatId { get; set; }
        public Guid ChildStatId { get; set; }
    }
}
