using Foundation.Common.Entities;
using Foundation.Core.SDK.Database.Mongo;
using Foundation.Services.UPx.Types.Payloads;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SDKAuth = Foundation.Core.SDK.Auth.JWT;

namespace Foundation.Services.UPx.Controllers;

public class RefillStationController : Controller
{
    private SDKAuth::IAuthorizationService AuthorizationService { get; }

    private IRepository<Use> UseRepository { get; }

    public RefillStationController(SDKAuth::IAuthorizationService authorizationService, IRepository<Use> useRepository)
    {
        AuthorizationService = authorizationService;

        UseRepository = useRepository;
    }

    [HttpGet]
    [Route("refillstation/uses")]
    public Task<List<Use>> GetUsesAsync() =>
        UseRepository.GetAllAsync();

    [HttpGet]
    [Route("refillstation/resumes")]
    public async Task<List<Resume>> GetResumesAsync()
    {
        var uses = await UseRepository.GetAllAsync();

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

    [HttpGet]
    [Route("refillstation/resumes/total")]
    public async Task<Resume> GetTotalResumeAsync()
    {
        var uses = await UseRepository.GetAllAsync();

        return new Resume()
        {
            TotalUses = uses.Count,
            TotalDuration = uses.Sum(_ => _.Duration),
            EconomizedPlastic = uses.Sum(_ => _.EconomizedPlastic),
            DistributedWater = uses.Sum(_ => _.DistributedWater),
            BottleQuantityEquivalent = uses.Sum(_ => _.BottleQuantityEquivalent)
        };
    }

    [HttpPut]
    [Authorize]
    [Route("refillstation/")]
    public async Task<RegisterStationUsePayload> RegisterStationUseAsync(RegisterStationUseInput input)
    {
        string? token = await HttpContext.GetTokenAsync("access_token");
        if (token is null || token.Length is 0)
        {
            StatusCode(401);
            return new RegisterStationUsePayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        var result = await AuthorizationService.CheckAuthorizationAsync(token);
        if (!result.IsValid)
        {
            Console.WriteLine("============== INVALID TOKEN ==================");
            StatusCode(401);
            return new RegisterStationUsePayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        var roles = AuthorizationService.ExtractRoles(result).Select(r => (int)r);

        bool allowed;
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
            StatusCode(401);
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

        await UseRepository.InsertAsync(use);

        return new RegisterStationUsePayload { Successful = true };
    }
}