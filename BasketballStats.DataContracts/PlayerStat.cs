﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.DataContracts
{
    public class PlayerStat : Stat
    {
        public Guid PlayerId { get; set; }
    }
}
