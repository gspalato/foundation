using Microsoft.AspNetCore.Identity;
using Reality.Common.Entities;
using Reality.Common.Roles;
using Reality.Services.Identity.Repositories;
using RCommonEntities = Reality.Common.Entities;

namespace Reality.Services.Identity.Services
{
    public interface IUserService
    {
        Task<FullUser?> GetUserAsync(string username);
        Task<FullUser?> CreateUserAsync(string username, string password);
        Task DeleteUserAsync(string username);
    }

    public class UserService : IUserService
    {
        private readonly UserRepository UserRepository;
        private readonly IPasswordHasher<string> Hasher;


        public UserService(UserRepository userRepository, IPasswordHasher<string> hasher)
        {
            UserRepository = userRepository;
            Hasher = hasher;
        }

        public async Task<Reality.Common.Entities.FullUser?> CreateUserAsync(string username, string password)
        {
            if (await GetUserAsync(username) is not null)
                return null;

            var hashedPassword = Hasher.HashPassword(username, password);

            var user = new Reality.Common.Entities.FullUser()
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = hashedPassword,
                Roles = Array.Empty<Role>()
            };

            await UserRepository.CreateUserAsync(user);

            return user;
        }

        public async Task DeleteUserAsync(string username)
        {
            await UserRepository.DeleteUserAsync(username);
        }

        public async Task<Reality.Common.Entities.FullUser?> GetUserAsync(string username)
        {
            return await UserRepository.GetUserAsync(username);
        }
    }
}
