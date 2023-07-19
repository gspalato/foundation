using Reality.Common.Entities;
using Reality.SDK.Database.Mongo;

namespace Reality.Services.UPx.Types
{
    public class Subscription
    {
        [Subscribe(MessageType = typeof(IEnumerable<Resume>))]
        [Topic("OnStationUpdate")]
        public async Task<IEnumerable<Resume>> OnStationUpdate([Service] IRepository<Use> useRepository)
            => await new Query().GetResumesAsync(useRepository);

    }
}