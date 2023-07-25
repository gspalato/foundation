using MongoDB.Driver;
using Foundation.Common.Configurations;

namespace Foundation.Core.SDK.Database.Mongo
{
    public interface IDatabaseContext
    {
        MongoClient GetClient();
        IMongoCollection<T> GetCollection<T>(string name);
    }

    public class DatabaseContext : IDatabaseContext
    {
        private readonly MongoClient Client;
        private readonly IMongoDatabase Database;

        public DatabaseContext(IBaseConfiguration config)
        {
            Client = new MongoClient(new MongoClientSettings()
            {
                Credential = MongoCredential.CreateCredential(
                    config.DatabaseName,
                    config.DatabaseUser,
                    config.DatabasePassword
                ),
                Server = new MongoServerAddress(config.DatabaseHost),

            });
            Database = Client.GetDatabase(config.DatabaseName);
        }

        public MongoClient GetClient()
        {
            return Client;
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return Database.GetCollection<T>(name);
        }
    }
}