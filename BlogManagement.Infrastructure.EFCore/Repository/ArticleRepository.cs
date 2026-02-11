using _0_Framework.Application;
using _0_Framework.Infrastructure;
using BlogManagement.Application.Contracts.Article;
using BlogManagement.Domain.ArticleAgg;
using Microsoft.EntityFrameworkCore;

namespace BlogManagement.Infrastructure.EFCore.Repository
{
    public class ArticleRepository : BaseRepository<long, Article>, IArticleRepository
    {
        private readonly BlogContext _context;

        public ArticleRepository(BlogContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EditArticle?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _context.Articles.Select(x => new EditArticle
            {
                Id = x.Id,
                CanonicalAddress = x.CanonicalAddress,
                CategoryId = x.CategoryId,
                Body = x.Body,
                Keywords = x.Keywords,
                MetaDescription = x.MetaDescription,
                PictureAlt = x.PictureAlt,
                PictureTitle = x.PictureTitle,
                PublishDate = x.PublishDate.ToFarsi().ToPersianNumber(),
                ShortDescription = x.ShortDescription,
                Slug = x.Slug,
                Title = x.Title,
            }).FirstOrDefaultAsync(x => x.Id == id, cancellation);
        }

        public async Task<Article> GetWithCategory(long id, CancellationToken cancellation)
        {
            return await _context.Articles.Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id, cancellation);
        }

        public async Task<List<ArticleViewModel>> Search(ArticleSearchModel searchModel, CancellationToken cancellation)
        {
            var query = _context.Articles.Select(x => new ArticleViewModel
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                Category = x.Category.Name,
                Picture = x.Picture,
                PublishDate = x.PublishDate.ToFarsi().ToPersianNumber(),
                ShortDescription = x.ShortDescription,
                Title = x.Title,
                ViewCount = x.ViewCount
            });

            if (!string.IsNullOrWhiteSpace(searchModel.Title))
                query = query.Where(x => x.Title.Contains(searchModel.Title));

            if (searchModel.CategoryId > 0)
                query = query.Where(x => x.CategoryId == searchModel.CategoryId);

            return await query.OrderByDescending(x => x.Id).ToListAsync(cancellation);
        }
    }
}
