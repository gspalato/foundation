using Reality.Common.Entities;
using Reality.Common.Services;
using Reality.Services.IoT.UPx.Repositories;

namespace Reality.Services.IoT.UPx.Mutations
{
    public class Mutation
    {
        public async Task<bool> RegisterFountainUseAsync(int startTimestamp, int endTimestamp, int duration,
            double economizedWater, double economizedPlastic, string token,
            [Service] IUseRepository useRepository, [Service] IAuthorizationService authorizationService)
        {
            var result = authorizationService.CheckAuthorizationAsync(token);
            var roles = authorizationService.ExtractRoles(result).Select(r => (int)r);

            // Check if role is Project or above.
            if (!roles.Any(r => r >= 2))
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
