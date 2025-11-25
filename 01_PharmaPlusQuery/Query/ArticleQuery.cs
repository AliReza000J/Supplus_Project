using _0_Framework.Application;
using _01_LampshadeQuery.Contracts.Comment;
using _01_PharmaPlusQuery.Contracts.Article;
using BlogManagement.Infrastructure.EFCore;
using CommentManagement.Domain.CommentAgg;
using CommnetManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;

namespace _01_PharmaPlusQuery.Query
{
    public class ArticleQuery : IArticleQuery
    {
        private readonly BlogContext _blogContext;
        private readonly CommentContext _commentContext;

        public ArticleQuery(BlogContext blogContext, CommentContext commentContext)
        {
            _blogContext = blogContext;
            _commentContext = commentContext;
        }

        public async Task<ArticleQueryModel> GetArticleDetails(string slug)
        {
            var article = await _blogContext.Articles
               .Include(x => x.Category)
               .Where(x => x.PublishDate <= DateTime.Now)
               .Select(x => new ArticleQueryModel
               {
                   Id = x.Id,
                   Title = x.Title,
                   CategoryName = x.Category.Name,
                   CategorySlug = x.Category.Slug,
                   Slug = x.Slug,
                   CanonicalAddress = x.CanonicalAddress,
                   Body = x.Body,
                   Keywords = x.Keywords,
                   MetaDescription = x.MetaDescription,
                   Picture = x.Picture,
                   PictureAlt = x.PictureAlt,
                   PictureTitle = x.PictureTitle,
                   PublishDate = x.PublishDate.ToFarsi(),
                   ShortDescription = x.ShortDescription,
               }).FirstOrDefaultAsync(x => x.Slug == slug);

            if (article == null)
                return article;

            if (!string.IsNullOrWhiteSpace(article.Keywords))
                article.KeywordList = article.Keywords.Split(",").ToList();


            var comments = await _commentContext.Comments
                .Where(x => !x.IsCanceled)
                .Where(x => x.IsConfirmed)
                .Where(x => x.Type == ((int)CommentType.Article))
                .Where(x => x.OwnerRecordId == article.Id)
                .Select(x => new CommentQueryModel
                {
                    Id = x.Id,
                    Message = x.Message,
                    Username = x.Username,
                    CreationDate = x.CreationDate.ToFarsi(),
                    Reply = x.AdminReply,
                    DislikeCount = x.DislikeCount,
                    LikeCount = x.LikeCount,
                    Rate = x.Rate,
                    ReplyDate = x.AdminReplyDate.ToFarsi(),
                    IsBuyer = x.IsBuyer,
                    UserId = long.Parse(x.UserIdentifier),
                    LastModifedDate = x.LastModified.ToFarsi()
                }).OrderByDescending(x => x.Id).ToListAsync();

            article.RateCount = comments.Count;

            if (comments.Any())
            {
                int sum = 0;
                foreach (var comment in comments.Where(
                    comment => comment.Rate >= 1 && comment.Rate <= 5))
                {
                    sum += comment.Rate;
                }

                article.AvgRate = (float)sum / (float)article.RateCount;
            }
            else
            {
                article.AvgRate = 0;
            }

            article.Comments = comments.Where(
                x => !string.IsNullOrWhiteSpace(x.Message)).ToList();
            article.CommentCount = article.Comments.Count;

            return article;
        }

        public async Task<List<ArticleQueryModel>> LatestArticles()
        {
            return await _blogContext.Articles
                .Include(x => x.Category)
                .Where(x => x.PublishDate <= DateTime.Now)
                .Select(x => new ArticleQueryModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    Picture = x.Picture,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    PublishDate = x.PublishDate.ToFarsi(),
                    ShortDescription = x.ShortDescription,
                    CategoryName = x.Category.Name,
                    CategorySlug = x.Category.Slug,
                }).ToListAsync();
        }

        public async Task<PagedResult<ArticleQueryModel>> Search(string value, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _blogContext.Articles
                .Include(x => x.Category)
                .AsQueryable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value.Trim().ToLower()
                             .Replace("ي", "ی")
                             .Replace("ك", "ک");

                query = query.Where(x =>
                    EF.Functions.Like(x.Title.ToLower(), $"%{value}%") ||
                    EF.Functions.Like(x.ShortDescription.ToLower(), $"%{value}%") ||
                    EF.Functions.Like(x.Keywords.ToLower(), $"%{value}%") ||
                    EF.Functions.Like(x.Category.Name.ToLower(), $"%{value}%"));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ArticleQueryModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    Picture = x.Picture,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    PublishDate = x.PublishDate.ToFarsi(),
                    ShortDescription = x.ShortDescription,
                    CategoryName = x.Category.Name,
                    CategorySlug = x.Category.Slug
                })
                .ToListAsync();

            return new PagedResult<ArticleQueryModel>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
    }
}

