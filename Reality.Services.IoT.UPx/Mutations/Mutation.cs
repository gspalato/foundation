using Reality.Common.Entities;
using Reality.Services.IoT.UPx.Repositories;
using Reality.Services.IoT.UPx.Services;

namespace Reality.Services.IoT.UPx.Mutations
{
    public class Mutation
    {
        public async Task<bool> RegisterFountainUseAsync(int startTimestamp, int endTimestamp, int duration,
            double economizedWater, double economizedPlastic, string token,
            [Service] IUseRepository useRepository, [Service] UseService useService)
        {
            if (!(await useService.CheckAuthenticationAsync(token)))
                return false;

            await useRepository.InsertAsync(new() {
                StartTimestamp = startTimestamp,
                EndTimestamp = endTimestamp,
                Duration = duration,
                EconomizedWater = economizedWater,
                EconomizedPlastic = economizedPlastic
            });

            return true;
        }
    }
}
