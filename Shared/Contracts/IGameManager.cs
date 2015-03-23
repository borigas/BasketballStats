using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.DataContracts;

namespace BasketballStats.Shared.Contracts
{
    public interface IGameManager
    {
        StatResult<Stat> AddStat(Game game, Player player, string statName);

        StatResult<Stat> AddDependentStat(Game game, Player player, string statName);

        StatResult<Shot> AddShot(Game game, Shot shot);
    }
}
