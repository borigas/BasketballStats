using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballStats.Shared.DataContracts.Interfaces
{
    public interface IStatSummary
    {
        Dictionary<Guid, StatSummary> StatSummaries { get; set; }
        Dictionary<Guid, ShotTypeSummary> ShotTypeSummaries { get; set; }
        List<Foul> Fouls { get; set; }
    }
}
