using Foundation.Common.Entities;
using Foundation.Core.SDK.Database.Mongo;
using HotChocolate;

namespace Foundation.Services.UPx.Types;

public class Query
{
    public Task<List<Use>> GetUsesAsync([Service] IRepository<Use> useRepository) =>
        useRepository.GetAllAsync();

    public async Task<List<Resume>> GetResumesAsync([Service] IRepository<Use> useRepository)
    {
        var uses = await useRepository.GetAllAsync();

        static long GetDayTimestamp(int timestamp)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified).AddSeconds(timestamp).Date;
            var offset = new DateTimeOffset(date);
            return offset.ToUnixTimeSeconds();
        }

        // Group by
        var perDateGroup = uses.GroupBy(_ => GetDayTimestamp(_.StartTimestamp));

        var list = perDateGroup.Select(_ =>
        {
            return new Resume()
            {
                Timestamp = _.Key,
                TotalUses = _.Count(),
                TotalDuration = _.Sum(_ => _.Duration),
                EconomizedPlastic = _.Sum(_ => _.EconomizedPlastic),
                DistributedWater = _.Sum(_ => _.DistributedWater),
                BottleQuantityEquivalent = _.Sum(_ => _.BottleQuantityEquivalent)
            };
        });

        return list.ToList();
    }

    public async Task<Resume> GetTotalResumeAsync([Service] IRepository<Use> useRepository)
    {
        var uses = await useRepository.GetAllAsync();

        return new Resume()
        {
            TotalUses = uses.Count,
            TotalDuration = uses.Sum(_ => _.Duration),
            EconomizedPlastic = uses.Sum(_ => _.EconomizedPlastic),
            DistributedWater = uses.Sum(_ => _.DistributedWater),
            BottleQuantityEquivalent = uses.Sum(_ => _.BottleQuantityEquivalent)
        };
    }
}
