using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using Reality.Common;
using Reality.Common.Data;
using Reality.Common.Entities;

namespace Reality.Services.Identity.Services
{
    public interface IUserService
    {
        Task<User?> GetUserAsync(string username);
        Task<User?> CreateUserAsync(string username, string password);
        Task DeleteUserAsync(string username);
    }

    public class UserService : IUserService
    {
        private readonly IDatabaseContext DatabaseContext;
        private readonly IPasswordHasher<string> Hasher;

        private readonly IMongoCollection<User> Users;

        public UserService(IDatabaseContext databaseContext, IPasswordHasher<string> hasher)
        {
            DatabaseContext = databaseContext;
            Hasher = hasher;

            Users = databaseContext.GetCollection<User>("users");
        }

        public async Task<User?> CreateUserAsync(string username, string password)
        {
            if (await GetUserAsync(username) is not null)
                return null;

            var hashedPassword = Hasher.HashPassword(username, password);

            var user = new User()
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
            var filter = Builders<User>.Filter.Where(x => x.Username == username);
            await Users.FindOneAndDeleteAsync(filter);
        }

        public async Task<User?> GetUserAsync(string username)
        {
            var filter = Builders<User>.Filter.Where(x => x.Username == username);
            return await Users.Find(filter).FirstOrDefaultAsync();
        }
    }
}
