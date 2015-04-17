using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BasketballStats.Shared.Common;
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

        IGameManager _gameManager = null;

        Team _homeTeam = null;
        Team _awayTeam = null;
        Season _season = null;
        GameSettings _gameSettings = null;
        Game _game = null;

        [TestInitialize]
        public void TestInit()
        {
            Settings.CurrentTime = DateTime.UtcNow;

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
                PeriodLength = TimeSpan.FromMinutes(8),
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
            DateTime now1 = Settings.CurrentTime;
            Settings.CurrentTime = now1;
            Lineup lineup1 = _gameManager.AssignLineup(_game.AwayTeam, players);

            Assert.IsNotNull(lineup1);
            Assert.IsTrue(players.SequenceEqual(lineup1.Players));
            Assert.AreEqual(_awayTeam, lineup1.Team);
            Assert.AreEqual(_game, lineup1.Game);
            Assert.AreEqual(lineup1.StartDateTime, now1);
            Assert.AreEqual(lineup1.EndDateTime, now1);
            Assert.AreEqual(TimeSpan.Zero, lineup1.StartGameTime);

            Assert.IsNotNull(_game.AwayTeam.Lineups);
            Assert.AreEqual(1, _game.AwayTeam.Lineups.Count);
            Assert.AreEqual(lineup1, _game.AwayTeam.Lineups.Last());

            _gameManager.StartClock(_game);
            DateTime now2 = Settings.CurrentTime.AddMilliseconds(MS_TO_SLEEP);
            Settings.CurrentTime = now2;
            _gameManager.StopClock(_game);

            // Assign 2nd lineup
            players = _awayTeam.Players.GetRange(0, 5);
            Lineup lineup2 = _gameManager.AssignLineup(_game.AwayTeam, players);

            Assert.IsNotNull(lineup2);
            Assert.IsTrue(players.SequenceEqual(lineup2.Players));
            Assert.AreEqual(_awayTeam, lineup2.Team);
            Assert.AreEqual(_game, lineup2.Game);

            Assert.AreEqual(lineup2.StartDateTime, now2);
            Assert.AreEqual(lineup2.EndDateTime, now2);
            Assert.AreEqual(MS_TO_SLEEP, lineup2.StartGameTime.TotalMilliseconds);

            Assert.AreEqual(lineup2.StartGameTime, lineup1.EndGameTime);
            Assert.AreEqual(lineup2.StartDateTime, lineup1.EndDateTime);

            Assert.IsNotNull(_game.AwayTeam.Lineups);
            Assert.AreEqual(2, _game.AwayTeam.Lineups.Count);
            Assert.AreEqual(lineup2, _game.AwayTeam.Lineups.Last());

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

            Settings.CurrentTime = Settings.CurrentTime.AddMilliseconds(MS_TO_SLEEP);

            TimeSpan ellapsedTime1 = _gameManager.GetEllapsedTime(_game);
            Assert.AreEqual(MS_TO_SLEEP, ellapsedTime1.TotalMilliseconds);

            Settings.CurrentTime = Settings.CurrentTime.AddMilliseconds(MS_TO_SLEEP);

            TimeSpan ellapsedTime2 = _gameManager.GetEllapsedTime(_game);
            Assert.AreEqual(MS_TO_SLEEP + ellapsedTime1.TotalMilliseconds, ellapsedTime2.TotalMilliseconds);

            _gameManager.StopClock(_game);

            Settings.CurrentTime = Settings.CurrentTime.AddMilliseconds(MS_TO_SLEEP);

            TimeSpan ellapsedTime3 = _gameManager.GetEllapsedTime(_game);
            Assert.AreEqual(ellapsedTime2, ellapsedTime3);
        }

        [TestMethod]
        public void GameManager_SetTime()
        {
            TimeSpan timeToSet = TimeSpan.FromMinutes(1);
            _gameManager.SetEllapsedTime(_game, timeToSet);

            Settings.CurrentTime = Settings.CurrentTime.AddMilliseconds(MS_TO_SLEEP);

            Assert.AreEqual(timeToSet, _gameManager.GetEllapsedTime(_game));

            _gameManager.StartClock(_game);

            Settings.CurrentTime = Settings.CurrentTime.AddMilliseconds(MS_TO_SLEEP);

            Assert.AreEqual(timeToSet.TotalMilliseconds + MS_TO_SLEEP, _gameManager.GetEllapsedTime(_game).TotalMilliseconds);

            _gameManager.SetEllapsedTime(_game, timeToSet);

            Assert.AreEqual(timeToSet, _gameManager.GetEllapsedTime(_game));

            Settings.CurrentTime = Settings.CurrentTime.AddMilliseconds(2 * MS_TO_SLEEP);

            Assert.AreEqual(timeToSet.TotalMilliseconds + 2 * MS_TO_SLEEP, _gameManager.GetEllapsedTime(_game).TotalMilliseconds);
        }
    }
}
