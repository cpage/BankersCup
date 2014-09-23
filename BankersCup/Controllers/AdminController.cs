﻿using BankersCup.DataAccess;
using BankersCup.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace BankersCup.Controllers
{
    public class AdminController : Controller
    {
        
        public async Task<ActionResult> Authorize()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Authorize(string authCode)
        {
            if(authCode == DateTime.Now.ToString("yyyyMMddHHmm"))
            {
                FormsAuthentication.SetAuthCookie("admin", false);
                
            }
            return View();

        }

        public async Task<ActionResult> Index(int id)
        {
            var game = await DocumentDBRepository.GetGameById(id);
            return View(game);
        }

        public async Task<ActionResult> Register(int id, int? teamId)
        {
            var game = await DocumentDBRepository.GetGameById(id);

            Team teamModel = new Team();

            if (teamId == null)
            {
                teamModel.Players = new List<Player>() { new Player() };
            }
            else
            {
                teamModel = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == teamId.Value);
            }

            return View(teamModel);
        }

        [HttpPost]
        public async Task<ActionResult> Register(int id, Team newTeam)
        {
            var game = await DocumentDBRepository.GetGameById(id);
            if (newTeam.TeamId == 0)
            {
                if (game.RegisteredTeams.Count == 0)
                {
                    newTeam.TeamId = 1;
                }
                else
                {
                    newTeam.TeamId = game.RegisteredTeams.Max(t => t.TeamId) + 1;
                }
                newTeam.RegistrationCode = new Random().Next(0, 1000).ToString();
                game.RegisteredTeams.Add(newTeam);
            }
            else
            {
                var currentTeam = game.RegisteredTeams.First(t => t.TeamId == newTeam.TeamId);
                currentTeam.TeamName = newTeam.TeamName;
                currentTeam.StartingHole = newTeam.StartingHole;
                currentTeam.Players = newTeam.Players;
                foreach (var player in currentTeam.Players.Where(p => p.IsRemoved).ToList())
                {
                    currentTeam.Players.Remove(player);
                }
            }

            await DocumentDBRepository.UpdateGame(game);

            return RedirectToAction("Index", new { id = id });
        }

        public async Task<ActionResult> GenerateSampleGame()
        {
            var allGames = await DocumentDBRepository.GetAllGamesAsync(true);


            Game game = new Game();
            if (allGames.Count > 0)
                game.GameId = allGames.Max(g => g.GameId) + 1;
            else
                game.GameId = 1;

            game.DocumentId = game.GameId.ToString();

            game.Name = "Banker's Cup 2014";
            game.GameDate = DateTime.Now.AddDays(7);


            game.GameCourse = new Course() { Name = "Glen Abbey" };

            game.GameCourse.Holes = new List<HoleInfo>()
            {
                new HoleInfo() { HoleNumber = 1, Distance = 460, Par = 5 },
                new HoleInfo() { HoleNumber = 2, Distance = 380, Par = 4 },
                new HoleInfo() { HoleNumber = 3, Distance = 123, Par = 3 },
                new HoleInfo() { HoleNumber = 4, Distance = 345, Par = 4 },
                new HoleInfo() { HoleNumber = 5, Distance = 452, Par = 5 },
                new HoleInfo() { HoleNumber = 6, Distance = 395, Par = 4 },
                new HoleInfo() { HoleNumber = 7, Distance = 135, Par = 3 },
                new HoleInfo() { HoleNumber = 8, Distance = 391, Par = 4 },
                new HoleInfo() { HoleNumber = 9, Distance = 383, Par = 4 },
                new HoleInfo() { HoleNumber = 10, Distance = 369, Par = 4 },
                new HoleInfo() { HoleNumber = 11, Distance = 435, Par = 4 },
                new HoleInfo() { HoleNumber = 12, Distance = 152, Par = 3 },
                new HoleInfo() { HoleNumber = 13, Distance = 481, Par = 5 },
                new HoleInfo() { HoleNumber = 14, Distance = 330, Par = 4 },
                new HoleInfo() { HoleNumber = 15, Distance = 115, Par = 3 },
                new HoleInfo() { HoleNumber = 16, Distance = 452, Par = 5 },
                new HoleInfo() { HoleNumber = 17, Distance = 365, Par = 4 },
                new HoleInfo() { HoleNumber = 18, Distance = 461, Par = 5 },
            };

            game.RegisteredTeams = new List<Team>()
            {
                new Team() {
                    RegistrationCode = "12345",
                    TeamId = 1,
                    TeamName = "Infusion",
                    StartingHole = 1,
                    TeeTime = game.GameDate.Date.AddHours(9),
                    Players = new List<Player>() {
                        new Player() {
                            Company = "Infusion",
                            Email = "bbaldasti@infusion.com",
                            Name = "Bill Baldasti"
                        },
                        new Player() {
                            Company = "Infusion",
                            Email = "sellis@infusion.com",
                            Name = "Steve Ellis"
                        },
                        new Player() {
                            Company = "Infusion",
                            Email = "cpage@infusion.com",
                            Name = "Chris Page"
                        }
                    }
                },
                new Team() {
                    RegistrationCode = "67890",
                    TeamId = 2,
                    TeamName = "Indigo",
                    StartingHole = 10,
                    TeeTime = game.GameDate.Date.AddHours(9).AddMinutes(15),
                    Players = new List<Player>() {
                        new Player() {
                            Company = "Indigo",
                            Email = "bpo@indigo.ca",
                            Name = "Bo P"
                        },
                        new Player() {
                            Company = "Indigo",
                            Email = "mtolley@indigo.ca",
                            Name = "Mark Tolley"
                        },
                        new Player() {
                            Company = "Indigo",
                            Email = "sgoneau@indigo.ca",
                            Name = "Stephen Goneau"
                        }
                    }
                }

            };

            game.Scores = new List<TeamHoleScore>()
            {
                //new TeamHoleScore() {
                //    HoleNumber = 1,
                //    Score = 3,
                //    TeamId = 1
                //},
                //new TeamHoleScore() {
                //    HoleNumber = 10,
                //    Score = 5,
                //    TeamId = 2
                //},
                //new TeamHoleScore() {
                //    HoleNumber = 2,
                //    Score = 4,
                //    TeamId = 1
                //},
                //new TeamHoleScore() {
                //    HoleNumber = 11,
                //    Score = 4,
                //    TeamId = 2
                //},
                //new TeamHoleScore() {
                //    HoleNumber = 3,
                //    Score = 4,
                //    TeamId = 1
                //},
                //new TeamHoleScore() {
                //    HoleNumber = 4,
                //    Score = 6,
                //    TeamId = 1
                //},
            };

            foreach (var s in game.Scores)
            {
                s.AgainstPar = s.Score - game.GameCourse.Holes[s.HoleNumber - 1].Par;
            }

            await DocumentDBRepository.CreateGame(game);

            return RedirectToAction("Index", "Game");

        }

        public async Task<ActionResult> PurgeGames()
        {
            return View(new Tuple<bool, string>(false, string.Empty));
        }

        [HttpPost]
        public async Task<ActionResult> PurgeGames(string confirmationCode)
        {
            if (confirmationCode == DateTime.Now.ToString("yyyyMMddHHmm"))
            {

                var allGames = await DocumentDBRepository.GetAllGamesAsync(true);
                foreach (var game in allGames)
                {
                    await DocumentDBRepository.DeleteGame(game.GameId, true);
                }

                return View(new Tuple<bool, string>(true, "Success"));
            }

            return View(new Tuple<bool, string>(true, "Try again"));
        }

        public async Task<ActionResult> ImportTeams(int id)
        {

            return View(new ImportTeamsViewModel() { GameId = id });

        }

        [HttpPost]
        public async Task<ActionResult> ImportTeams(int id, ImportTeamsViewModel importVM)
        {
            var game = await DocumentDBRepository.GetGameById(id);

            if (importVM.TeamsFile != null)
            {
                if (game.RegisteredTeams == null)
                {
                    game.RegisteredTeams = new List<Team>();
                }

                if (importVM.RemoveExistingTeams)
                {
                    game.RegisteredTeams.Clear();
                }

                string contents = string.Empty;
                using (var reader = new StreamReader(importVM.TeamsFile.InputStream))
                {
                    contents = await reader.ReadToEndAsync();
                }

                int successfullyCreated = 0;
                var teamLines = contents.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int count = 1; count < teamLines.Length; count++)
                {

                    var teamData = teamLines[count].Split(',');
                    if (teamData.Length != 10)
                    {
                        continue;
                    }

                    var team = new Team();
                    team.TeamId = game.GetNextTeamId();
                    team.TeamName = teamData[0];
                    team.RegistrationCode = teamData[1];
                    int startingHole;
                    if (!Int32.TryParse(teamData[2], out startingHole))
                    {
                        startingHole = 1;
                    }
                    team.StartingHole = startingHole;
                    team.TeeTime = game.GameDate.Date.AddHours(9);

                    var player1 = new Player();
                    player1.Name = teamData[4];
                    player1.Company = teamData[5];
                    player1.Email = teamData[6];

                    var player2 = new Player();
                    player2.Name = teamData[7];
                    player2.Company = teamData[8];
                    player2.Email = teamData[9];

                    team.Players = new List<Player>() { player1, player2 };

                    game.RegisteredTeams.Add(team);
                    successfullyCreated++;
                }


                await DocumentDBRepository.UpdateGame(game);

                importVM.Message = string.Format("{0} teams successfully created.", successfullyCreated);
                return View(importVM);
            }
            return RedirectToAction("Index", new { id = id });

        }

        public async Task<ActionResult> Leaderboard(int id)
        {
            var game = await DocumentDBRepository.GetGameById(id);
            var leaderboard = new List<LeaderboardEntryViewModel>();
            foreach(var team in game.RegisteredTeams)
            {
                var leaderboardEntry = new LeaderboardEntryViewModel();
                leaderboardEntry.TeamName = team.TeamName;
                
                var teamScores = game.Scores.Where(s => s.TeamId == team.TeamId);
                leaderboardEntry.AgainstPar = teamScores.Sum(s => s.AgainstPar);
                
                leaderboardEntry.TotalScore = teamScores.Sum(s => s.Score);
                int currentHole;
                if(teamScores.Count() == 0)
                {
                    currentHole = team.StartingHole;
                }
                else if(teamScores.Count() == 18)
                {
                    currentHole = 0;
                }
                else
                {
                    currentHole = teamScores.Max(s => s.HoleNumber) + 1;
                }

                if(currentHole > 18)
                {
                    currentHole = 1;
                }

                leaderboardEntry.CurrentHole = currentHole;
                leaderboardEntry.HolesPlayed = teamScores.Count();

                leaderboard.Add(leaderboardEntry);

            }
            return View(leaderboard.OrderBy(s => s.AgainstPar).ToList());
        }
    }
}