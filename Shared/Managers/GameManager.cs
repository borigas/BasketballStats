using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.Common;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;
using BasketballStats.Shared.DataContracts.Db;

namespace BasketballStats.Shared.Managers
{
    public class GameManager : IGameManager
    {
        public Game CreateGame(Season season, Team homeTeam, Team awayTeam, GameSettings gameSettings)
        {
            Game game = new Game()
            {
                Stats = new List<Stat>(),
                Shots = new List<Shot>(),
                Possessions = new List<Possession>(),

                SeasonId = season.Id,
                Season = season,

                GameSettings = gameSettings,
            };

            game.HomeTeam = CreateTeamGame(homeTeam, game);
            game.AwayTeam = CreateTeamGame(awayTeam, game);

            game.GameClock = new GameClock()
            {
                IsClockRunning = false,
                EllapsedPeriodTimeAtLastClockStop = TimeSpan.Zero,
                LastClockStartTime = Settings.CurrentTime,
            };

            return game;
        }

        private TeamGame CreateTeamGame(Team team, Game game)
        {
            var teamGame = new TeamGame()
            {
                Game = game,
                Team = team,
                Lineups = new List<Lineup>(),
                Fouls = new List<Foul>(),
                ShotTypeSummaries = new List<ShotTypeSummary>(),
                StatSummary = new List<StatSummary>(),
            };

            teamGame.Players = team.Players.Select(player => new PlayerGame()
            {
                Fouls = new List<Foul>(),
                Game = game,
                Player = player,
                ShotTypeSummaries = new List<ShotTypeSummary>(),
                StatSummary = new List<StatSummary>()
            }).ToList();

            return teamGame;
        }

        public Lineup AssignLineup(TeamGame teamGame, List<Player> players)
        {
            const int playersInAFullLineup = 5;
            if (players.Count > playersInAFullLineup)
            {
                throw new ArgumentException("players");
            }

            DateTime now = Settings.CurrentTime;
            TimeSpan gameTime = GetGameTime(teamGame.Game).TotalEllapsedTime;

            Lineup lineup = new Lineup()
            {
                Game = teamGame.Game,
                Players = players,
                Team = teamGame.Team,
                StartDateTime = now,
                StartGameTime = gameTime,
                EndDateTime = now,
                EndGameTime = gameTime,
            };

            Lineup oldLineup = teamGame.Lineups.LastOrDefault();
            if (oldLineup != null)
            {
                oldLineup.EndDateTime = now;
                oldLineup.EndGameTime = gameTime;
            }

            teamGame.Lineups.Add(lineup);
            return lineup;
        }

        private T CreateGameEvent<T>(TeamGame teamGame) where T : DbGameEvent, new()
        {
            DateTime now = Settings.CurrentTime;
            throw new Exception();
            TimeSpan gameTime = TimeSpan.Zero;
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
                StartGameTime = gameTime,
                EndGameTime = gameTime,

            };
            return gameEvent;
        }

        public StatResult<Stat> AddStat(TeamGame game, Player player, string statName)
        {
            Stat stat = new Stat()
            {

            };
            throw new NotImplementedException();
        }

        public StatResult<Stat> AddDependentStat(TeamGame game, Player player, string statName)
        {
            throw new NotImplementedException();
        }

        public StatResult<Shot> AddShot(TeamGame game, Shot shot)
        {
            throw new NotImplementedException();
        }

        public void StartClock(Game game)
        {
            DateTime now = Settings.CurrentTime;

            game.GameClock.IsClockRunning = true;
            game.GameClock.LastClockStartTime = now;
        }

        public void StopClock(Game game)
        {
            var gameTime = GetGameTime(game);
            game.GameClock.EllapsedPeriodTimeAtLastClockStop = gameTime.PeriodEllapsedTime;
            game.GameClock.IsClockRunning = false;
        }

