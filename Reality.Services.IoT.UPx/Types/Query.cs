﻿using System.Globalization;
using HotChocolate;
using Reality.Common.Entities;
using Reality.Services.IoT.UPx.Repositories;

namespace Reality.Services.IoT.UPx.Types
{
    public class Query
    {
        public async Task<IEnumerable<Use>> GetUsesAsync([Service] IUseRepository useRepository) =>
            await useRepository.GetUsesAsync();

        public async Task<IEnumerable<Resume>> GetResumesAsync([Service] IUseRepository useRepository)
        {
            var uses = await useRepository.GetUsesAsync();

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

        public async Task<Resume> GetTotalResumeAsync([Service] IUseRepository useRepository)
        {
            var uses = await useRepository.GetUsesAsync();

            return new Resume()
            {
                Date = "Total",
                TotalDuration = uses.Sum(_ => _.Duration),
                EconomizedPlastic = uses.Sum(_ => _.EconomizedPlastic),
                DistributedWater = uses.Sum(_ => _.DistributedWater),
                BottleQuantityEquivalent = uses.Sum(_ => _.BottleQuantityEquivalent)
            };
        }
    }
}