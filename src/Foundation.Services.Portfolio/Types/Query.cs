using HotChocolate;
using Foundation.Common.Entities;
using Foundation.SDK.Database.Mongo;

namespace Foundation.Services.Portfolio.Types
{
    public class Query
    {
        public async Task<List<Project>> GetProjectsAsync([Service] IRepository<Project> projectRepository)
        {
            return await projectRepository.GetAllAsync();
        }
    }
}