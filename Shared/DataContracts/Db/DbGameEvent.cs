﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts.Db
{
    public class DbGameEvent : DbBase
    {
        public Guid GameId { get; set; }
        public Guid TeamId { get; set; }
        public TimeSpan StartGameTime { get; set; }
        public TimeSpan EndGameTime { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
