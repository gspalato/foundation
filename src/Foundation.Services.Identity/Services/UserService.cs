using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using Foundation.Common.Roles;
using Foundation.Common.Entities;
using Foundation.SDK.Database.Mongo;

namespace Foundation.Services.Identity.Services
{
    public interface IUserService
    {
        Task<FullUser?> GetUserAsync(string username);

        Task<FullUser?> GetUserByIdAsync(string id);

        Task<FullUser?> CreateUserAsync(string username, string password);

        Task DeleteUserAsync(string username);
    }

    public class UserService : IUserService
    {
        private readonly IPasswordHasher<string> Hasher;

        private readonly IMongoCollection<FullUser> Users;

        public UserService(IDatabaseContext databaseContext, IPasswordHasher<string> hasher)
        {
            Hasher = hasher;

            Users = databaseContext.GetCollection<FullUser>(nameof(Users));
        }

        public async Task<FullUser?> CreateUserAsync(string username, string password)
        {
            if (await GetUserAsync(username) is not null)
                return null;

            var hashedPassword = Hasher.HashPassword(username, password);

            var user = new FullUser()
            {
                Username = username,
                PasswordHash = hashedPassword,
                Roles = Array.Empty<Role>()
            };

            await Users.InsertOneAsync(user);

            return user;
        }

        public async Task DeleteUserAsync(string username)
        {
            var filter = Builders<FullUser>.Filter.Where(x => x.Username == username);
            await Users.FindOneAndDeleteAsync(filter);
        }

        public async Task<FullUser?> GetUserAsync(string username)
        {
            var filter = Builders<FullUser>.Filter.Where(x => x.Username == username);
            var found = await Users.FindAsync(filter);
            return found.FirstOrDefault();
        }

        public async Task<FullUser?> GetUserByIdAsync(string id)
        {
            var filter = Builders<FullUser>.Filter.Where(x => x.Id == id);
            var found = await Users.FindAsync(filter);
            return found.FirstOrDefault();
        }
    }
}
