using Foundation.Common.Entities;
using Foundation.Core.SDK.Auth.JWT;
using Foundation.Core.SDK.Database.Mongo;
using Foundation.Services.UPx.Types.Payloads;
using HotChocolate;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Foundation.Services.UPx.Types;

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

    public async Task<RegisterDisposalPayload> RegisterDisposalAsync(RegisterDisposalInput input,
    [Service] IAuthorizationService authorizationService, [Service] IRepository<EcobucksProfile> profileRepository,
    [Service] IRepository<DisposalClaim> disposalRepository, [Service] ILogger<Mutation> logger)
    {
        var invalidTokenError = ErrorBuilder.New()
            .SetMessage("Invalid token. Try regenerating your token.")
            .SetCode("401")
            .Build();

        var notOperatorError = ErrorBuilder.New()
            .SetMessage("User is not an operator. Only operators can register disposals.")
            .SetCode("401")
            .Build();

        var registerFailError = ErrorBuilder.New()
            .SetMessage("Failed to register disposal.")
            .SetCode("500")
            .Build();

        Console.WriteLine("=======> BEFORE VALIDATION <=======");
        var validationResult = await authorizationService.CheckAuthorizationAsync(input.OperatorToken);
        if (!validationResult.IsValid)
            throw new GraphQLException(invalidTokenError);
        Console.WriteLine("=======> AFTER VALIDATION <=======");

        Console.WriteLine("=======> BEFORE USER EXTRACTION <=======");
        var op = authorizationService.ExtractUser(validationResult);
        if (op == null)
            throw new GraphQLException(invalidTokenError);
        Console.WriteLine("=======> AFTER USER EXTRACTION <=======");

        Console.WriteLine("=======> BEFORE ECOBUCKS PROFILE FETCH <=======");
        var profile = await profileRepository.GetByIdAsync(op.Id);
        if (!profile.IsOperator)
            throw new GraphQLException(notOperatorError);
        Console.WriteLine("=======> AFTER ECOBUCKS PROFILE FETCH <=======");

        DisposalClaim disposal = new()
        {
            OperatorId = op.Id,
            Token = Guid.NewGuid().ToString(),
            IsClaimed = false,
            Disposals = input.Disposals,
        };

        try
        {
            Console.WriteLine("=======> BEFORE DISPOSAL DATABASE INSERTION <=======");
            await disposalRepository.InsertAsync(disposal);
            Console.WriteLine("=======> AFTER DISPOSAL DATABASE INSERTION <=======");
        }
        catch (Exception e)
        {
            Console.WriteLine("=======> ERROR ON DISPOSAL DATABASE INSERTION <=======");
            logger.LogError(e.Message);
            throw new GraphQLException(registerFailError);
        }

        return new RegisterDisposalPayload
        {
            Successful = true,
            Disposal = disposal
        };
    }

    public async Task<ClaimDisposalAndCreditsPayload> ClaimDisposalAndCreditsAsync(ClaimDisposalAndCreditsInput input,
    [Service] IAuthorizationService authorizationService, [Service] IRepository<EcobucksProfile> profileRepository,
    [Service] IRepository<DisposalClaim> disposalRepository)
    {
        var invalidTokenError = ErrorBuilder.New()
            .SetMessage("Invalid token. Try regenerating your token.")
            .SetCode("401")
            .Build();

        var notUserError = ErrorBuilder.New()
            .SetMessage("User is an operator. Only users can claim disposals.")
            .SetCode("401")
            .Build();

        var invalidDisposalError = ErrorBuilder.New()
            .SetMessage("Invalid disposal token.")
            .SetCode("400")
            .Build();

        var alreadyClaimedError = ErrorBuilder.New()
            .SetMessage("Disposal already claimed.")
            .SetCode("400")
            .Build();

        var claimFailedError = ErrorBuilder.New()
            .SetMessage("Failed to claim disposal.")
            .SetCode("500")
            .Build();

        var validationResult = await authorizationService.CheckAuthorizationAsync(input.UserToken);
        if (!validationResult.IsValid)
        {
            return new ClaimDisposalAndCreditsPayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        var user = authorizationService.ExtractUser(validationResult);
        if (user == null)
        {
            Console.WriteLine("Invalid token. Failed to extract user.");
            return new ClaimDisposalAndCreditsPayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        var profile = await profileRepository.GetByIdAsync(user.Id);

        DisposalClaim disposal;
        try
        {
            var disposalFilter = Builders<DisposalClaim>.Filter.Eq("Token", input.DisposalToken);

            disposal = await disposalRepository.Collection.Find(disposalFilter).FirstOrDefaultAsync();
            if (disposal is null)
            {
                return new ClaimDisposalAndCreditsPayload
                {
                    Successful = false,
                    Error = "Invalid disposal token."
                };
            }

            if (disposal.IsClaimed || disposal.UserId is not null)
            {
                return new ClaimDisposalAndCreditsPayload
                {
                    Successful = false,
                    Error = "Disposal already claimed."
                };
            }

            var disposalUpdate = Builders<DisposalClaim>.Update
                .Set("IsClaimed", true)
                .Set("UserId", user.Id);

            await disposalRepository.Collection.UpdateOneAsync(disposalFilter, disposalUpdate);

            var profileFilter = Builders<EcobucksProfile>.Filter.Eq(_ => _.Id, user.Id);
            var profileUpdate = Builders<EcobucksProfile>.Update.Inc("Credits", disposal.Credits);

            await profileRepository.Collection.UpdateOneAsync(profileFilter, profileUpdate);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new GraphQLException(claimFailedError);
        }

        return new ClaimDisposalAndCreditsPayload
        {
            Successful = true,
            Disposal = disposal
        };
    }
}
