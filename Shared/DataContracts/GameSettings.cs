﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts
{
    public class GameSettings
    {
        public int PeriodsInGame { get; set; }
        public TimeSpan PeriodLength { get; set; }
        public TimeSpan ExtraPeriodLength { get; set; }
    }
}