using Foundation.Common.Entities;
using Foundation.Common.Services;
using Foundation.SDK.Database.Mongo;
using Foundation.Services.UPx.Types.Payloads;

namespace Foundation.Services.UPx.Types
{
    public class Mutation
    {
        public async Task<RegisterStationUsePayload> RegisterStationUseAsync(RegisterStationUseInput input,
            [Service] IRepository<Use> useRepository, [Service] IAuthorizationService authorizationService)
        {
            if (input.Token.Length is 0)
                return new RegisterStationUsePayload
                {
                    Successful = false,
                    Error = "Token is empty."
                };

            var result = await authorizationService.CheckAuthorizationAsync(input.Token);
            var roles = authorizationService.ExtractRoles(result).Select(r => (int)r);
            bool allowed;

            Console.WriteLine("Claims: " + String.Join(", ", result.Claims.Select(c => c.Key + ": " + c.Value)));

            if (!result.IsValid)
            {
                Console.WriteLine("Invalid token.");
                return new RegisterStationUsePayload
                {
                    Successful = false,
                    Error = "Invalid token."
                };
            }

            try
            {
                Console.WriteLine("Roles: " + String.Join(", ", roles));
                allowed = roles.Any(r => r <= 2);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                allowed = false;
            }

            // Check if role is Project or above.
            if (!allowed)
            {
                Console.WriteLine("Unauthorized.");
                return new RegisterStationUsePayload
                {
                    Successful = false,
                    Error = "Unauthorized."
                };
            }

            Use use = new()
            {
                StartTimestamp = input.StartTimestamp,
                EndTimestamp = input.EndTimestamp,
                Duration = input.Duration,
                DistributedWater = input.DistributedWater,
                EconomizedPlastic = input.EconomizedPlastic,
                BottleQuantityEquivalent = input.BottleQuantityEquivalent
            };

            await useRepository.InsertAsync(use);

            return new RegisterStationUsePayload { Successful = true };
        }
    }
}
