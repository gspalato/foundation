using Reality.Common.Entities;
using Reality.Services.Database.Repositories;

namespace Reality.Services.Database.Queries
{
    public class Query
    {
        public Task<IEnumerable<BlogPost>> GetBlogPostsAsync([Service] IBlogPostRepository blogPostRepository) =>
            blogPostRepository.GetAllAsync();

        public Task<BlogPost> GetBlogPostById(string id, [Service] IBlogPostRepository blogPostRepository) =>
            blogPostRepository.GetByIdAsync(id);
    }
}