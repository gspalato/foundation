using MongoDB.Driver;

namespace Reality.SDK.Database.Mongo
{
    public interface IRepository<T> where T : BaseEntity
    {
        IMongoCollection<T> Collection { get; }

        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(string id);
        Task<T> InsertAsync(T entity);
        Task<bool> RemoveAsync(string id);
    }

    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        public IMongoCollection<T> Collection { get; private set; }

        public Repository(IDatabaseContext dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext));

            Collection = dataContext.GetCollection<T>(typeof(T).Name);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Collection.Find(_ => true).ToListAsync();
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq(_ => _.Id, id);

            return await Collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<T> InsertAsync(T entity)
        {
            await Collection.InsertOneAsync(entity);

            return entity;
        }

        public async Task<bool> RemoveAsync(string id)
        {
            var result = await Collection.DeleteOneAsync(Builders<T>.Filter.Eq(_ => _.Id, id));

            return result.DeletedCount is 1;
        }
    }
}
