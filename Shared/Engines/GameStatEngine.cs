using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.Common;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;
using BasketballStats.Shared.DataContracts.Db;

namespace BasketballStats.Shared.Engines
{
    public class GameStatEngine : IGameStatEngine
    {
        private T CreateGameEvent<T>(TeamGame teamGame, GameTime gameTime) where T : DbGameEvent, new()
        {
            DateTime now = Settings.CurrentTime;

            T gameEvent = new T()
            {
                Id = Guid.NewGuid(),
                GameId = teamGame.Game.Id,
                TeamId = teamGame.Team.Id,
                CreatedAt = now,
                CreatedBy = Settings.CurrentUser,
                UpdatedAt = now,
                UpdatedBy = Settings.CurrentUser,
                StartDateTime = now,
                EndDateTime = now,
                StartGameTime = gameTime.TotalEllapsedTime,
                EndGameTime = gameTime.TotalEllapsedTime,
            };
            return gameEvent;
        }

        public StatResult<Stat> AddStat(TeamGame teamGame, Player player, GameTime gameTime, StatType statType)
        {
            Stat stat = CreateGameEvent<Stat>(teamGame, gameTime);

            stat.StatName = statType.StatName;

            // Id for Team and Game were already set by CreateGameEvent
            stat.Team = teamGame.Team;
            stat.Game = teamGame.Game;


            stat.Lineup = teamGame.Lineups.Last();
            stat.LineupId = stat.Lineup.Id;
            
            stat.Player = player;
            if (stat.Player != null)
            {
                stat.PlayerId = stat.Player.Id;
            }

            stat.Possession = teamGame.Game.Possessions.LastOrDefault();
            if (stat.Possession != null)
            {
                stat.PossessionId = stat.Possession.Id;
            }


            StatResult<Stat> result = new StatResult<Stat>()
            {
                 DependentStats = statType.DependentStats,
                 Stat = stat,
            };
            return result;
        }

        public StatResult<Stat> AddDependentStat(Stat parentStat, TeamGame game, Player player, GameTime gameTime, StatType statType)
        {
            throw new NotImplementedException();
        }

        public StatResult<Shot> AddShot(TeamGame game, Shot shot, GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
