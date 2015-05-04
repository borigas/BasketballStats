using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.DataContracts.Interfaces;

namespace BasketballStats.Shared.DataContracts
{
    public class PlayerGame : IStatsSummary
    {
        public Player Player { get; set; }
        public Game Game { get; set; }

        public Dictionary<Guid, StatSummary> StatSummaries { get; set; }
        public Dictionary<Guid, ShotTypeSummary> ShotTypeSummaries { get; set; }
        public List<Foul> Fouls { get; set; }
    }
}
