using BankersCup.DataAccess;
using BankersCup.Filters;
using BankersCup.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace BankersCup.Controllers
{
    [AdminAccessRequired]
    public class AdminController : Controller
    {
        
        public AdminController() : base()
        {
            ViewBag.ShowAd = false;
        }

        public async Task<ActionResult> FixHole2(int id)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);

            var hole2 = game.GameCourse.Holes.FirstOrDefault(h => h.HoleNumber == 2);
            hole2.Par = 3;

            var hole2scores = game.Scores.Where(s => s.HoleNumber == 2).ToList();

            foreach(var hole in hole2scores)
            {
                hole.AgainstPar = hole.Score - hole2.Par;
            }

            await DocumentDBRepository.UpdateGame(game);

            return RedirectToAction("Leaderboard", new { id = id });


        }

        public async Task<ActionResult> ExportScores(int id)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            List<string> teamScoreDataList = new List<string>();

            string[] headerData = new string[] {
                "Team ID",
                "Team Name",
                "Player 1",
                "Player 2",
                "Hole 1",
                "Hole 2",
                "Hole 3",
                "Hole 4",
                "Hole 5",
                "Hole 6",
                "Hole 7",
                "Hole 8",
                "Hole 9",
                "Hole 10",
                "Hole 11",
                "Hole 12",
                "Hole 13",
                "Hole 14",
                "Hole 15",
                "Hole 16",
                "Hole 17",
                "Hole 18"
            };

            teamScoreDataList.Add(String.Join(",", headerData));

            foreach(var team in game.RegisteredTeams)
            {
                string[] teamData = new string[22];
                teamData[0] = team.TeamId.ToString();
                teamData[1] = team.TeamName;
                teamData[2] = string.Format("{0} ({1})", team.Players[0].Name, team.Players[0].Company);
                teamData[3] = string.Format("{0} ({1})", team.Players[1].Name, team.Players[1].Company);

                int counter = 4;
                foreach(var teamScore in game.Scores.Where(s => s.TeamId == team.TeamId).OrderBy(s => s.HoleNumber))
                {
                    teamData[counter++] = teamScore.Score.ToString();
                }

                teamScoreDataList.Add(String.Join(",", teamData));
            }


            MemoryStream memoryData = new MemoryStream();

            var storage = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["CloudStorage"].ConnectionString);

            //var uri = new StorageUri(new Uri("https://golfcupstorage.blob.core.windows.net/"));
            //var creds = new StorageCredentials("golfcupstorage", "mivdKZtOsJgSWIHZ780uWcvhyo4SXwp+fFN2efNKDiYUq3dvi0efH08AAbgMytP520y6PQd35Lro0IlsPEUZRg==");
            var client = storage.CreateCloudBlobClient();
            var container = client.GetContainerReference("gamescoreexports");
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });
            var gameScoresBlob = container.GetBlockBlobReference(string.Format("{0}_TeamScores_{1}.csv", game.Name, DateTime.Now.ToString("yyyyMMddTHHmm")));


            using(var writer = new StreamWriter(memoryData))
            {
                foreach (var line in teamScoreDataList)
                {
                    await writer.WriteLineAsync(line);
                }

                await writer.FlushAsync();
                memoryData.Position = 0;
                
                await gameScoresBlob.UploadFromStreamAsync(memoryData);

            }

            var vm = new ExportScoresViewModel() { GameId = game.GameId, GameName = game.Name, Url = gameScoresBlob.Uri.ToString() };
            return View(vm);

            //return File(memoryData.ToArray(), "text/csv", string.Format("{0}_TeamScores_{1}.csv", game.Name, DateTime.Now.ToString("yyyy-MM-ddTHH-mm" )));
            
        }
        public async Task<ActionResult> Authorize()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Authorize(string authCode)
        {
            if(authCode == DateTime.Now.ToString("yyyyddMM"))
            {
                this.HttpContext.Response.SetCookie(new HttpCookie("AdminAccess", DateTime.Now.ToString("yyyyMMdd")));
                return RedirectToAction("Index", new { id = 1 });
            }

            return View();

        }

        public async Task<ActionResult> Index(int id)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;
            return View(game);
        }

        public async Task<ActionResult> Register(int id, int? teamId)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

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
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

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

                int playerId = 1;
                foreach (var player in newTeam.Players)
                {
                    player.PlayerId = playerId++;
                    player.RegistrationCode = new Random().Next(0, 1000).ToString();
                }

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

                var newPlayerId = currentTeam.Players.Max(p => p.PlayerId) + 1;
                foreach (var player in currentTeam.Players.Where(p => p.PlayerId == 0))
                {
                    player.PlayerId = newPlayerId++;
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

            game.Name = "Banker's Cup 2015";
            game.GameDate = DateTime.Parse("2015/09/21");


            game.GameCourse = new Course() { Name = "Diamondback" };

            game.GameCourse.Holes = new List<HoleInfo>()
            {
                new HoleInfo() { HoleNumber = 1, Distance = 494, Par = 5 },
                new HoleInfo() { HoleNumber = 2, Distance = 397, Par = 4 },
                new HoleInfo() { HoleNumber = 3, Distance = 190, Par = 3 },
                new HoleInfo() { HoleNumber = 4, Distance = 381, Par = 4 },
                new HoleInfo() { HoleNumber = 5, Distance = 498, Par = 5 },
                new HoleInfo() { HoleNumber = 6, Distance = 429, Par = 4 },
                new HoleInfo() { HoleNumber = 7, Distance = 410, Par = 4 },
                new HoleInfo() { HoleNumber = 8, Distance = 341, Par = 4 },
                new HoleInfo() { HoleNumber = 9, Distance = 190, Par = 3 },
                new HoleInfo() { HoleNumber = 10, Distance = 422, Par = 4 },
                new HoleInfo() { HoleNumber = 11, Distance = 521, Par = 5 },
                new HoleInfo() { HoleNumber = 12, Distance = 159, Par = 3 },
                new HoleInfo() { HoleNumber = 13, Distance = 407, Par = 4 },
                new HoleInfo() { HoleNumber = 14, Distance = 367, Par = 4 },
                new HoleInfo() { HoleNumber = 15, Distance = 176, Par = 3 },
                new HoleInfo() { HoleNumber = 16, Distance = 357, Par = 4 },
                new HoleInfo() { HoleNumber = 17, Distance = 438, Par = 4 },
                new HoleInfo() { HoleNumber = 18, Distance = 540, Par = 5 },
            };

            game.RegisteredTeams = new List<Team>()
            {
                new Team() {
                    TeamId = 1,
                    TeamName = "Infusion",
                    StartingHole = 1,
                    TeeTime = game.GameDate.Date.AddHours(9),
                    Players = new List<Player>() {
                        new Player() {
                            PlayerId = 1,
                            Company = "Infusion",
                            Email = "bbaldasti@infusion.com",
                            Name = "Bill Baldasti",
                            RegistrationCode = "12345"
                        },
                        new Player() {
                            PlayerId = 2,
                            Company = "Infusion",
                            Email = "sellis@infusion.com",
                            Name = "Steve Ellis",
                            RegistrationCode = "23456"
                        },
                        new Player() {
                            PlayerId = 3,
                            Company = "Infusion",
                            Email = "cpage@infusion.com",
                            Name = "Chris Page",
                            RegistrationCode = "34567"
                        }
                    }
                },
                new Team() {
                    TeamId = 2,
                    TeamName = "Indigo",
                    StartingHole = 10,
                    TeeTime = game.GameDate.Date.AddHours(9).AddMinutes(15),
                    Players = new List<Player>() {
                        new Player() {
                            PlayerId = 1,
                            Company = "Indigo",
                            Email = "bpo@indigo.ca",
                            Name = "Bo P",
                            RegistrationCode = "45678"
                        },
                        new Player() {
                            PlayerId = 2,
                            Company = "Indigo",
                            Email = "mtolley@indigo.ca",
                            Name = "Mark Tolley",
                            RegistrationCode = "56789"
                        },
                        new Player() {
                            PlayerId = 3,
                            Company = "Indigo",
                            Email = "sgoneau@indigo.ca",
                            Name = "Stephen Goneau",
                            RegistrationCode = "67890"
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

            return RedirectToAction("Index", "Admin", new { id = game.GameId });

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
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            if (importVM.TeamsFile != null)
            {
                if (game.RegisteredTeams == null)
                {
                    game.RegisteredTeams = new List<Team>();
                }

                if (importVM.RemoveExistingTeams)
                {
                    game.RegisteredTeams.Clear();
                    game.Scores.Clear();
                }

                string contents = string.Empty;
                using (var reader = new StreamReader(importVM.TeamsFile.InputStream))
                {
                    contents = await reader.ReadToEndAsync();
                }

                int successfullyCreated = 0;
                Dictionary<string, int> regCode = new Dictionary<string, int>();
                
                var teamLines = contents.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int count = 1; count < teamLines.Length; count++)
                {

                    var teamData = teamLines[count].Split(',');
                    if (teamData.Length != 12)
                    {
                        continue;
                    }

                    var team = new Team();
                    team.TeamId = game.GetNextTeamId();
                    team.TeamName = teamData[0];
                    team.Institution = teamData[1];
                    int startingHole;
                    if (!Int32.TryParse(teamData[2], out startingHole))
                    {
                        startingHole = 1;
                    }
                    team.StartingHole = startingHole;
                    team.TeeTime = DateTime.Parse(teamData[3]);

                    int playerId = 1;
                    var player1 = new Player();
                    player1.PlayerId = playerId++;
                    player1.Name = teamData[4];
                    player1.Company = teamData[5];
                    player1.Email = teamData[6];
                    if(player1.Email == "n@na.ca")
                    {
                        player1.Email = player1.Name.Replace(' ', '.').ToLower() + "@" + player1.Company.ToLower() + ".com";
                    }
                    player1.RegistrationCode = teamData[7];

                    if(regCode.ContainsKey(player1.RegistrationCode))
                    {
                        regCode[player1.RegistrationCode]++;
                    }
                    else
                    {
                        regCode.Add(player1.RegistrationCode, 1);
                    }

                    var player2 = new Player();
                    player2.PlayerId = playerId++;
                    player2.Name = teamData[8];
                    player2.Company = teamData[9];
                    player2.Email = teamData[10];
                    if (player2.Email == "n@na.ca")
                    {
                        player2.Email = player2.Name.Replace(' ', '.').ToLower() + "@" + player2.Company.ToLower() + ".com";
                    }
                    player2.RegistrationCode = teamData[11];
                    if (regCode.ContainsKey(player2.RegistrationCode))
                    {
                        regCode[player2.RegistrationCode]++;
                    }
                    else
                    {
                        regCode.Add(player2.RegistrationCode, 1);
                    }

                    team.Players = new List<Player>() { player1, player2 };

                    game.RegisteredTeams.Add(team);
                    successfullyCreated++;
                }


                var duplicates = regCode.Where(codeCount => codeCount.Value > 1).ToList();
                if(duplicates.Count > 0)
                {
                    string errorMessage = "Duplicate registration codes found: ";
                    duplicates.ForEach(kv => errorMessage += kv.Key + ",");
                    throw new Exception(errorMessage);
                }
                await DocumentDBRepository.UpdateGame(game);

                importVM.Message = string.Format("{0} teams successfully created.", successfullyCreated);
                return View(importVM);
            }
            return RedirectToAction("Index", new { id = id });

        }

        public async Task<ActionResult> Leaderboard(int id)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            var leaderboard = new AdminLeaderboardViewModel();
            leaderboard.ByTeams = new List<LeaderboardEntryViewModel>();
            leaderboard.ByInstitutions = new List<LeaderboardEntryViewModel>();

            var leaderboardListByTeam = new List<LeaderboardEntryViewModel>();
            foreach(var team in game.RegisteredTeams)
            {
                var leaderboardEntry = new LeaderboardEntryViewModel();
                leaderboardEntry.TeamName = team.TeamName;
                leaderboardEntry.TeamId = team.TeamId;
                
                var teamScores = game.Scores.Where(s => s.TeamId == team.TeamId);

                leaderboardEntry.AgainstPar = teamScores.Count() > 0 ? teamScores.Sum(s => s.AgainstPar) : Int32.MaxValue;
                
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
                    currentHole = teamScores.Last().HoleNumber + 1;
                }

                if(currentHole > 18)
                {
                    currentHole = 1;
                }

                leaderboardEntry.CurrentHole = currentHole;
                leaderboardEntry.HolesPlayed = teamScores.Count();

                leaderboardListByTeam.Add(leaderboardEntry);

            }

            var leaderboardListByInstitution = new List<LeaderboardEntryViewModel>();
            foreach(string institution in game.RegisteredTeams.Select(t => t.Institution).Distinct().AsEnumerable())
            {
                var leaderboardEntry = new LeaderboardEntryViewModel();
                var teamIdsForInstitution = game.RegisteredTeams.Where(t => t.Institution == institution).Select(t => t.TeamId).ToList();
                var institutionScores = game.Scores.Where(s => teamIdsForInstitution.Contains(s.TeamId));
                leaderboardEntry.TeamName = institution;
                leaderboardEntry.HolesPlayed = institutionScores.Count();
                leaderboardEntry.TotalScore = institutionScores.Sum(s => s.Score);
                leaderboardEntry.AgainstPar = institutionScores.Sum(s => s.AgainstPar);

                leaderboardListByInstitution.Add(leaderboardEntry);
            }

            leaderboard.ByTeams = leaderboardListByTeam.OrderBy(s => s.AgainstPar).ToList();
            leaderboard.ByInstitutions = leaderboardListByInstitution.OrderBy(s => s.AgainstPar).ToList();

            return View(leaderboard);
        }

        public async Task<ActionResult> Contacts()
        {
            ViewBag.GameId = 1;
            List<ContactDetails> contacts = await DocumentDBRepository.GetAllContactsAsync();
            return View(contacts);
        }

        public async Task<ActionResult> Scorecard(int id, int teamId)
        {
            var game = await DocumentDBRepository.GetGameByIdAsync(id);
            ViewBag.GameId = game.GameId;

            ScorecardViewModel vm = new ScorecardViewModel();
            vm.CurrentTeam = game.RegisteredTeams.FirstOrDefault(t => t.TeamId == teamId);
            vm.GameId = game.GameId;
            vm.HoleScores = game.Scores.Where(s => s.TeamId == teamId).ToList();
            
            return View(vm);
        }
    }
}