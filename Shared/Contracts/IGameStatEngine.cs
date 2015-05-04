using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.DataContracts;
using BasketballStats.Shared.DataContracts.Db;

namespace BasketballStats.Shared.Contracts
{
    public interface IGameStatEngine
    {
        StatResult<Stat> AddStat(TeamGame teamGame, Player player, GameTime gameTime, StatType statType);

        StatResult<Stat> AddDependentStat(Stat parentStat, TeamGame game, Player player, GameTime gameTime, StatType statType);

        StatResult<Shot> AddShot(TeamGame game, Shot shot, GameTime gameTime);

        Possession AssignPossession(Game game, TeamGame teamGame, GameTime gameTime);
    }
}
