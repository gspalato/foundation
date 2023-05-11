using Reality.Services.IoT.UPx.Entities;
using Reality.Services.IoT.UPx.Repositories;

namespace Reality.Services.IoT.UPx.Queries
{
    public class Query
    {
        public Task<IEnumerable<Use>> GetUsesAsync([Service] IUseRepository useRepository) =>
            useRepository.GetAllAsync();
    }
}