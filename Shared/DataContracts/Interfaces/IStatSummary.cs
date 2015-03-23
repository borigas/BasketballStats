using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts.Interfaces
{
    public interface IStatSummary
    {
        List<StatSummary> StatSummary { get; set; }
        List<ShotTypeSummary> ShotTypeSummaries { get; set; }
        List<Foul> Fouls { get; set; }
    }
}
