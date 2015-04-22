using System;
using System.Collections.Generic;
using System.Linq;
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
                RegulationPeriodLength = TimeSpan.FromMinutes(8),
                ExtraPeriodLength = TimeSpan.FromMinutes(4),
                RegulationPeriodsInGame = 4,
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
            string[] periodNames = new string[] { "1st", "2nd", "3rd", "4th", "OT", "2OT", "3OT" };
            bool[] isRegulationPeriods = new bool[] { true, true, true, true, false, false, false };

            for (var periodIndex = 0; periodIndex < periodNames.Length; periodIndex++)
            {
                string periodName = periodNames[periodIndex];
                bool isRegulationPeriod = isRegulationPeriods[periodIndex];
                TimeSpan currentPeriodLength = isRegulationPeriod ? _gameSettings.RegulationPeriodLength : _game.GameSettings.ExtraPeriodLength;
                TimeSpan timeEllapsedBeforePeriod = TimeSpan.Zero;
                for (var j = 0; j < periodIndex; j++)
                {
                    timeEllapsedBeforePeriod += isRegulationPeriods[j] ? _game.GameSettings.RegulationPeriodLength : _game.GameSettings.ExtraPeriodLength;
                }

                if (periodIndex > 0)
                {
                    // Move into the current period
                    _gameManager.AdvancePeriod(_game);
                }

                // Clock hasn't started for the period yet. It is full
                GameTime gameTime = _gameManager.GetGameTime(_game);
                TimeSpan expectedPeriodEllapsedTime = TimeSpan.Zero;
                bool isClockRunning = false;
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);

                // Clock still full. No time has ellapsed. Clock started
                _gameManager.StartClock(_game);
                gameTime = _gameManager.GetGameTime(_game);
                isClockRunning = true;
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);

                // Run the clock down to a minute. Clock still running
                TimeSpan quarterRemainingTime = TimeSpan.FromMinutes(1);
                expectedPeriodEllapsedTime = currentPeriodLength.Subtract(quarterRemainingTime);
                Settings.CurrentTime = Settings.CurrentTime.Add(expectedPeriodEllapsedTime);
                gameTime = _gameManager.GetGameTime(_game);
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);

                // Clock stopped. Time should be same as last check. Clock not running now
                _gameManager.StopClock(_game);
                gameTime = _gameManager.GetGameTime(_game);
                isClockRunning = false;
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);

                // Clock shouldn't move when stopped
                Settings.CurrentTime = Settings.CurrentTime.AddHours(1);
                gameTime = _gameManager.GetGameTime(_game);
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);


                // Start clock and run down to 0
                _gameManager.StartClock(_game);
                Settings.CurrentTime = Settings.CurrentTime.Add(quarterRemainingTime);
                expectedPeriodEllapsedTime = currentPeriodLength;
                isClockRunning = true;
                gameTime = _gameManager.GetGameTime(_game);
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);

                // Clock will not continue past 0 in each quarter
                Settings.CurrentTime = Settings.CurrentTime.AddHours(1);
                gameTime = _gameManager.GetGameTime(_game);
                isClockRunning = false;
            }
        }

        private void CheckGameTime(GameTime gameTime, bool expectedIsClockRunning, TimeSpan expectedPeriodEllapsedTime, GameSettings gameSettings, TimeSpan timeEllapsedBeforePeriod, int expectedPeriodIndex, string expectedPeriodName, bool expectedIsRegulationPeriod)
        {
            Assert.AreEqual(expectedPeriodEllapsedTime, gameTime.PeriodEllapsedTime);

            TimeSpan expectedTotalEllapsedTime = timeEllapsedBeforePeriod + expectedPeriodEllapsedTime;
            Assert.AreEqual(expectedTotalEllapsedTime, gameTime.TotalEllapsedTime);

            TimeSpan periodLength = expectedIsRegulationPeriod ? gameSettings.RegulationPeriodLength : gameSettings.ExtraPeriodLength;
            TimeSpan expectedPeriodRemainingTime = periodLength - expectedPeriodEllapsedTime;
            Assert.AreEqual(expectedPeriodRemainingTime, gameTime.PeriodRemainingTime);

            Assert.AreEqual(expectedPeriodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(expectedPeriodName, gameTime.PeriodName);
            Assert.AreEqual(expectedIsRegulationPeriod, gameTime.IsRegulationPeriod);
            Assert.AreEqual(expectedIsClockRunning, gameTime.IsClockRunning);
        }

        [TestMethod]
        public void GameManager_SetTime()
        {
            int periodIndex = 2;
            TimeSpan timeToSet = TimeSpan.FromMinutes(1);
            _gameManager.SetEllapsedTime(_game, periodIndex, timeToSet);

            // Time is set properly
            GameTime gameTime = _gameManager.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet, gameTime.PeriodEllapsedTime);

            // Clock isn't started by setting the clock
            Settings.CurrentTime = Settings.CurrentTime.AddHours(1);

            gameTime = _gameManager.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet, gameTime.PeriodEllapsedTime);

            // Clock can run after setting time
            _gameManager.StartClock(_game);

            TimeSpan timeToRun = TimeSpan.FromMinutes(1);
            Settings.CurrentTime = Settings.CurrentTime.Add(timeToRun);

            gameTime = _gameManager.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet + timeToRun, gameTime.PeriodEllapsedTime);

            // Clock can be set while running
            timeToSet = TimeSpan.FromMinutes(3);
            _gameManager.SetEllapsedTime(_game, periodIndex, timeToSet);

            gameTime = _gameManager.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet, gameTime.PeriodEllapsedTime);

            // Clock isn't stopped by setting a time
            Settings.CurrentTime = Settings.CurrentTime.Add(timeToRun);
            gameTime = _gameManager.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet + timeToRun, gameTime.PeriodEllapsedTime);

            // Clock can be set backwards
            periodIndex = 0;
            _gameManager.SetEllapsedTime(_game, periodIndex, timeToSet);
            gameTime = _gameManager.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet, gameTime.PeriodEllapsedTime);
        }
    }
}
