﻿using BankersCup.Models;
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
            ViewBag.GameId = 1;

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
        public async Task<ActionResult> Details(int? id)
        {
            
            var game = await DocumentDBRepository.GetGameById(id.GetValueOrDefault(1));
            ViewBag.GameId = game.GameId;

            var currentTeamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId);

            GameDetailsViewModel vm = new GameDetailsViewModel();
            vm.GameId = game.GameId;
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Join(JoinGameViewModel joinModel)
        {
            // get game and validate if the registration code exists
            var game = await DocumentDBRepository.GetGameById(joinModel.Id);
            ViewBag.GameId = game.GameId;

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
            ViewBag.GameId = game.GameId;

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
            var scores = game.Scores.Where(s => s.HoleNumber == currentHole);
            double averageScore = 0.0;
            if(scores.Count() > 0)
            {
                averageScore = game.Scores.Where(s => s.HoleNumber == currentHole).Average(s => s.Score);
            }

            AddHoleScoreViewModel vm = new AddHoleScoreViewModel()
            {
                GameId = game.GameId,
                HoleNumber = holeInfo.HoleNumber,
                Par = holeInfo.Par,
                Distance = holeInfo.Distance,
                TeamId = teamId,
                TeamScore = existingHoleScore == null ? holeInfo.Par : existingHoleScore.Score,
                AverageScore = averageScore
            };
            
            return View(vm);
        }

        [HttpPost]
        [RegistrationRequired]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddHole(AddHoleScoreViewModel newScore)
        {
            var game = await DocumentDBRepository.GetGameById(newScore.GameId);
            ViewBag.GameId = game.GameId;

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

            string action = "Details";
            dynamic actionParams;

            if(newScore.NextHoleAfterSave || newScore.PreviousHoleAfterSave)
            {
                action = "AddHole";

                int nextHole = newScore.HoleNumber;
                if (newScore.NextHoleAfterSave)
                {
                    if (++nextHole > 18)
                    {
                        nextHole = 1;
                    }
                }
                if (newScore.PreviousHoleAfterSave)
                {
                    if (--nextHole < 1)
                    {
                        nextHole = 18;
                    }
                }
                actionParams = new { id = newScore.GameId, holeNumber = nextHole };
            }
            else
            {
                actionParams = new { id = newScore.GameId };
            }

            await DocumentDBRepository.UpdateGame(game);
            
            return RedirectToAction(action, actionParams);
            

        }

        [RegistrationRequired]
        public async Task<ActionResult> Leaderboard(int id)
        {
            var game = await DocumentDBRepository.GetGameById(id);
            ViewBag.GameId = game.GameId;

            LeaderboardViewModel leaderboardVM = new LeaderboardViewModel();
            int teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId);
            leaderboardVM.CurrentTeam = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == teamId);
            leaderboardVM.HolesPlayed = calculateHolesPlayedForCurrentTeam(game);
            leaderboardVM.Teams = calculateLeaderboardBasedOnCurrentTeam(game);

            return View(leaderboardVM);
        }

        [RegistrationRequired]
        public async Task<ActionResult> Scorecard(int id)
        {
            var game = await DocumentDBRepository.GetGameById(id);
            ViewBag.GameId = game.GameId;

            ScorecardViewModel vm = new ScorecardViewModel();
            vm.GameId = game.GameId;
            vm.CurrentTeam = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId));
            vm.HoleScores = game.Scores.Where(s => s.TeamId == vm.CurrentTeam.TeamId).ToList();
            return View(vm);
        }

        [RegistrationRequired]
        public async Task<ActionResult> Contact(int id, string topic)
        {
            var game = await DocumentDBRepository.GetGameById(id);
            ViewBag.GameId = game.GameId;

            var team = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, id));

            ContactDetailsViewModel contactDetails = new ContactDetailsViewModel();
            contactDetails.CurrentTeam = team;
            contactDetails.Topic = topic;
            
            return View(contactDetails);
        }

        [RegistrationRequired]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(int id, ContactDetailsViewModel details)
        {
            if(!ModelState.IsValid)
            {
                return View(details);
            }

            var game = await DocumentDBRepository.GetGameById(id);

            var contact = new ContactDetails();
            contact.Name = details.Name;
            contact.Email = details.Email;
            contact.Company = details.Company;
            contact.Topic = details.Topic;
            contact.Consent = details.Consent;
            contact.DateCreated = DateTime.Now;
            contact.EventName = game.Name;
            
            await DocumentDBRepository.CreateContact(contact);

            return RedirectToAction("Details", "Game", new { id = id });
        }

        private int calculateHolesPlayedForCurrentTeam(Game currentGame)
        {
            var teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, currentGame.GameId);
            int holesPlayed = currentGame.Scores.Count(s => s.TeamId == teamId);

            return holesPlayed;

        }

        private List<LeaderboardEntryViewModel> calculateLeaderboardBasedOnCurrentTeam(Game currentGame)
        {
            var leaderboard = new List<LeaderboardEntryViewModel>();
            foreach (var team in currentGame.RegisteredTeams)
            {
                var leaderboardEntry = new LeaderboardEntryViewModel();
                leaderboardEntry.TeamName = team.TeamName;
                int holesPlayed = calculateHolesPlayedForCurrentTeam(currentGame);

                var teamScores = currentGame.Scores.Where(s => s.TeamId == team.TeamId).Take(holesPlayed);
                leaderboardEntry.AgainstPar = teamScores.Sum(s => s.AgainstPar);

                leaderboardEntry.TotalScore = teamScores.Sum(s => s.Score);
                int currentHole;
                if (teamScores.Count() == 0)
                {
                    currentHole = team.StartingHole;
                }
                else if (teamScores.Count() == 18)
                {
                    currentHole = 0;
                }
                else
                {
                    currentHole = teamScores.Max(s => s.HoleNumber) + 1;
                }

                if (currentHole > 18)
                {
                    currentHole = 1;
                }

                leaderboardEntry.CurrentHole = currentHole;
                leaderboardEntry.HolesPlayed = teamScores.Count();

                leaderboard.Add(leaderboardEntry);

            }

            return leaderboard.OrderBy(s => s.AgainstPar).ToList();
        }


    }


}
