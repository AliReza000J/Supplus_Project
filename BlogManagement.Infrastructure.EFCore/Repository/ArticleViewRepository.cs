using _0_Framework.Infrastructure;
using BlogManagement.Domain.UserArticleInteractionAgg;

namespace BlogManagement.Infrastructure.EFCore.Repository
{
    public class ArticleViewRepository: BaseRepository<long, ArticleView>, IArticleViewRepository
    {
        private readonly BlogContext _context;

        public ArticleViewRepository(BlogContext context) : base(context)
        {
            _context = context;
        }
    }
}
