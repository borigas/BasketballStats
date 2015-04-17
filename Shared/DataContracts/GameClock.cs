using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts
{
    public class GameClock
    {
        public bool IsClockRunning { get; set; }
        public TimeSpan EllapsedTimeAtLastClockStop{ get; set; }
        public DateTime LastClockStartTime { get; set; }
        public int PeriodIndex { get; set; }
    }
}
