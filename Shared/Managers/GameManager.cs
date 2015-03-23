using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;

namespace BasketballStats.Shared.Managers
{
    public class GameManager : IGameManager
    {
        public StatResult<Stat> AddStat(Game game, Player player, string statName)
        {
            throw new NotImplementedException();
        }

        public StatResult<Stat> AddDependentStat(Game game, Player player, string statName)
        {
            throw new NotImplementedException();
        }

        public StatResult<Shot> AddShot(Game game, Shot shot)
        {
            throw new NotImplementedException();
        }
    }
}
