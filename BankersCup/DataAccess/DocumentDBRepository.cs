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

        public static async Task<List<Game>> GetAllGamesAsync()
        {
            return await Task<List<Game>>.Run( () => Client.CreateDocumentQuery<Game>(Collection.DocumentsLink).AsEnumerable().ToList());
            
        }

        public static async Task<Game> GetGameById(int gameId)
        {
            return await Task<Game>.Run(() => Client.CreateDocumentQuery<Game>(Collection.DocumentsLink).Single(g => g.Id == gameId));

        }

        public static async Task<Document> CreateGame(Game newGame)
        {
            return await Client.CreateDocumentAsync(Collection.DocumentsLink, newGame);
        }

        public static async Task<Game> UpdateGame(Game game)
        {
            return await Client.ReplaceDocumentAsync()
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

        private static DocumentCollection collection;
        private static DocumentCollection Collection
        {
            get
            {
                if (collection == null)
                {
                    ReadOrCreateCollection(Database.SelfLink).Wait();
                }

                return collection;
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

        private static string collectionId;
        private static String CollectionId
        {
            get
            {
                if (string.IsNullOrEmpty(collectionId))
                {
                    collectionId = ConfigurationManager.AppSettings["documentDB.gameCollection"];
                }

                return collectionId;
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

        private static async Task ReadOrCreateCollection(string databaseLink)
        {
            var collections = Client.CreateDocumentCollectionQuery(databaseLink)
                              .Where(col => col.Id == CollectionId).ToArray();

            if (collections.Any())
            {
                collection = collections.First();
            }
            else
            {
                collection = await Client.CreateDocumentCollectionAsync(databaseLink,
                    new DocumentCollection { Id = CollectionId });
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