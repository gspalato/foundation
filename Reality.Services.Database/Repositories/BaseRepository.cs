using MongoDB.Driver;
using Reality.Common.Entities;
using Reality.Common.Data;

namespace Reality.Services.Database.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        private readonly IMongoCollection<T> Collection;

        public BaseRepository(IDatabaseContext dataContext)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

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

            return result.DeletedCount > 0;
        }
    }
}
