using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.Common;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;
using BasketballStats.Shared.DataContracts.Db;
using BasketballStats.Shared.DataContracts.Exceptions;
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

            Settings.CurrentTime = DateTime.Now.Date;
        }

        [TestMethod]
        public void GameStatEngine_AssignPossession()
        {
            GameTime gameTime = _gameManager.GetGameTime(_game);
            var possession = _gameStatEngine.AssignPossession(_game, _game.AwayTeam, gameTime);

            Assert.IsNotNull(_game.Possessions);
            Assert.AreEqual(possession, _game.Possessions.LastOrDefault());
            Assert.AreEqual(_game.AwayTeam.Team.Id, possession.TeamId);
        }

        [TestMethod]
        public void GameStatEngine_AddStat()
        {
            Player player = _homeTeam.Players.First();
            StatType statType = new StatType()
            {
                Id = Guid.NewGuid(),
                StatName = "Defensive Rebound",
                WillEndPossession = true,
                RequiresParentStat = false,
                DependentStats = new List<StatType>(),
            };
            GameTime gameTime = _gameManager.GetGameTime(_game);
            var possession = _gameStatEngine.AssignPossession(_game, _game.AwayTeam, gameTime);

            Settings.CurrentTime = Settings.CurrentTime.Add(gameTime.TotalEllapsedTime);
            gameTime = _gameManager.GetGameTime(_game);

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

            // Check team stats got implemented correctly
            TeamGame teamGame = _game.HomeTeam;
            Assert.AreEqual(1, teamGame.StatSummaries.Count);
            Assert.AreEqual(1, teamGame.StatSummaries[stat.StatTypeId].StatCount);
            Assert.AreEqual(statType.StatName, teamGame.StatSummaries[stat.StatTypeId].StatName);


            // Check that a new possession was added
            Assert.AreEqual(Settings.CurrentTime, possession.EndDateTime);
            Assert.AreEqual(gameTime.TotalEllapsedTime, possession.EndGameTime);
            Assert.AreEqual(2, _game.Possessions.Count);

            Possession nextPossession = _game.Possessions.Last();
            Assert.AreEqual(Settings.CurrentTime, nextPossession.StartDateTime);
            Assert.AreEqual(gameTime.TotalEllapsedTime, nextPossession.StartGameTime);
            Assert.AreEqual(_homeTeam.Id, nextPossession.TeamId);
            Assert.AreEqual(_game.Id, nextPossession.GameId);
        }

        [ExpectedException(typeof(PlayerGameNotFoundException))]
        [TestMethod]
        public void GameStatEngine_AddStat_InvalidPlayer()
        {
            _gameStatEngine.AddStat(_game.HomeTeam, new Player(), new GameTime(), new StatType());
        }

        [ExpectedException(typeof(PlayerGameNotFoundException))]
        [TestMethod]
        public void GameStatEngine_AddStat_PlayerFromWrongTeam()
        {
            _gameStatEngine.AddStat(_game.HomeTeam, _game.AwayTeam.Players.First().Player, new GameTime(), new StatType());
        }

        // TODO Support for stat before a possession is created
        // TODO Support team stat without a player
        // TODO Support stat with 2 players? Ex. Jump ball
        //      Maybe implement as a jump ball with a 2nd jump ball as a dependent stat
        // TODO Support stats only being available to offense/defense
    }
}
