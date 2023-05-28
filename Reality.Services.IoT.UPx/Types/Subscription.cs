using Reality.Common.Entities;
using Reality.Services.IoT.UPx.Repositories;

namespace Reality.Services.IoT.UPx.Types
{
    public class Subscription
    {
        [Subscribe(MessageType = typeof(IEnumerable<Resume>))]
        [Topic("OnStationUpdate")]
        public async Task<IEnumerable<Resume>> OnStationUpdate([Service] IUseRepository useRepository)
            => await new Query().GetResumesAsync(useRepository);

    }
}