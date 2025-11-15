using _0_Framework.Application;
using _01_LampshadeQuery.Contracts.Comment;
using _01_PharmaPlusQuery.Contracts.Article;
using BlogManagement.Infrastructure.EFCore;
using CommentManagement.Domain.CommentAgg;
using CommnetManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
                    IsBuyer = x.IsBuyer
                }).OrderByDescending(x => x.Id).ToListAsync();

            if(comments.Any())
            {
                int sum = 0;
                foreach (var comment in comments.Where(comment => comment.Rate >= 1 && comment.Rate <= 5))
                {
                    sum += comment.Rate;
                }

                int count = comments.Count;

                article.AvgRate = sum / count;
            }
            else
            {
                article.AvgRate = 0;
            }

                article.Comments = comments;

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

        public async Task<List<ArticleQueryModel>> Search(string value)
        {
            var query = _blogContext.Articles
            .Include(x => x.Category)
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
            })
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
                    EF.Functions.Like(x.CategoryName.ToLower(), $"%{value}%"));
            }

            var articles = await query.OrderByDescending(x => x.Id).ToListAsync();

            return articles;
        }
    }
}
