using Reality.Common.Data;
using Reality.Common.Entities;
using Reality.Common.Repositories;

namespace Reality.Services.Portfolio.Repositories;

public interface IProjectRepository : IBaseRepository<Project>
{

}

public class ProjectRepository : BaseRepository<Project>, IProjectRepository
{
    public ProjectRepository(IDatabaseContext context) : base(context)
    {

    }
}