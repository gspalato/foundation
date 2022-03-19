using Reality.Common.Entities;
using Reality.Services.Portfolio.ProjectService.Repositories;

namespace Reality.Services.ProjectService.Mutations
{
    public class Mutation
    {
        public async Task<IEnumerable<Project>> RefreshProjectDatabaseAsync([Service] IProjectRepository projectRepository)
        {
            // Add refresh logic here (grab from GitHub, etc.)
            return await projectRepository.GetAllAsync();
        }   
    }
}
