using BankersCup.Models;
using BankersCup.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BankersCup.DataAccess;
using System.Threading.Tasks;

namespace BankersCup.Controllers
{
    public class GameController : Controller
    {
        // GET: Game
        public async Task<ActionResult> Index()
        {

            var games = await DocumentDBRepository.GetAllGamesAsync();

            var gamesVM = new List<GameSummaryViewModel>();
            foreach(var game in games)
            {
                gamesVM.Add(new GameSummaryViewModel()
                {
                    GameId = game.GameId,
                    CourseName = game.GameCourse.Name,
                    GameDate = game.GameDate,
                    EventName = game.Name,
                    NumberOfTeams = game.RegisteredTeams.Count
                });


            }

            return View(gamesVM);
        }

        [RegistrationRequired]
        public async Task<ActionResult> Details(int id)
        {

            var game = await DocumentDBRepository.GetGameById(id);

            var currentTeamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId);

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
        public async Task<ActionResult> Create()
        {
            return View();
        }

        // POST: Game/Create
        [HttpPost]
        public async Task<ActionResult> Create(FormCollection collection)
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
        public async Task<ActionResult> Edit(int id)
        {
            return View();
        }

        // POST: Game/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(int id, FormCollection collection)
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
        public async Task<ActionResult> Delete(int id)
        {
            await DocumentDBRepository.DeleteGame(id);

            return RedirectToAction("Index");
        }

        // POST: Game/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(int id, FormCollection collection)
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

        public async Task<ActionResult> Join(int id)
        {
            // get game here
            JoinGameViewModel vm = new JoinGameViewModel() { Id = id };
            return View(vm);
        }

        [HttpPost]
        public async Task<ActionResult> Join(JoinGameViewModel joinModel)
        {
            // get game and validate if the registration code exists
            var game = await DocumentDBRepository.GetGameById(joinModel.Id);

            var team = game.RegisteredTeams.FirstOrDefault(t => t.RegistrationCode == joinModel.RegistrationCode);
            if(team != null)
            {
                RegistrationHelper.SetRegistrationCookie(this.HttpContext, joinModel.Id, team.TeamId);
                return RedirectToAction("Details", new { Id = joinModel.Id } );
            }

            return View(joinModel);
        }

        [RegistrationRequired]
        public async Task<ActionResult> AddHole(int id, int? holeNumber)
        {
            // get game here
            var game = await DocumentDBRepository.GetGameById(id);

            var teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId);

            var teamScores = game.Scores.Where(s => s.TeamId == teamId);

            if(holeNumber == null && teamScores.Count() >= game.GameCourse.Holes.Count)
            {
                return View(new AddHoleScoreViewModel() { AllHoleScoresEntered = true, GameId = game.GameId });
            }

            int currentHole = 0;
            var existingHoleScore = game.Scores.FirstOrDefault(s => s.TeamId == teamId && s.HoleNumber == holeNumber.GetValueOrDefault());

            if (holeNumber == null || existingHoleScore == null)
            {

                if (teamScores.Count() == 0)
                {
                    currentHole = game.RegisteredTeams.First(t => t.TeamId == teamId).StartingHole;
                }
                else
                {
                    currentHole = ++teamScores.Last().HoleNumber;
                }

                if (currentHole == 19)
                {
                    currentHole = 1;
                }
            }
            else
            {
                currentHole = holeNumber.Value;
            }
            var holeInfo = game.GameCourse.Holes.First(h => h.HoleNumber == currentHole);

            AddHoleScoreViewModel vm = new AddHoleScoreViewModel()
            {
                GameId = game.GameId,
                HoleNumber = holeInfo.HoleNumber,
                Par = holeInfo.Par,
                Distance = holeInfo.Distance,
                TeamId = teamId,
                TeamScore = existingHoleScore == null ? holeInfo.Par : existingHoleScore.Score
            };
            
            return View(vm);
        }

        [HttpPost]
        [RegistrationRequired]
        public async Task<ActionResult> AddHole(AddHoleScoreViewModel newScore)
        {
            var game = await DocumentDBRepository.GetGameById(newScore.GameId);

            var score = game.Scores.FirstOrDefault(s => s.TeamId == newScore.TeamId && s.HoleNumber == newScore.HoleNumber);

            if(score == null)
            {
                score = new TeamHoleScore();
                game.Scores.Add(score);
                score.TeamId = newScore.TeamId;
                score.HoleNumber = newScore.HoleNumber;
            }

            score.Score = newScore.TeamScore;
            score.AgainstPar = newScore.TeamScore - newScore.Par;
            

            await DocumentDBRepository.UpdateGame(game);
            return RedirectToAction("Details", new { id = newScore.GameId });

        }

        private int calculateHolesPlayedForCurrentTeam(Game currentGame)
        {
            var teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, currentGame.GameId);
            int holesPlayed = currentGame.Scores.Count(s => s.TeamId == teamId);

            return holesPlayed;

        }

        private List<LeaderboardEntryViewModel> calculateLeaderboardBasedOnCurrentTeam(Game currentGame)
        {
            var teamNames = currentGame.RegisteredTeams.ToDictionary(x => x.TeamId, v => v.TeamName);
            int holesPlayed = calculateHolesPlayedForCurrentTeam(currentGame);
            var teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, currentGame.GameId);

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