        [Obsolete("Move clock functions to a GameClockEngine")]
        public GameTime GetGameTime(Game game)
        {
            bool isClockRunning = game.GameClock.IsClockRunning;
            TimeSpan periodEllapsedTime = game.GameClock.EllapsedPeriodTimeAtLastClockStop;
            if (isClockRunning)
            {
                var runningTime = Settings.CurrentTime - game.GameClock.LastClockStartTime;
                periodEllapsedTime += runningTime;
            }

            TimeSpan periodLength = GetCurrentPeriodLength(game);
            if (periodEllapsedTime > periodLength)
            {
                // The period clock has expired. Stop the clock
                isClockRunning = false;
                periodEllapsedTime = periodLength;
            }

            TimeSpan totalEllapsedTime = periodEllapsedTime;
            for (int previousPeriodIndex = 0; previousPeriodIndex < game.GameClock.PeriodIndex; previousPeriodIndex++)
            {
                totalEllapsedTime += GetPeriodLength(game, previousPeriodIndex);
            }

            TimeSpan periodRemainingTime = periodLength - periodEllapsedTime;
            int periodIndex = game.GameClock.PeriodIndex;
            string periodName = GetPeriodName(game);
            bool isRegulationPeriod = IsRegulationPeriod(game, periodIndex);
            return new GameTime()
            {
                IsClockRunning = isClockRunning,
                IsRegulationPeriod = isRegulationPeriod,
                PeriodEllapsedTime = periodEllapsedTime,
                PeriodIndex = periodIndex,
                PeriodName = periodName,
                PeriodRemainingTime = periodRemainingTime,
                TotalEllapsedTime = totalEllapsedTime,
            };
        }

        private string GetPeriodName(Game game)
        {
            int periodIndex = game.GameClock.PeriodIndex;
            int periodCount = periodIndex + 1;
            bool isRegulationPeriod = IsRegulationPeriod(game, periodIndex);

            if (isRegulationPeriod)
            {
                string[] specialRegulationPeriodNames = new string[] { "1st", "2nd", "3rd" };
                if (periodIndex < specialRegulationPeriodNames.Length)
                {
                    return specialRegulationPeriodNames[periodIndex];
                }
                else
                {
                    return periodCount.ToString() + "th";
                }
            }
            else
            {
                int extraPeriodCount = periodCount - game.GameSettings.RegulationPeriodsInGame;
                string overtimeCounterPrefix = string.Empty;
                if (extraPeriodCount > 1)
                {
                    overtimeCounterPrefix = extraPeriodCount.ToString();
                }
                return overtimeCounterPrefix + "OT";
            }
        }

        private bool IsRegulationPeriod(Game game, int periodIndex)
        {
            int periodCount = periodIndex + 1;
            int regulationPeriods = game.GameSettings.RegulationPeriodsInGame;
            return periodCount <= regulationPeriods;
        }

        private TimeSpan GetCurrentPeriodLength(Game game)
        {
            return GetPeriodLength(game, game.GameClock.PeriodIndex);
        }

        private TimeSpan GetPeriodLength(Game game, int periodIndex)
        {
            bool isRegulationPeriod = IsRegulationPeriod(game, periodIndex);
            if (isRegulationPeriod)
            {
                return game.GameSettings.RegulationPeriodLength;
            }
            else
            {
                return game.GameSettings.ExtraPeriodLength;
            }
        }

        public void AdvancePeriod(Game game)
        {
            game.GameClock.IsClockRunning = false;
            game.GameClock.EllapsedPeriodTimeAtLastClockStop = TimeSpan.Zero;
            game.GameClock.PeriodIndex++;
        }

        public void SetEllapsedTime(Game game, int periodIndex, TimeSpan timeSpan)
        {
            game.GameClock.LastClockStartTime = Settings.CurrentTime;
            game.GameClock.EllapsedPeriodTimeAtLastClockStop = timeSpan;
            game.GameClock.PeriodIndex = periodIndex;
        }
    }
}
