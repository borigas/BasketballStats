using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasketballStats.Shared.Contracts;
using BasketballStats.Shared.DataContracts;

namespace BasketballStats.Shared.Managers
{
    public class GameManager : IGameManager
    {
        public Game CreateGame(Season season, Team homeTeam, Team awayTeam)
        {
            Game game = new Game()
            {
                Stats = new List<Stat>(),
                Shots = new List<Shot>(),
                Possessions = new List<Possession>(),

                SeasonId = season.Id,
                Season = season,
            };

            game.HomeTeam = CreateTeamGame(homeTeam, game);
            game.AwayTeam = CreateTeamGame(awayTeam, game);

            return game;
        }

        private TeamGame CreateTeamGame(Team team, Game game)
        {
            var teamGame = new TeamGame()
            {
                Game = game,
                Team = team,
                Lineups = new List<Lineup>(),
                Fouls = new List<Foul>(),
                ShotTypeSummaries = new List<ShotTypeSummary>(),
                StatSummary = new List<StatSummary>(),
            };

            teamGame.Players = team.Players.Select(player => new PlayerGame()
            {
                Fouls = new List<Foul>(),
                Game = game,
                Player = player,
                ShotTypeSummaries = new List<ShotTypeSummary>(),
                StatSummary = new List<StatSummary>()
            }).ToList();

            return teamGame;
        }

        public Lineup AssignLineup(TeamGame teamGame, List<Player> players)
        {
            throw new NotImplementedException();
        }

        public StatResult<Stat> AddStat(TeamGame game, Player player, string statName)
        {
            throw new NotImplementedException();
        }

        public StatResult<Stat> AddDependentStat(TeamGame game, Player player, string statName)
        {
            throw new NotImplementedException();
        }

        public StatResult<Shot> AddShot(TeamGame game, Shot shot)
        {
            throw new NotImplementedException();
        }
    }
}
