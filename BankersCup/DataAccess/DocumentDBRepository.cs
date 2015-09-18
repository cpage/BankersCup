using BankersCup.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BankersCup.DataAccess
{
    public static class DocumentDBRepository
    {
        static Game localGame;
        static DocumentDBRepository()
        {
            Game game = new Game();
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
                            Company = "Indigo",
                            Email = "bpo@indigo.ca",
                            Name = "Bo P"
                        },
                        new Player() {
                            Company = "Indigo",
                            Email = "mtolley@indigo.ca",
                            Name = "Mark Tolley"
                        }
                    }
                }

            };

            game.Scores = new List<TeamHoleScore>()
            {
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

            localGame = game;
 
        }
        public static async Task<List<Game>> GetAllGamesAsync(bool includeDeleted = false)
        {
            string sql = "SELECT * FROM games g";
            if(!includeDeleted)
            {
                sql += " WHERE g.IsDeleted = false";
            }

            //return await Task<List<Game>>.Run(() => new List<Game>() { localGame });
            return await Task<List<Game>>.Run(() => Client.CreateDocumentQuery<Game>(GameCollection.DocumentsLink, sql).AsEnumerable().ToList());
            
        }

        public static async Task<List<ContactDetails>> GetAllContactsAsync()
        {
            return await Task<List<ContactDetails>>.Run(() => Client.CreateDocumentQuery<ContactDetails>(ContactCollection.DocumentsLink).AsEnumerable().ToList());
        }

        public static async Task<Game> GetGameByIdAsync(int gameId)
        {
            return await Task<Game>.Run(() => Client.CreateDocumentQuery<Game>(GameCollection.DocumentsLink).Where(g => g.GameId == gameId).AsEnumerable().FirstOrDefault());
            //return await Task<Game>.Run(() => localGame);

        }

        public static async Task<Document> CreateGame(Game newGame)
        {
            return await Client.CreateDocumentAsync(GameCollection.DocumentsLink, newGame);
        }

        public static async Task<Document> UpdateGame(Game game)
        {
            return await Client.ReplaceDocumentAsync(game.SelfLink, game);
        }

        public static async Task<Document> DeleteGame(int id, bool hardDelete = false)
        {
            var game = await GetGameByIdAsync(id);
            if (game == null)
                return null;

            

            if(!hardDelete)
            {
                game.IsDeleted = true;
                return await Client.ReplaceDocumentAsync(game.SelfLink, game);
            }

            return await Client.DeleteDocumentAsync(game.SelfLink);
        }

        public static async Task<Document> CreateContact(ContactDetails details)
        {
            return await Client.CreateDocumentAsync(ContactCollection.DocumentsLink, details);
        }
        private static Database database;
        private static Database Database
        {
            get
            {
                if (database == null)
                {
                    ReadOrCreateDatabase().Wait();
                }

                return database;
            }
        }

        private static DocumentCollection gameCollection;
        private static DocumentCollection GameCollection
        {
            get
            {
                if (gameCollection == null)
                {
                    ReadOrCreateGameCollection(Database.SelfLink).Wait();
                }

                return gameCollection;
            }
        }

        private static DocumentCollection contactCollection;
        private static DocumentCollection ContactCollection
        {
            get
            {
                if (contactCollection == null)
                {
                    ReadOrCreateContactCollection(Database.SelfLink).Wait();
                }

                return contactCollection;
            }
        }

        private static string databaseId;
        private static String DatabaseId
        {
            get
            {
                if (string.IsNullOrEmpty(databaseId))
                {
                    databaseId = ConfigurationManager.AppSettings["documentDB.databaseName"];
                }

                return databaseId;
            }
        }

        private static string gameCollectionId;
        private static String GameCollectionId
        {
            get
            {
                if (string.IsNullOrEmpty(gameCollectionId))
                {
                    gameCollectionId = ConfigurationManager.AppSettings["documentDB.gameCollection"];
                }

                return gameCollectionId;
            }
        }

        private static string contactCollectionId;
        private static String ContactCollectionId
        {
            get
            {
                if (string.IsNullOrEmpty(contactCollectionId))
                {
                    contactCollectionId = ConfigurationManager.AppSettings["documentDB.contactCollection"];
                }

                return contactCollectionId;
            }
        }

        private static DocumentClient client;
        
        private static DocumentClient Client
        {
            get
            {
                if (client == null)
                {
                    String endpoint = ConfigurationManager.AppSettings["documentDB.endpoint"];
                    string authKey = ConfigurationManager.AppSettings["documentDB.primaryAccessKey"];
                    Uri endpointUri = new Uri(endpoint);
                    client = new DocumentClient(endpointUri, authKey);
                    
                }

                return client;
            }
        }

        private static async Task ReadOrCreateGameCollection(string databaseLink)
        {
            var collections = Client.CreateDocumentCollectionQuery(databaseLink)
                              .Where(col => col.Id == GameCollectionId).ToArray();

            if (collections.Any())
            {
                gameCollection = collections.First();
            }
            else
            {
                gameCollection = await Client.CreateDocumentCollectionAsync(databaseLink,
                    new DocumentCollection { Id = GameCollectionId });
            }
        }

        private static async Task ReadOrCreateContactCollection(string databaseLink)
        {
            var collections = Client.CreateDocumentCollectionQuery(databaseLink)
                              .Where(col => col.Id == ContactCollectionId).ToArray();

            if (collections.Any())
            {
                contactCollection = collections.First();
            }
            else
            {
                contactCollection = await Client.CreateDocumentCollectionAsync(databaseLink,
                    new DocumentCollection { Id = ContactCollectionId });
            }
        }

        private static async Task ReadOrCreateDatabase()
        {
            var databases = Client.CreateDatabaseQuery()
                            .Where(db => db.Id == DatabaseId).ToArray();

            if (databases.Any())
            {
                database = databases.First();
            }
            else
            {
                Database database = new Database { Id = DatabaseId };
                database = await Client.CreateDatabaseAsync(database);
            }
        }


    }
}