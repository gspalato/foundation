using HotChocolate;
using HotChocolate.Types;
using Reality.Common.Entities;
using Reality.Services.UPx.Repositories;

namespace Reality.Services.UPx.Types
{
    public class Subscription
    {
        [Subscribe(MessageType = typeof(IEnumerable<Resume>))]
        [Topic("OnStationUpdate")]
        public async Task<IEnumerable<Resume>> OnStationUpdate([Service] IUseRepository useRepository)
            => await new Query().GetResumesAsync(useRepository);

    }
}