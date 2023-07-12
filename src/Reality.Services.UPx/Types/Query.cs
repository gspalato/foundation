﻿using Reality.Common.Entities;
using Reality.Services.UPx.Repositories;

namespace Reality.Services.UPx.Types
{
    public class Query
    {
        public Task<IEnumerable<Use>> GetUsesAsync([Service] IUseRepository useRepository) =>
            useRepository.GetAllAsync();

        public async Task<IEnumerable<Resume>> GetResumesAsync([Service] IUseRepository useRepository)
        {
            var uses = await useRepository.GetAllAsync();

            long GetDayTimestamp(int timestamp)
            {
                var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified).AddSeconds(timestamp).Date;
                var offset = new DateTimeOffset(date);
                return offset.ToUnixTimeSeconds();
            }

            // Group by
            var perDateGroup = uses.GroupBy(_ => GetDayTimestamp(_.StartTimestamp));
            
            var list = perDateGroup.Select(_ => {
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

            return list;
        }

        public async Task<Resume> GetTotalResumeAsync([Service] IUseRepository useRepository)
        {
            var uses = await useRepository.GetAllAsync();

            return new Resume()
            {
                TotalUses = uses.Count(),
                TotalDuration = uses.Sum(_ => _.Duration),
                EconomizedPlastic = uses.Sum(_ => _.EconomizedPlastic),
                DistributedWater = uses.Sum(_ => _.DistributedWater),
                BottleQuantityEquivalent = uses.Sum(_ => _.BottleQuantityEquivalent)
            };
        }
    }
}