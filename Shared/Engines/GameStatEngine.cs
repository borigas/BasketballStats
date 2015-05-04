using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.Common;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;
using BasketballStats.Shared.DataContracts.Db;
using BasketballStats.Shared.DataContracts.Exceptions;
using BasketballStats.Shared.DataContracts.Interfaces;

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

        private void AddStatSummary(IStatsSummary statsSummary, Stat stat)
        {
            StatSummary currentStatSummary;
            if (!statsSummary.StatSummaries.TryGetValue(stat.StatTypeId, out currentStatSummary))
            {
                currentStatSummary = new StatSummary()
                {
                    StatTypeId = stat.StatTypeId,
                    StatName = stat.StatName,
                    StatCount = 0,
                };
                statsSummary.StatSummaries[currentStatSummary.StatTypeId] = currentStatSummary;
            }
            currentStatSummary.StatCount++;
        }

        public StatResult<Stat> AddStat(TeamGame teamGame, Player player, GameTime gameTime, StatType statType)
        {
            Stat stat = CreateGameEvent<Stat>(teamGame, gameTime);

            // Create the stat
            stat.StatTypeId = statType.Id;
            stat.StatName = statType.StatName;
            // Id for Team and Game were already set by CreateGameEvent
            stat.Team = teamGame.Team;
            stat.Game = teamGame.Game;
            // Lineup has to exist
            stat.Lineup = teamGame.Lineups.Last();
            stat.LineupId = stat.Lineup.Id;
            // Player could be null for Team stats
            stat.Player = player;
            if (stat.Player != null)
            {
                stat.PlayerId = stat.Player.Id;
            }
            // Possession could be null if it happens before a team estiblishes possession
            stat.Possession = teamGame.Game.Possessions.LastOrDefault();
            if (stat.Possession != null)
            {
                stat.PossessionId = stat.Possession.Id;
            }

            AddStatSummary(teamGame, stat);

            PlayerGame playerGame = teamGame.Players.FirstOrDefault(teamPlayer => teamPlayer.Player == player);
            if (playerGame == null)
            {
                throw new PlayerGameNotFoundException();
            }
            AddStatSummary(playerGame, stat);

            if (statType.WillEndPossession)
            {
                ChangePossession(teamGame.Game, gameTime);
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

        public Possession ChangePossession(Game game, GameTime gameTime)
        {
            if (!game.Possessions.Any())
            {
                throw new PossessionNotAssignedException();
            }
            Possession currentPossession = game.Possessions.Last();

            TeamGame newPossessionTeam;
            if (currentPossession.TeamId == game.HomeTeamId)
            {
                newPossessionTeam = game.AwayTeam;
            }
            else
            {
                newPossessionTeam = game.HomeTeam;
            }

            return AssignPossession(game, newPossessionTeam, gameTime);
        }

        public Possession AssignPossession(Game game, TeamGame teamGame, GameTime gameTime)
        {
            Possession possession = CreateGameEvent<Possession>(teamGame, gameTime);

            Possession lastPossession = game.Possessions.LastOrDefault();
            if (lastPossession != null)
            {
                lastPossession.EndDateTime = possession.StartDateTime;
                lastPossession.EndGameTime = possession.StartGameTime;
            }

            game.Possessions.Add(possession);

            return possession;
        }
    }
}
