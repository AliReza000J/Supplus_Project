using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Article;
using _01_PharmaPlusQuery.Contracts.ArticleCategory;
using BlogManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;

namespace _01_PharmaPlusQuery.Query
{
    public class ArticleCategoryQuery : IArticleCategoryQuery
    {
        private readonly BlogContext _blogContext;

        public ArticleCategoryQuery(BlogContext context)
        {
            _blogContext = context;
        }

        public async Task<List<ArticleCategoryQueryModel>> GetArticleCategories(CancellationToken cancellation)
        {
            return await _blogContext.ArticleCategories
                .Include(x => x.Articles)
                .Select(x => new ArticleCategoryQueryModel
                {
                    Name = x.Name,
                    Picture = x.Picture,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    Slug = x.Slug,
                    ArticlesCount = x.Articles.Count
                }).ToListAsync(cancellation);
        }

        public async Task<ArticleCategoryQueryModel> GetArticleCategory(string slug, CancellationToken cancellation = default)
        {
            var category = await _blogContext.ArticleCategories
                .Where(x => x.Slug == slug)
                .Select(x => new ArticleCategoryQueryModel
                {
                    Slug = x.Slug,
                    Name = x.Name,
                    Description = x.Description,
                    Picture = x.Picture,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    Keywords = x.Keywords,
                    MetaDescription = x.MetaDescription,
                    CanonicalAddress = x.CanonicalAddress,

                    ArticlesCount = x.Articles.Count(),

                    ArticleIds = x.Articles.Select(a => a.Id).ToList()
                })
                .FirstOrDefaultAsync(cancellation);

            if (category == null)
                return null;

            // Keywords Safe
            if (!string.IsNullOrWhiteSpace(category.Keywords))
                category.KeywordList = category.Keywords.Split(",").ToList();

            if (category.ArticleIds != null && category.ArticleIds.Any())
            {
                category.Articles = await _blogContext.Articles
                    .Where(a => category.ArticleIds.Contains(a.Id))
                    .Select(a => new ArticleQueryModel
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Slug = a.Slug,
                        Picture = a.Picture,
                        PictureAlt = a.PictureAlt,
                        PictureTitle = a.PictureTitle,
                        PublishDate = a.PublishDate.ToFarsi(),
                        ShortDescription = a.ShortDescription,
                        CategoryName = a.Category.Name,
                        CategorySlug = a.Category.Slug
                    })
                    .OrderByDescending(a => a.Id)
                    .ToListAsync(cancellation);
            }
            else
            {
                category.Articles = new List<ArticleQueryModel>();
            }

            return category;
        }

    }
}
