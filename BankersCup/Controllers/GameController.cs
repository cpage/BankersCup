using BankersCup.Models;
using BankersCup.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BankersCup.Controllers
{
    public class GameController : Controller
    {
        // GET: Game
        public ActionResult Index()
        {
            
            var currentGame = MvcApplication.CurrentGame;
            var currentGameVM = new GameSummaryViewModel()
            {
                GameId = currentGame.Id,
                CourseName = currentGame.GameCourse.Name,
                GameDate = currentGame.GameDate,
                EventName = currentGame.Name,
                NumberOfTeams = currentGame.RegisteredTeams.Count
            };

            List<GameSummaryViewModel> games = new List<GameSummaryViewModel>() { currentGameVM };
            return View(games);
        }

        // GET: Game/Details/5
        [RegistrationRequired]
        public ActionResult Details(int id)
        {

            var game = MvcApplication.CurrentGame;

            var currentTeamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.Id);

            GameDetailsViewModel vm = new GameDetailsViewModel();
            vm.GameId = id;
            vm.GameCourse = game.GameCourse;
            vm.CurrentTeamScores = game.Scores.Where(s => s.TeamId == currentTeamId).ToList();
            vm.HolesPlayed = calculateHolesPlayedForCurrentTeam(game);

            vm.CurrentTeam = game.RegisteredTeams.First(t => t.TeamId == currentTeamId);

            vm.Leaderboard = calculateLeaderboardBasedOnCurrentTeam(game);

            return View(vm);
        }

        // GET: Game/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Game/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Game/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Game/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Game/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Game/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Join(int id)
        {
            // get game here
            JoinGameViewModel vm = new JoinGameViewModel() { Id = id };
            return View(vm);
        }

        [HttpPost]
        public ActionResult Join(JoinGameViewModel joinModel)
        {
            // get game and validate if the registration code exists
            var team = MvcApplication.CurrentGame.RegisteredTeams.FirstOrDefault(t => t.RegistrationCode == joinModel.RegistrationCode);
            if(team != null)
            {
                RegistrationHelper.SetRegistrationCookie(this.HttpContext, joinModel.Id, team.TeamId);
                return RedirectToAction("Details", new { Id = joinModel.Id } );
            }

            return View(joinModel);
        }

        public ActionResult AddHole(int id)
        {
            // get game here
            var game = MvcApplication.CurrentGame;

            var teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.Id);

            var teamScores = game.Scores.Where(s => s.TeamId == teamId);

            if(teamScores.Count() >= game.GameCourse.Holes.Count)
            {
                return View(new AddHoleScoreViewModel() { AllHoleScoresEntered = true, GameId = game.Id });
            }
            
            var currentHole = teamScores.Last().HoleNumber;
            if(++currentHole == 19)
            {
                currentHole = 1;
            }

            var holeInfo = game.GameCourse.Holes.First(h => h.HoleNumber == currentHole);

            AddHoleScoreViewModel vm = new AddHoleScoreViewModel()
            {
                GameId = MvcApplication.CurrentGame.Id,
                HoleNumber = holeInfo.HoleNumber,
                Par = holeInfo.Par,
                Distance = holeInfo.Distance,
                TeamId = teamId,
                TeamScore = holeInfo.Par
            };
            
            return View(vm);
        }

        [HttpPost]
        public ActionResult AddHole(AddHoleScoreViewModel newScore)
        {
            TeamHoleScore score = new TeamHoleScore();
            score.TeamId = newScore.TeamId;
            score.HoleNumber = newScore.HoleNumber;
            score.Score = newScore.TeamScore;
            score.AgainstPar = newScore.TeamScore - newScore.Par;

            MvcApplication.CurrentGame.Scores.Add(score);

            return RedirectToAction("Details", new { id = newScore.GameId });

        }

        private int calculateHolesPlayedForCurrentTeam(Game currentGame)
        {
            var teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, currentGame.Id);
            int holesPlayed = currentGame.Scores.Count(s => s.TeamId == teamId);

            return holesPlayed;

        }

        private List<LeaderboardEntryViewModel> calculateLeaderboardBasedOnCurrentTeam(Game currentGame)
        {
            var teamNames = currentGame.RegisteredTeams.ToDictionary(x => x.TeamId, v => v.TeamName);
            int holesPlayed = calculateHolesPlayedForCurrentTeam(currentGame);
            var teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, currentGame.Id);

            var teamScores = currentGame.Scores.GroupBy(k => k.TeamId).ToDictionary(k => k.Key, v => v.ToList());
            var leaderBoard = new List<LeaderboardEntryViewModel>();
            foreach(var team in teamScores)
            {
                leaderBoard.Add(new LeaderboardEntryViewModel() { TeamName = teamNames[team.Key], TotalScore = team.Value.Take(holesPlayed).Sum(h => h.Score), AgainstPar = team.Value.Take(holesPlayed).Sum(h => h.AgainstPar) });
            }

            return leaderBoard.OrderBy(s => s.AgainstPar).ToList();
        }


    }


}
