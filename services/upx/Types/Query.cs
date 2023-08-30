using Foundation.Common.Entities;
using Foundation.Core.SDK.Auth.JWT;
using Foundation.Core.SDK.Database.Mongo;
using HotChocolate;
using MongoDB.Driver;

namespace Foundation.Services.UPx.Types;

// foundation generate query
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

    // Ecobucks
    public async Task<EcobucksProfile?> GetEcobucksProfileAsync(string token,
    [Service] IAuthorizationService authorizationService, [Service] IRepository<DisposalClaim> disposalRepository,
    [Service] IRepository<EcobucksProfile> profileRepository)
    {
        var invalidTokenError = ErrorBuilder.New()
            .SetMessage("Invalid token. Try regenerating your token.")
            .SetCode("401")
            .Build();

        var validationResult = await authorizationService.CheckAuthorizationAsync(token);
        if (!validationResult.IsValid)
            throw new GraphQLException(invalidTokenError);

        var user = authorizationService.ExtractUser(validationResult);
        if (user == null)
            throw new GraphQLException(invalidTokenError);

        EcobucksProfile profile;
        try
        {
            profile = await profileRepository.GetByIdAsync(user.Id);
            if (profile == null)
            {
                // Initialize a Ecobucks profile.
                profile = new EcobucksProfile()
                {
                    Id = user.Id,
                    Name = user.Username,
                    Username = user.Username,
                    Credits = 0,
                    IsOperator = false
                };

                await profileRepository.InsertAsync(profile);

                Console.WriteLine("Created a new Ecobucks profile. ({0})", user.Username);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null!;
        }

        return profile;
    }

    public async Task<List<DisposalClaim>> GetUserDisposalsAsync(string token,
    [Service] IAuthorizationService authorizationService, [Service] IRepository<DisposalClaim> disposalRepository)
    {
        var invalidTokenError = ErrorBuilder.New()
            .SetMessage("Invalid token. Try regenerating your token.")
            .SetCode("401")
            .Build();

        var validationResult = await authorizationService.CheckAuthorizationAsync(token);
        if (!validationResult.IsValid)
            throw new GraphQLException(invalidTokenError);

        var user = authorizationService.ExtractUser(validationResult);
        if (user == null)
            throw new GraphQLException(invalidTokenError);

        var disposals = await disposalRepository.Collection.Find(_ => _.Id == user.Id).ToListAsync();

        return disposals;
    }
}
