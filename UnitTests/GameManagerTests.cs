using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;
using BasketballStats.Shared.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class GameManagerTests
    {
        const int MS_TO_SLEEP = 50;
        const int MS_ALLOWABLE_DIFF = 10;

        IGameManager _gameManager = null;

        Team _homeTeam = null;
        Team _awayTeam = null;
        Season _season = null;
        GameSettings _gameSettings = null;
        Game _game = null;

        [TestInitialize]
        public void TestInit()
        {
            _gameManager = new GameManager();

            _homeTeam = new Team()
            {
                Name = "Home Team",
                Players = Enumerable.Range(0, 10).Select(i => new Player()
                {
                    Number = i,
                    FirstName = "Player " + i,
                    LastName = "Home " + i,
                }).ToList(),
            };
            _awayTeam = new Team()
            {
                Name = "Away Team",
                Players = Enumerable.Range(0, 10).Select(i => new Player()
                {
                    Number = i,
                    FirstName = "Player " + i,
                    LastName = "Away " + i,
                }).ToList(),
            };
            _season = new Season()
            {
                Name = "Test Season",
            };
            _gameSettings = new GameSettings()
            {
                PeriodLengthInMinutes = 8,
                PeriodsInGame = 4,
            };
            _game = _gameManager.CreateGame(_season, _homeTeam, _awayTeam, _gameSettings);
        }

        [TestMethod]
        public void GameManager_CreateGame()
        {
            Assert.IsNotNull(_game);

            Assert.AreEqual(_homeTeam.Id, _game.HomeTeamId);
            InspectTeamGame(_game, _game.HomeTeam, _homeTeam);
            Assert.AreEqual(_awayTeam.Id, _game.AwayTeamId);
            InspectTeamGame(_game, _game.AwayTeam, _awayTeam);

            Assert.AreEqual(_season, _game.Season);
            Assert.AreEqual(_season.Id, _game.SeasonId);

            Assert.IsNotNull(_game.Possessions);
            Assert.IsNotNull(_game.Shots);
            Assert.IsNotNull(_game.Stats);

            Assert.IsNotNull(_game.GameSettings);
            Assert.AreEqual(_gameSettings, _game.GameSettings);

            Assert.IsNotNull(_game.GameClock);
            Assert.AreEqual(false, _game.GameClock.IsClockRunning);
            Assert.AreEqual(TimeSpan.Zero, _game.GameClock.EllapsedTimeAtLastClockStop);
        }

        private void InspectTeamGame(Game game, TeamGame teamGame, Team team)
        {
            Assert.IsNotNull(teamGame);
            Assert.AreEqual(team, teamGame.Team);
            Assert.AreEqual(game, teamGame.Game);

            Assert.IsNotNull(teamGame.Players);
            Assert.AreEqual(team.Players.Count, teamGame.Players.Count);
            for (var i = 0; i < team.Players.Count; i++)
            {
                Assert.IsNotNull(teamGame.Players[i].StatSummary);
                Assert.IsNotNull(teamGame.Players[i].ShotTypeSummaries);
                Assert.IsNotNull(teamGame.Players[i].Fouls);

                Assert.AreEqual(team.Players[i], teamGame.Players[i].Player);
            }

            Assert.IsNotNull(teamGame.Fouls);
            Assert.IsNotNull(teamGame.Lineups);
            Assert.IsNotNull(teamGame.ShotTypeSummaries);
            Assert.IsNotNull(teamGame.StatSummary);
        }

        [TestMethod]
        public void GameManager_AssignLineup()
        {
            // Assign 1st lineup
            var players = _awayTeam.Players.GetRange(2, 5);
            Lineup lineup = _gameManager.AssignLineup(_game.AwayTeam, players);

            Assert.IsNotNull(lineup);
            Assert.IsTrue(players.SequenceEqual(lineup.Players));
            Assert.AreEqual(_awayTeam, lineup.Team);
            Assert.AreEqual(_game, lineup.Game);

            Assert.IsNotNull(_game.AwayTeam.Lineups);
            Assert.AreEqual(1, _game.AwayTeam.Lineups.Count);
            Assert.AreEqual(lineup, _game.AwayTeam.Lineups.Last());

            // Assign 2nd lineup
            players = _awayTeam.Players.GetRange(0, 5);
            lineup = _gameManager.AssignLineup(_game.AwayTeam, players);

            Assert.IsNotNull(lineup);
            Assert.IsTrue(players.SequenceEqual(lineup.Players));
            Assert.AreEqual(_awayTeam, lineup.Team);

            Assert.IsNotNull(_game.AwayTeam.Lineups);
            Assert.AreEqual(2, _game.AwayTeam.Lineups.Count);
            Assert.AreEqual(lineup, _game.AwayTeam.Lineups.Last());

            Assert.AreEqual(0, _game.HomeTeam.Lineups.Count, "Home team lineups should not be affected by away team");
        }

        [TestMethod]
        public void GameManager_AssignLineup_TooFewPlayers()
        {
            // Too few players is allowed. Sometimes teams run out of players

            var players = _game.AwayTeam.Team.Players.GetRange(0, 4);
            Lineup lineup = _gameManager.AssignLineup(_game.AwayTeam, players);

            Assert.IsNotNull(lineup);
            Assert.IsTrue(players.SequenceEqual(lineup.Players));
            Assert.AreEqual(_awayTeam, lineup.Team);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void GameManager_AssignLineup_TooManyPlayers()
        {
            var players = _game.AwayTeam.Team.Players.GetRange(0, 6);
            Lineup lineup = _gameManager.AssignLineup(_game.AwayTeam, players);
        }

        [TestMethod]
        public void GameManager_StartStopTime()
        {
            Assert.AreEqual(TimeSpan.Zero, _gameManager.GetEllapsedTime(_game));

            _gameManager.StartClock(_game);

            Thread.Sleep(MS_TO_SLEEP);

            TimeSpan ellapsedTime1 = _gameManager.GetEllapsedTime(_game);
            Assert.AreEqual(MS_TO_SLEEP, ellapsedTime1.TotalMilliseconds, MS_ALLOWABLE_DIFF);

            Thread.Sleep(MS_TO_SLEEP);

            TimeSpan ellapsedTime2 = _gameManager.GetEllapsedTime(_game);
            Assert.AreEqual(MS_TO_SLEEP + ellapsedTime1.TotalMilliseconds, ellapsedTime2.TotalMilliseconds, MS_ALLOWABLE_DIFF);

            _gameManager.StopClock(_game);

            Thread.Sleep(MS_TO_SLEEP);

            TimeSpan ellapsedTime3 = _gameManager.GetEllapsedTime(_game);
            Assert.AreEqual(ellapsedTime2.TotalMilliseconds, ellapsedTime3.TotalMilliseconds, MS_ALLOWABLE_DIFF);
        }

        [TestMethod]
        public void GameManager_SetTime()
        {
            TimeSpan timeToSet = TimeSpan.FromMinutes(1);
            _gameManager.SetEllapsedTime(_game, timeToSet);

            Thread.Sleep(MS_TO_SLEEP);

            Assert.AreEqual(timeToSet, _gameManager.GetEllapsedTime(_game));

            _gameManager.StartClock(_game);

            Thread.Sleep(MS_TO_SLEEP);

            Assert.AreEqual(timeToSet.TotalMilliseconds + MS_TO_SLEEP, _gameManager.GetEllapsedTime(_game).TotalMilliseconds, MS_ALLOWABLE_DIFF);

            _gameManager.SetEllapsedTime(_game, timeToSet);

            Assert.AreEqual(timeToSet.TotalMilliseconds, _gameManager.GetEllapsedTime(_game).TotalMilliseconds, MS_ALLOWABLE_DIFF);

            Thread.Sleep(2 * MS_TO_SLEEP);

            Assert.AreEqual(timeToSet.TotalMilliseconds + 2 * MS_TO_SLEEP, _gameManager.GetEllapsedTime(_game).TotalMilliseconds, MS_ALLOWABLE_DIFF);
        }
    }
}
