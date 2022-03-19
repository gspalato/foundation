using Reality.Common.Entities;
using Reality.Common.Data;
using Reality.Common.Repositories;

namespace Reality.Services.Portfolio.BlogService.Repositories
{
    public interface IBlogPostRepository : IBaseRepository<BlogPost>
    {

    }

    public class BlogPostRepository : BaseRepository<BlogPost>, IBlogPostRepository
    {
        public BlogPostRepository(IDatabaseContext dataContext) : base(dataContext)
        {

        }
    }
}