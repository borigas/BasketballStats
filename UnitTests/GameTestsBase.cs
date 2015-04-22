using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.Common;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;
using BasketballStats.Shared.Managers;

namespace UnitTests
{
    public class GameTestsBase
    {
        protected IGameManager _gameManager = null;

        protected Team _homeTeam = null;
        protected Team _awayTeam = null;
        protected Season _season = null;
        protected GameSettings _gameSettings = null;
        protected Game _game = null;

        public virtual void TestInit()
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
    }
}
