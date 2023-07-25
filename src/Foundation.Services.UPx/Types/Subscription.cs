using Foundation.Common.Entities;
using Foundation.Core.SDK.Database.Mongo;

namespace Foundation.Services.UPx.Types
{
    public class Subscription
    {
        [Subscribe(MessageType = typeof(IEnumerable<Resume>))]
        [Topic("OnStationUpdate")]
        public async Task<IEnumerable<Resume>> OnStationUpdate([Service] IRepository<Use> useRepository)
            => await new Query().GetResumesAsync(useRepository);

    }
}