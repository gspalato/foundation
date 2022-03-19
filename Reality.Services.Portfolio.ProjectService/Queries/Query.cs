using Reality.Common.Entities;
using Reality.Services.Portfolio.ProjectService.Repositories;

namespace Reality.Services.Portfolio.ProjectService.Queries
{
    public class Query
    {
        public Task<IEnumerable<Project>> GetProjectsAsync([Service] IProjectRepository projectRepository) =>
            projectRepository.GetAllAsync();

        public Task<Project> GetProjectById(string id, [Service] IProjectRepository projectRepository) =>
            projectRepository.GetByIdAsync(id);
    }
}