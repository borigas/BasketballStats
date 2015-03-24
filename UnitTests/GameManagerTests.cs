using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        [TestMethod]
        public void GameManager_CreateGame()
        {
            Game game = _gameManager.CreateGame(_season, _homeTeam, _awayTeam);

            Assert.IsNotNull(game);

            Assert.AreEqual(_homeTeam.Id, game.HomeTeamId);
            InspectTeamGame(game, game.HomeTeam, _homeTeam);
            Assert.AreEqual(_awayTeam.Id, game.AwayTeamId);
            InspectTeamGame(game, game.AwayTeam, _awayTeam);

            Assert.AreEqual(_season, game.Season);
            Assert.AreEqual(_season.Id, game.SeasonId);

            Assert.IsNotNull(game.Possessions);
            Assert.IsNotNull(game.Shots);
            Assert.IsNotNull(game.Stats);
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
        }
    }
}
