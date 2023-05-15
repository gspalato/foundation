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

            var perDateGroup = uses.GroupBy(_ => {
                var date = DateTimeOffset.FromUnixTimeSeconds(_.StartTimestamp);
                return (date.Day, date.Month, date.Year);
            });
            
            var list = perDateGroup.Select(_ => {
                return new Resume()
                {
                    Timestamp = new DateTime(_.Key.Year, _.Key.Month, _.Key.Day, 0, 0, 0, 0).Second,
                    TotalDuration = _.Sum(_ => _.Duration),
                    EconomizedPlastic = _.Sum(_ => _.EconomizedPlastic),
                    EconomizedWater = _.Sum(_ => _.EconomizedWater)
                };
            });

            return list;
        }
    }
}