using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.DataContracts.Interfaces;

namespace BasketballStats.Shared.DataContracts
{
    public class TeamGame : IStatSummary
    {
        public Team Team { get; set; }
        public Game Game { get; set; }
        public List<Lineup> Lineups { get; set; }
        public List<PlayerGame> Players { get; set; }

        public List<StatSummary> StatSummary { get; set; }
        public List<ShotTypeSummary> ShotTypeSummaries { get; set; }
        public List<Foul> Fouls { get; set; }
    }
}
