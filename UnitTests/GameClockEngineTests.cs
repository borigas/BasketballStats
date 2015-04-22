using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.Common;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;
using BasketballStats.Shared.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class GameClockEngineTests : GameTestsBase
    {
        IGameClockEngine _gameClockEngine = null;

        [TestInitialize]
        public override void TestInit()
        {
            base.TestInit();

            _gameClockEngine = new GameClockEngine();
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
                    _gameClockEngine.AdvancePeriod(_game);
                }

                // Clock hasn't started for the period yet. It is full
                GameTime gameTime = _gameClockEngine.GetGameTime(_game);
                TimeSpan expectedPeriodEllapsedTime = TimeSpan.Zero;
                bool isClockRunning = false;
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);

                // Clock still full. No time has ellapsed. Clock started
                _gameClockEngine.StartClock(_game);
                gameTime = _gameClockEngine.GetGameTime(_game);
                isClockRunning = true;
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);

                // Run the clock down to a minute. Clock still running
                TimeSpan quarterRemainingTime = TimeSpan.FromMinutes(1);
                expectedPeriodEllapsedTime = currentPeriodLength.Subtract(quarterRemainingTime);
                Settings.CurrentTime = Settings.CurrentTime.Add(expectedPeriodEllapsedTime);
                gameTime = _gameClockEngine.GetGameTime(_game);
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);

                // Clock stopped. Time should be same as last check. Clock not running now
                _gameClockEngine.StopClock(_game);
                gameTime = _gameClockEngine.GetGameTime(_game);
                isClockRunning = false;
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);

                // Clock shouldn't move when stopped
                Settings.CurrentTime = Settings.CurrentTime.AddHours(1);
                gameTime = _gameClockEngine.GetGameTime(_game);
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);


                // Start clock and run down to 0
                _gameClockEngine.StartClock(_game);
                Settings.CurrentTime = Settings.CurrentTime.Add(quarterRemainingTime);
                expectedPeriodEllapsedTime = currentPeriodLength;
                isClockRunning = true;
                gameTime = _gameClockEngine.GetGameTime(_game);
                CheckGameTime(gameTime, isClockRunning, expectedPeriodEllapsedTime, _game.GameSettings, timeEllapsedBeforePeriod, periodIndex, periodName, isRegulationPeriod);

                // Clock will not continue past 0 in each quarter
                Settings.CurrentTime = Settings.CurrentTime.AddHours(1);
                gameTime = _gameClockEngine.GetGameTime(_game);
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
            _gameClockEngine.SetEllapsedTime(_game, periodIndex, timeToSet);

            // Time is set properly
            GameTime gameTime = _gameClockEngine.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet, gameTime.PeriodEllapsedTime);

            // Clock isn't started by setting the clock
            Settings.CurrentTime = Settings.CurrentTime.AddHours(1);

            gameTime = _gameClockEngine.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet, gameTime.PeriodEllapsedTime);

            // Clock can run after setting time
            _gameClockEngine.StartClock(_game);

            TimeSpan timeToRun = TimeSpan.FromMinutes(1);
            Settings.CurrentTime = Settings.CurrentTime.Add(timeToRun);

            gameTime = _gameClockEngine.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet + timeToRun, gameTime.PeriodEllapsedTime);

            // Clock can be set while running
            timeToSet = TimeSpan.FromMinutes(3);
            _gameClockEngine.SetEllapsedTime(_game, periodIndex, timeToSet);

            gameTime = _gameClockEngine.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet, gameTime.PeriodEllapsedTime);

            // Clock isn't stopped by setting a time
            Settings.CurrentTime = Settings.CurrentTime.Add(timeToRun);
            gameTime = _gameClockEngine.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet + timeToRun, gameTime.PeriodEllapsedTime);

            // Clock can be set backwards
            periodIndex = 0;
            _gameClockEngine.SetEllapsedTime(_game, periodIndex, timeToSet);
            gameTime = _gameClockEngine.GetGameTime(_game);
            Assert.AreEqual(periodIndex, gameTime.PeriodIndex);
            Assert.AreEqual(timeToSet, gameTime.PeriodEllapsedTime);
        }
    }
}
