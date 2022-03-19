using Reality.Common.Entities;
using Reality.Common.Data;
using Reality.Common.Repositories;

namespace Reality.Services.Portfolio.ProjectService.Repositories
{
    public interface IProjectRepository : IBaseRepository<Project>
    {

    }

    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        public ProjectRepository(IDatabaseContext dataContext) : base(dataContext)
        {

        }
    }
}