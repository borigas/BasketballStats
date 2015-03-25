﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.DataContracts.Interfaces;

namespace BasketballStats.Shared.DataContracts
{
    public class PlayerGame : IStatSummary
    {
        public Player Player { get; set; }
        public Game Game { get; set; }

        public List<StatSummary> StatSummary { get; set; }
        public List<ShotTypeSummary> ShotTypeSummaries { get; set; }
        public List<Foul> Fouls { get; set; }
    }
}