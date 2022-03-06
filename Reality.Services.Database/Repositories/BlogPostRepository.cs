using Reality.Common.Entities;
using Reality.Common.Data;

namespace Reality.Services.Database.Repositories
{
    public class BlogPostRepository : BaseRepository<BlogPost>, IBlogPostRepository
    {
        public BlogPostRepository(IDatabaseContext dataContext) : base(dataContext)
        {

        }
    }
}