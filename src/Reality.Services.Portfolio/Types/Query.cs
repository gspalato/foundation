using HotChocolate;
using Reality.SDK.Database.Mongo;
using Project = Reality.Common.Entities.Project;

namespace Reality.Services.Portfolio.Types
{
    public class Query
    {
        public async Task<IEnumerable<Project>> GetProjectsAsync([Service] IRepository<Project> projectRepository)
        {
            return await projectRepository.GetAllAsync();
        }
    }
}