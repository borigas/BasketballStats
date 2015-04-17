using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts
{
    public class GameSettings
    {
        public int RegulationPeriodsInGame { get; set; }
        public TimeSpan RegulationPeriodLength { get; set; }
        public TimeSpan ExtraPeriodLength { get; set; }
    }
}
