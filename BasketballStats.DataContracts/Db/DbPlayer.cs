using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.DataContracts
{
    public class DbPlayer : DbBase
    {
        public Guid TeamId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Number { get; set; }
    }
}
