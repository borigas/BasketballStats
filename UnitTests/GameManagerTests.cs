using System;
using System.Collections.Generic;
using System.Linq;
using BasketballStats.Shared.Common;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;
using BasketballStats.Shared.DataContracts.Exceptions;
using BasketballStats.Shared.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class GameManagerTests : GameTestsBase
    {
        [TestInitialize]
        public override void TestInit()
        {
            base.TestInit();
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
            Assert.AreEqual(TimeSpan.Zero, _game.GameClock.EllapsedPeriodTimeAtLastClockStop);
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
            // Assign 1st lineups
            _gameManager.AssignLineup(_game.HomeTeam, _homeTeam.Players.GetRange(0, 5));
            Assert.AreEqual(1, _game.HomeTeam.Lineups.Count, "Home team should have a lineup");

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
            TimeSpan timeToRun = TimeSpan.FromMinutes(1);
            DateTime now2 = Settings.CurrentTime.Add(timeToRun);
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
            Assert.AreEqual(timeToRun, lineup2.StartGameTime);

            Assert.AreEqual(lineup2.StartGameTime, lineup1.EndGameTime);
            Assert.AreEqual(lineup2.StartDateTime, lineup1.EndDateTime);

            Assert.IsNotNull(_game.AwayTeam.Lineups);
            Assert.AreEqual(2, _game.AwayTeam.Lineups.Count);
            Assert.AreEqual(lineup2, _game.AwayTeam.Lineups.Last());

            Assert.AreEqual(1, _game.HomeTeam.Lineups.Count, "Home team lineups should not be affected by away team");
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

        [ExpectedException(typeof(LineupNotAssignedException))]
        [TestMethod]
        public void GameManager_StartClockWithoutLineup()
        {
            _gameManager.StartClock(_game);
        }
    }
}
