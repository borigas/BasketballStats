using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.Common;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;

namespace BasketballStats.Shared.Engines
{
    public class GameClockEngine : IGameClockEngine
    {
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
