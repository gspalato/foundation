using HotChocolate;
using Reality.Common.Entities;
using Reality.SDK.Database.Mongo;

namespace Reality.Services.Portfolio.Types
{
    public class Query
    {
        public async Task<List<Project>> GetProjectsAsync([Service] IRepository<Project> projectRepository)
        {
            return await projectRepository.GetAllAsync();
        }
    }
}