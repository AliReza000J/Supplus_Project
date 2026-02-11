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
        private readonly ICacheService _cacheService;

        public ArticleQuery(BlogContext blogContext,
            CommentContext commentContext, ICacheService cache)
        {
            _blogContext = blogContext;
            _commentContext = commentContext;
            _cacheService = cache;
        }

        public async Task<ArticleQueryModel> GetArticleDetails(string slug, CancellationToken cancellation = default)
        {
            var cacheKey = $"article-details-{slug}";

            // -----------------------------
            // 1) Get static parts from cache
            // -----------------------------
            var article = await _cacheService.GetOrCreateAsync(
                cacheKey,
                factory: async () =>
                {
                    return await _blogContext.Articles
                        .AsNoTracking()
                        .Where(x => x.PublishDate <= DateTime.Now)
                        .Where(x => x.Slug == slug)
                        .Select(x => new ArticleQueryModel
                        {
                            Id = x.Id,
                            Title = x.Title,
                            Slug = x.Slug,
                            CanonicalAddress = x.CanonicalAddress,
                            Body = x.Body,
                            Keywords = x.Keywords,
                            MetaDescription = x.MetaDescription,
                            Picture = x.Picture,
                            PictureAlt = x.PictureAlt,
                            PictureTitle = x.PictureTitle,
                            ShortDescription = x.ShortDescription,
                            PublishDate = x.PublishDate.ToFarsi().ToPersianNumber(),

                            CategoryName = x.Category.Name,
                            CategorySlug = x.Category.Slug
                        })
                        .FirstOrDefaultAsync(cancellation);
                },
                absoluteExpiration: TimeSpan.FromHours(1),
                slidingExpiration: TimeSpan.FromMinutes(20),
                size: 1
            );

            if (article == null)
                return null;

            // -----------------------------
            // 2) Split Keywords
            // -----------------------------
            if (!string.IsNullOrWhiteSpace(article.Keywords))
                article.KeywordList = article.Keywords.Split(",").ToList();

            long articleId = article.Id;

            // -----------------------------
            // 3) Get comments (REAL-TIME)
            // -----------------------------
            var comments = await _commentContext.Comments
                .AsNoTracking()
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Article)
                .Where(x => x.RecordId == articleId)
                .OrderByDescending(x => x.Id)
                .Select(x => new CommentQueryModel
                {
                    Id = x.Id,
                    Message = x.Message,
                    Username = x.Username,
                    CreationDate = x.CreationDate.ToFarsi().ToPersianNumber(),
                    Reply = x.AdminReply,
                    DislikeCount = x.DislikeCount,
                    LikeCount = x.LikeCount,
                    Rate = x.Rate,
                    ReplyDate = x.AdminReplyDate.ToFarsi().ToPersianNumber(),
                    HasEdited = x.CreationDate == x.LastModified,
                    UserId = x.UserId,
                    LastModifedDate = x.LastModified.ToFarsi().ToPersianNumber(),
                    LikedByUserIds = new List<long>(),
                    DislikedByUserIds = new List<long>()
                })
                .ToListAsync(cancellation);

            var commentIds = comments.Select(c => c.Id).ToList();

            var reactions = await _commentContext.CommentReactions
                .Where(r => commentIds.Contains(r.CommentId))
                .AsNoTracking()
                .Select(r => new
                {
                    r.CommentId,
                    r.Reaction,
                    r.UserId
                })
                .ToListAsync(cancellation);

            var reactionsLookup = reactions
                .GroupBy(r => r.CommentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var comment in comments)
            {
                if (!reactionsLookup.TryGetValue(comment.Id, out var relatedReactions))
                    continue;

                comment.LikedByUserIds = relatedReactions
                    .Where(r => r.Reaction == ReactionType.Like)
                    .Select(r => r.UserId)
                    .Where(id => id > 0)
                    .ToList();

                comment.DislikedByUserIds = relatedReactions
                    .Where(r => r.Reaction == ReactionType.Dislike)
                    .Select(r => r.UserId)
                    .Where(id => id > 0)
                    .ToList();
            }

            // -----------------------------
            // 4) Calculate RateCount / AvgRate / CommentCount
            // -----------------------------
            int rateCount =
                comments.Count(c => c.Rate >= 1 && c.Rate <= 5);

            int commentCount =
                comments.Count(c => !string.IsNullOrWhiteSpace(c.Message));

            float avgRate = rateCount == 0
                ? 0
                : (float)comments
                    .Where(c => c.Rate >= 1 && c.Rate <= 5)
                    .Sum(c => c.Rate) / rateCount;

            // Only comments that have a message
            var filteredComments = comments
                .Where(c => !string.IsNullOrWhiteSpace(c.Message))
                .ToList();

            // -----------------------------
            // 5) Assign dynamic results
            // -----------------------------
            article.Comments = filteredComments;
            article.CommentCount = commentCount;
            article.RateCount = rateCount;
            article.AvgRate = avgRate;

            return article;
        }

        public async Task<List<ArticleQueryModel>> GetTopRatedArticles(int take = 10, CancellationToken cancellation = default)
        {
            if (take <= 0)
                take = 10;

            var publishedArticledIds = await _blogContext.Articles
                .Where(x => x.PublishDate <= DateTime.Now)
                .Select(x => x.Id)
                .ToListAsync(cancellation);

            if (!publishedArticledIds.Any())
                return new List<ArticleQueryModel>();

            // ---------- Calculate AvgRate from Comments ----------
            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Article)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.RecordId)
                .Select(g => new
                {
                    ArticleId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync(cancellation);

            var popularIds = avgRates
                .Where(x => publishedArticledIds.Contains(x.ArticleId))
                .OrderByDescending(x => x.AvgRate)
                .ThenByDescending(x => x.RateCount)
                .Take(take)
                .Select(x => x.ArticleId)
                .ToList();

            if (!popularIds.Any())
                return new List<ArticleQueryModel>();

            // ---------- Load Article Info ----------
            var articles = await _blogContext.Articles
                .Include(x => x.Category)
                .Where(x => x.PublishDate <= DateTime.Now
                && popularIds.Contains(x.Id))
                .Select(x => new ArticleQueryModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    Picture = x.Picture,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    PublishDate = x.PublishDate.ToFarsi().ToPersianNumber(),
                    ShortDescription = x.ShortDescription,
                    CategoryName = x.Category.Name,
                    CategorySlug = x.Category.Slug,
                }).AsNoTracking().ToListAsync(cancellation);

            foreach (var article in articles)
            {
                var rateInfo = avgRates.FirstOrDefault(x => x.ArticleId == article.Id);
                if (rateInfo != null)
                {
                    article.AvgRate = (float)rateInfo.AvgRate;
                    article.RateCount = rateInfo.RateCount;
                }
            }

            return articles
                .OrderByDescending(x => x.AvgRate)
                .ThenByDescending(x => x.RateCount)
                .ToList();
        }

        public async Task<List<ArticleQueryModel>> GetLatestArticles(CancellationToken cancellation)
        {
            // ---------- Calculate AvgRate from Comments ----------
            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Article)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.RecordId)
                .Select(g => new
                {
                    ArticleId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync(cancellation);

            var articles = await _blogContext.Articles
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
                    PublishDate = x.PublishDate.ToFarsi().ToPersianNumber(),
                    ShortDescription = x.ShortDescription,
                    CategoryName = x.Category.Name,
                    CategorySlug = x.Category.Slug,
                }).ToListAsync(cancellation);

            foreach (var article in articles)
            {
                var rateInfo = avgRates.FirstOrDefault(x => x.ArticleId == article.Id);
                if (rateInfo != null)
                {
                    article.AvgRate = (float)rateInfo.AvgRate;
                    article.RateCount = rateInfo.RateCount;
                }
            }

            return articles;
        }

        public async Task<PagedResult<ArticleQueryModel>> Search(string value, int page, int pageSize, CancellationToken cancellation)
        {
            // ---------- Calculate AvgRate from Comments ----------
            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Article)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.RecordId)
                .Select(g => new
                {
                    ArticleId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync(cancellation);

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _blogContext.Articles
                .Include(x => x.Category)
                .Where(x => x.PublishDate <= DateTime.Now)
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

            var totalCount = await query.CountAsync(cancellation);

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
                    PublishDate = x.PublishDate.ToFarsi().ToPersianNumber(),
                    ShortDescription = x.ShortDescription,
                    CategoryName = x.Category.Name,
                    CategorySlug = x.Category.Slug
                })
                .ToListAsync(cancellation);

            foreach (var article in items)
            {
                var rateInfo = avgRates.FirstOrDefault(x => x.ArticleId == article.Id);
                if (rateInfo != null)
                {
                    article.AvgRate = (float)rateInfo.AvgRate;
                    article.RateCount = rateInfo.RateCount;
                }
            }

            return new PagedResult<ArticleQueryModel>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<List<ArticleQueryModel>> GetMostViewedArticles(int take = 10, CancellationToken cancellation = default)
        {
            if (take <= 0)
                take = 10;

            var publishedArticledIds = await _blogContext.Articles
                .Where(x => x.PublishDate <= DateTime.Now)
                .Select(x => x.Id)
                .ToListAsync(cancellation);

            if (!publishedArticledIds.Any())
                return new List<ArticleQueryModel>();

            // ---------- Calculate AvgRate from Comments ----------
            var avgRates = await _commentContext.Comments
                .Where(x => !x.IsCanceled && x.IsConfirmed)
                .Where(x => x.Type == (int)CommentType.Article)
                .Where(x => x.Rate >= 1 && x.Rate <= 5)
                .GroupBy(x => x.RecordId)
                .Select(g => new
                {
                    ArticleId = g.Key,
                    AvgRate = g.Average(c => c.Rate),
                    RateCount = g.Count()
                })
                .ToListAsync(cancellation);

            var popularIds = await _blogContext.Articles
                .Where(x => publishedArticledIds.Contains(x.Id))
                .OrderByDescending(x => x.ViewCount)
                .Take(take)
                .Select(x => x.Id)
                .ToListAsync(cancellation);

            if (!popularIds.Any())
                return new List<ArticleQueryModel>();

            // ---------- Load Article Info ----------
            var articles = await _blogContext.Articles
                .Include(x => x.Category)
                .Where(x => x.PublishDate <= DateTime.Now
                && popularIds.Contains(x.Id))
                .Select(x => new ArticleQueryModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    Picture = x.Picture,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    PublishDate = x.PublishDate.ToFarsi().ToPersianNumber(),
                    ViewCount = x.ViewCount,
                    ShortDescription = x.ShortDescription,
                    CategoryName = x.Category.Name,
                    CategorySlug = x.Category.Slug,
                }).AsNoTracking().ToListAsync(cancellation);

            foreach (var article in articles)
            {
                var rateInfo = avgRates.FirstOrDefault(x => x.ArticleId == article.Id);
                if (rateInfo != null)
                {
                    article.AvgRate = (float)rateInfo.AvgRate;
                    article.RateCount = rateInfo.RateCount;
                }
            }

            return articles
                .OrderByDescending(x => x.ViewCount)
                .ThenByDescending(x => x.AvgRate)
                .ThenByDescending(x => x.RateCount)
                .ToList();
        }

        public async Task<long> GetArticleIdBySlug(string articleSlug, CancellationToken cancellation = default)
        {
            return await _blogContext.Articles
                .Where(x => x.Slug == articleSlug)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellation);
        }
    }
}

