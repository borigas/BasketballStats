using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts
{
    public class GameTime
    {
        public TimeSpan TotalEllapsedTime { get; set; }
        public TimeSpan PeriodEllapsedTime { get; set; }
        public TimeSpan PeriodRemainingTime { get; set; }
        public int PeriodIndex { get; set; }
        public bool IsRegulationPeriod { get; set; }
        public string PeriodName { get; set; }
        public bool IsClockRunning { get; set; }
    }
}
