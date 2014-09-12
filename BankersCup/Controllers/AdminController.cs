using BankersCup.DataAccess;
using BankersCup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BankersCup.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public async Task<ActionResult> Index(int id)
        {
            var game = await DocumentDBRepository.GetGameById(id);

            return View(game.RegisteredTeams);
        }

        public async Task<ActionResult> Register(int id)
        {
            Team teamModel = new Team();
            teamModel.Players = new List<Player>() { new Player() };
            return View(teamModel);
        }

        [HttpPost]
        public async Task<ActionResult> Register(int id, Team newTeam)
        {
            var game = await DocumentDBRepository.GetGameById(id);
            newTeam.RegistrationCode = new Random().Next(0, 1000).ToString();
            game.RegisteredTeams.Add(newTeam);

            await DocumentDBRepository.UpdateGame(game);

            return RedirectToAction("Index");
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

            game.Scores = new List<TeamHoleScore>() {
                new TeamHoleScore() {
                    HoleNumber = 1,
                    Score = 3,
                    TeamId = 1
                },
                new TeamHoleScore() {
                    HoleNumber = 10,
                    Score = 5,
                    TeamId = 2
                },
                new TeamHoleScore() {
                    HoleNumber = 2,
                    Score = 4,
                    TeamId = 1
                },
                new TeamHoleScore() {
                    HoleNumber = 11,
                    Score = 4,
                    TeamId = 2
                },
                new TeamHoleScore() {
                    HoleNumber = 3,
                    Score = 4,
                    TeamId = 1
                },
                new TeamHoleScore() {
                    HoleNumber = 4,
                    Score = 6,
                    TeamId = 1
                },
            };

            foreach (var s in game.Scores)
            {
                s.AgainstPar = s.Score - game.GameCourse.Holes[s.HoleNumber - 1].Par;
            }

            await DocumentDBRepository.CreateGame(game);

            return RedirectToAction("Index", "Game");

        }
    }
}