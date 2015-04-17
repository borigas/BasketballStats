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
        Game CreateGame(Season season, Team homeTeam, Team awayTeam, GameSettings gameSettings);

        Lineup AssignLineup(TeamGame teamGame, List<Player> players);

        StatResult<Stat> AddStat(TeamGame game, Player player, string statName);

        StatResult<Stat> AddDependentStat(TeamGame game, Player player, string statName);

        StatResult<Shot> AddShot(TeamGame game, Shot shot);

        void StartClock(Game game);

        void StopClock(Game game);

        TimeSpan GetEllapsedTime(Game game);
        GameTime GetGameTime(Game game);

        void SetEllapsedTime(Game game, TimeSpan timeSpan);
        void SetGameTime(Game game, GameTime gameTime);

        void AdvancePeriod(Game game);
    }
}
