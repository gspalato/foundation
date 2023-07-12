using HotChocolate;
using Reality.Services.Portfolio.Repositories;
using Project = Reality.Common.Entities.Project;

namespace Reality.Services.Portfolio.Types
{
    public class Query
    {
        public async Task<IEnumerable<Project>> GetProjectsAsync([Service] IProjectRepository projectRepository)
        {
            return await projectRepository.GetAllAsync();
        }
    }
}