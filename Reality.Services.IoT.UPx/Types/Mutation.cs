using HotChocolate.Subscriptions;
using Reality.Common.Entities;
using Reality.Common.Services;
using Reality.Services.IoT.UPx.Repositories;

namespace Reality.Services.IoT.UPx.Types
{
    public class Mutation
    {
        public async Task<bool> RegisterStationUseAsync(int startTimestamp, int endTimestamp, int duration,
            double distributedWater, double economizedPlastic, double bottleQuantityEquivalent, string token,
            [Service] IUseRepository useRepository, [Service] IAuthorizationService authorizationService,
            [Service] ITopicEventSender sender)
        {
            if (token.Length is 0)
                return false;

            var result = await authorizationService.CheckAuthorizationAsync(token);
            var roles = authorizationService.ExtractRoles(result).Select(r => (int)r);
            bool allowed;

            if (!result.IsValid)
                return false;

            try
            {
                allowed = roles.Any(r => r <= 2);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                allowed = false;
            }

            // Check if role is Project or above.
            if (!allowed)
                return false;

            Use use = new() {
                Id = Guid.NewGuid().ToString(),
                StartTimestamp = startTimestamp,
                EndTimestamp = endTimestamp,
                Duration = duration,
                DistributedWater = distributedWater,
                EconomizedPlastic = economizedPlastic,
                BottleQuantityEquivalent = bottleQuantityEquivalent
            };

            await useRepository.CreateUseAsync(use);

            await sender.CompleteAsync(nameof(Reality.Services.IoT.UPx.Types.Subscription.OnStationUpdate));

            return true;
        }
    }
}
