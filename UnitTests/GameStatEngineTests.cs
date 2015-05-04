using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.Common;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;
using BasketballStats.Shared.DataContracts.Db;
using BasketballStats.Shared.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class GameStatEngineTests : GameTestsBase
    {
        IGameStatEngine _gameStatEngine = null;

        [TestInitialize]
        public override void TestInit()
        {
            base.TestInit();

            _gameManager.AssignLineup(_game.HomeTeam, _homeTeam.Players.Take(5).ToList());
            _gameManager.AssignLineup(_game.AwayTeam, _awayTeam.Players.Take(5).ToList());

            _gameStatEngine = new GameStatEngine();
        }

        [TestMethod]
        public void GameStatEngine_AddStat()
        {
            Player player = _homeTeam.Players.First();
            GameTime gameTime = new GameTime()
            {
                TotalEllapsedTime = TimeSpan.FromMinutes(1),
            };
            StatType statType = new StatType()
            {
                Id = Guid.NewGuid(),
                StatName = "Defensive Rebound",
                WillEndPossession = true,
                RequiresParentStat = false,
                DependentStats = new List<StatType>(),
            };
            var possession = new Possession()
            {
                TeamId = _homeTeam.Id,
                GameId = _game.Id,
            };
            _game.Possessions.Add(possession);

            Settings.CurrentTime = Settings.CurrentTime.Add(TimeSpan.FromMinutes(1));

            StatResult<Stat> statResult = _gameStatEngine.AddStat(_game.HomeTeam, player, gameTime, statType);

            Assert.AreEqual(statType.DependentStats, statResult.DependentStats);

            Stat stat = statResult.Stat;
            Assert.AreNotEqual(Guid.Empty, stat.Id);
            Assert.AreEqual(statType.StatName, stat.StatName);
            Assert.AreEqual(statType.Id, stat.StatTypeId);

            // Check that the returned stat was created properly
            Assert.AreEqual(player, stat.Player);
            Assert.AreEqual(_homeTeam, stat.Team);
            Assert.AreEqual(_game, stat.Game);
            Assert.AreEqual(_game.HomeTeam.Lineups.Last(), stat.Lineup);
            Assert.AreEqual(possession, stat.Possession);

            // Check that the player's stats were incremented properly
            PlayerGame playerGame = _game.HomeTeam.Players.FirstOrDefault(plyr => plyr.Player == player);
            Assert.AreEqual(1, playerGame.StatSummaries.Count);
            Assert.AreEqual(1, playerGame.StatSummaries[stat.StatTypeId].StatCount);
            Assert.AreEqual(statType.StatName, playerGame.StatSummaries[stat.StatTypeId].StatName);

            // TODO Check team stats got implemented correctly

            // TODO Check that possession was incremented if necessary
            //      Check that start/end of possession is correct
        }

        // TODO Support for stat before a possession is created
        // TODO Support team stat without a player
        // TODO Support stat with 2 players? Jump ball
        //      Maybe implement as a jump ball with a 2nd jump ball as a dependent stat
    }
}
