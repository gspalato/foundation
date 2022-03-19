using MongoDB.Driver;
using Reality.Common.Configurations;

namespace Reality.Common.Data
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

        public DatabaseContext(BaseConfiguration config)
        {
            Client = new MongoClient(new MongoUrl(config.Database_Url));
            Database = Client.GetDatabase(config.Database_Name);
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