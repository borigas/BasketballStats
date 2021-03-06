﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.DataContracts.Db;

namespace BasketballStats.Shared.DataContracts
{
    public class Lineup : DbLineup
    {
        public Game Game { get; set; }
        public Team Team { get; set; }
        public List<Player> Players { get; set; }
    }
}
