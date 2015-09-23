using BankersCup.Models;
using BankersCup.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BankersCup.DataAccess;
using System.Threading.Tasks;
using BankersCup.Helpers;
using System.Configuration;

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

            int defaultGameId;
            if (!Int32.TryParse(ConfigurationManager.AppSettings["defaultGameId"], out defaultGameId))
            {
                defaultGameId = 1;
            }

            var game = await DocumentDBRepository.GetGameByIdAsync(id.GetValueOrDefault(defaultGameId));
            ViewBag.GameId = game.GameId;

            var currentRegistration = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId);

            GameDetailsViewModel vm = new GameDetailsViewModel();
            vm.GameId = game.GameId;
            vm.GameCourse = game.GameCourse;
            vm.CurrentTeamScores = game.Scores.Where(s => s.TeamId == currentRegistration.TeamId).ToList();
            vm.HolesPlayed = calculateHolesPlayedForCurrentTeam(game);

            vm.CurrentTeam = game.RegisteredTeams.First(t => t.TeamId == currentRegistration.TeamId);

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

        [RegistrationRequired]
        public async Task<ActionResult> MyTeam(int id)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;
            var regValues = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, id);

            var currentTeam = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == regValues.TeamId);
            var currentPlayer = currentTeam.Players.FirstOrDefault(p => p.PlayerId == regValues.PlayerId);
            var details = new GameDetailsViewModel();
            details.GameId = id;
            details.CurrentTeam = currentTeam;
            details.GameCourse = game.GameCourse;
            details.CurrentPlayerId = currentPlayer.PlayerId;

            return View(details);
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
            var game = await DocumentDBRepository.GetGameByIdAsync(joinModel.Id);
            ViewBag.GameId = game.GameId;

            
            var team = game.RegisteredTeams.FirstOrDefault(t => t.Players.Any(p => p.RegistrationCode == joinModel.RegistrationCode));
            if(team != null)
            {
                var player = team.Players.FirstOrDefault(p => p.RegistrationCode == joinModel.RegistrationCode);
                RegistrationHelper.SetRegistrationCookie(this.HttpContext, joinModel.Id, team.TeamId, player.PlayerId);
                return RedirectToAction("MyTeam", new { Id = joinModel.Id } );
            }

            return View(joinModel);
        }

        [RegistrationRequired]
        public async Task<ActionResult> AddHole(int id, int? holeNumber)
        {
            // get game here
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            var teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId).TeamId;

            var teamScores = game.Scores.Where(s => s.TeamId == teamId);

            if(holeNumber == null && teamScores.Count() >= game.GameCourse.Holes.Count)
            {
                return View(new AddHoleScoreViewModel() { AllHoleScoresEntered = true, GameId = game.GameId });
            }

            int currentHole = 0;
            var existingHoleScore = game.Scores.FirstOrDefault(s => s.TeamId == teamId && s.HoleNumber == holeNumber.GetValueOrDefault());

            if (holeNumber == null)
            {

                if (teamScores.Count() == 0)
                {
                    currentHole = game.RegisteredTeams.First(t => t.TeamId == teamId).StartingHole;
                }
                else
                {
                    currentHole = teamScores.Last().HoleNumber + 1;
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

            var comments = game.Comments.Where(c => c.HoleNumber == currentHole)
                .OrderByDescending(c => c.CreatedOn)
                .Select(c => new GameCommentViewModel()
                {
                    Comment = c.CommentText,
                    CreatedOn = c.CreatedOn,
                    HoleNumber = c.HoleNumber,
                    PlayerName = c.PlayerName,
                    TeamName = c.TeamName
                })
                .ToList();

            AddHoleScoreViewModel vm = new AddHoleScoreViewModel()
            {
                GameId = game.GameId,
                HoleNumber = holeInfo.HoleNumber,
                Par = holeInfo.Par,
                Distance = holeInfo.Distance,
                TeamId = teamId,
                TeamScore = existingHoleScore == null ? holeInfo.Par : existingHoleScore.Score,
                AverageScore = averageScore,
                Comments = comments
            };
            
            return View(vm);
        }

        [HttpPost]
        [RegistrationRequired]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddHole(int id, AddHoleScoreViewModel newScore)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(newScore.GameId);
            ViewBag.GameId = game.GameId;

            if (newScore.SaveScore)
            {
                var score = game.Scores.FirstOrDefault(s => s.TeamId == newScore.TeamId && s.HoleNumber == newScore.HoleNumber);

                if (score == null)
                {
                    score = new TeamHoleScore();
                    game.Scores.Add(score);
                    score.TeamId = newScore.TeamId;
                    score.HoleNumber = newScore.HoleNumber;
                }

                score.Score = newScore.TeamScore;
                score.AgainstPar = newScore.TeamScore - newScore.Par;
            }

            if(newScore.SaveComment && !string.IsNullOrWhiteSpace(newScore.NewComment))
            {
                var regValues = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId);
                var team = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == regValues.TeamId);
                var player = team.Players.FirstOrDefault(p => p.PlayerId == regValues.PlayerId);

                game.Comments.Add(new GameComment()
                {
                    GameId = game.GameId,
                    PlayerId = player.PlayerId,
                    PlayerName = player.Name,
                    TeamId = team.TeamId,
                    TeamName = team.TeamName,
                    CreatedOn = DateTime.Now,
                    HoleNumber = newScore.HoleNumber,
                    CommentText = newScore.NewComment
                });
            }

            int nextHole = newScore.HoleNumber;
            if (newScore.MoveNext)
            {
                if (++nextHole > 18)
                {
                    nextHole = 1;
                }
            }
            if (newScore.MovePrevious)
            {
                if (--nextHole < 1)
                {
                    nextHole = 18;
                }
            }

            if (newScore.SaveScore || newScore.SaveComment)
            {
                await DocumentDBRepository.UpdateGame(game);
            }

            return RedirectToAction("AddHole", new { id = newScore.GameId, holeNumber = nextHole });
            

        }

        [RegistrationRequired]
        public async Task<ActionResult> Leaderboard(int id)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            LeaderboardViewModel leaderboardVM = new LeaderboardViewModel();
            int teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId).TeamId;
            leaderboardVM.CurrentTeam = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == teamId);
            leaderboardVM.HolesPlayed = calculateHolesPlayedForCurrentTeam(game);
            leaderboardVM.Teams = calculateLeaderboardBasedOnCurrentTeam(game);

            leaderboardVM.Comments = game.Comments
                .Where(c => c.PlayerId > 0 || c.HoleNumber == 0)
                .OrderByDescending(c => c.CreatedOn)
                .Take(5)
                .Select(c => new GameCommentViewModel()
                {
                    Comment = c.CommentText,
                    CreatedOn = c.CreatedOn,
                    HoleNumber = c.HoleNumber,
                    PlayerName = c.PlayerName,
                    TeamName = c.TeamName
                })
                .ToList();

            return View(leaderboardVM);
        }

        [RegistrationRequired]
        public async Task<ActionResult> Scorecard(int id)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            ScorecardViewModel vm = new ScorecardViewModel();
            vm.GameId = game.GameId;
            vm.CurrentTeam = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId).TeamId);
            vm.HoleScores = game.Scores.Where(s => s.TeamId == vm.CurrentTeam.TeamId).ToList();
            return View(vm);
        }

        [RegistrationRequired]
        public async Task<ActionResult> Contact(int id, string topic)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            var regValues = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, id);
            var team = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == regValues.TeamId);
            var player = team.Players.FirstOrDefault(p => p.PlayerId == regValues.PlayerId);

            ContactDetailsViewModel contactDetails = new ContactDetailsViewModel();
            contactDetails.CurrentTeam = team;
            contactDetails.Topic = topic;
            contactDetails.Email = player.Email;
            contactDetails.CurrentPlayerId = player.PlayerId;

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

            var game = await DocumentDBRepository.GetGameByIdAsync(id);

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


        [RegistrationRequired]
        public async Task<ActionResult> Gallery(int id)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            

            return View(createGalleryVM(game));
        }

        [RegistrationRequired, HttpPost]
        public async Task<ActionResult> Gallery(int id, GalleryViewModel vm)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            int bytesRead = 0;
            var imageBytes = new byte[vm.UploadedImage.InputStream.Length];
            var imageStream = vm.UploadedImage.InputStream;

            while(bytesRead < imageStream.Length)
            {
                bytesRead += imageStream.Read(imageBytes, bytesRead, (int)imageStream.Length - bytesRead);
            }

            var url = await AzureStorageRepository.SaveImageAsync(game.GameId, vm.UploadedImage.FileName, imageBytes);

            var image = new Image();
            image.Caption = vm.UploadedImage.FileName;
            image.GameId = game.GameId;
            image.ImageUrl = url;
            var regValues = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId);

            image.UploadedById = regValues.PlayerId;
            image.UploadedByTeamId = regValues.TeamId;
            image.UploadedOn = DateTime.Now;

            if(game.Images == null)
            {
                game.Images = new List<Image>();
            }

            game.Images.Add(image);

            await DocumentDBRepository.UpdateGame(game);


            return View(createGalleryVM(game));
        }

        [RegistrationRequired]
        public async Task<ActionResult> Comments(int id)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            if(game.Comments == null)
            {
                game.Comments = new List<GameComment>();
            }

            GameCommentListViewModel vm = createGameCommentListVM(game);

            return View(vm);
        }

        [RegistrationRequired, HttpPost]
        public async Task<ActionResult> Comments(int id, GameCommentListViewModel vm)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            var regValues = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, game.GameId);
            var team = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == regValues.TeamId);
            var teamName = team.TeamName;
            var playerName = team.Players.FirstOrDefault(p => p.PlayerId == regValues.PlayerId).Name;

            var newComment = new GameComment()
            {
                PlayerId = regValues.PlayerId,
                PlayerName = playerName,
                TeamId = regValues.TeamId,
                TeamName = teamName,
                GameId = game.GameId,
                CommentText = vm.NewComment,
                CreatedOn = DateTime.Now
            };

            if(game.Comments == null)
            {
                game.Comments = new List<GameComment>();
            }

            game.Comments.Add(newComment);

            await DocumentDBRepository.UpdateGame(game);


            return View(createGameCommentListVM(game));
        }

        private int calculateHolesPlayedForCurrentTeam(Game currentGame)
        {
            var teamId = RegistrationHelper.GetRegistrationCookieValue(this.HttpContext, currentGame.GameId).TeamId;
            int holesPlayed = currentGame.Scores.Count(s => s.TeamId == teamId);

            return holesPlayed;

        }

        private List<LeaderboardEntryViewModel> calculateLeaderboardBasedOnCurrentTeam(Game currentGame, bool includeTeamsWithNoScores = false)
        {
            var leaderboard = new List<LeaderboardEntryViewModel>();
            foreach (var team in currentGame.RegisteredTeams)
            {
                var leaderboardEntry = new LeaderboardEntryViewModel();
                leaderboardEntry.TeamName = team.TeamName;
                int holesPlayed = calculateHolesPlayedForCurrentTeam(currentGame);

                var teamScores = currentGame.Scores.Where(s => s.TeamId == team.TeamId).Take(holesPlayed);
                if(teamScores.Count() == 0)
                {
                    continue;
                }

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

        private GameCommentListViewModel createGameCommentListVM(Game game)
        {
            var vm = new GameCommentListViewModel();

            vm.ExistingComments = new List<GameCommentViewModel>();

            vm.ExistingComments = game.Comments
                //.Where(c => c.PlayerId > 0 || c.HoleNumber == 0)
                .OrderByDescending(c => c.CreatedOn)
                .Select(c => new GameCommentViewModel() { Comment = c.CommentText, CreatedOn = c.CreatedOn, PlayerName = c.PlayerName, TeamName = c.TeamName, HoleNumber = c.HoleNumber })
                .ToList();
            return vm;
        }


        private GalleryViewModel createGalleryVM(Game game)
        {
            if(game.Images == null)
            {
                game.Images = new List<Image>();
            }

            var vm = new GalleryViewModel();
            vm.Images = game.Images
                .OrderByDescending(i => i.UploadedOn)
                .Select(i => new GalleryImageViewModel() { Caption = i.Caption, HoleNumber = i.HoleNumber, TakenBy = i.UploadedById, Url = i.ImageUrl })
                .ToList();

            return vm;
        }

    }


}
