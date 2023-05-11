using Reality.Common.Data;
using Reality.Common.Repositories;
using Reality.Services.IoT.UPx.Entities;

namespace Reality.Services.IoT.UPx.Repositories
{
    public interface IUseRepository : IBaseRepository<Use>
    {

    }

    public class UseRepository : BaseRepository<Use>, IUseRepository
    {
        public UseRepository(IDatabaseContext dataContext) : base(dataContext)
        {

        }
    }
}