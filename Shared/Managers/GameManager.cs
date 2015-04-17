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
                EllapsedTimeAtLastClockStop = TimeSpan.Zero,
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
            TimeSpan gameTime=GetEllapsedTime(teamGame.Game);

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
            game.GameClock.EllapsedTimeAtLastClockStop = GetEllapsedTime(game);
            game.GameClock.IsClockRunning = false;
        }

        public TimeSpan GetEllapsedTime(Game game)
        {
            TimeSpan ellapsedTime = game.GameClock.EllapsedTimeAtLastClockStop;
            if (game.GameClock.IsClockRunning)
            {
                //int lastStopPeriod = 
                    //GetPeriodNumber(game.GameClock.EllapsedTimeAtLastClockStop, game.GameSettings);

                //throw new NotImplementedException();
                //TimeSpan maxEllapsedTime = (lastStopPeriod + 1);

                var runningTime = Settings.CurrentTime - game.GameClock.LastClockStartTime;
                ellapsedTime += runningTime;
            }
            return ellapsedTime;
        }

        public void SetEllapsedTime(Game game, TimeSpan timeSpan)
        {
            game.GameClock.LastClockStartTime = Settings.CurrentTime;
            game.GameClock.EllapsedTimeAtLastClockStop = timeSpan;
        }

        public int GetPeriodNumber(TimeSpan ellapsedTime, GameSettings gameSettings){
            double period = ellapsedTime.TotalMilliseconds / gameSettings.PeriodLength.TotalMilliseconds;
            return (int)period;
        }
    }
}
