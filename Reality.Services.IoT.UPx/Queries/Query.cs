using System.Globalization;
using HotChocolate;
using Reality.Common.Entities;
using Reality.Services.IoT.UPx.Repositories;

namespace Reality.Services.IoT.UPx.Queries
{
    public class Query
    {
        public Task<IEnumerable<Use>> GetUsesAsync([Service] IUseRepository useRepository) =>
            useRepository.GetAllAsync();

        public async Task<IEnumerable<Resume>> GetResumeAsync([Service] IUseRepository useRepository)
        {
            var uses = await useRepository.GetAllAsync();

            string GetDateString(int timestamp)
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).Date.ToShortDateString();
            }

            // Group by
            var perDateGroup = uses.GroupBy(_ => GetDateString(_.StartTimestamp));
            
            var list = perDateGroup.Select(_ => {
                return new Resume()
                {
                    Date = _.Key,
                    TotalDuration = _.Sum(_ => _.Duration),
                    EconomizedPlastic = _.Sum(_ => _.EconomizedPlastic),
                    DistributedWater = _.Sum(_ => _.DistributedWater),
                    BottleQuantityEquivalent = _.Sum(_ => _.BottleQuantityEquivalent)
                };
            });

            return list;
        }
    }
}