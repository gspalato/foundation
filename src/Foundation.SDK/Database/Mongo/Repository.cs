using MongoDB.Driver;

namespace Foundation.SDK.Database.Mongo
{
    public interface IRepository<T> where T : Foundation.Common.Entities.BaseEntity
    {
        IMongoCollection<T> Collection { get; }

        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(string id);
        Task<T> InsertAsync(T entity);
        Task<bool> RemoveAsync(string id);
    }

    public class Repository<T> : IRepository<T> where T : Foundation.Common.Entities.BaseEntity
    {
        public IMongoCollection<T> Collection { get; private set; }

        public Repository(IDatabaseContext dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext));

            Collection = dataContext.GetCollection<T>(typeof(T).Name);
        }

        public async Task<List<T>> GetAllAsync()
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
